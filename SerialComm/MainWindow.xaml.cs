using System;
using System.Drawing.Printing;
using System.Windows;
using System.IO.Ports;
using System.Threading;

namespace SerialComm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _mode = 0;
        private string _prefix = "";
        private SerialPort _port;

        private readonly ScreenHandler _screenHandler;
        private LinkHandler? _linkHandler;
        private ResultWindow _window;
        public bool OpenedTimer { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            _port = new SerialPort();
            _port.DataReceived += GetData;

            _screenHandler = new ScreenHandler(this);
            WindowBox.ItemsSource = _screenHandler.GetScreenNames();
            WindowBox.SelectionChanged += SelectScreen;
            WindowBox.SelectedIndex = 0;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ConnectBtn.IsEnabled = true;
            DisconnectBtn.IsEnabled = false;
            ModeBtn.IsEnabled = false;
            SendBtn.IsEnabled = false;
            LockBtn.IsEnabled = false;
            ResetBtn.IsEnabled = false;
            PortBox.IsEnabled = true;
            WindowBtn.IsEnabled = false;
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                PortBox.Items.Add(port);
            }
        }

        private void SelectScreen(object sender, RoutedEventArgs e)
        {
            try
            {
                _screenHandler.SelectedScreen = _screenHandler.GetScreens()[WindowBox.SelectedIndex];
            }
            catch (Exception)
            {
                _screenHandler.SelectedScreen = _screenHandler.GetScreens()[0];
                WindowBox.SelectedIndex = 0;
            }
        }

        private void OpenScreen(object sender, RoutedEventArgs e)
        {
            _screenHandler.StopTimer();
            _linkHandler = new LinkHandler(ResultLink.Text);
            _window = new ResultWindow(_screenHandler, _linkHandler);
            _window.Show();
        }

        private void Connect(object sender, EventArgs ea)
        {
            ConnectBtn.IsEnabled = false;
            DisconnectBtn.IsEnabled = true;
            ModeBtn.IsEnabled = true;
            SendBtn.IsEnabled = true;
            LockBtn.IsEnabled = true;
            ResetBtn.IsEnabled = true;
            PortBox.IsEnabled = false;
            WindowBtn.IsEnabled = true;
            try
            {
                _port.PortName = PortBox.Text;
                _port.BaudRate = 115200;
                _port.Parity = Parity.None;
                _port.StopBits = StopBits.One;
                _port.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                Disconnect(sender, ea);
            }
        }
        private void Disconnect(object sender, EventArgs ea)
        {
            ConnectBtn.IsEnabled = true;
            DisconnectBtn.IsEnabled = false;
            ModeBtn.IsEnabled = false;
            SendBtn.IsEnabled = false;
            LockBtn.IsEnabled = false;
            ResetBtn.IsEnabled = false;
            PortBox.IsEnabled = true;
            Prefix.IsEnabled = true;
            WindowBtn.IsEnabled = false;
            try
            {
                _port.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeMode(object sender, EventArgs ea)
        {
            if (_mode == 0)
            {
                _mode = 1;
                ModeLabel.Content = "Write";
            }
            else
            {
                _mode = 0;
                ModeLabel.Content = "Read";
            }
        }

        private void Reset(object sender, EventArgs ea)
        {
            _port.WriteLine("2");
        }

        private void Lock(object sender, EventArgs ea)
        {
            var resp = MessageBox.Show("Lock prefix?", "Lock", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resp == MessageBoxResult.Yes)
            {
                _prefix = Prefix.Text;
                Prefix.IsEnabled = false;
            }
        }

        private void Write(object sender, EventArgs ea)
        {
            if (_port.IsOpen)
            {
                var m = SendText.Text;
                var n = m.Replace("\n", "").Replace("\r", "");
                _port.WriteLine("1");
                Thread.Sleep(500);
                _port.WriteLine($"{_prefix}{n}#");
            }
        }

        private void Read()
        {
            Dispatcher.Invoke(() =>
            {
                var data = "";
                if (_port.IsOpen)
                {
                    _port.WriteLine("0");
                    Thread.Sleep(500);
                    data = _port.ReadExisting();
                    var n = data.Replace("\n", "").Replace("\r", "");
                    if (n.Equals("300"))
                    {
                        ReceiveText.Text = "Reading failed";
                        return;
                    }
                    else if(n.Equals("301"))
                    {
                        ReceiveText.Text = "Reading authentication failed";
                        return;
                    }
                    else if (n.Equals("400"))
                    {
                        ReceiveText.Text = "Writing failed";
                        return;
                    }
                    else if (n.Equals("401"))
                    {
                        ReceiveText.Text = "Writing authentication failed";
                        return;
                    }
                }

                ReceiveText.Text = data.Trim();
            });
        }

        private void GetData(object sender, EventArgs ea)
        {
            Thread.Sleep(500);
            if (_mode == 0)
            {
                Read();
            } 
            else if (_mode == 1)
            {
                Dispatcher.Invoke(() =>
                {
                    ReceiveText.Text = "Writing new data";
                });
            }
        }


    }
}
