using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Input;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.Linq;

namespace CBR.Components.Controls
{
    [TemplatePart(Name = PART_FilterBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_ClearButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_Header, Type = typeof(TextBlock))]
    public class FilterControl : Control
    {
        #region Declarations

        private const string PART_FilterBox = "PART_FilterBox";
        private const string PART_ClearButton = "PART_ClearButton";
        private const string PART_Header = "PART_Header";

        private Button _clearButton = null;
        private TextBox _filterBox = null;
        private TextBlock _headerBlock = null;

        #endregion

        #region Constructors

        static FilterControl()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterControl),
                                                     new FrameworkPropertyMetadata(typeof(FilterControl)));
        }

        #endregion

        #region FilterText property

        public static readonly DependencyProperty FilterTextProperty = 
			DependencyProperty.Register("FilterText", typeof(string), typeof(FilterControl), new PropertyMetadata(string.Empty));

        public string FilterText
        {
            get
            {
                return (string)GetValue(FilterTextProperty);
            }
            set
            {
                SetValue(FilterTextProperty, value);
            }
        }

        #endregion

        #region Header property

        public static readonly DependencyProperty HeaderProperty = 
			DependencyProperty.Register("Header", typeof(string), typeof(FilterControl), new UIPropertyMetadata(string.Empty));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        #endregion

        #region Overridden Functions/Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _headerBlock = GetTemplateChild(PART_Header) as TextBlock;

            _filterBox = GetTemplateChild(PART_FilterBox) as TextBox;

            if (_filterBox != null)
            {
                _filterBox.LostKeyboardFocus += new System.Windows.Input.KeyboardFocusChangedEventHandler(OnLostKeyboardFocus);
                _filterBox.GotKeyboardFocus += new System.Windows.Input.KeyboardFocusChangedEventHandler(OnGotKeyboardFocus);
                _filterBox.TextChanged += new TextChangedEventHandler(OnFilterBoxTextChanged);
            }

            _clearButton = GetTemplateChild(PART_ClearButton) as Button;

            if (_clearButton != null)
            {
                _clearButton.Click += new RoutedEventHandler(OnClearButtonClick);
            }

            this.KeyDown += new KeyEventHandler(OnControlKeyDown);
            this.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnKeyboardGotFocus);
        }

        #endregion

        #region Private Functions/Methods

        private void OnKeyboardGotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Keyboard.Focus(_filterBox);
        }

        private void OnControlKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !string.IsNullOrEmpty(FilterText))
            {
                Clear();
            }
        }

        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        public void Clear()
        {
            _filterBox.Text = string.Empty;
            Keyboard.Focus(_filterBox);
        }

        private void OnFilterBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(_filterBox.Text))
            {
                _clearButton.Visibility = Visibility.Collapsed;

                if (!_filterBox.IsFocused)
                    _headerBlock.Visibility = Visibility.Visible;
                else
                    _headerBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                _clearButton.Visibility = Visibility.Visible;
                _headerBlock.Visibility = Visibility.Collapsed;
            }
            FilterText = this._filterBox.Text;
        }

        private void OnGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            _headerBlock.Visibility = Visibility.Collapsed;
        }

        private void OnLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(_filterBox.Text))
            {
                _headerBlock.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}
