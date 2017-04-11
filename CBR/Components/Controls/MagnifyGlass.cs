using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CBR.Components.Controls
{
    [TemplatePart(Name = "PART_magnifierCanvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_magnifierView", Type = typeof(Ellipse))]
    public partial class MagnifyGlass : Control
    {
        #region --------------------CONSTRUCTOR--------------------

        /// <summary>
        /// Constructor
        /// </summary>
        static MagnifyGlass()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MagnifyGlass),
                new FrameworkPropertyMetadata(typeof(MagnifyGlass)));
        }

        #endregion

        #region --------------------DEPENDENCY PROPERTIES--------------------

        #region VisualToDisplay

        /// <summary>
        /// VisualToDisplay DependencyProperty
        /// </summary>
        public static readonly DependencyProperty VisualToDisplayProperty = DependencyProperty.Register("VisualToDisplay", typeof(Visual),
            typeof(MagnifyGlass), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualToDisplayChanged)));

        /// <summary>
        /// VisualToDisplay property wrapper
        /// </summary>
        public Visual VisualToDisplay
        {
            get { return (Visual)GetValue(VisualToDisplayProperty); }
            set { SetValue(VisualToDisplayProperty, value); }
        }

        /// <summary>
        /// VisualToDisplay callback method
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnVisualToDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
                return;

            MagnifyGlass element = d as MagnifyGlass;
            element.UpdateInternVisual((Visual)e.NewValue);

            if (element.MagnifierView != null)
            {
                ((VisualBrush)element.MagnifierView.Fill).Visual = (Visual)e.NewValue;
            }
        }
        #endregion

        #region ScaleProperty

        public static readonly DependencyProperty ScaleProperty =
               DependencyProperty.Register("Scale", typeof(double), typeof(MagnifyGlass),
               new FrameworkPropertyMetadata(new PropertyChangedCallback(OnScaleChanged)));

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
                return;

            MagnifyGlass element = d as MagnifyGlass;
            element._scaleTransform.ScaleX = (double)e.NewValue;
            element._scaleTransform.ScaleY = (double)e.NewValue;
        }
        #endregion

        #region ZoomProperty

        public static readonly DependencyProperty ZoomProperty =
               DependencyProperty.Register("Zoom", typeof(double), typeof(MagnifyGlass));

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        #endregion

        #endregion

        #region --------------------INTERNALS--------------------

        private ScaleTransform _scaleTransform = new ScaleTransform();
        internal FrameworkElement MagnifierCanvas { get; set; }
        internal Ellipse MagnifierView { get; set; }

        /// <summary>
        /// Gets the parts out of the template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            MagnifierCanvas = (FrameworkElement)this.GetTemplateChild("PART_magnifierCanvas");
            MagnifierView = (Ellipse)this.GetTemplateChild("PART_magnifierView");

            _scaleTransform.CenterX = 0.5;
            _scaleTransform.CenterY = 0.5;
            MagnifierCanvas.RenderTransform = _scaleTransform;
        }

        private void UpdateInternVisual(Visual newValue)
        {
            if (MagnifierView != null)
            {
                VisualBrush vb = (VisualBrush)MagnifierView.Fill;
                if (vb.Visual != newValue)
                {
                    if (vb.IsFrozen)
                    {
                        vb = vb.Clone();
                        MagnifierView.Fill = vb;
                    }
                    vb.Visual = newValue;
                }
            }
        }

        /// <summary>
        /// Update the position and the content of the magnifier
        /// </summary>
        /// <param name="pos"></param>
        public void Update(Point pos)
        {
            if (MagnifierCanvas != null && MagnifierView != null)
            {
                UpdateInternVisual(VisualToDisplay);
                
                VisualBrush b = (VisualBrush)MagnifierView.Fill;

                Rect viewBox = b.Viewbox;
                double xoffset = viewBox.Width / 2.0;
                double yoffset = viewBox.Height / 2.0;
                viewBox.X = pos.X - xoffset;
                viewBox.Y = pos.Y - yoffset;

                viewBox.Width = Zoom;
                viewBox.Height = Zoom;

                b.Viewbox = viewBox;

                Canvas.SetLeft(MagnifierCanvas, pos.X - MagnifierView.Width * _scaleTransform .ScaleX / 2);
                Canvas.SetTop(MagnifierCanvas, pos.Y - MagnifierView.Height * _scaleTransform.ScaleY / 2);
            }
        }
        #endregion
    }
}
