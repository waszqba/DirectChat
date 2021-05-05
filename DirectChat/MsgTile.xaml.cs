using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Logika interakcji dla klasy MsgTile.xaml
    /// </summary>
    public partial class MsgTile : UserControl
    {
        private List<TextBlock> _blockList = new();
        private bool _active = false;
        public MsgTile(string message, EndPoint address, DateTime time, bool inbound = true)
        {
            InitializeComponent();
            MsgBlock.Text = message;
            AddressBlock.Text = address.ToString()!;
            TimeBlock.Text = ParseTime(time);
            _blockList.Add(MsgBlock);
            _blockList.Add(AddressBlock);
            _blockList.Add(TimeBlock);
            SetWeight(inbound);
        }

        public void Deactivate()
        {
            _active = false;
            MainBorder.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        public void Activate()
        {
            _active = true;
            SetWeight(false);
            MainBorder.Background = new SolidColorBrush(Color.FromRgb(233, 233, 227));
        }

        public void NewMessage(string message, DateTime time)
        {
            MsgBlock.Text = message;
            TimeBlock.Text = ParseTime(time);
            if (!_active) SetWeight(true);
        }

        private static string ParseTime(DateTime time)
        {
            return $"{PadTime(time.Hour)}:{PadTime(time.Minute)}";
        }

        private static string PadTime(int time)
        {
            return time < 10 ? $"0{time}" : time.ToString();
        }

        private void SetWeight(bool bold)
        {
            foreach (var textBlock in _blockList) textBlock.FontWeight = FontWeight.FromOpenTypeWeight(bold ? 700 : 400);
        }
    }
}
