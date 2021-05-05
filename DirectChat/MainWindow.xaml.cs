using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace DirectChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, ChatTemplate> _chatWindows = new();
        private readonly Dictionary<string, MsgTile> _chatTiles = new();
        private string _currentConvo = "Nowa Konwersacja";
        private MessageBroker? _broker;

        public MainWindow()
        {
            InitializeComponent();
            SetupWindow config = new();
            config.Show();
            Hide();
        }

        public bool Init(ConnectionMeta settings)
        {
            _broker = new MessageBroker(settings, OnMsg, OnDisconnect);
            if (_broker.Booted) Show();
            Title = $"Twój adres: {GetIp()}:{settings.port}";
            return _broker.Booted;
        }

        private void OnMsg(string msg, EndPoint endPoint)
        {
            Dispatcher.Invoke(() => { SpawnMessage(msg, endPoint); });
        }

        private void OnDisconnect(EndPoint endPoint)
        {

            // TODO: target specific conversation
            MessageBox.Show("Rozmówca się rozłączył, koniec pracy programu.");
            RemoteClose();
        }

        public void RemoteClose()
        {
            Dispatcher.Invoke(Close);
        }

        private void SpawnMessage(string msg, EndPoint endPoint)
        {
            var stringPoint = endPoint.ToString()!;
            if (_chatTiles.ContainsKey(stringPoint))
            {
                _chatTiles[stringPoint].NewMessage(msg, DateTime.Now);
                _chatWindows[stringPoint].SpawnMessage(msg, true);
                return;
            }
            var chatWindow = new ChatTemplate(s =>
            {
                _broker!.Send(s, stringPoint);
                _chatTiles[stringPoint].NewMessage(s, DateTime.Now);
            })
            {
                Visibility = Visibility.Collapsed
            };
            if (msg != "") chatWindow.SpawnMessage(msg, true);
            var chatTile = new MsgTile(msg == "" ? "Nowa Wiadomość" : msg, endPoint, DateTime.Now, msg != "");
            chatTile.MouseUp += SwitchChat;
            chatTile.Cursor = Cursors.Hand;
            _chatTiles.Add(stringPoint, chatTile);
            _chatWindows.Add(stringPoint, chatWindow);
            TopicsPanel.Children.Add(_chatTiles[stringPoint]);
            Grid.SetColumn(chatWindow, 1);
            MainGrid.Children.Add(_chatWindows[stringPoint]);
        }

        private void SwitchChat(object sender, MouseButtonEventArgs? e)
        {
            if (_currentConvo != "Nowa Konwersacja")
            {
                _chatWindows[_currentConvo].Visibility = Visibility.Collapsed;
                _chatTiles[_currentConvo].Deactivate();
            }
            _currentConvo = sender is string s ? s : ((MsgTile)sender).AddressBlock.Text;

            if (_currentConvo == "Nowa Konwersacja") return;

            _chatWindows[_currentConvo].Visibility = Visibility.Visible;
            _chatTiles[_currentConvo].Activate();
        }

        private void SwitchProxy(object sender, MouseButtonEventArgs e)
        {
            SwitchChat("Nowa Konwersacja", null);
        }

        private static string? GetIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return (from ip in host.AddressList where ip.AddressFamily == AddressFamily.InterNetwork select ip.ToString()).FirstOrDefault();
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AddressBox.Text == "" || PortBox.Text == "")
            {
                MessageBox.Show("Musisz podać adres i port hosta",
                    "Brakujące dane");
                return;
            }

            _broker!.NewConvo(new ConnectionMeta()
            {
                ip = AddressBox.Text,
                remotePort = int.Parse(PortBox.Text),
                host = false,
            });
            var endPoint = new IPEndPoint(IPAddress.Parse(AddressBox.Text), int.Parse(PortBox.Text));
            var stringPoint = endPoint.ToString();
            SpawnMessage("", endPoint);
            SwitchChat(stringPoint, null);
        }
    }
}