using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DirectChat
{
    /// <summary>
    /// Logika interakcji dla klasy SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        private bool _started = false;
        public SetupWindow()
        {
            InitializeComponent();
        }

        private ConnectionMeta _config = new ConnectionMeta();

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            _config.host = (bool)((Button)sender).Tag;
            if (_config.host)
            {
                if (portBox.Text == "")
                {
                    MessageBox.Show("Jako host musisz podać port", "Nieokreślony port");
                    return;
                }
                _config.port = Int32.Parse(portBox.Text);
            }
            else
            {
                if (ipBox.Text == "" || port2Box.Text == "")
                {
                    MessageBox.Show("Jako gość musisz podać adres i port hosta",
                        "Brakujące dane");
                    return;
                }
                _config.ip = ipBox.Text;
                _config.remotePort = Int32.Parse(port2Box.Text);
            }

            _started = ((MainWindow) Application.Current.MainWindow).Init(_config);
            if (_started) Close();
        }

        private void SetupWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if(!_started) ((MainWindow) Application.Current.MainWindow).RemoteClose();
        }
    }
}
