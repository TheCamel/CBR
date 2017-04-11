using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Data;

namespace CBR.Components.Controls
{
	/// <summary>
	/// Represents spinner control
	/// </summary>
	[ContentProperty("Value")]
	[TemplatePart(Name = "PART_TextBox", Type = typeof(System.Windows.Controls.TextBox))]
	[TemplatePart(Name = "PART_ButtonUp", Type = typeof(System.Windows.Controls.Primitives.RepeatButton))]
	[TemplatePart(Name = "PART_ButtonDown", Type = typeof(System.Windows.Controls.Primitives.RepeatButton))]
	public class SpinBox : Control
	{
		#region Events

        /// <summary>
        /// Occurs when value has been changed
        /// </summary>
        public event RoutedPropertyChangedEventHandler<double> ValueChanged;

        #endregion

        #region Fields

        // Parts of the control (must be in control template)
        System.Windows.Controls.TextBox textBox;
        System.Windows.Controls.Primitives.RepeatButton buttonUp;
        System.Windows.Controls.Primitives.RepeatButton buttonDown;

        #endregion

        #region Properties

        #region Value

        /// <summary>
        /// Gets or sets current value
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Value.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ValueProperty;

        private static object CoerceValue(DependencyObject d, object basevalue)
        {
            SpinBox spinner = (SpinBox)d;
            double value = (double)basevalue;
            value = GetLimitedValue(spinner, value);
            return value;
        }

        private static double GetLimitedValue(SpinBox spinner, double value)
        {
            value = Math.Max(spinner.Minimum, value);
            value = Math.Min(spinner.Maximum, value);
            return value;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpinBox spinner = (SpinBox)d;
            spinner.ValueToTextBoxText();

            if (spinner.ValueChanged != null) spinner.ValueChanged(spinner, new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue));
        }

        private void ValueToTextBoxText()
        {
            if (IsTemplateValid())
            {
                textBox.Text = Value.ToString(Format, CultureInfo.CurrentCulture);
                Text = textBox.Text;
            }
        }

        #endregion

        #region Text

        /// <summary>
        /// Gets current text from the spinner
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            private set { SetValue(TextPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey TextPropertyKey =
            DependencyProperty.RegisterReadOnly("Text", typeof(string),
            typeof(SpinBox), new UIPropertyMetadata(null));

        /// <summary>
        /// Using a DependencyProperty as the backing store for Text.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty TextProperty = TextPropertyKey.DependencyProperty;

        #endregion

        #region Increment

        /// <summary>
        /// Gets or sets a value added or subtracted from the value property
        /// </summary>
        public double Increment
        {
            get { return (double)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Increment.
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register("Increment", typeof(double), typeof(SpinBox), new UIPropertyMetadata(1.0d));

        #endregion

        #region Minimum

        /// <summary>
        /// Gets or sets minimun value
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Minimum.
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MinimumProperty;

        static object CoerceMinimum(DependencyObject d, object basevalue)
        {
            SpinBox spinner = (SpinBox)d;
            double value = (double)basevalue;
            if (spinner.Maximum < value) return spinner.Maximum;
            return value;
        }

        static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpinBox spinner = (SpinBox)d;
            double value = (double)CoerceValue(d, spinner.Value);
            if (value != spinner.Value) spinner.Value = value;
        }

        #endregion

        #region Maximum

        /// <summary>
        /// Gets or sets maximum value
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Maximum.
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MaximumProperty;

        static object CoerceMaximum(DependencyObject d, object basevalue)
        {
            SpinBox spinner = (SpinBox)d;
            double value = (double)basevalue;
            if (spinner.Minimum > value) return spinner.Minimum;
            return value;
        }

        static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpinBox spinner = (SpinBox)d;
            double value = (double)CoerceValue(d, spinner.Value);
            if (value != spinner.Value) spinner.Value = value;
        }

        #endregion

        #region Format

        /// <summary>
        /// Gets or sets string format of value
        /// </summary>
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Format.
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register("Format", typeof(string), typeof(SpinBox), new UIPropertyMetadata("F1", OnFormatChanged));

        static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpinBox spinner = (SpinBox)d;
            spinner.ValueToTextBoxText();
        }

        #endregion

        #region Delay

        /// <summary>
        /// Gets or sets the amount of time, in milliseconds, 
        /// the Spinner waits while it is pressed before it starts repeating. 
        /// The value must be non-negative. This is a dependency property.
        /// </summary>
        public int Delay
        {
            get { return (int)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Delay.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register("Delay", typeof(int), typeof(SpinBox),
            new UIPropertyMetadata(400));

        #endregion

        #region Interval

        /// <summary>
        /// Gets or sets the amount of time, in milliseconds, 
        /// between repeats once repeating starts. The value must be non-negative. 
        /// This is a dependency property.
        /// </summary>
        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Interval.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(SpinBox), new UIPropertyMetadata(80));

        #endregion

        #region InputWidth

        /// <summary>
        /// Gets or sets width of the value input part of spinner
        /// </summary>               
        public double InputWidth
        {
            get { return (double)GetValue(InputWidthProperty); }
            set { SetValue(InputWidthProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for InputWidth.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty InputWidthProperty =
            DependencyProperty.Register("InputWidth", typeof(double), typeof(SpinBox), new UIPropertyMetadata(double.NaN));

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor
        /// </summary>
		static SpinBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpinBox), new FrameworkPropertyMetadata(typeof(SpinBox)));

            MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(SpinBox), new UIPropertyMetadata(double.MaxValue, OnMaximumChanged, CoerceMaximum));
            MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(SpinBox), new UIPropertyMetadata(0.0d, OnMinimumChanged, CoerceMinimum));
            ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(SpinBox), new UIPropertyMetadata(0.0d, OnValueChanged, CoerceValue));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (IsTemplateValid())
            {
                buttonUp.Click -= OnButtonUpClick;
                buttonDown.Click -= OnButtonDownClick;
                BindingOperations.ClearAllBindings(buttonDown);
                BindingOperations.ClearAllBindings(buttonUp);
            }

            // Get template childs
            textBox = GetTemplateChild("PART_TextBox") as System.Windows.Controls.TextBox;
            buttonUp = GetTemplateChild("PART_ButtonUp") as System.Windows.Controls.Primitives.RepeatButton;
            buttonDown = GetTemplateChild("PART_ButtonDown") as System.Windows.Controls.Primitives.RepeatButton;

            // Check template
            if (!IsTemplateValid())
            {
                return;
            }

            // Bindings
            Bind(this, buttonUp, "Delay", System.Windows.Controls.Primitives.RepeatButton.DelayProperty, BindingMode.OneWay);
            Bind(this, buttonDown, "Delay", System.Windows.Controls.Primitives.RepeatButton.DelayProperty, BindingMode.OneWay);
            Bind(this, buttonUp, "Interval", System.Windows.Controls.Primitives.RepeatButton.IntervalProperty, BindingMode.OneWay);
            Bind(this, buttonDown, "Interval", System.Windows.Controls.Primitives.RepeatButton.IntervalProperty, BindingMode.OneWay);


            // Events subscribing
            buttonUp.Click += OnButtonUpClick;
            buttonDown.Click += OnButtonDownClick;
            textBox.LostKeyboardFocus += OnTextBoxLostKeyboardFocus;
            textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;

            ValueToTextBoxText();
        }

        bool IsTemplateValid()
        {
            return textBox != null && buttonUp != null && buttonDown != null;
        }

		/// <summary>
		/// Binds elements property
		/// </summary>
		/// <param name="source">Source element</param>
		/// <param name="target">Target element</param>
		/// <param name="path">Property path</param>
		/// <param name="property">Property to bind</param>
		/// <param name="mode">Binding mode</param>
		internal void Bind(object source, FrameworkElement target, string path, DependencyProperty property, BindingMode mode)
		{
			Binding binding = new Binding();
			binding.Path = new PropertyPath(path);
			binding.Source = source;
			binding.Mode = mode;
			target.SetBinding(property, binding);
		}
        #endregion

        #region Event Handling

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);
			e.Handled = true;
			Value += (e.Delta > 0) ? Increment : -Increment;
		}
        /// <summary>
        /// Invoked when an unhandled System.Windows.Input.Keyboard.KeyUp�attached event reaches 
        /// an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The System.Windows.Input.KeyEventArgs that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            // Avoid Click invocation (from RibbonControl)
            if (e.Key == Key.Enter || e.Key == Key.Space) return;
            base.OnKeyUp(e);
        }

        private void OnButtonUpClick(object sender, RoutedEventArgs e)
        {
            Value += Increment;
        }

        private void OnButtonDownClick(object sender, RoutedEventArgs e)
        {
            Value -= Increment;
        }

        private void OnTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBoxTextToValue();
        }

        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBoxTextToValue();
            }

            if (e.Key == Key.Escape)
            {
                ValueToTextBoxText();
            }

            if (e.Key == Key.Enter
                || e.Key == Key.Escape)
            {
                // Move Focus
                textBox.Focusable = false;
                Focus();
                textBox.Focusable = true;
                e.Handled = true;
            }

            if (e.Key == Key.Up)
            {
                buttonUp.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }

            if (e.Key == Key.Down)
            {
                buttonDown.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        private void TextBoxTextToValue()
        {
            var text = textBox.Text;

            // Remove all except digits, signs and commas
            var stringBuilder = new StringBuilder();

            foreach (var symbol in text)
            {
                if (Char.IsDigit(symbol)
                    || symbol == ','
                    || symbol == '.'
                    || (symbol == '-' && stringBuilder.Length == 0))
                {
                    stringBuilder.Append(symbol);
                }
            }

            text = stringBuilder.ToString();

            double value;

            if (Double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                Value = GetLimitedValue(this, value);
            }

            ValueToTextBoxText();
        }

        #endregion
	}
}
