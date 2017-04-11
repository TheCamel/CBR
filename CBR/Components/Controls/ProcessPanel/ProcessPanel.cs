using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Animation;

namespace CBR.Components.Controls
{
	public class ProcessPanel : ItemsControl
	{
		#region --------------------CONSTRUCTORS--------------------

		static ProcessPanel()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ProcessPanel),
				new FrameworkPropertyMetadata(typeof(ProcessPanel)));
		}

		/// <summary>
        /// LoadingAnimation constructor.
        /// </summary>
		public ProcessPanel()
        {
			this.DefaultStyleKey = typeof(ProcessPanel);
        }

		#endregion

		#region --------------------DEPENDENCY PROPERTIES--------------------

		

		#endregion

		#region --------------------PROPERTIES--------------------

		#endregion

		Storyboard _CloseStoryboard;
		Storyboard _OpenStoryboard;
		DoubleAnimation _CloseAnim;
		DoubleAnimation _OpenAnim;
		DockPanel _panel;

		/// <summary>
		/// Gets the parts out of the template.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_panel = (DockPanel)GetTemplateChild("PART_Container");
			
			//this.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(_panel_MouseLeftButtonUp);
			_CloseStoryboard = (Storyboard)this.GetTemplateChild("PART_CloseStoryboard");
			_CloseAnim = _CloseStoryboard.Children[0] as DoubleAnimation;

			_OpenStoryboard = (Storyboard)this.GetTemplateChild("PART_OpenStoryboard");
			_OpenAnim = _OpenStoryboard.Children[0] as DoubleAnimation;
		}

		//void _panel_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		//{
		//    if (!IsOpen)
		//    {
		//        Console.WriteLine("play open : from " + _OpenAnim.From + "to" + _OpenAnim.To);
		//        _panel.BeginStoryboard(_OpenStoryboard);

		//        IsOpen = true;
		//        return;
		//    }

		//    this.MouseLeave += new System.Windows.Input.MouseEventHandler(_panel_MouseLeave);
		//}

		//void _panel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		//{
		//    if (IsOpen)
		//        IsOpen = false;

		//    this.MouseLeave -= new System.Windows.Input.MouseEventHandler(_panel_MouseLeave);
			
		//    Console.WriteLine("play close : from " + _CloseAnim.From + "to" + _CloseAnim.To);
		//    _panel.BeginStoryboard(_CloseStoryboard);
		//}

		protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			{
				Console.WriteLine("play open : from " + _OpenAnim.From + "to" + _OpenAnim.To);
				_OpenStoryboard.Begin();

				_OpenAnim.From = _OpenAnim.To;
				_OpenAnim.To += 35;

				_CloseAnim.From = _OpenAnim.To;
				_CloseAnim.To = _OpenAnim.From;
			}
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
			{
				_OpenAnim.To = _OpenAnim.From;
				_OpenAnim.From -= 35;

				_CloseAnim.From = _OpenAnim.To;
				_CloseAnim.To = _OpenAnim.From;

				Console.WriteLine("play close : from " + _CloseAnim.From + "to" + _CloseAnim.To);
				_CloseStoryboard.Begin();
			}
		}
	}
}
