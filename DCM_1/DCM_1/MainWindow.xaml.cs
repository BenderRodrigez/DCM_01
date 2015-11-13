using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DCM_1.Annotations;
using Microsoft.Win32;
using TestHaarCSharp;
using Color = System.Drawing.Color;

namespace DCM_1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public ImageSource SourceImage { get; set; }
        public BitmapImage ResultImage { get; set; }
        public string PerformanceComparison { get; set; }
        public string SourceFileSize { get; set; }
        public string ResultFileSize { get; set; }
        private string _fileName;
        private VectorQuantization _vq;
        private Stopwatch _sompressStopWath;
        private Stopwatch _decompressStopWath;
        private readonly OpenFileDialog _openDialog = new OpenFileDialog();
        private CompressedImage _compressed;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_openDialog.ShowDialog(this).Value)
            {
                SourceImage = new BitmapImage(new Uri(_openDialog.FileName));
                ResultImage = new BitmapImage();
                var task = new Task(Compress);
                task.Start();
            }
        }

        private void Decompress()
        {
            _decompressStopWath = new Stopwatch();
            _decompressStopWath.Start();
            var comprBitmap = new Bitmap(_compressed.Width, _compressed.Height);
            var img = RLE.Decompress(_compressed.Image);
            var k = 0;
            for (int i = 0; i < comprBitmap.Width; i++)
            {
                for (int j = 0; j < comprBitmap.Height; j++)
                {
                    var pos = img[k];
                    comprBitmap.SetPixel(i, j,
                        Color.FromArgb(_compressed.CodeBook[pos][3], _compressed.CodeBook[pos][0], _compressed.CodeBook[pos][2],
                            _compressed.CodeBook[pos][1]));
                    k++;
                }
            }
            comprBitmap = ApplyHaarTransform(false, false, 1, comprBitmap);
            _decompressStopWath.Stop();
            var stream = new MemoryStream();
            comprBitmap.Save(stream, ImageFormat.Png);
            ResultImage.BeginInit();
            ResultImage.StreamSource = stream;
            ResultImage.EndInit();
            OnPropertyChanged("ResultImage");
        }

        private void Compress()
        {
            _sompressStopWath = new Stopwatch();
            _fileName = _openDialog.FileName;
            OnPropertyChanged("SourceImage");
            var bitmap = new Bitmap(_fileName);
            _sompressStopWath.Start();
            bitmap = ProcessHaarTransform(bitmap);
            var k = 0;
            var colors = new List<Color>();
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    colors.Add(color);
                    k++;
                }
            }

            var image = colors.GroupBy(x => x)
                .Select(x => new[] {(double) x.Key.R, (double) x.Key.B, (double) x.Key.G, (double) x.Key.A})
                .AsParallel()
                .ToArray();

            _vq = new VectorQuantization(image, 4, 64);

            _compressed = new CompressedImage
            {
                Height = bitmap.Height,
                Width = bitmap.Width,
                CodeBook =
                    _vq.CodeBook.Select(
                        x =>
                            new[]
                            {
                                (byte) Math.Round(x[0]), (byte) Math.Round(x[1]), (byte) Math.Round(x[2]),
                                (byte) Math.Round(x[3])
                            }).ToArray()
            };
            k = 0;
            var img = new byte[bitmap.Width*bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var pos = _vq.QuantazationIndex(new[] {(double) color.R, color.B, color.G, color.A});
                    img[k] = (byte) pos;
                    k++;
                }
            }
            var rle = new RLE(img);
            _compressed.Image = rle.Compress();

            _sompressStopWath.Stop();
            Wind.Dispatcher.Invoke(Decompress);
            Wind.Dispatcher.Invoke(SetResults);
        }

        private Bitmap ProcessHaarTransform(Bitmap bmp)
        {
            return ApplyHaarTransform(true, false, 1, bmp);
        }

        private Bitmap ApplyHaarTransform(bool forward, bool safe, int iterations, Bitmap bmp)
        {
            var maxScale = WaveletTransform.GetMaxScale(bmp.Width, bmp.Height);
            if (iterations < 1 || iterations > maxScale)
            {
                MessageBox.Show(string.Format("Iteration must be Integer from 1 to {0}", maxScale));
                return new Bitmap(bmp.Width, bmp.Height);
            }

            var channels = ColorChannels.CreateColorChannels(safe, bmp.Width, bmp.Height);

            var transform = WaveletTransform.CreateTransform(forward, iterations);

            var imageProcessor = new ImageProcessor(channels, transform);
            imageProcessor.ApplyTransform(bmp);

            if (forward)
            {
                return new Bitmap(bmp);
            }
            return new Bitmap(bmp.Width, bmp.Height);
        }

        private void SetResults()
        {
            var val = _decompressStopWath.ElapsedMilliseconds / (double)_sompressStopWath.ElapsedMilliseconds;
            PerformanceComparison = string.Format("1/{0:F3}", val);

            SourceFileSize = _compressed.Width*_compressed.Height*4 + " байт";


            var compressedFileSize = _compressed.CodeBook.Length*4 + sizeof (int) + sizeof (int) + _compressed.Image.Length*3;
            ResultFileSize = compressedFileSize + " байт";
            OnPropertyChanged("PerformanceComparison");
            OnPropertyChanged("ResultFileSize");
            OnPropertyChanged("SourceFileSize");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    struct CompressedImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[][] CodeBook { get; set; }
        public Tuple<byte,short>[] Image { get; set; }
    }
}
