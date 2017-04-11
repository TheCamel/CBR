using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Services;

namespace CBR.Components.Dialogs
{
	/// <summary>
	/// Interaction logic for SimulateDialog.xaml
	/// </summary>
	public partial class SimulateDialog : Window
	{
		public SimulateDialog()
		{
			InitializeComponent();

			IsLandscape = true;

			//create a dispatch timer to load the image cache
			//_TimeLineClock = new DispatcherTimer();
			//_TimeLineClock.Interval = new TimeSpan(0, 0, 2);
			//_TimeLineClock.IsEnabled = true;
			//_TimeLineClock.Tick += new EventHandler(TimeLineClockElapsed);

		}

		public bool IsLandscape { get; set; }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_currentPage = BookData.Pages[0];

			_ImgContent.Source = _currentPage.Image;

			_currentZone = null;
			_DurationCounter = 0;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_ImgContent.LayoutTransform = _scaleTransform;
			_currentPage = BookData.Pages[0];

			_ImgContent.Source = _currentPage.Image;

			_currentZone = null;
			_DurationCounter = 0;
		}

		public Book BookData { get; set; }

		private ScaleTransform _scaleTransform = new ScaleTransform(1, 1, 0, 0);
		private CBR.Core.Models.Page _currentPage;
		private Zone _currentZone;
		private int _DurationCounter = 0;
		private DispatcherTimer _TimeLineClock;

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private double previousWidth;
		private double previousHeight;

		private void btnSwapLandscape_Click(object sender, RoutedEventArgs e)
		{
			if (IsLandscape)
			{
				previousWidth = this.ActualWidth;
				previousHeight = this.ActualHeight;

				this.Width = previousHeight;
				this.Height = previousWidth;

				this.mainGrid.LayoutTransform = new RotateTransform(90, 0.5, 0.5);
			}
			else
			{
				this.Width = previousWidth;
				this.Height = previousHeight;
				this.mainGrid.LayoutTransform = null;
			}

			IsLandscape = !IsLandscape;
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			//_TimeLineClock.Stop();
			//_TimeLineClock.IsEnabled = false;

			this.Close();
		}

		public void TimeLineClockElapsed(object tag, EventArgs args)
		{
			try
			{
				//increase timer duration
				_DurationCounter += 2;

				MoveToFrame(1);
			}
			catch (Exception err)
			{
				LogHelper.Manage("SimulateDialog:TimeLineClockElapsed", err);
			}
		}

		private void btnPrevious_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				MoveToFrame(-1);
			}
			catch (Exception err)
			{
				LogHelper.Manage("SimulateDialog:btnPrevious_Click", err);
			}
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				MoveToFrame(1);
			}
			catch (Exception err)
			{
				LogHelper.Manage("SimulateDialog:btnNext_Click", err);
			}
		}

		private void MoveToFrame(int step)
		{
			Page oldPage = _currentPage;
			Zone oldFrame = _currentZone;

			// if no frame stored or duration is over, search the new frame in the page
			DocumentFactory.Instance.GetService(BookData).GotoFrame(BookData, ref _currentPage, ref _currentZone, step);

			//we change the page, so change the image
			if (oldPage != _currentPage)
			{
				_ImgContent.Source = _currentPage.Image;
			}

			//we change the zone, so change the parameters
			if (oldFrame != _currentZone)
			{
				//System.Windows.Controls.Canvas.SetLeft(_ImgContent, -_currentZone.X);
				//System.Windows.Controls.Canvas.SetTop(_ImgContent, -_currentZone.Y);

				double scaleX = this._ScrollContainer.ViewportWidth / _currentZone.Width;
				double scaleY = this._ScrollContainer.ViewportHeight / _currentZone.Height;

				double scaleFactor = Math.Min(scaleX, scaleY);

				_scaleTransform.CenterX = _currentZone.X;
				_scaleTransform.CenterY = _currentZone.Y;

				_scaleTransform.ScaleX = scaleFactor;
				_scaleTransform.ScaleY = scaleFactor;

				// Scroll to the new position. 
				_ScrollContainer.ScrollToHorizontalOffset(_currentZone.X * scaleFactor);
				_ScrollContainer.ScrollToVerticalOffset(_currentZone.Y * scaleFactor);

				lblDebugInfo.Content = string.Format("Frame {0} for {1} second(s) on location ({2}, {3}, {4}, {5}) ",
					_currentZone.OrderNum, _currentZone.Duration, _currentZone.X, _currentZone.Y, _currentZone.Width, _currentZone.Height);
			}
		}

		#region ----------------MOVE IMAGE EVENTS----------------

		private Point _mouseDragStartPoint;
		private Point _scrollStartOffset;

		void _ImgContent_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			try
			{
				_ImgContent.Focus();

				//Start moving on the left button, let the right for popup menu
				if (e.LeftButton == MouseButtonState.Pressed && !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
				{
					_mouseDragStartPoint = e.GetPosition(_ScrollContainer);
					_scrollStartOffset.X = _ScrollContainer.HorizontalOffset;
					_scrollStartOffset.Y = _ScrollContainer.VerticalOffset;

					// Update the cursor if scrolling is possible 
					_ImgContent.Cursor = (_ScrollContainer.ExtentWidth > _ScrollContainer.ViewportWidth) ||
						(_ScrollContainer.ExtentHeight > _ScrollContainer.ViewportHeight) ?
						Cursors.ScrollAll : Cursors.Arrow;

					_ImgContent.CaptureMouse();

					lblDebugInfo.Content = string.Format("Image on ({0} , {1})", _scrollStartOffset.X, _scrollStartOffset.Y);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("SimulateDialog:_ImgContent_PreviewMouseLeftButtonDown", err);
			}
		}

		void _ImgContent_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			try
			{
				// else move the image
				if (_ImgContent.IsMouseCaptured)
				{
					// Get the new mouse position. 
					Point mouseDragCurrentPoint = e.GetPosition(_ScrollContainer);

					// Determine the new amount to scroll. 
					Point delta = new Point(
						(mouseDragCurrentPoint.X > this._mouseDragStartPoint.X) ?
							-(mouseDragCurrentPoint.X - this._mouseDragStartPoint.X) :
							(this._mouseDragStartPoint.X - mouseDragCurrentPoint.X),
						(mouseDragCurrentPoint.Y > this._mouseDragStartPoint.Y) ?
							-(mouseDragCurrentPoint.Y - this._mouseDragStartPoint.Y) :
							(this._mouseDragStartPoint.Y - mouseDragCurrentPoint.Y));

					// Scroll to the new position. 
					_ScrollContainer.ScrollToHorizontalOffset(this._scrollStartOffset.X + delta.X);
					_ScrollContainer.ScrollToVerticalOffset(this._scrollStartOffset.Y + delta.Y);

					lblDebugInfo.Content = string.Format("Image on ({0} , {1})", this._scrollStartOffset.X + delta.X, this._scrollStartOffset.Y + delta.Y);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("SimulateDialog:_ImgContent_PreviewMouseMove", err);
			}
		}

		void _ImgContent_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			try
			{
				//end image move
				if (_ImgContent.IsMouseCaptured)
				{
					_ImgContent.ReleaseMouseCapture();
					_ImgContent.Cursor = Cursors.Arrow;

					lblDebugInfo.Content = string.Format("Image on ({0} , {1})", _ScrollContainer.HorizontalOffset, _ScrollContainer.VerticalOffset);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("SimulateDialog:_ImgContent_PreviewMouseLeftButtonUp", err);
			}
		}
		#endregion

	}
}
