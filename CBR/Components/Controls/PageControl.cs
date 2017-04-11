using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using CBR.Core.Helpers;
using CBR.Components.Designer;
using CBR.Core.Services;
using System.Collections;

namespace CBR.Components.Controls
{
    public enum DisplayFitMode
    {
        None = 0, Width = 1, Height = 2
    }

	[TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
	[TemplatePart(Name = "PART_Image", Type = typeof(Image))]
	[TemplatePart(Name = "PART_DrawingLayer", Type = typeof(Canvas))]
	public class PageControl : Control
	{
		/// <summary>
		/// Initializes the metadata for the window
		/// </summary>
		static PageControl()
		{
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PageControl),
                new FrameworkPropertyMetadata(typeof(PageControl)));
		}

		#region --------------------DEPENDENCY PROPERTIES--------------------

        #region ImageSourceProperty

        public static readonly DependencyProperty ImageSourceProperty =
               DependencyProperty.Register("ImageSource", typeof(BitmapImage), typeof(PageControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnImageChanged)));

		public BitmapImage ImageSource
		{
			get { return (BitmapImage)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}

        private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
                return;

            PageControl element = d as PageControl;
            if (e.NewValue != null)
            {
                //manage the scrool in case of new image... are we top or bottom of the page
                element.ManageScrool();

                //check also the size and fit mode
                element.Fit(e.NewValue as ImageSource);
            }
        }
        #endregion

        #region ScaleProperty

        public static readonly DependencyProperty ScaleProperty =
			   DependencyProperty.Register("Scale", typeof(double), typeof(PageControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScaleChanged)));

		public double Scale
		{
			get { return (double)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
                return;

			PageControl element = d as PageControl;
			element._scaleTransform.ScaleX = (double)e.NewValue;
			element._scaleTransform.ScaleY = (double)e.NewValue;
		}
        #endregion

        #region FitModeProperty

        public static readonly DependencyProperty FitModeProperty =
			   DependencyProperty.Register("FitMode", typeof(DisplayFitMode), typeof(PageControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFitModeChanged)));

		public DisplayFitMode FitMode
		{
			get { return (DisplayFitMode)GetValue(FitModeProperty); }
			set { SetValue(FitModeProperty, value); }
		}

		private static void OnFitModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
                return;

			PageControl element = d as PageControl;
			element.Fit();
		}
        #endregion

        #region IsEditingProperty

        public static readonly DependencyProperty IsEditingProperty =
               DependencyProperty.Register("IsEditing", typeof(bool), typeof(PageControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnEditingChanged)));

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        private static void OnEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
                return;

            PageControl element = d as PageControl;
            element.SwapEdit((bool)e.NewValue);
        }
        #endregion

		#region FrameListProperty

		public static readonly DependencyProperty FrameListProperty =
			   DependencyProperty.Register("FrameList", typeof(IEnumerable), typeof(PageControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFrameListChanged)));

		public IEnumerable FrameList
		{
			get { return (IEnumerable)GetValue(FrameListProperty); }
			set { SetValue(FrameListProperty, value); }
		}

		private static void OnFrameListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
				return;

			//PageControl element = d as PageControl;
			//element.ApplyNewFrames((List<CBR.Core.Models.Zone>)e.NewValue);
		}

		#endregion

        #endregion

        #region --------------------override and events--------------------
        // should we wait at the end of the page that we press down one more time to go to the next page
		private bool _waitAtPageEnd = WorkspaceService.Instance.Settings.Extended.BehavePageTempo;

		// zooming
		private const int FIT_BORDER = 30;

		// moving the image
		private Point _mouseDragStartPoint;
		private Point _scrollStartOffset;

		private ScrollViewer _ScrollContainer;
		private Image _ImgContent;
		private DrawingLayer _DrawingLayer;
		private ScaleTransform _scaleTransform = new ScaleTransform();

		/// <summary>
		/// Applies the control template to the window
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				return;

			_scaleTransform.CenterX = 0.5;
			_scaleTransform.CenterY = 0.5;

			_ScrollContainer = (ScrollViewer) GetTemplateChild("PART_ScrollViewer");

			_ImgContent = (Image) GetTemplateChild("PART_Image");
			_ImgContent.LayoutTransform = _scaleTransform;
			_ImgContent.PreviewMouseWheel += new MouseWheelEventHandler(_ImgContent_PreviewMouseWheel);
			_ImgContent.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_ImgContent_PreviewMouseLeftButtonDown);
			_ImgContent.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(_ImgContent_PreviewMouseLeftButtonUp);
			_ImgContent.PreviewMouseMove += new MouseEventHandler(_ImgContent_PreviewMouseMove);

			this.PreviewKeyDown += new KeyEventHandler(_ImgContent_PreviewKeyUp);

			_DrawingLayer = (DrawingLayer)GetTemplateChild("PART_DrawingLayer");
			_DrawingLayer.PreviewMouseWheel += new MouseWheelEventHandler(_DrawingLayout_PreviewMouseWheel);
		}


        void _ImgContent_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageDown || e.Key == Key.Down)
            {
                if( ManageScroolDown() )
	               e.Handled = true;
                return;
            }
            else if (e.Key == Key.PageUp || e.Key == Key.Up)
            {
                if( ManageScroolUp() )
					e.Handled = true;
                return;
            }
        }

        private void Fit(ImageSource img)
        {
            if (img == null)
                return;

            if (FitMode == DisplayFitMode.Height)
            {
                Scale = (this._ScrollContainer.ViewportHeight - FIT_BORDER) / img.Height;
                RaiseZoomChanged();
            }
            else if (FitMode == DisplayFitMode.Width)
            {
                Scale = (this._ScrollContainer.ViewportWidth - FIT_BORDER) / img.Width;
                RaiseZoomChanged();
            }
        }

		private void Fit()
		{
            if (this._ImgContent.Source == null) return;

            Fit(this._ImgContent.Source);
		}

		//private void ApplyNewFrames(List<CBR.Core.Models.Zone> list)
		//{
		//    try
		//    {
		//        //remove all previous
		//        _DrawingLayer.Children.Clear();

		//        if (list != null)
		//        {
		//            //loop and add corresponding designer items
		//            foreach (CBR.Core.Models.Zone zn in list)
		//            {
		//                DesignerItem item = new DesignerItem();
		//                item.DataContext = zn;

		//                Canvas.SetLeft(item, zn.X);
		//                Canvas.SetTop(item, zn.Y);
		//                item.Width = zn.Width;
		//                item.Height = zn.Height;

		//                _DrawingLayer.Children.Add(item);
		//            }
		//            _DrawingLayer.DeselectAll();
		//        }
		//    }
		//    catch (Exception err)
		//    {
		//        LogHelper.Manage("PageView:ApplyNewFrames", err);
		//    }
		//}

		//private void RetreiveFrames()
		//{
		//    try
		//    {
		//        List<CBR.Core.Models.Zone> internList = (List<CBR.Core.Models.Zone>)GetValue(FrameListProperty);
		//        if (internList != null)
		//        {
		//            internList.Clear();

		//            //loop and add corresponding designer items
		//            foreach (DesignerItem elems in _DrawingLayer.Children)
		//            {
		//                Core.Models.Zone zn = elems.DataContext as Core.Models.Zone;

		//                zn.X = Canvas.GetLeft(elems);
		//                zn.Y = Canvas.GetTop(elems);
		//                zn.Width = elems.Width;
		//                zn.Height = elems.Height;
		//                internList.Add(zn);
		//            }
		//        }
		//    }
		//    catch (Exception err)
		//    {
		//        LogHelper.Manage("PageView:ApplyNewFrames", err);
		//    }
		//}

        #region -----------------image layer-----------------

        void _ImgContent_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

                //_ImgContent.Focus();
                _ImgContent.CaptureMouse();
            }
        }

        void _ImgContent_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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
			}
		}

		void _ImgContent_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			//end image move
			if (_ImgContent.IsMouseCaptured)
			{
				_ImgContent.ReleaseMouseCapture();
				_ImgContent.Cursor = Cursors.Arrow;
			}
		}

		void _ImgContent_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
            HandleMouseWheel(e);
		}
        #endregion

        #region -----------------drawing layer-----------------

		//Point _startPos;
		//DesignerItem _newDesignerItem = null;

		//void _DrawingLayout_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		//{
		//    if (e.LeftButton == MouseButtonState.Pressed)
		//    {
		//        _startPos = e.GetPosition(_DrawingLayer);

		//        _newDesignerItem = new DesignerItem();
		//        _newDesignerItem.DataContext = CreateNextZone();

		//        Canvas.SetLeft(_newDesignerItem, Math.Max(0, _startPos.X));
		//        Canvas.SetTop(_newDesignerItem, Math.Max(0, _startPos.Y));

		//        _DrawingLayer.Children.Add(_newDesignerItem);
		//        _DrawingLayer.DeselectAll();

		//        _DrawingLayer.CaptureMouse();
		//    }
		//}

		//void _DrawingLayout_PreviewMouseMove(object sender, MouseEventArgs e)
		//{
		//    if (_DrawingLayer.IsMouseCaptured && _newDesignerItem != null)
		//    {
		//        Point last = e.GetPosition(_DrawingLayer);
		//        _newDesignerItem.Width = Math.Abs(_startPos.X - last.X);
		//        _newDesignerItem.Height = Math.Abs(_startPos.Y - last.Y);
		//    }
		//}

		//void _DrawingLayout_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		//{
		//    if (_DrawingLayer.IsMouseCaptured)
		//    {
		//        _DrawingLayer.ReleaseMouseCapture();

		//        if (_newDesignerItem != null)
		//        {
		//            Point last = e.GetPosition(_DrawingLayer);
		//            _newDesignerItem.Width = Math.Abs(_startPos.X - last.X);
		//            _newDesignerItem.Height = Math.Abs(_startPos.Y - last.Y);
                
		//            _newDesignerItem = null;
		//        }
		//    }
		//}

        void _DrawingLayout_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            HandleMouseWheel(e);
        }

		//private Core.Models.Zone CreateNextZone()
		//{
		//    List<Core.Models.Zone> zones = new List<Core.Models.Zone>();

		//    foreach (DesignerItem elems in _DrawingLayer.Children)
		//    {
		//        zones.Add(elems.DataContext as Core.Models.Zone);
		//    }

		//    return new Core.Models.Zone() { OrderNum = FrameList.Count > 0 ? FrameList.Max(p => p.OrderNum) + 1 : 0, Duration = 10 };
		//}
            
        private void SwapEdit(bool isEditing)
        {
			//if (this._ImgContent == null) return;
			//if (this._DrawingLayer == null) return;

			//this._ImgContent.IsHitTestVisible = !isEditing;
			//this._DrawingLayer.IsHitTestVisible = isEditing;

			//this._ImgContent.IsEnabled = !isEditing;
			//this._DrawingLayer.IsEnabled = isEditing;
        }

        #endregion

        void HandleMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            //zooming
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                double scale = _scaleTransform.ScaleX;
                scale += (e.Delta > 0) ? 0.1 : -0.1;
                scale = scale <= 0.2 ? 0.2 : scale;
                scale = scale >= 5 ? 5 : scale;
                _scaleTransform.ScaleX = _scaleTransform.ScaleY = scale;

                e.Handled = true;

                RaiseZoomChanged();
            }
            else //scrolling
            {
                if (e.Delta > 0)
                {
                    ManageScroolUp();
                }
                else
                {
                    ManageScroolDown();
                }
            }
        }

        #endregion

        #region -----------------manage scrool-----------------

        private void ManageScrool()
        {
            try
            {
                if (_ScrollContainer.VerticalOffset == 0)
                    _ScrollContainer.ScrollToBottom();

                if (_ScrollContainer.VerticalOffset + _ScrollContainer.ViewportHeight >= _ScrollContainer.ExtentHeight)
                    _ScrollContainer.ScrollToHome();
            }
            catch (Exception err)
            {
                LogHelper.Manage("PageView:ManageScrool", err);
            }
        }

        /// <summary>
		/// Scrool up if at the top of the page
		/// </summary>
		private bool ManageScroolUp()
		{
			try
			{
				if (_ScrollContainer.VerticalOffset == 0)
				{
					if (_waitAtPageEnd == true)
					{
						_waitAtPageEnd = false;
						return false;
					}
					else _waitAtPageEnd = WorkspaceService.Instance.Settings.Extended.BehavePageTempo;

					RaisePageNeeded(-1);
					return true;
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("PageView:ManageScroolUp", err);
			}
			return false;
		}

		/// <summary>
		/// Scrool down if at the end of the page
		/// </summary>
		private bool ManageScroolDown()
		{
			try
			{
				if (_ScrollContainer.VerticalOffset + _ScrollContainer.ViewportHeight >= _ScrollContainer.ExtentHeight)
				{
					if (_waitAtPageEnd == true)
					{
						_waitAtPageEnd = false;
						return false;
					}
					else _waitAtPageEnd = WorkspaceService.Instance.Settings.Extended.BehavePageTempo;

					RaisePageNeeded(1);
					return true;
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("PageView:ManageScroolDown", err);
			}
			return false;
		}

		#endregion

		#region -----------------page need event-----------------
		/// <summary>
		/// Page registered event
		/// </summary>
		public static readonly RoutedEvent PageNeededEvent = EventManager.RegisterRoutedEvent("PageNeededEvent",
																RoutingStrategy.Bubble,
																typeof(PageNeededEventHandler), typeof(PageControl));

        /// <summary>
		/// page changed event handler delegate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		public delegate void PageNeededEventHandler(object sender, PageRoutedEventArgs e);

		/// <summary>
		/// Page chaned event handler
		/// </summary>
		public event PageNeededEventHandler OnPageNeeded
        {
			add { AddHandler(PageNeededEvent, value); }
			remove { RemoveHandler(PageNeededEvent, value); }
        }

		/// <summary>
		/// Raise the page changed event
		/// </summary>
		/// <param name="offset"></param>
        protected void RaisePageNeeded(int offset)
        {
            PageRoutedEventArgs args = new PageRoutedEventArgs(offset);
			args.RoutedEvent = PageNeededEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// PageRoutedEventArgs : a custom event argument class
        /// </summary>
        public class PageRoutedEventArgs : RoutedEventArgs
        {
            private int _PageOffset;

            public PageRoutedEventArgs(int offset)
            {
                this._PageOffset = offset;
            }

            public int PageOffset
            {
                get { return _PageOffset; }
            }
        }

		#endregion

		#region -----------------zoom event-----------------

		/// <summary>
		/// Zoom registered event 
		/// </summary>
		public static readonly RoutedEvent ZoomChangedEvent = EventManager.RegisterRoutedEvent("ZoomChangedEvent",
																RoutingStrategy.Bubble,
																typeof(ZoomChangedEventHandler), typeof(PageControl));

		/// <summary>
		/// the event handler delegate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void ZoomChangedEventHandler(object sender, ZoomRoutedEventArgs e);

		/// <summary>
		/// zoom changed event handler
		/// </summary>
		public event ZoomChangedEventHandler OnZoomChanged
		{
			add { AddHandler(ZoomChangedEvent, value); }
			remove { RemoveHandler(ZoomChangedEvent, value); }
		}

		/// <summary>
		/// Raise the zoom changed event
		/// </summary>
		protected void RaiseZoomChanged()
		{
			ZoomRoutedEventArgs args = new ZoomRoutedEventArgs( _scaleTransform.ScaleX );
			args.RoutedEvent = ZoomChangedEvent;
			RaiseEvent(args);
		}

		/// <summary>
		/// Zoom changed event arguments
		/// </summary>
		public class ZoomRoutedEventArgs : RoutedEventArgs
		{
			private double _Scale;

			public ZoomRoutedEventArgs(double scale)
			{
				this._Scale = scale;
			}

			public double Scale
			{
				get { return _Scale; }
			}
		}
		#endregion
	}
}
