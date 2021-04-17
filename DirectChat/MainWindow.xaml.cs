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
        public MainWindow()
        {
            InitializeComponent();
            SetupWindow config = new SetupWindow();
            config.Show();
            Hide();
        }

        private MessageBroker _broker;

        public bool Init(ConnectionMeta settings)
        {
            _broker = new MessageBroker(settings, OnMsg, OnDisconnect);
            if (_broker.Booted) Show();
            Application.Current.MainWindow.Title = settings.host
                ? $"Gospodarz: {GetIp()}:{settings.port}"
                : $"Połączono w roli gościa z {settings.ip}:{settings.remotePort}";
            return _broker.Booted;
        }

        private void OnMsg(string msg)
        {
            Dispatcher.Invoke(() => { SpawnMessage(msg, true); });
        }

        private void OnDisconnect()
        {
            MessageBox.Show("Rozmówca się rozłączył, koniec pracy programu.");
            RemoteClose();
        }

        public void RemoteClose()
        {
            Dispatcher.Invoke(Close);
        }

        private void SendMessage(TextBox box)
        {
            if (box.Text == "") return;
            _broker.Send(box.Text + '\0');
            // MyBlock.Text = box.Text;
            SpawnMessage(box.Text, false);
            box.Text = "";
        }

        private void SpawnMessage(string msg, bool inbound)
        {
            Panel.Children.Add(new MessageBubble().Spawn(msg, inbound, DateTime.Now));
            Scroller.ScrollToBottom();
        }

        private void MyBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            var box = (TextBox) sender;
            if (e.Key != Key.Enter) return;
            if (Keyboard.Modifiers == ModifierKeys.Shift) return;
            e.Handled = true;
            SendMessage(box);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SendMessage(MyBox);
        }

        private static string GetIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return null;
        }
    }
}