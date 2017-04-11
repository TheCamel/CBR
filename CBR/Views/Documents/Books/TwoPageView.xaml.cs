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
using CBR.Core.Helpers;
using CBR.ViewModels;
using CBR.Components.Controls;

namespace CBR.Views
{
	/// <summary>
	/// Interaction logic for TwoPageView.xaml
	/// </summary>
	public partial class TwoPageView : UserControl
	{
		public TwoPageView()
		{
			using (new TimeLogger("TwoPageView.TwoPageView"))
			{
				InitializeComponent();
                VisualHelper.AllowFocus(this);
			}
		}

		private void btnPrevious_Click(object sender, RoutedEventArgs e)
		{
			this.PageViewer.AnimateToPreviousPage(true, 2000);
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			this.PageViewer.AnimateToNextPage(true, 2000);
		}

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
