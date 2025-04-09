using EchoSim22.Models;
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
        FlecsValues fl = new FlecsValues();

        private string? PortName = string.Empty;
        private bool OpenPort = false;
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
            _timer.Interval = TimeSpan.FromMilliseconds(1000);

        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (OpenPort) 
                {
                    //TODO доработать интервал между передачей строк Fuck Visual Studio

                    _serialPort.WriteLine($"{fl.F300} ppg,      {fl.HPVT} ppg,       {fl.RateFloat} gpm,       " +
                        $"0.0 bpm,         {fl.PSPress} psi,        {fl.DSPress} psi,      " +
                        $"3517 gal,      3517 gal,       0.0 bbl,       0.0 bbl,    " +
                        $"100.00 cmt%,    100.00 wtr%,       0.0 bpm,       " +
                        "0.0 bpm,         0 DigOut,          0,       100 F, " +
                        "     8.68 ppg,      1.00 ft,      7.67 ppg,    -14.75 ft,    " +
                        "0.0 un,    0.0 un,    0.0 un,       " +
                        "0.0 bbl,    0.0 un,       0.0 bbl,    102.40 %," +
                        "     0.000 ppg,       0.0 bbl,       0.0 bbl,       0.0 bpm,");                
                }
            }
            catch (Exception ex) 
            {
            
            }
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

            if (ComPortBoxName.SelectedItem != null)
            {
                PortName = ComPortBoxName.SelectedItem.ToString();

                FindPortButton.Background = Brushes.Aqua;
            }

            if (PortName != String.Empty)
            {

                ConnectPortButton.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Com Port не выбран");
                FindPortButton.Background = Brushes.Red;
                ConnectPortButton.IsEnabled = false;
            }
        }

        private async void ConnectPortButton_Click(object sender, RoutedEventArgs e)
        {
            var message_Connect_Port = $"Подключить порт {PortName}?";
            var message_Disconnect_Port = $"Отключить {PortName}";
            var caption_MessageBox = "Вопрос!";

            //NOTE открытие порта
            //TODO Флаг открытого порта            

            if (!OpenPort)
            {
                var result_MessageBox = MessageBox.Show(message_Connect_Port,
                    caption_MessageBox,
                    MessageBoxButton.OKCancel);

                if (result_MessageBox == MessageBoxResult.OK)
                {
                    for (int i = 5; i > 0; i--)
                    {
                        ConnectPortButton.Content = i.ToString();
                        await Task.Delay(700);
                        ConnectPortButton.Background = Brushes.GreenYellow;
                    }
                    ConnectingPort(true);
                }
            }
            else
            {
                var result_MessageBox = MessageBox.Show(message_Disconnect_Port,
                    caption_MessageBox,
                    MessageBoxButton.OKCancel);

                if (result_MessageBox == MessageBoxResult.OK)
                {
                    ConnectingPort(false);
                }
            }
        }

        private bool ConnectingPort(bool connectPort)
        {
            if (connectPort)
            {
                _serialPort = new SerialPort(PortName, 9600, Parity.None, 8, StopBits.One);

                try
                {
                    _serialPort.Open();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Порт не открыт");
                    OpenPort = false;
                }

                if (_serialPort.IsOpen)
                {
                    _timer.Start();

                    ConnectPortButton.Content = "Стоп";
                    FindPortButton.IsEnabled = false;
                    ComPortBoxName.IsEnabled = false;

                    OpenPort = true;
                }
            }
            else
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();

                    OpenPort = false;
                }

                _timer.Stop();

                FindPortButton.IsEnabled = true;
                ComPortBoxName.IsEnabled = true;

                ConnectPortButton.Background = Brushes.Red;
                ConnectPortButton.Content = "Подключить";
            }

            return OpenPort;
        }
    }
}