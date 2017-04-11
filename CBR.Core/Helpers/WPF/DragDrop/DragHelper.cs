using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;

namespace CBR.Core.Helpers
{
    /// <summary>
    /// event handler for source control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void StartDragEventHandler(object sender, MouseEventArgs e);
    
    /// <summary>
    /// This class allow to associate drag and drop functionnality to a control
    /// </summary>
    public class DragHelper
    {
        #region ----------------CONSTRUCTOR----------------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="attachedView"></param>
        public DragHelper(Control attachedView, Type managedItemType)
        {
			_ManagedItemType = managedItemType;

            _Attached = attachedView;
            _Attached.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PreviewMouseLeftButtonDown);
            _Attached.PreviewMouseMove += new MouseEventHandler(PreviewMouseMove);
        }

        #endregion

        #region ----------------INTERNALS AND PROPERTIES----------------

		private Type _ManagedItemType = null;

        private Control _Attached = null;
        /// <summary>
        /// Attached control
        /// </summary>
        public Control Attached
        {
            get { return _Attached; }
            set { _Attached = value; }
        }

        /// <summary>
        /// event to get start drag notification
        /// </summary>
        public event StartDragEventHandler OnStartDrag;
        public event StartDragEventHandler OnContinueDrag;

        private bool _canDrop;
        private bool _isDoDragDrop;
        private bool _previousDrop;
        private Point _startPoint;
        private FrameworkElement _DragScope = null;
        private DragAdorner _DragAdorner = null;
        private DragEventHandler _Draghandler = null;
        #endregion

        #region ----------------METHODS----------------

        public void DoDragDrop(DataObject data, UIElement source)
        {
            try
            {
                _isDoDragDrop = true;

                // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                _DragScope = Application.Current.MainWindow.Content as FrameworkElement;
                System.Diagnostics.Debug.Assert(_DragScope != null);

                // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                _previousDrop = _DragScope.AllowDrop;
                _DragScope.AllowDrop = true;

                // Let's wire our usual events.. 
                // GiveFeedback just tells it to use no standard cursors..  

                //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                //_Attached.GiveFeedback += feedbackhandler;

                // The DragOver event ... 
                _Draghandler = new DragEventHandler(ScopeDragOver);
                _DragScope.PreviewDragOver += _Draghandler;

                //// Drag Leave is optional, but write up explains why I like it .. 
                //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                //DragScope.DragLeave += dragleavehandler;

                //// QueryContinue Drag goes with drag leave... 
                //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                //DragScope.QueryContinueDrag += queryhandler;

                //Here we create our adorner.. 
                _DragAdorner = new DragAdorner(_DragScope, source, true, 0.5);
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(_DragScope as Visual);
                layer.Add(_DragAdorner);

                DragDropEffects de = DragDrop.DoDragDrop(_Attached, data, DragDropEffects.Move);

                //_Attached.GiveFeedback -= feedbackhandler;
                //DragScope.DragLeave -= dragleavehandler;
                //DragScope.QueryContinueDrag -= queryhandler;
            }
            catch (Exception err)
            {
                LogHelper.Manage("DragHelper:DoDragDrop", err);
            }
            finally
            {
                Cleanup();
            }
        }

        public void DragDropContinue(bool result)
        {
            try
            {
                _canDrop = result;
            }
            catch (Exception err)
            {
                LogHelper.Manage("DragHelper:DragDropContinue", err);
            }
        }

        void Cleanup()
        {
            try
            {
                Mouse.Capture(null);
                if (_DragScope != null)
                {
                    _DragScope.AllowDrop = _previousDrop;

                    if (_DragAdorner != null)
                    {
                        AdornerLayer.GetAdornerLayer(_DragScope).Remove(_DragAdorner);
                        _DragAdorner = null;
                    }

                    _DragScope.PreviewDragOver -= _Draghandler;
                }

                _canDrop = _isDoDragDrop = false;
            }
            catch (Exception err)
            {
                LogHelper.Manage("DragHelper:Cleanup", err);
            }
        }

        void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
			// Get the dragged item type ?
			object item = VisualHelper.FindAnchestor((DependencyObject)e.OriginalSource, _ManagedItemType);
			if (item != null)
			{
				_startPoint = e.GetPosition(null);
				_canDrop = _isDoDragDrop = false;
			}
        }

        void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
				if (e.LeftButton == MouseButtonState.Pressed && !_isDoDragDrop)
				{
					Point position = e.GetPosition(null);

					if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
						Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
					{
						if (OnStartDrag != null)
							OnStartDrag(this, e);
					}
				}
				else
                if (e.LeftButton == MouseButtonState.Pressed && _isDoDragDrop )
                {
                    Point position = e.GetPosition(null);

                    if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        Mouse.Capture(Application.Current.MainWindow);

                        if (OnContinueDrag != null)
                            OnContinueDrag(this, e);
                    }
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("DragHelper:PreviewMouseMove", err);
            }
        }

        void ScopeDragOver(object sender, DragEventArgs e)
        {
            try
            {
                if (_DragAdorner != null)
                {
                    _DragAdorner.LeftOffset = e.GetPosition(_DragScope).X;
                    _DragAdorner.TopOffset = e.GetPosition(_DragScope).Y;
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("DragHelper:ScopeDragOver", err);
            }
        }

        #endregion
    }
}
