using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Data;

namespace CBR.Components.Controls
{
	public enum PositionMode { Free, TopLeft, TopRight, BottomLeft, BottomRight }

	public class ZoomFlyer : Control
	{
		#region --------------------CONSTRUCTORS--------------------

		static ZoomFlyer()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomFlyer),
				new FrameworkPropertyMetadata(typeof(ZoomFlyer)));
		}

		public ZoomFlyer()
        {
			this.DefaultStyleKey = typeof(ZoomFlyer);
        }

		#endregion

		#region --------------------DEPENDENCY PROPERTIES--------------------

		#region ScaleProperty

		public static readonly DependencyProperty ScaleProperty =
			   DependencyProperty.Register("Scale", typeof(double), typeof(ZoomFlyer),null);

		public double Scale
		{
			get { return (double)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		#endregion

		#region PositionProperty

		public static readonly DependencyProperty PositionProperty =
			   DependencyProperty.Register("Position", typeof(PositionMode), typeof(ZoomFlyer),
					new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPositionChanged)));

		public PositionMode Position
		{
			get { return (PositionMode)GetValue(PositionProperty); }
			set { SetValue(PositionProperty, value); }
		}

		private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
				return;

			ZoomFlyer element = d as ZoomFlyer;
			element.UpdatePosition();
		}

		private void UpdatePosition()
		{
			if (Position == PositionMode.BottomLeft)
			{
				this.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
				this.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			}
			if (Position == PositionMode.BottomRight)
			{
				this.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
				this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			}
			if (Position == PositionMode.TopLeft)
			{
				this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
				this.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			}
			if (Position == PositionMode.TopRight)
			{
				this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
				this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			}
		}
		#endregion

		#endregion
	}
}
