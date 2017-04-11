using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;

namespace CBR.Core.Helpers
{
    public static class VisualHelper
    {
        public static void AllowFocus(UIElement element)
        {
			return;
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(element))
            {
                element.Focusable = true;
                element.SetValue(KeyboardNavigation.IsTabStopProperty, true);
                element.SetValue(KeyboardFocus.OnProperty, element);
            }
        }

        public static object GetObjectAtPoint<ItemContainer>(this ItemsControl control, Point p)
                                     where ItemContainer : DependencyObject
        {
            // ItemContainer - can be ListViewItem, or TreeViewItem and so on(depends on control) 
            ItemContainer obj = GetContainerAtPoint<ItemContainer>(control, p);
            if (obj == null)
                return null;

            return control.ItemContainerGenerator.ItemFromContainer(obj);
        }

        public static ItemContainer GetContainerAtPoint<ItemContainer>(this ItemsControl control, Point p)
                                 where ItemContainer : DependencyObject
        {
            HitTestResult result = VisualTreeHelper.HitTest(control, p);
			if (result != null)
			{
				DependencyObject obj = result.VisualHit;

				while (VisualTreeHelper.GetParent(obj) != null && !(obj is ItemContainer))
				{
					obj = VisualTreeHelper.GetParent(obj);
				}

				// Will return null if not found 
				return obj as ItemContainer;
			}
			else return null;
        } 


        /// <summary>
        /// Find ancestor type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <returns></returns>
        public static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

		public static DependencyObject FindAnchestor(DependencyObject current, Type child)
		{
			do
			{
				if (current.GetType() == child)
				{
					return current;
				}
				current = VisualTreeHelper.GetParent(current);
			}
			while (current != null);
			return null;
		}

    }
}
