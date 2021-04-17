using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DirectChat
{

    enum Bound { In, Out }

    class MessageBubble: Border
    {
        private readonly Color _inboundColor = Color.FromRgb(220, 248, 198);
        private readonly Color _outboundColor = Color.FromRgb(255, 255, 255);
        private const double _maxWidth = 600;
        private const double Radius = 5;

        private readonly Effect _shadow = new DropShadowEffect()
        {
            ShadowDepth = 1,
            BlurRadius = 2,
            Direction = 270,
            Opacity = 0.3
        };

        public MessageBubble()
        {
            BorderThickness = new Thickness(5);
            Margin = new Thickness(5, 2.5, 5, 2.5);
            Effect = _shadow;
        }

        public MessageBubble Spawn(string msg, bool inbound, DateTime time)
        {
            Child = GetBlock(msg, inbound, time);
            if (inbound)
                SetProperties(HorizontalAlignment.Left, _inboundColor, 0, Radius);
            else
                SetProperties(HorizontalAlignment.Right, _outboundColor, Radius, 0);
            return this;
        }

        private StackPanel GetBlock(string msg, bool inbound, DateTime time)
        {
            var stack = new StackPanel() {
                MaxWidth = _maxWidth,
                Background = new SolidColorBrush(inbound ? _inboundColor : _outboundColor),
                Orientation = Orientation.Horizontal,
            };
            stack.Children.Add(new TextBox()
            {
                Text = msg,
                TextWrapping = TextWrapping.Wrap,
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                MaxWidth = _maxWidth - 30,
                FontSize = 16,
                Background = new SolidColorBrush(inbound ? _inboundColor : _outboundColor)
            });
            stack.Children.Add(new TextBlock()
            {
                Text = $"{PadTime(time.Hour)}:{PadTime(time.Minute)}",
                Background = new SolidColorBrush(inbound ? _inboundColor : _outboundColor),
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                VerticalAlignment = VerticalAlignment.Bottom,
            });
            return stack;
        }

        private static string PadTime(int time)
        {
            return time < 10 ? $"0{time}" : time.ToString();
        }

        private void SetProperties(HorizontalAlignment align, Color color, double left, double right)
        {
            var colorBrush = new SolidColorBrush(color);
            HorizontalAlignment = align;
            BorderBrush = colorBrush;
            Background = colorBrush;
            CornerRadius = new CornerRadius(Radius, Radius, right, left);
        }
    }
}
