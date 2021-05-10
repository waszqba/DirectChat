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
        private bool _finalized = false;

        public ChatTemplate(Action<string> send)
        {
            InitializeComponent();
            _send = send;
        }

        private void SendMessage(TextBox box)
        {
            if (_finalized) return;
            var text = box.Text.Trim();
            if (text == "") return;
            _send(text + '\0');
            SpawnMessage(text, false);
            box.Text = "";
        }

        private void Finalizer()
        {
            _finalized = true;
            SpawnMessage("Rozmówca rozłączył się", true, true);
            MyBox.IsReadOnly = true;
            MyBox.Text = "Nie możesz napisać wiadomości. Rozmówca rozłączył się.";
            MyBox.Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150));
            MyBox.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }

        public void FinalizeChat()
        {
            Dispatcher.Invoke(Finalizer);
        }

        public void SpawnMessage(string msg, bool inbound, bool final = false)
        {
            Panel.Children.Add(new MessageBubble().Spawn(msg, inbound, DateTime.Now, final));
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
