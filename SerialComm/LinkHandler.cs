using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using Newtonsoft.Json;

namespace SerialComm
{
    /// <summary>
    /// Handles retrieving data from links
    /// </summary>
    public class LinkHandler
    {
        private readonly string _mainLink;
        private readonly System.Windows.Threading.DispatcherTimer _timer = new();
        public readonly MainWindow _mainWindow;
        private readonly HttpClient _httpClient;
        public List<Racer> Racers { get; set; } = new List<Racer>();

        public LinkHandler(string mainLink, MainWindow mw)
        {
            _mainLink = mainLink;
            _mainWindow = mw;
            _httpClient = new HttpClient();
            ReadMainLink();

            _timer.Tick += RefreshResults;
            _timer.Interval = new TimeSpan(0, 0, 15);
            _timer.Start();
        }

        private async void ReadMainLink()
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(_mainLink);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _mainWindow.LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: {e.Message}\n";
                _mainWindow.Scroller.ScrollToBottom();
                _mainWindow.CanOpen = false;
                return;
            }

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                Racers = JsonConvert.DeserializeObject<List<Racer>>(responseString);
            }
            else
            {
                MessageBox.Show($"Error: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _mainWindow.LogsTextBlock.Text += $"[{DateTime.Now}][WARNING]: {response.StatusCode}\n";
                _mainWindow.Scroller.ScrollToBottom();
            }
        }
        public void StopTimer()
        {
            _timer.Stop();
        }

        private void RefreshResults(object? sender, EventArgs e)
        {
            ReadMainLink();
        }
    }
}
