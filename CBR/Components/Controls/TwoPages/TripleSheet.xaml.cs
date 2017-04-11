using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace CBR.Components.Controls
{
    /// <summary>
    /// Interaction logic for TripleSheet.xaml
    /// </summary>
    partial class TripleSheet : System.Windows.Controls.ContentControl
    {
        public TripleSheet()
        {
            InitializeComponent();
        }

        private const int animationDuration = 500;

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            ApplyParameters(new PageParameters(this.RenderSize));
        }

        void anim_Completed(object sender, EventArgs e)
        {
            ApplyParameters(new PageParameters(this.RenderSize));

            if (Status == PageStatus.TurnAnimation)
            {
                Status = PageStatus.None;
                RaiseEvent(new RoutedEventArgs(TripleSheet.PageTurnedEvent, this));
            }
            else
                Status = PageStatus.None;
        }

        void anim_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            PageParameters? parameters = ComputePage(this, CornerPoint, origin);
            _cornerPoint = CornerPoint;
			if (parameters != null)
				ApplyParameters(parameters.Value);
        }

        internal CornerOrigin origin = CornerOrigin.BottomRight;
        private const double gripSize = 130;
        private PageStatus _status = PageStatus.None;
        internal Action<PageStatus> SetStatus = null;
        internal Read<PageStatus> GetStatus = null;

        public PageStatus Status
        {
            private get 
            {
                if (GetStatus != null)
                    return GetStatus();
                else
                    return _status;
            }
            set 
            { 
                if (SetStatus != null)
                    SetStatus(value);
                else
                    _status = value; 
                gridShadow.Visibility = value == PageStatus.None ? Visibility.Hidden : Visibility.Visible;
                canvasReflection.Visibility = value == PageStatus.None ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private Point _cornerPoint;
        
        private Point CornerPoint
        {
            get { return (Point) GetValue(TripleSheet.CornerPointProperty); }
            set { SetValue(TripleSheet.CornerPointProperty, value); }
        }

        private void ApplyParameters(PageParameters parameters)
        {
            pageReflection.Opacity = parameters.Page0ShadowOpacity;

            rectangleRotate.Angle = parameters.Page1RotateAngle; 
            rectangleRotate.CenterX = parameters.Page1RotateCenterX; 
            rectangleRotate.CenterY = parameters.Page1RotateCenterY; 
            rectangleTranslate.X = parameters.Page1TranslateX; 
            rectangleTranslate.Y = parameters.Page1TranslateY; 
            clippingFigure.Figures.Clear();
            clippingFigure.Figures.Add(parameters.Page1ClippingFigure); 
            
            RectangleGeometry rg = (RectangleGeometry) clippingPage0.Geometry1;
            rg.Rect = new Rect(parameters.RenderSize);
            PathGeometry pg = (PathGeometry) clippingPage0.Geometry2;
            pg.Figures.Clear();
            pg.Figures.Add(parameters.Page2ClippingFigure);

            pageReflection.StartPoint = parameters.Page1ReflectionStartPoint;
            pageReflection.EndPoint = parameters.Page1ReflectionEndPoint;
            
            pageShadow.StartPoint = parameters.Page0ShadowStartPoint;
            pageShadow.EndPoint = parameters.Page0ShadowEndPoint;
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            if ((Status == PageStatus.DropAnimation) || (Status == PageStatus.TurnAnimation))
                return;

            //Application.Current.MainWindow.Title += "M";

            UIElement source = sender as UIElement;
            Point p = args.GetPosition(source);

            if (!(sender as UIElement).IsMouseCaptured)
            {
                CornerOrigin? tmp = GetCorner(source, p);

                if (tmp.HasValue)
                    origin = tmp.Value;
                else
                {
                    if (Status == PageStatus.DraggingWithoutCapture)
                    {
                        DropPage(ComputeAnimationDuration(source, p, origin));
                    }
                    return;
                }
                Status = PageStatus.DraggingWithoutCapture;
            }

            PageParameters? parameters = ComputePage(source, p, origin);
            _cornerPoint = p;
            if (parameters != null)
                ApplyParameters(parameters.Value);
        }

        private static int ComputeAnimationDuration(UIElement source, Point p, CornerOrigin origin) 
        {
            double ratio = ComputeProgressRatio(source, p, origin);

            return Convert.ToInt32(animationDuration * (ratio / 2 + 0.5));
        }

        private static double ComputeProgressRatio(UIElement source, Point p, CornerOrigin origin) {
            if ((origin == CornerOrigin.BottomLeft) || (origin == CornerOrigin.TopLeft))
                return p.X / source.RenderSize.Width;
            else
                return (source.RenderSize.Width - p.X) / source.RenderSize.Width;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs args)
        {
            //Application.Current.MainWindow.Title += "D";

            //UIElement source = sender as UIElement;
            //Point p = args.GetPosition(source);
            
            //if (GetCorner(source, p).HasValue)
            //    TurnPage(animationDuration);
        }

        private CornerOrigin? GetCorner(UIElement source, Point position) 
        {
            CornerOrigin? result = null;

            Rect topLeftRectangle = new Rect(0, 0, gripSize, gripSize);
            Rect topRightRectangle = new Rect(source.RenderSize.Width - gripSize, 0, gripSize, gripSize);
            Rect bottomLeftRectangle = new Rect(0, source.RenderSize.Height - gripSize, gripSize, gripSize);
            Rect bottomRightRectangle = new Rect(source.RenderSize.Width - gripSize, source.RenderSize.Height - gripSize, gripSize, gripSize);

            if (IsTopLeftCornerEnabled && topLeftRectangle.Contains(position))
                result = CornerOrigin.TopLeft;
            else if (IsTopRightCornerEnabled && topRightRectangle.Contains(position))
                result = CornerOrigin.TopRight;
            else if (IsBottomLeftCornerEnabled && bottomLeftRectangle.Contains(position))
                result = CornerOrigin.BottomLeft;
            else if (IsBottomRightCornerEnabled && bottomRightRectangle.Contains(position))
                result = CornerOrigin.BottomRight;

            return result;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            if ((Status == PageStatus.DropAnimation) || (Status == PageStatus.TurnAnimation))
                return;

            UIElement source = sender as UIElement;
            Point p = args.GetPosition(source);
            
            CornerOrigin? tmp = GetCorner(source, p);

            if (tmp.HasValue)
            {
                origin = tmp.Value;
                this.CaptureMouse();
            }
            else
                return;

            Status = PageStatus.Dragging;
        }
        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            if (this.IsMouseCaptured)
            {
                Status = PageStatus.None;

                UIElement source = sender as UIElement;
                Point p = args.GetPosition(source);

                if (IsOnNextPage(args.GetPosition(this), this, origin))
                    TurnPage(animationDuration);
                else 
                    DropPage(ComputeAnimationDuration(source, p, origin));

                this.ReleaseMouseCapture();
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs args) 
        {
            if (Status == PageStatus.DraggingWithoutCapture)
            {
                DropPage(animationDuration);
            }
        }
        private Point OriginToPoint(UIElement source, CornerOrigin origin) 
        {
            switch (origin)
            {
                case CornerOrigin.BottomLeft:
                    return new Point(0, source.RenderSize.Height);
                case CornerOrigin.BottomRight:
                    return new Point(source.RenderSize.Width, source.RenderSize.Height);
                case CornerOrigin.TopRight:
                    return new Point(source.RenderSize.Width, 0);
                default:
                    return new Point(0, 0);
            }
        }
        private Point OriginToOppositePoint(UIElement source, CornerOrigin origin) 
        {
            switch (origin)
            {
                case CornerOrigin.BottomLeft:
                    return new Point(source.RenderSize.Width * 2, source.RenderSize.Height);
                case CornerOrigin.BottomRight:
                    return new Point(-source.RenderSize.Width, source.RenderSize.Height);
                case CornerOrigin.TopRight:
                    return new Point(-source.RenderSize.Width, 0);
                default:
                    return new Point(source.RenderSize.Width * 2, 0);
            }
        }
        private bool IsOnNextPage(Point p, UIElement source, CornerOrigin origin) 
        {
            switch (origin)
            {
                case CornerOrigin.BottomLeft:
                case CornerOrigin.TopLeft:
                    return p.X > source.RenderSize.Width;
                default:
                    return p.X < 0;
            }
        }

        private void DropPage(int duration) 
        {
            Status = PageStatus.DropAnimation;

            UIElement source = this as UIElement;
            CornerPoint = _cornerPoint;

            this.BeginAnimation(TripleSheet.CornerPointProperty, null);
            PointAnimation anim = 
                new PointAnimation(
                    OriginToPoint(this, origin), 
                    new Duration(TimeSpan.FromMilliseconds(duration)));
            anim.AccelerationRatio = 0.6;

            anim.CurrentTimeInvalidated += new EventHandler(anim_CurrentTimeInvalidated);
            anim.Completed += new EventHandler(anim_Completed);
            this.BeginAnimation(TripleSheet.CornerPointProperty, anim);
        }

        public void TurnPage() 
        {
            TurnPage(animationDuration);
        }

        private void TurnPage(int duration)
        {
            Status = PageStatus.TurnAnimation;

            UIElement source = this as UIElement;
            CornerPoint = _cornerPoint;

            this.BeginAnimation(TripleSheet.CornerPointProperty, null);
            PointAnimation anim = 
                new PointAnimation(
                    OriginToOppositePoint(this, origin),
                    new Duration(TimeSpan.FromMilliseconds(duration)));
            anim.AccelerationRatio = 0.6;

            anim.CurrentTimeInvalidated +=new EventHandler(anim_CurrentTimeInvalidated);
            anim.Completed += new EventHandler(anim_Completed);
            this.BeginAnimation(TripleSheet.CornerPointProperty, anim);
        }

        public void AutoTurnPage(CornerOrigin fromCorner, int duration)
        {
            if (Status != PageStatus.None)
                return;

            Status = PageStatus.TurnAnimation;

            UIElement source = this as UIElement;

            this.BeginAnimation(TripleSheet.CornerPointProperty, null);

            Point startPoint = OriginToPoint(this, fromCorner);
            Point endPoint = OriginToOppositePoint(this, fromCorner);

            CornerPoint = startPoint;
            origin = fromCorner;

            BezierSegment bs =
                new BezierSegment(startPoint, new Point(endPoint.X + (startPoint.X - endPoint.X) / 3, 250), endPoint, true);
            
            PathGeometry path = new PathGeometry();
            PathFigure figure = new PathFigure();
            figure.StartPoint = startPoint;
            figure.Segments.Add(bs);
            figure.IsClosed = false;
            path.Figures.Add(figure);

            PointAnimationUsingPath anim =
                new PointAnimationUsingPath();
            anim.PathGeometry = path;
            anim.Duration = new Duration(TimeSpan.FromMilliseconds(duration));
            anim.AccelerationRatio = 0.6;

            anim.CurrentTimeInvalidated += new EventHandler(anim_CurrentTimeInvalidated);
            anim.Completed += new EventHandler(anim_Completed);
            this.BeginAnimation(TripleSheet.CornerPointProperty, anim);
        }

        public bool IsTopLeftCornerEnabled 
        {
            get { return (bool) GetValue(TripleSheet.IsTopLeftCornerEnabledProperty) ; }
            set { SetValue(TripleSheet.IsTopLeftCornerEnabledProperty, value); }
        }
        public bool IsTopRightCornerEnabled 
        {
            get { return (bool) GetValue(TripleSheet.IsTopRightCornerEnabledProperty) ; }
            set { SetValue(TripleSheet.IsTopRightCornerEnabledProperty, value); }
        }
        public bool IsBottomLeftCornerEnabled 
        {
            get { return (bool) GetValue(TripleSheet.IsBottomLeftCornerEnabledProperty) ; }
            set { SetValue(TripleSheet.IsBottomLeftCornerEnabledProperty, value); }
        }
        public bool IsBottomRightCornerEnabled 
        {
            get { return (bool) GetValue(TripleSheet.IsBottomRightCornerEnabledProperty) ; }
            set { SetValue(TripleSheet.IsBottomRightCornerEnabledProperty, value); }
        }

        public static DependencyProperty CornerPointProperty;
        public static DependencyProperty IsTopLeftCornerEnabledProperty;
        public static DependencyProperty IsTopRightCornerEnabledProperty;
        public static DependencyProperty IsBottomLeftCornerEnabledProperty;
        public static DependencyProperty IsBottomRightCornerEnabledProperty;
        
        public static readonly RoutedEvent PageTurnedEvent;
        
        public event RoutedEventHandler PageTurned
        {
            add
            {
                base.AddHandler(PageTurnedEvent, value);
            }
            remove
            {
                base.RemoveHandler(PageTurnedEvent, value);
            }
        }

		static TripleSheet()
        {
			PageTurnedEvent = EventManager.RegisterRoutedEvent("PageTurned", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TripleSheet));
			CornerPointProperty = DependencyProperty.Register("CornerPoint", typeof(Point), typeof(TripleSheet));
			IsTopLeftCornerEnabledProperty = DependencyProperty.Register("IsTopLeftCornerEnabled", typeof(bool), typeof(TripleSheet), new PropertyMetadata(true));
			IsTopRightCornerEnabledProperty = DependencyProperty.Register("IsTopRightCornerEnabled", typeof(bool), typeof(TripleSheet), new PropertyMetadata(true));
			IsBottomLeftCornerEnabledProperty = DependencyProperty.Register("IsBottomLeftCornerEnabled", typeof(bool), typeof(TripleSheet), new PropertyMetadata(true));
			IsBottomRightCornerEnabledProperty = DependencyProperty.Register("IsBottomRightCornerEnabled", typeof(bool), typeof(TripleSheet), new PropertyMetadata(true));
        }

		private void ContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ApplyParameters(new PageParameters(this.RenderSize));
		}
    }

    public delegate T Read<T>();
}
