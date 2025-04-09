using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EchoSim22
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static SerialPort? _serialPort;
        DispatcherTimer _timer = new DispatcherTimer();
        private string? PortName = string.Empty;
        public MainWindow()
        {
            InitializeComponent();

            //FIXME Delete a Chart
            double[] dataX = { 1, 2, 3, 4, 5 };
            double[] dataY = { 1, 4, 9, 16, 25 };
            MainPlot.Plot.Add.Scatter(dataX, dataY);

            //FIXME Disabling FPS
            //MainPlot.UserInputProcessor.DoubleLeftClickBenchmark(false);

            // clear existing menu items
            MainPlot.Menu?.Clear();

            // NOTE Setting AutoScale
            MainPlot.Menu?.Add("По центру", (plot) =>
            {
                plot.Axes.AutoScale();
                plot.PlotControl?.Refresh();
            });

            MainPlot.Refresh();

            _timer.Tick += new EventHandler(Timer_Tick);
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            //_timer.Start();

        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void FindPort_Click(object sender, RoutedEventArgs e)
        {

            var portName = SerialPort.GetPortNames();

            if (portName != null)
            {
                ComPortBoxName.Items.Clear();

                ComPortBoxName.IsEnabled = true;
                foreach (string name in portName)
                {
                    ComPortBoxName.Items.Add(name);
                }
                ComPortBoxName.Items.Add(String.Empty);
            }

        }


        private void ComPortBoxName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //PortName = string.Empty;

            if (ComPortBoxName.SelectedItem != null)
            {
                PortName = ComPortBoxName.SelectedItem.ToString();

                FindPortButton.Content = "Обновить порт";
                FindPortButton.Background = Brushes.Aqua;
            }

            if (PortName != String.Empty)
            {

                ConnectPortButton.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Com Port не выбран");
            }
        }

        private async void ConnectPortButton_Click(object sender, RoutedEventArgs e)
        {
            var message_MessageBox = $"Подключить порт {PortName}?";
            var caption_MessageBox = "Вопрос!";

            //NOTE открытие порта
            //TODO Флаг открытого порта
            var opening_port = false;

            if (!_serialPort.IsOpen)
            {
                var result_MessageBox = MessageBox.Show(message_MessageBox, caption_MessageBox,
                MessageBoxButton.OKCancel);
                if (result_MessageBox == MessageBoxResult.OK) { opening_port = true; }
            }
            else
            {
                opening_port=false;
            }
            

            if (opening_port)
            {
                for (int i = 5; i > 0; i--)
                {
                    ConnectPortButton.Background = Brushes.Red;
                    ConnectPortButton.Content = i.ToString();
                    await Task.Delay(700);
                    ConnectPortButton.Background = Brushes.GreenYellow;
                }

                var connectPort = ConnectingPort(true);

                if (connectPort)
                {
                    ConnectPortButton.Content = "Отключить";
                    FindPortButton.IsEnabled = false;
                    ComPortBoxName.IsEnabled = false;
                }

                

            }
        }

        private bool ConnectingPort(bool connectPort)
        {
            _serialPort = new SerialPort(PortName, 9600, Parity.None, 8, StopBits.One);

            var openPort = false;

            try
            {
                _serialPort.Open();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Порт не открыт");
                openPort = false;
            }

            if (_serialPort.IsOpen)
            {
                _timer.Start();

                openPort = true;
            }

            return openPort;
        }
    }
}