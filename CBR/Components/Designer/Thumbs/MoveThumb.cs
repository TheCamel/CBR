using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using CBR.Core.Helpers;

namespace CBR.Components.Designer
{
    public class MoveThumb : Thumb
    {
        private DesignerItem designerItem;
		private DrawingLayer designerCanvas;

        public MoveThumb()
        {
            DragStarted += new DragStartedEventHandler(this.MoveThumb_DragStarted);
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.designerItem = DataContext as DesignerItem;

            if (this.designerItem != null)
            {
				this.designerCanvas = VisualHelper.FindAnchestor<DrawingLayer>(this.designerItem);
            }
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.designerItem != null && this.designerCanvas != null && this.designerItem.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

				double maxRight = double.MinValue;
				double maxBottom = double.MinValue;

                foreach (DesignerItem item in this.designerCanvas.SelectedItems)
                {
					minLeft = Math.Min(Canvas.GetLeft(item), minLeft);
					minTop = Math.Min(Canvas.GetTop(item), minTop);
					maxRight = Math.Max(Canvas.GetLeft(item)+item.Width, maxRight);
					maxBottom = Math.Max(Canvas.GetTop(item)+item.Height, maxBottom);
				}

				double deltaHorizontal;
				double deltaVertical;

				if( e.HorizontalChange > 0 )
					deltaHorizontal = Math.Min(this.designerCanvas.ActualWidth-maxRight, e.HorizontalChange);
				else
					deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);

				if( e.VerticalChange > 0 )
					deltaVertical = Math.Min(this.designerCanvas.ActualHeight-maxBottom, e.VerticalChange);
				else
					deltaVertical = Math.Max(-minTop, e.VerticalChange);

                foreach (DesignerItem item in this.designerCanvas.SelectedItems)
                {
                    Canvas.SetLeft(item, Canvas.GetLeft(item) + deltaHorizontal);
                    Canvas.SetTop(item, Canvas.GetTop(item) + deltaVertical);
                }

                this.designerCanvas.InvalidateMeasure();
                e.Handled = true;
            }
        }
    }
}
