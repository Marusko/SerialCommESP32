using System;
using System.Windows;

namespace SerialComm
{
    /// <summary>
    /// Interaction logic for ResultWindow.xaml
    /// </summary>
    public partial class ResultWindow
    {
        private readonly ScreenHandler _screenHandler;
        private readonly LinkHandler _linkHandler;
        public ResultWindow(ScreenHandler sh, LinkHandler linkHandler)
        {
            InitializeComponent();
            _screenHandler = sh;
            Loaded += WindowLoaded;
            Closed += Ending;
            _linkHandler = linkHandler;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (_screenHandler.SelectedScreen == null) return;
            WindowState = WindowState.Normal;
            Left = _screenHandler.SelectedScreen.WorkingArea.Left;
            Top = _screenHandler.SelectedScreen.WorkingArea.Top;
            Width = _screenHandler.SelectedScreen.WorkingArea.Width;
            Height = _screenHandler.SelectedScreen.WorkingArea.Height;
            WindowState = WindowState.Maximized;
        }

        private void Ending(object? sender, EventArgs e)
        {
            _linkHandler.StopTimer();
            _linkHandler.MainWindow.WindowBtn.IsEnabled = true;
            _linkHandler.MainWindow.WindowBox.IsEnabled = true;
            _linkHandler.MainWindow.ResultLink.IsEnabled = true;
        }
    }
}
