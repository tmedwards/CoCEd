using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CoCEd.Common
{
    [TemplatePart(Name = "popup", Type = typeof(Popup))]
    [TemplatePart(Name = "button", Type = typeof(ToggleButton))]
    public sealed class MyPiercingBox : Control
    {
        static MyPiercingBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MyPiercingBox), new FrameworkPropertyMetadata(typeof(MyPiercingBox)));
        }

        Popup _popup;
        ToggleButton _button;
        public override void OnApplyTemplate()
        {
            if (_popup != null) _popup.Closed -= popup_Closed;
            if (_button != null) _button.Checked -= button_Checked;
            if (_button != null) _button.Unchecked -= button_Unchecked;

            _popup = GetTemplateChild("popup") as Popup;
            _button = GetTemplateChild("button") as ToggleButton;

            if (_button != null) _button.Unchecked += button_Unchecked;
            if (_button != null) _button.Checked += button_Checked;
            if (_popup != null) _popup.Closed += popup_Closed;
        }

        void popup_Closed(object sender, EventArgs e)
        {
            _button.IsChecked = false;
        }

        void button_Unchecked(object sender, RoutedEventArgs e)
        {
            _popup.IsOpen = false;
        }

        void button_Checked(object sender, RoutedEventArgs e)
        {
            _popup.IsOpen = true;
        }
    }
}
