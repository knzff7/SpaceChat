using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SocketIOClient;

namespace SpaceChat
{
    public class ChannelData
    {
        [System.Text.Json.Serialization.JsonPropertyName("voice")]
        public List<string> Voice { get; set; } = new();
    }

    public partial class MainWindow : Window
    {
        private SocketIOClient.SocketIO? client;
        private string currentChannel = "";
        private readonly string serverUrl = "http://localhost:3000";

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                if (client != null)
                    await client.DisconnectAsync();

                client = new SocketIOClient.SocketIO(serverUrl);

                client.On("update_channels", response =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        var data = response.GetValue<ChannelData>();
                        var channels = data?.Voice ?? new List<string>();

                        ChannelsItemsControl.ItemsSource = channels;
                        StatusText.Text = $"✅ Подключено | Каналов: {channels.Count}";
                    });
                });

                client.OnConnected += (sender, e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Title = "Space P2P - Подключено ✅";
                        StatusText.Text = "✅ Соединение установлено";
                    });
                };

                client.OnDisconnected += (sender, e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Title = "Space P2P - Отключено";
                        StatusText.Text = "❌ Отключено от сервера";
                    });
                };

                StatusText.Text = "⏳ Подключение к серверу...";
                await client.ConnectAsync();
            }
            catch
            {
                Dispatcher.Invoke(() => 
                    StatusText.Text = "❌ Не удалось подключиться к серверу");
            }
        }

        private async void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "⏳ Переподключение...";
            await Task.Delay(800);
            ConnectToServer();
        }

        private async void AddChannel_Click(object sender, RoutedEventArgs e)
        {
            if (client?.Connected != true)
            {
                MessageBox.Show("Нет подключения к серверу!", "Ошибка");
                return;
            }

            string channelName = "Канал-" + new Random().Next(100, 999);
            await client.EmitAsync("create_channel", new { type = "voice", name = channelName });
        }

        private async void ChannelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string channelName)
            {
                currentChannel = channelName;
                StatusText.Text = $"Вы в канале: {channelName}";

                if (client?.Connected == true)
                    await client.EmitAsync("join_channel", new { channel = channelName });
            }
        }

        private void StartScreenShare_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция трансляции экрана в разработке.", "Информация");
        }

        protected override async void OnClosed(EventArgs e)
        {
            if (client != null)
                await client.DisconnectAsync();
            base.OnClosed(e);
        }
    }
}