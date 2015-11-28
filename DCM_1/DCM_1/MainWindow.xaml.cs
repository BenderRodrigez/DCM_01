using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
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
        private Stopwatch _compressStopWath;
        private Stopwatch _decompressStopWath;
        private readonly OpenFileDialog _openDialog = new OpenFileDialog();
        private ICompressedImage _compressed;
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
            Bitmap comprBitmap;
            if (VQCompression)
            {
                comprBitmap = VqDecompress();
            }
            else
            {
                _decompressStopWath = new Stopwatch();
                _decompressStopWath.Start();
                var comprImg = (DwtImage)_compressed;
                var deflate = new DeflateStream(new MemoryStream(_compressedData), CompressionMode.Decompress);
                var comprStream = new MemoryStream();
                deflate.CopyTo(comprStream);
                _compressedData = comprStream.ToArray();
                _compressedData = HuffmanEncoder.Decode(_compressedData);
                comprImg.FromByteArray(_compressedData);
                var img = RLE.DecompressImage(comprImg.Image, comprImg.Width, comprImg.Height);
                comprBitmap = ApplyHaarTransform(false, false, CodeBookSizePow, img);
                _decompressStopWath.Stop();
            }
            var stream = new MemoryStream();
            comprBitmap.Save(stream, ImageFormat.Png);
            ResultImage.BeginInit();
            ResultImage.StreamSource = stream;
            ResultImage.EndInit();
            OnPropertyChanged("ResultImage");
        }

        private Bitmap VqDecompress()
        {
            _decompressStopWath = new Stopwatch();
            _decompressStopWath.Start();
            var deflate = new DeflateStream(new MemoryStream(_compressedData), CompressionMode.Decompress);
            var comprStream = new MemoryStream();
            deflate.CopyTo(comprStream);
            _compressedData = comprStream.ToArray();
            _compressedData = HuffmanEncoder.Decode(_compressedData);
            _compressed.FromByteArray(_compressedData);
            var comprBitmap = new Bitmap(_compressed.Width, _compressed.Height);
            var comprImg = (VqImage) _compressed;
            var img = RLE.Decompress(comprImg.Image);
            var k = 0;
            for (int i = 0; i < comprBitmap.Width; i++)
            {
                for (int j = 0; j < comprBitmap.Height; j++)
                {
                    var pos = img[k];
                    comprBitmap.SetPixel(i, j,
                        Color.FromArgb(comprImg.CodeBook[pos][0], comprImg.CodeBook[pos][2],
                            comprImg.CodeBook[pos][1]));
                    k++;
                }
            }
            _decompressStopWath.Stop();
            return comprBitmap;
        }

        private void VqCompress()
        {
            _compressStopWath = new Stopwatch();
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

            _compressed = new VqImage
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
            var imgCompr = ((VqImage) _compressed);
            imgCompr.Image = rle.Compress();
            _compressedData = imgCompr.ToByteArray();
            _compressedData = HuffmanEncoder.Encode(_compressedData);
            byte[] result;

            using (var resultStream = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(resultStream, CompressionLevel.Optimal))
                {
                    compressionStream.Write(_compressedData, 0, _compressedData.Length);
                }
                result = resultStream.ToArray();
            }
            _compressedData = result;
            _compressStopWath.Stop();
        }

        private void Compress()
        {
            if(VQCompression) VqCompress();
            else
            {
                _compressStopWath = new Stopwatch();
                _fileName = _openDialog.FileName;
                OnPropertyChanged("SourceImage");
                var bitmap = new Bitmap(_fileName);
                _compressStopWath.Start();
                bitmap = ApplyHaarTransform(true, false, CodeBookSizePow, bitmap);

                var rle = new RLE(bitmap);
                var imgCompr = new DwtImage {Height = bitmap.Height, Width = bitmap.Width, Image = rle.CompressImage()};
                _compressedData = imgCompr.ToByteArray();
                _compressedData = HuffmanEncoder.Encode(_compressedData);
                byte[] result;

                using (var resultStream = new MemoryStream())
                {
                    using (var compressionStream = new DeflateStream(resultStream, CompressionLevel.Optimal))
                    {
                        compressionStream.Write(_compressedData, 0, _compressedData.Length);
                    }
                    result = resultStream.ToArray();
                }
                _compressedData = result;
                _compressStopWath.Stop();
                _compressed = imgCompr;
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
            var val = _decompressStopWath.ElapsedMilliseconds / (double)_compressStopWath.ElapsedMilliseconds;
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
}
