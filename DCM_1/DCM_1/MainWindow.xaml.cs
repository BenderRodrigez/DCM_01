using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DCM_1.Annotations;
using Microsoft.Win32;
using NeuronDotNet.Core;
using NeuronDotNet.Core.Initializers;
using NeuronDotNet.Core.SOM;
using NeuronDotNet.Core.SOM.NeighborhoodFunctions;
using Color = System.Drawing.Color;

namespace DCM_1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ImageSource SourceImage { get; set; }
        public ImageSource ResultImage { get; set; }
        private string _fileName;
        private KohonenNetwork _network;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog(this).Value)
            {
                _fileName = openDialog.FileName;
                SourceImage = new BitmapImage(new Uri(openDialog.FileName));
                OnPropertyChanged("SourceImage");
                var bitmap = new System.Drawing.Bitmap(_fileName);
                var image = new double[bitmap.Width*bitmap.Height][];
                var k = 0;
                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        var color = bitmap.GetPixel(i, j);
                        image[k] = new[] {(double) color.R, (double) color.B, (double) color.G};
                        k++;
                    }
                }

                ProvideKohonenCom(image);

                var compressed = new CompressedImage
                {
                    Height = bitmap.Height,
                    Width = bitmap.Width,
                    CodeBook = new byte[64][]
                };
                var cb = 0;
                foreach (var neuron in _network.OutputLayer.Neurons)
                {
                    compressed.CodeBook[cb] = new[]
                    {
                        (byte) Math.Round(neuron.SourceSynapses[0].Weight), (byte) Math.Round(neuron.SourceSynapses[1].Weight),
                        (byte) Math.Round(neuron.SourceSynapses[2].Weight)
                    };
                    cb++;
                }

                compressed.Image = new byte[bitmap.Width*bitmap.Height];
                k = 0;
                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        var color = bitmap.GetPixel(i, j);
                        _network.Run(new[] {(double) color.R, (double) color.B, (double) color.G});
                        
                        compressed.Image[k] = (byte)(_network.Winner.Coordinate.Y*bitmap.Width+_network.Winner.Coordinate.X);
                        k++;
                    }
                }

                var comprBitmap = new Bitmap(bitmap.Width, bitmap.Height);
                k = 0;
                for (int i = 0; i < comprBitmap.Width; i++)
                {
                    for (int j = 0; j < comprBitmap.Height; j++)
                    {
                        comprBitmap.SetPixel(i, j, Color.FromArgb(0, compressed.CodeBook[k][0], compressed.CodeBook[k][1], compressed.CodeBook[k][2]));
                    }
                }
                //ResultImage = new DrawingImage();
            }
        }

        private bool[,] ProvideKohonenCom(double[][] trainingSet)
        {
            var outputLayerSize = 8;
            var isWinner = new bool[outputLayerSize, outputLayerSize];
            var learningRadius = outputLayerSize / 2;
            var neigborhoodFunction = new GaussianFunction(learningRadius);
            const LatticeTopology topology = LatticeTopology.Hexagonal;
            var max = trainingSet.Max(x => x.Max());
            var min = trainingSet.Min(x => x.Min());
            var inputLayer = new KohonenLayer(trainingSet[0].Length);
            var outputLayer = new KohonenLayer(new System.Drawing.Size(outputLayerSize, outputLayerSize), neigborhoodFunction, topology);
            new KohonenConnector(inputLayer, outputLayer) { Initializer = new RandomFunction(min, max) };
            outputLayer.SetLearningRate(0.2, 0.05d);
            outputLayer.IsRowCircular = false;
            outputLayer.IsColumnCircular = false;
            _network = new KohonenNetwork(inputLayer, outputLayer);

            var progress = 1;
            _network.BeginEpochEvent += (senderNetwork, args) => Array.Clear(isWinner, 0, isWinner.Length);

            _network.EndSampleEvent += delegate
            {
                isWinner[_network.Winner.Coordinate.X, _network.Winner.Coordinate.Y] =
                    true;
            };

            _network.EndEpochEvent += delegate
            {
                /*progressBar1.Value = ((progress++) * 100) / 500;
                PlotWinnersNeurons(isWinner);
                Application.DoEvents();*/
            };
            var trSet = new TrainingSet(trainingSet[0].Length);
            foreach (var x in trainingSet)
            {
                trSet.Add(new TrainingSample(x));
            }
            _network.Learn(trSet, 500);
            return isWinner;
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
        public byte[] Image { get; set; }
    }
}
