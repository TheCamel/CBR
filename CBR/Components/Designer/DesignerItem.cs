using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CBR.Core.Helpers;

namespace CBR.Components.Designer
{
    public class DesignerItem : ContentControl
    {
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register("IsSelected", typeof(bool),
                                      typeof(DesignerItem),
                                      new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty MoveThumbTemplateProperty =
            DependencyProperty.RegisterAttached("MoveThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetMoveThumbTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(MoveThumbTemplateProperty);
        }

        public static void SetMoveThumbTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(MoveThumbTemplateProperty, value);
        }

        static DesignerItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }

        public DesignerItem()
        {
			//this.Loaded += new RoutedEventHandler(this.DesignerItem_Loaded);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
			DrawingLayer designer = VisualHelper.FindAnchestor<DrawingLayer>(this) as DrawingLayer;

            if (designer != null)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                {
                    this.IsSelected = !this.IsSelected;
                }
                else
                {
                    if (!this.IsSelected)
                    {
                        designer.DeselectAll();
                        this.IsSelected = true;
                    }
                }
            }

            e.Handled = false;
        }

		//private void DesignerItem_Loaded(object sender, RoutedEventArgs e)
		//{
		//    if (this.Template != null)
		//    {
		//        ContentPresenter contentPresenter =
		//            this.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;

		//        MoveThumb thumb =
		//            this.Template.FindName("PART_MoveThumb", this) as MoveThumb;

		//        //if (contentPresenter != null && thumb != null)
		//        //{
		//        //    UIElement contentVisual =
		//        //        VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;

		//        //    if (contentVisual != null)
		//        //    {
		//        //        ControlTemplate template =
		//        //            DesignerItem.GetMoveThumbTemplate(contentVisual) as ControlTemplate;

		//        //        if (template != null)
		//        //        {
		//        //            thumb.Template = template;
		//        //        }
		//        //    }
		//        //}
		//    }
		//}
    }
}
