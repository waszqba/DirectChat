using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DirectChat
{
    /// <summary>
    /// Logika interakcji dla klasy ChatTemplate.xaml
    /// </summary>
    public partial class ChatTemplate : UserControl
    {
        private Action<string> _send;

        public ChatTemplate(Action<string> send)
        {
            InitializeComponent();
            _send = send;
        }

        private void SendMessage(TextBox box)
        {
            var text = box.Text.Trim();
            if (text == "") return;
            _send(text + '\0');
            SpawnMessage(text, false);
            box.Text = "";
        }

        public void SpawnMessage(string msg, bool inbound)
        {
            Panel.Children.Add(new MessageBubble().Spawn(msg, inbound, DateTime.Now));
            Scroller.ScrollToBottom();
        }

        private void MyBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            var box = (TextBox)sender;
            if (e.Key != Key.Enter) return;
            if (Keyboard.Modifiers == ModifierKeys.Shift) return;
            e.Handled = true;
            SendMessage(box);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SendMessage(MyBox);
        }
    }
}
