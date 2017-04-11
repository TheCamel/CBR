using System;
using System.Collections.Generic;
using System.Linq;
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
using CBR.ViewModels;
using CBR.Core.Helpers;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for BookView.xaml
    /// </summary>
    public partial class BookView : UserControl
    {
        #region -----------------private and constructor-----------------

        public BookView()
        {
			using (new TimeLogger("BookView.BookView"))
			{
				InitializeComponent();
                VisualHelper.AllowFocus(this);
			}
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
			//this.PageViewer.Focus();
			this.PageViewer.OnPageNeeded += new Components.Controls.PageControl.PageNeededEventHandler(PageViewer_OnPageNeeded);
			this.PageViewer.OnZoomChanged += new Components.Controls.PageControl.ZoomChangedEventHandler(PageViewer_OnZoomChanged);
		}

		void PageViewer_OnZoomChanged(object sender, Components.Controls.PageControl.ZoomRoutedEventArgs e)
		{
			ComicViewModel bvm = DataContext as ComicViewModel;
			bvm.Scale = e.Scale;
		}

		void PageViewer_OnPageNeeded(object sender, Components.Controls.PageControl.PageRoutedEventArgs e)
		{
			ComicViewModel bvm = DataContext as ComicViewModel;
            //if( bvm.IsInEditMode )
            //    bvm.CurrentPage.Frames = this.PageViewer.FrameList;
			
            bvm.BookChangePageCommand.Execute( e.PageOffset.ToString() );
		}

		#endregion

		#region -----------------EVENTS-----------------

		/// <summary>
		/// update the magnifier
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PageViewer_PreviewMouseMove(object sender, MouseEventArgs e)
		{
            //manage the magnifier
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (Magnifier.Visibility == System.Windows.Visibility.Visible)
                    Magnifier.Update(Mouse.GetPosition(PageViewerGrid));
                e.Handled = true;
            }
		}

        private void PageViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //manage the magnifier
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Magnifier.Update(Mouse.GetPosition(PageViewerGrid));
                Magnifier.Visibility = Visibility.Visible;

                this.PageViewer.CaptureMouse();
                e.Handled = true;
            }
        }

        private void PageViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //manage the magnifier
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Magnifier.Visibility = Visibility.Hidden;
                this.PageViewer.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void PageViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //manage the magnifier
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Magnifier.Update(Mouse.GetPosition(PageViewerGrid));
                Magnifier.Visibility = Visibility.Visible;

                this.PageViewer.CaptureMouse();
                e.Handled = true;
            }
        }

        private void PageViewer_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            //manage the magnifier
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Magnifier.Visibility = Visibility.Hidden;
                this.PageViewer.ReleaseMouseCapture();
                e.Handled = true;
            }
        }
        #endregion
    }
}
