using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CBR.Core.Helpers;
using System.Windows.Documents;
using System.Collections.Generic;

namespace CBR.Components.Designer
{
    public class ResizeThumb : Thumb
    {
        private DesignerItem designerItem;
		private DrawingLayer designerCanvas;
		private Adorner adorner;

        public ResizeThumb()
        {
            DragStarted += new DragStartedEventHandler(this.ResizeThumb_DragStarted);
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
			DragCompleted += new DragCompletedEventHandler(this.ResizeThumb_DragCompleted);
        }

        private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.designerItem = DataContext as DesignerItem;

            if (this.designerItem != null)
            {
				this.designerCanvas = VisualHelper.FindAnchestor<DrawingLayer>(this.designerItem) as DrawingLayer;

				AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.designerCanvas);
				if (adornerLayer != null)
				{
					this.adorner = new SizeAdorner(this.designerItem);
					adornerLayer.Add(this.adorner);
				}
			}
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.designerItem != null && this.designerCanvas != null && this.designerItem.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;
                double minDeltaHorizontal = double.MaxValue;
                double minDeltaVertical = double.MaxValue;
                double dragDeltaVertical, dragDeltaHorizontal;

				IEnumerable<DesignerItem> items = this.designerCanvas.SelectedItems;

				foreach (DesignerItem item in items)
                {
                    minLeft = Math.Min(Canvas.GetLeft(item), minLeft);
                    minTop = Math.Min(Canvas.GetTop(item), minTop);

                    minDeltaVertical = Math.Min(minDeltaVertical, item.ActualHeight - item.MinHeight);
                    minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.ActualWidth - item.MinWidth);
                }

				foreach (DesignerItem item in items)
                {
                    switch (VerticalAlignment)
                    {
                        case VerticalAlignment.Bottom:
                            dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                            item.Height = item.ActualHeight - dragDeltaVertical;
                            break;
                        case VerticalAlignment.Top:
                            dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                            Canvas.SetTop(item, Canvas.GetTop(item) + dragDeltaVertical);
                            item.Height = item.ActualHeight - dragDeltaVertical;
                            break;
                    }

                    switch (HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                            Canvas.SetLeft(item, Canvas.GetLeft(item) + dragDeltaHorizontal);
                            item.Width = item.ActualWidth - dragDeltaHorizontal;
                            break;
                        case HorizontalAlignment.Right:
                            dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                            item.Width = item.ActualWidth - dragDeltaHorizontal;
                            break;
                    }
                }

                e.Handled = true;
            }
        }

		private void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			if (this.adorner != null)
			{
				AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.designerCanvas);
				if (adornerLayer != null)
				{
					adornerLayer.Remove(this.adorner);
				}

				this.adorner = null;
			}
		}
    }
}
