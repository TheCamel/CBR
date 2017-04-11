using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace CBR.Components.Controls
{
    public class RatingControl : Control
    {
        static RatingControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingControl),
                new FrameworkPropertyMetadata(typeof(RatingControl)));
        }

        public static readonly DependencyProperty RatingValueProperty =
                            DependencyProperty.Register("RatingValue", typeof(int), typeof(RatingControl),
                                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                           RatingValueChanged));

        public int RatingValue
        {
            get { return (int)GetValue(RatingValueProperty); }
            set
            {
                if (value < 0)
                {
                    SetValue(RatingValueProperty, 0);
                }
                else if (value > 5)
                {
                    SetValue(RatingValueProperty, 5);
                }
                else
                {
                    SetValue(RatingValueProperty, value);
                }
            }
        }

        private StackPanel _Conteneur = null;

        /// <summary>
        /// Gets the parts out of the template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.IsTabStop = false;

            _Conteneur = (StackPanel)this.GetTemplateChild("PART_RatingContentPanel");

            foreach (ToggleButton item in _Conteneur.Children)
                item.Click += new RoutedEventHandler(RatingButtonClickEventHandler);

            UpdateButtons(_Conteneur, (int)GetValue(RatingValueProperty));
        }

        private static void RatingValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            StackPanel conteneur = (StackPanel)(sender as RatingControl).GetTemplateChild("PART_RatingContentPanel");

            if (conteneur != null)
            {
                (sender as RatingControl).UpdateButtons(conteneur, (int)e.NewValue);
            }
        }


        private void UpdateButtons(StackPanel conteneur, int ratingValue)
        {
            if (conteneur != null)
            {
                UIElementCollection children = conteneur.Children;
                ToggleButton button = null;

                for (int i = 0; i < ratingValue; i++)
                {
                    button = children[i] as ToggleButton;
                    if (button != null)
                        button.IsChecked = true;
                }

                for (int i = ratingValue; i < children.Count; i++)
                {
                    button = children[i] as ToggleButton;
                    if (button != null)
                        button.IsChecked = false;
                }
            }
        }

        private void RatingButtonClickEventHandler(Object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;

            int newRating = int.Parse((String)button.Tag);

            if ((bool)button.IsChecked || newRating < RatingValue)
            {
                RatingValue = newRating;
            }
            else
            {
                RatingValue = newRating - 1;
            }

            e.Handled = true;
        }
    }
}
