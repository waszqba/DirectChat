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

        private readonly ConnectionMeta _config = new();

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            _config.host = true;
            
            if (portBox.Text == "")
            {
                MessageBox.Show("Musisz podać port, aby móc nasłuchiwać połączeń przychodzących", "Nieokreślony port");
                return;
            }
            _config.port = int.Parse(portBox.Text);

            _started = ((MainWindow) Application.Current.MainWindow!).Init(_config);
            if (_started) Close();
        }

        private void SetupWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if(!_started) ((MainWindow) Application.Current.MainWindow!).RemoteClose();
        }
    }
}
