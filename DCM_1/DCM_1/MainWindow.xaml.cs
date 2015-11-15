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
using Huffman;
using Microsoft.Win32;
using TestHaarCSharp;
using Color = System.Drawing.Color;

namespace DCM_1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    internal sealed partial class MainWindow : INotifyPropertyChanged
    {
        public ImageSource SourceImage { get; set; }
        public BitmapImage ResultImage { get; set; }
        public string PerformanceComparison { get; set; }
        public string SourceFileSize { get; set; }
        public string ResultFileSize { get; set; }
        public bool VQCompression { get; set; }

        public int CodeBookSizePow
        {
            get { return _codeBookSizePow; }
            set
            {
                _codeBookSizePow = value; 
                OnPropertyChanged();
            }
        }

        private string _fileName;
        private VectorQuantization _vq;
        private Stopwatch _sompressStopWath;
        private Stopwatch _decompressStopWath;
        private readonly OpenFileDialog _openDialog = new OpenFileDialog();
        private CompressedImage _compressed;
        private int _codeBookSizePow = 6;
        private byte[] _compressedData;

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
            _compressed.FromByteArray(HuffmanEncoder.Decode(_compressedData));
            var comprBitmap = new Bitmap(_compressed.Width, _compressed.Height);
            var img = RLE.Decompress(_compressed.Image);
            var k = 0;
            for (int i = 0; i < comprBitmap.Width; i++)
            {
                for (int j = 0; j < comprBitmap.Height; j++)
                {
                    var pos = img[k];
                    comprBitmap.SetPixel(i, j,
                        Color.FromArgb(_compressed.CodeBook[pos][0], _compressed.CodeBook[pos][2],
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

        private void VqCompress()
        {
            _sompressStopWath = new Stopwatch();
            _fileName = _openDialog.FileName;
            OnPropertyChanged("SourceImage");
            var bitmap = new Bitmap(_fileName);
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

            var image = colors.GroupBy(x => x).AsParallel()
                .Select(x => new[] { (double)x.Key.R, (double)x.Key.B, (double)x.Key.G })
                .AsParallel()
                .ToArray();

            _vq = new VectorQuantization(image, 3, (int)Math.Pow(2, CodeBookSizePow));

            _compressed = new CompressedImage
            {
                Height = bitmap.Height,
                Width = bitmap.Width,
                CodeBook =
                    _vq.CodeBook.Select(
                        x =>
                            new[]
                                {
                                    (byte) Math.Round(x[0]), (byte) Math.Round(x[1]), (byte) Math.Round(x[2])
                                }).ToArray()
            };

            k = 0;
            var img = new byte[bitmap.Width * bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var pos = _vq.QuantazationIndex(new[] { (double)color.R, color.B, color.G });
                    img[k] = (byte)pos;
                    k++;
                }
            }
            var rle = new RLE(img);
            _compressed.Image = rle.Compress();
            _compressedData = _compressed.ToByteArray();
            _compressedData = HuffmanEncoder.Encode(_compressedData);
            _sompressStopWath.Stop();
        }

        private void Compress()
        {
            if(VQCompression) VqCompress();
            else
            {
                _sompressStopWath = new Stopwatch();
                _fileName = _openDialog.FileName;
                OnPropertyChanged("SourceImage");
                var bitmap = new Bitmap(_fileName);
                _sompressStopWath.Start();
                bitmap = ApplyHaarTransform(true, false, 1, bitmap);

                var rle = new RLE(img);
                _compressed.Image = rle.Compress();
                _compressedData = _compressed.ToByteArray();
                _compressedData = HuffmanEncoder.Encode(_compressedData);

                _sompressStopWath.Stop();
            }
            Wind.Dispatcher.Invoke(Decompress);
            Wind.Dispatcher.Invoke(SetResults);
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
            return bmp;
        }

        private void SetResults()
        {
            var val = _decompressStopWath.ElapsedMilliseconds / (double)_sompressStopWath.ElapsedMilliseconds;
            PerformanceComparison = string.Format("1/{0:F3}", val);

            SourceFileSize = _compressed.Width*_compressed.Height*4 + " байт";


            var compressedFileSize = _compressedData.Length;
            ResultFileSize = compressedFileSize + " байт";
            OnPropertyChanged("PerformanceComparison");
            OnPropertyChanged("ResultFileSize");
            OnPropertyChanged("SourceFileSize");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    struct DwtCompressedImage
    {
        public int[] Image;
        public int Width;
        public int Height;
    }

    struct CompressedImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[][] CodeBook { get; set; }
        public Tuple<byte,ushort>[] Image { get; set; }

        public void FromByteArray(byte[] data)
        {
            var int32 = new byte[4];
            Array.Copy(data, int32, 4);
            Width = BitConverter.ToInt32(int32, 0);
            Array.Copy(data, 4, int32, 0, 4);
            Height = BitConverter.ToInt32(int32, 0);
            var cbSize = data[8];
            var cb = new List<byte[]>();
            for (int i = 0; i <= cbSize; i++)
            {
                var codeWord = new byte[3];
                Array.Copy(data, 9 + i*3, codeWord, 0, 3);
                cb.Add(codeWord);
            }
            CodeBook = cb.ToArray();

            var img = new List<Tuple<byte, ushort>>(data.Length - 12 + cbSize*3);
            for (int i = 12 + cbSize*3; i < data.Length - 2; i += 3)
            {
                var single = new byte[2];
                Array.Copy(data, i + 1, single, 0, 2);
                img.Add(new Tuple<byte, ushort>(data[i], BitConverter.ToUInt16(single, 0)));
            }
            Image = img.ToArray();
        }

        public byte[] ToByteArray()
        {
            var bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(Width));
            bytes.AddRange(BitConverter.GetBytes(Height));

            var cbSize = (byte) (CodeBook.Length - 1);
            bytes.Add(cbSize);
            foreach (var b in CodeBook)
            {
                bytes.AddRange(b);
            }

            foreach (var tuple in Image)
            {
                bytes.Add(tuple.Item1);
                bytes.AddRange(BitConverter.GetBytes(tuple.Item2));
            }
            return bytes.ToArray();
        }
    }
}
