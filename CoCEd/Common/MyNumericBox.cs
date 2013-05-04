using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoCEd.Common
{
    [TemplatePart(Name = "border", Type = typeof(Border))]
    [TemplatePart(Name = "textBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "toolTip", Type = typeof(TextBlock))]
    [TemplatePart(Name = "minButton", Type = typeof(Button))]
    [TemplatePart(Name = "maxButton", Type = typeof(Button))]
    public class MyNumericBox : Control
    {
        public static readonly DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(MyNumericBox), new PropertyMetadata(Double.MinValue, OnPropertiesChanged));
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(MyNumericBox), new PropertyMetadata(Double.MaxValue, OnPropertiesChanged));
        public static readonly DependencyProperty ShowMinButtonProperty = DependencyProperty.Register("ShowMinButton", typeof(bool), typeof(MyNumericBox), new PropertyMetadata(true, OnPropertiesChanged));
        public static readonly DependencyProperty ShowMaxButtonProperty = DependencyProperty.Register("ShowMaxButton", typeof(bool), typeof(MyNumericBox), new PropertyMetadata(true, OnPropertiesChanged));

        public static readonly DependencyProperty IsIntegerProperty = DependencyProperty.Register("IsInteger", typeof(bool), typeof(MyNumericBox), new PropertyMetadata(true, OnPropertiesChanged));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(MyNumericBox), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty ErrorBrushProperty = DependencyProperty.Register("ErrorBrush", typeof(Brush), typeof(MyNumericBox), new PropertyMetadata(Brushes.Pink, OnPropertiesChanged));
        public static readonly DependencyProperty MinLabelProperty = DependencyProperty.Register("MinLabel", typeof(string), typeof(MyNumericBox), new PropertyMetadata("", OnPropertiesChanged));
        public static readonly DependencyProperty MaxLabelProperty = DependencyProperty.Register("MaxLabel", typeof(string), typeof(MyNumericBox), new PropertyMetadata("", OnPropertiesChanged));
        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit", typeof(string), typeof(MyNumericBox), new PropertyMetadata("", OnPropertiesChanged));
        public static readonly DependencyProperty TipProperty = DependencyProperty.Register("Tip", typeof(string), typeof(MyNumericBox), new PropertyMetadata(""));

        static MyNumericBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MyNumericBox), new FrameworkPropertyMetadata(typeof(MyNumericBox)));
        }

        TextBlock _toolTip;
        TextBox _textBox;
        Button _minButton;
        Button _maxButton;
        Border _border;

        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public bool ShowMinButton
        {
            get { return (bool)GetValue(ShowMinButtonProperty); }
            set { SetValue(ShowMinButtonProperty, value); }
        }

        public bool ShowMaxButton
        {
            get { return (bool)GetValue(ShowMaxButtonProperty); }
            set { SetValue(ShowMaxButtonProperty, value); }
        }

        public bool IsInteger
        {
            get { return (bool)GetValue(IsIntegerProperty); }
            set { SetValue(IsIntegerProperty, value); }
        }

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public string MinLabel
        {
            get { return (string)GetValue(MinLabelProperty); }
            set { SetValue(MinLabelProperty, value); }
        }

        public string MaxLabel
        {
            get { return (string)GetValue(MaxLabelProperty); }
            set { SetValue(MaxLabelProperty, value); }
        }

        public string Tip
        {
            get { return (string)GetValue(TipProperty); }
            set { SetValue(TipProperty, value); }
        }

        public Brush ErrorBrush
        {
            get { return (Brush)GetValue(ErrorBrushProperty); }
            set { SetValue(ErrorBrushProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_minButton != null) _minButton.Click -= minButton_Click;
            if (_maxButton != null) _maxButton.Click -= maxButton_Click;
            if (_textBox != null) _textBox.TextChanged -= textBox_OnTextChanged;
            if (_textBox != null) _textBox.GotMouseCapture -= textBox_OnFocused;
            if (_textBox != null) _textBox.GotKeyboardFocus -= textBox_OnFocused;
            if (_textBox != null) _textBox.LostKeyboardFocus -= textBox_LostFocus;
            if (_textBox != null) _textBox.PreviewKeyDown -= textBox_PreviewKeyDown;

            _border = GetTemplateChild("border") as Border;
            _textBox = GetTemplateChild("textBox") as TextBox;
            _minButton = GetTemplateChild("minButton") as Button;
            _maxButton = GetTemplateChild("maxButton") as Button;
            _toolTip = GetTemplateChild("toolTip") as TextBlock;

            if (_minButton != null) _minButton.Click += minButton_Click;
            if (_maxButton != null) _maxButton.Click += maxButton_Click;
            if (_textBox != null) _textBox.TextChanged += textBox_OnTextChanged;
            if (_textBox != null) _textBox.GotMouseCapture += textBox_OnFocused;
            if (_textBox != null) _textBox.GotKeyboardFocus += textBox_OnFocused;
            if (_textBox != null) _textBox.LostKeyboardFocus += textBox_LostFocus;
            if (_textBox != null) _textBox.PreviewKeyDown += textBox_PreviewKeyDown;

            OnTextChanged();
            OnValueChanged();
        }

        void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (Value >= Max) return;
                if (Value != (int)Value) Value = 1 + (int)Value;
                else ++Value;
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                if (Value <= Min) return;
                if (Value != (int)Value) Value = (int)Value;
                else --Value;
                e.Handled = true;
            }
        }

        void textBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ++Value;
            e.Handled = true;
        }

        void textBox_OnFocused(object sender, EventArgs e)
        {
            _textBox.SelectAll();
        }

        void textBox_LostFocus(object sender, EventArgs e)
        {
            if (_hasError) return;
            DoPrettyFormat();
        }

        void minButton_Click(object sender, RoutedEventArgs e)
        {
            Value = Min;   
        }
        void maxButton_Click(object sender, RoutedEventArgs e)
        {
            Value = Max;
        }

        void textBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OnTextChanged();
        }

        static void OnPropertiesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MyNumericBox box = (MyNumericBox)obj;
            if (box._minButton == null) return;
            box.OnTextChanged();
            box.OnValueChanged();
        }

        static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MyNumericBox box = (MyNumericBox)obj;
            if (box._minButton == null) return;
            box.OnValueChanged();
        }

        void OnValueChanged()
        {
            if (!_textChanged) DoPrettyFormat();
            SetUpButton(_minButton, Min, MinLabel, ShowMinButton);
            SetUpButton(_maxButton, Max, MaxLabel, ShowMaxButton);

            if (Unit == "inches")
            {
                if (Value >= 12) _toolTip.Text = String.Format("{0:0} cm ; {1:0}' {2:0}", Value * 2.54, (int)(Value / 12), Value % 12);
                else if (Value >= 4) _toolTip.Text = String.Format("{0:0} cm", Value * 2.54);
                else _toolTip.Text = String.Format("{0:0.0} cm", Value * 2.54);
            }
            else if (Unit == "feet")
            {
                _toolTip.Text = String.Format("{0:0}cm", Value * 12 * 2.54);
            }
            else _toolTip.Text = Tip;
        }

        void SetUpButton(Button button, double value, string label, bool show)
        {
            if (!String.IsNullOrEmpty(label)) button.Content = label;
            else if (value != Double.MinValue && value != Double.MaxValue) button.Content = value.ToString(CultureInfo.InvariantCulture);
            else button.Content = "";

            button.Visibility = (show && (button.Content as string) != "") ? Visibility.Visible : Visibility.Collapsed;
        }

        bool _hasError;
        bool _textChanged;
        void OnTextChanged()
        {
            _textChanged = true;
            try
            {
                if (TrySetValue(_textBox.Text))
                {
                    _hasError = false;
                    _border.Background = Brushes.Transparent;
                }
                else
                {
                    _hasError = true;
                    _border.Background = ErrorBrush;
                }
            }
            finally
            {
                _textChanged = false;
            }
        }

        bool TrySetValue(string str)
        {
            str = str.Trim();
            if (IsPrettyFormat(str, Value)) return true;

            if (IsInteger)
            {
                // Adobe encoded integer on 29bits (7+7+7+8)
                const int AmfMax = (1 << 28) - 1;
                const int AmfMin = -(1 << 28);

                int value;
                if (!Int32.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) &&
                    !Int32.TryParse(str, NumberStyles.Integer, CultureInfo.CurrentCulture, out value)) return false;
                if (value < AmfMin || value > AmfMax) return false;
                if (value < Min || value > Max) return false;

                Value = value;
                return true;
            }
            else
            {
                double value;
                if (!Double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
                    !Double.TryParse(str, NumberStyles.Float, CultureInfo.CurrentCulture, out value)) return false;
                if (value < Min || value > Max) return false;

                Value = value;
                return true;
            }
        }

        void DoPrettyFormat()
        {
            var format = IsInteger ? "0" : "0.0";
            _textBox.Text = Value.ToString(format, CultureInfo.CurrentCulture);
        }

        bool IsPrettyFormat(string text, double value)
        {
            //if (value.ToString(CultureInfo.CurrentCulture) == text) return true;
            if (IsInteger && value.ToString("0", CultureInfo.CurrentCulture) == text) return true;
            if (!IsInteger && value.ToString("0.0", CultureInfo.CurrentCulture) == text) return true;

            /*if (value.ToString(CultureInfo.InvariantCulture) == text) return true;
            if (IsInteger && value.ToString("0", CultureInfo.InvariantCulture) == text) return true;
            if (!IsInteger && value.ToString("0.0", CultureInfo.InvariantCulture) == text) return true;*/

            return false;
        }
    }
}
