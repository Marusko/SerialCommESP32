using System;
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
        private readonly SerialPort _port;
        private bool _prefixLocked = false;
        public bool CanOpen = true;

        private readonly ScreenHandler _screenHandler;
        private LinkHandler? _linkHandler;
        private ResultWindow _window;
        public bool OpenedWindow { get; set; }
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
            SendText.IsEnabled = false;
            LockBtn.IsEnabled = false;
            ResetBtn.IsEnabled = false;
            PortBox.IsEnabled = true;
            WindowBtn.IsEnabled = false;
            WindowBox.IsEnabled = false;
            ResultLink.IsEnabled = false;
            Prefix.IsEnabled = false;
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
            LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Selected screen {_screenHandler.SelectedScreen.DeviceName}\n";
            Scroller.ScrollToBottom();
        }

        private void OpenScreen(object sender, RoutedEventArgs e)
        {
            CanOpen = true;
            WindowBtn.IsEnabled = false;
            WindowBox.IsEnabled = false;
            ResultLink.IsEnabled = false;
            OpenedWindow = true;
            _linkHandler = new LinkHandler(ResultLink.Text, this);
            if (CanOpen)
            {
                _screenHandler.StopTimer();
                _window = new ResultWindow(_screenHandler, _linkHandler);
                _window.Show();
                LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Opened window on {_screenHandler.SelectedScreen.DeviceName}\n";
                Scroller.ScrollToBottom();
            }
            else
            {
                _linkHandler.StopTimer();
                _linkHandler = null;
                OpenedWindow = false;
                WindowBtn.IsEnabled = true;
                WindowBox.IsEnabled = true;
                ResultLink.IsEnabled = true;
            }
        }

        private void Connect(object sender, EventArgs ea)
        {
            ConnectBtn.IsEnabled = false;
            DisconnectBtn.IsEnabled = true;
            ModeBtn.IsEnabled = true;
            LockBtn.IsEnabled = true;
            ResetBtn.IsEnabled = true;
            PortBox.IsEnabled = false;
            WindowBtn.IsEnabled = true;
            WindowBox.IsEnabled = true;
            ResultLink.IsEnabled = true;
            Prefix.IsEnabled = true;
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
                LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: {e.Message}\n";
                Disconnect(sender, ea);
                LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Disconnected\n";
                Scroller.ScrollToBottom();
            }
            LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Connected to port: {PortBox.Text}\n";
            Scroller.ScrollToBottom();
        }
        private void Disconnect(object sender, EventArgs ea)
        {
            ConnectBtn.IsEnabled = true;
            DisconnectBtn.IsEnabled = false;
            ModeBtn.IsEnabled = false;
            SendBtn.IsEnabled = false;
            SendText.IsEnabled = false;
            LockBtn.IsEnabled = false;
            ResetBtn.IsEnabled = false;
            PortBox.IsEnabled = true;
            WindowBtn.IsEnabled = false;
            WindowBox.IsEnabled = false;
            ResultLink.IsEnabled = false;
            Prefix.IsEnabled = false;
            try
            {
                _port.Close();
                _prefix = "";
                Prefix.Text = "";
                LockBtn.Content = "Lock";
                _prefixLocked = false;
                _mode = 0;
                ModeLabel.Content = "Read";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: {e.Message}\n";
                Scroller.ScrollToBottom();
            }
            LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Disconnected\n";
            Scroller.ScrollToBottom();
        }

        private void ChangeMode(object sender, EventArgs ea)
        {
            if (_mode == 0)
            {
                _mode = 1;
                ModeLabel.Content = "Write";
                LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Writing mode\n";
                Scroller.ScrollToBottom();
            }
            else
            {
                _mode = 0;
                ModeLabel.Content = "Read";
                LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Reading mode\n";
                Scroller.ScrollToBottom();
                SendBtn.IsEnabled = false;
                SendText.IsEnabled = false;
            }
        }

        private void Reset(object sender, EventArgs ea)
        {
            _port.WriteLine("2");
            _mode = 0;
            ModeLabel.Content = "Read";
            SendBtn.IsEnabled = false;
            SendText.IsEnabled = false;
            LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: RESET\n";
            Scroller.ScrollToBottom();
        }

        private void ClearLogs(object sender, EventArgs ea)
        {
            LogsTextBlock.Text = "";
        }

        private void Lock(object sender, EventArgs ea)
        {
            if (!_prefixLocked)
            {
                var resp = MessageBox.Show("Lock prefix?", "Lock", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (resp == MessageBoxResult.Yes)
                {
                    _prefix = Prefix.Text;
                    Prefix.IsEnabled = false;
                    LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Prefix {_prefix} locked\n";
                    Scroller.ScrollToBottom();
                    LockBtn.Content = "Unlock";
                    _prefixLocked = true;
                }
            }
            else
            {
                var resp = MessageBox.Show("Unlock prefix?", "Unlock", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (resp == MessageBoxResult.Yes)
                {
                    LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Prefix {_prefix} unlocked\n";
                    Scroller.ScrollToBottom();
                    _prefix = "";
                    Prefix.IsEnabled = true;
                    LockBtn.Content = "Lock";
                    _prefixLocked = false;
                }
            }
        }

        private void Write(object sender, EventArgs ea)
        {
            if (_port.IsOpen)
            {
                SendBtn.IsEnabled = false;
                SendText.IsEnabled = false;
                var m = SendText.Text;
                var n = m.Replace("\n", "").Replace("\r", "");
                _port.WriteLine("1");
                Thread.Sleep(500);
                _port.WriteLine($"{_prefix}-{n}#");
                LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Writing {_prefix}-{n}\n";
                Scroller.ScrollToBottom();
                SendText.Text = "";
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
                    var n = data.Replace("\n", "").Replace("\r", "").Replace(" ", "");
                    if (n.Equals("300"))
                    {
                        LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: Reading failed\n";
                        Scroller.ScrollToBottom();
                    }
                    else if(n.Equals("301"))
                    {
                        LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: Reading authentication failed\n";
                        Scroller.ScrollToBottom();
                    }
                    else
                    {
                        LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Received: {n}\n";
                        Scroller.ScrollToBottom();
                        var split = n.Split("-");
                        if (split[0].Equals(_prefix))
                        {
                            FindRacer(split[1]);
                        }
                    }
                }
            });
        }

        private void FindRacer(string bib)
        {
            if (_linkHandler is not null)
            {
                var r = _linkHandler.Racers.Find(x => x.Bib.Equals(bib));
                Dispatcher.Invoke(() =>
                {
                    if (_window is not null && r is not null)
                    {
                        _window.NameLabelR.Content = r.Name;
                        _window.BibLabelR.Content = r.Bib;
                        _window.TimeLabelR.Content = r.Time;
                        _window.AGRLabelR.Content = r.AgeGroupRank;
                        _window.GRLabelR.Content = r.GenderRank;
                        _window.ORLabelR.Content = r.OverallRank;
                        LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Number {bib} found\n";
                        Scroller.ScrollToBottom();
                    }
                    else
                    {
                        LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: Number {bib} not found\n";
                        Scroller.ScrollToBottom();
                    }
                });
            }
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
                    var data = _port.ReadExisting();
                    var n = data.Replace("\n", "").Replace("\r", "").Replace(" ", "");
                    if (n.Equals("300"))
                    {
                        LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: Reading failed\n";
                        Scroller.ScrollToBottom();
                    }
                    else if (n.Equals("301"))
                    {
                        LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: Reading authentication failed\n";
                        Scroller.ScrollToBottom();
                    }
                    else if (n.Equals("400"))
                    {
                        LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: Writing failed\n";
                        Scroller.ScrollToBottom();
                    }
                    else if (n.Equals("401"))
                    {
                        LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: Writing authentication failed\n";
                        Scroller.ScrollToBottom();
                    }
                    else if (n.Equals("420"))
                    {
                        LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Writing successful\n";
                        Scroller.ScrollToBottom();
                    }
                    else
                    {
                        SendBtn.IsEnabled = true;
                        SendText.IsEnabled = true;
                        LogsTextBlock.Text += $"[{DateTime.Now}][INFO]: Ready to write\n";
                        Scroller.ScrollToBottom();
                    }
                });
            }
        }


    }
}
