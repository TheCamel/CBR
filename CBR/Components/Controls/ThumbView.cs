using System.Windows;
using System.Windows.Controls;

namespace CBR.Components.Controls
{
    public class SimpleThumbView : ViewBase
    {
        public static readonly DependencyProperty ItemTemplateProperty =
				  ItemsControl.ItemTemplateProperty.AddOwner(typeof(SimpleThumbView));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        protected override object DefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "SimpleThumbViewStyle"); }
        }

        protected override object ItemContainerDefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "ThumbViewItemStyle"); }
        }
    }


	public class ExtendedThumbView : ViewBase
	{
		public static readonly DependencyProperty ItemTemplateProperty =
				  ItemsControl.ItemTemplateProperty.AddOwner(typeof(ExtendedThumbView));

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		protected override object DefaultStyleKey
		{
			get { return new ComponentResourceKey(GetType(), "ExtendedThumbViewStyle"); }
		}

		protected override object ItemContainerDefaultStyleKey
		{
			get { return new ComponentResourceKey(GetType(), "ThumbViewItemStyle"); }
		}
	}
}
