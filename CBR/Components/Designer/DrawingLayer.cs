using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using CBR.Components.Designer;
using System.Windows.Input;
using System.Collections;
using System.ComponentModel;
using CBR.Core.Models;

namespace CBR.Components.Designer
{
	public class DrawingLayer : ItemsControl
	{
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return (item is DesignerItem);
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new DesignerItem();
		}

		public IEnumerable<DesignerItem> SelectedItems
		{
			get
			{
				List<DesignerItem> result = new List<DesignerItem>();

				foreach (Zone dc in ItemsSource)
				{
					DesignerItem test = ItemContainerGenerator.ContainerFromItem(dc) as DesignerItem;
					if (test.IsSelected == true)
						result.Add(test);
				}
				return result;
			}
		}

		public void SelectAll()
		{
			List<DesignerItem> result = new List<DesignerItem>();

			foreach (Zone dc in ItemsSource)
			{
				DesignerItem test = ItemContainerGenerator.ContainerFromItem(dc) as DesignerItem;
				if (!test.IsSelected)
					test.IsSelected = true;
			}
		}

		public void DeselectAll()
		{
			foreach (DesignerItem item in this.SelectedItems)
			{
				item.IsSelected = false;
			}
		}

		private Canvas _DrawingLayer;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				return;

			_DrawingLayer = (Canvas)GetTemplateChild("PART_DrawingLayer");
			_DrawingLayer.MouseLeftButtonDown += new MouseButtonEventHandler(_DrawingLayout_PreviewMouseLeftButtonDown);
			_DrawingLayer.MouseLeftButtonUp += new MouseButtonEventHandler(_DrawingLayout_PreviewMouseLeftButtonUp);
			_DrawingLayer.MouseMove += new MouseEventHandler(_DrawingLayout_PreviewMouseMove);

			_DrawingLayer.KeyDown += new KeyEventHandler(_DrawingLayer_KeyDown);
		}

		void _DrawingLayer_KeyDown(object sender, KeyEventArgs e)
		{
			if ( Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.A )
			{
				SelectAll();
			}
			if (e.Key == Key.Delete)
			{
				IEnumerable<DesignerItem> select = SelectedItems;
				if (select.Count() > 0)
				{
					IList ic = ItemsSource as IList;

					foreach (DesignerItem item in this.SelectedItems)
						ic.Remove(item.DataContext);
				}
			}
		}

		Point _startPos;
		DesignerItem _newDesignerItem = null;

		void _DrawingLayout_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_DrawingLayer.Focus();

			if (e.LeftButton == MouseButtonState.Pressed)
			{
				_startPos = e.GetPosition(_DrawingLayer);

				IList<Zone> ic = ItemsSource as IList<Zone>;

				Zone itemZone = new Zone() { X = _startPos.X, Y = _startPos.Y, Width = 1, Height = 1,
											 OrderNum = ic.Count > 0 ? ic.Max(p => p.OrderNum) + 1 : 0,
											 Duration = 10
						};
				ic.Add( itemZone );

				_newDesignerItem = ItemContainerGenerator.ContainerFromItem(itemZone) as DesignerItem;
				_newDesignerItem.Visibility = System.Windows.Visibility.Hidden;

				DeselectAll();

				_DrawingLayer.CaptureMouse();
			}
			e.Handled = true;
		}

		void _DrawingLayout_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (_DrawingLayer.IsMouseCaptured && _newDesignerItem != null)
			{
				Point last = e.GetPosition(_DrawingLayer);
				_newDesignerItem.Width = Math.Abs(_startPos.X - last.X);
				_newDesignerItem.Height = Math.Abs(_startPos.Y - last.Y);

				if (_newDesignerItem.Width > 2 && _newDesignerItem.Height > 2)
				{
					_newDesignerItem.IsSelected = true;
					_newDesignerItem.Visibility = System.Windows.Visibility.Visible;
				}

				e.Handled = true;
			}
		}

		void _DrawingLayout_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_DrawingLayer.IsMouseCaptured)
			{
				_DrawingLayer.ReleaseMouseCapture();

				if (_newDesignerItem != null)
				{
					Point last = e.GetPosition(_DrawingLayer);
					_newDesignerItem.Width = Math.Abs(_startPos.X - last.X);
					_newDesignerItem.Height = Math.Abs(_startPos.Y - last.Y);

					IList ic = ItemsSource as IList;

					if (_newDesignerItem.Width < 10 && _newDesignerItem.Height < 10)
						ic.Remove(_newDesignerItem.DataContext);
					
					_newDesignerItem = null;
				}
				e.Handled = true;
			}
		}
	}
}
