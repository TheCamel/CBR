using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace CBR.Components.Controls
{
	public class BrowseForControl : Control
	{
		public enum BrowseMode { ForFolder, ForFile };

		static BrowseForControl()
        {
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BrowseForControl),
				new FrameworkPropertyMetadata(typeof(BrowseForControl)));
        }

		#region --------------------DEPENDENCY PROPERTIES--------------------

		#region ModeProperty

		public static readonly DependencyProperty ModeProperty =
			   DependencyProperty.Register("Mode", typeof(BrowseMode), typeof(BrowseForControl), null);

		public BrowseMode Mode
		{
			get { return (BrowseMode)GetValue(ModeProperty); }
			set { SetValue(ModeProperty, value); }
		}

		#endregion

		#region FiltersProperty

		public static readonly DependencyProperty FiltersProperty =
			   DependencyProperty.Register("Filters", typeof(string), typeof(BrowseForControl), null);

		public string Filters
		{
			get { return (string)GetValue(FiltersProperty); }
			set { SetValue(FiltersProperty, value); }
		}

		#endregion

		#region SelectionProperty

		public static readonly DependencyProperty SelectionProperty =
			   DependencyProperty.Register("Selection", typeof(string), typeof(BrowseForControl),
			   new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectionChanged)));

		public string Selection
		{
			get { return (string)GetValue(SelectionProperty); }
			set { SetValue(SelectionProperty, value); }
		}

		private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
				return;

			BrowseForControl element = d as BrowseForControl;
			element.UpdateBox(e.NewValue as string);
		}

		#endregion

		#endregion

		#region --------------------EVENT--------------------

		public static readonly RoutedEvent BrowseEvent = EventManager.RegisterRoutedEvent("BrowseEvent",
																RoutingStrategy.Bubble,
																typeof(BrowseEventHandler), typeof(BrowseForControl));

		/// <summary>
		/// page changed event handler delegate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void BrowseEventHandler(object sender, BrowseEventArgs e);

		/// <summary>
		/// Page chaned event handler
		/// </summary>
		public event BrowseEventHandler OnBrowseEvent
		{
			add { AddHandler(BrowseEvent, value); }
			remove { RemoveHandler(BrowseEvent, value); }
		}

		/// <summary>
		/// Raise the page changed event
		/// </summary>
		/// <param name="offset"></param>
		protected void RaiseBrowseEvent(string selection)
		{
			BrowseEventArgs args = new BrowseEventArgs(selection);
			args.RoutedEvent = BrowseEvent;
			RaiseEvent(args);
		}

		/// <summary>
		/// PageRoutedEventArgs : a custom event argument class
		/// </summary>
		public class BrowseEventArgs : RoutedEventArgs
		{
			private string _Selection;

			public BrowseEventArgs(string selection)
			{
				this._Selection = selection;
			}

			public string Selection
			{
				get { return _Selection; }
			}
		}

		#endregion

		private TextBox _TextContainer = null;

		/// <summary>
		/// Gets the parts out of the template.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_TextContainer = (TextBox)this.GetTemplateChild("PART_Box");
			Button btn = (Button)this.GetTemplateChild("PART_Button");
			btn.Click += new RoutedEventHandler(btn_Click);
		}

		private void btn_Click(object sender, RoutedEventArgs e)
		{
			if (Mode == BrowseMode.ForFile)
			{
				using (System.Windows.Forms.OpenFileDialog browser = new System.Windows.Forms.OpenFileDialog())
				{
					browser.Multiselect = false;
					browser.AddExtension = false;
					browser.FileName = this._TextContainer.Text;
					browser.Filter = Filters;
					if (browser.ShowDialog(new Wpf32Window()) == System.Windows.Forms.DialogResult.OK)
					{
						_TextContainer.Text = browser.FileName;
						Selection = browser.FileName;
						RaiseBrowseEvent(Selection);
					}
				}
			}
			else if (Mode == BrowseMode.ForFolder)
			{
				using (System.Windows.Forms.FolderBrowserDialog browser = new System.Windows.Forms.FolderBrowserDialog())
				{
					browser.SelectedPath = this._TextContainer.Text;
					if (browser.ShowDialog(new Wpf32Window()) == System.Windows.Forms.DialogResult.OK)
					{
						_TextContainer.Text = browser.SelectedPath;
						Selection = browser.SelectedPath;
						RaiseBrowseEvent(Selection);
					}
				}
			}
		}

		protected void UpdateBox(string text)
		{
			if( _TextContainer != null ) 
				_TextContainer.Text = text;
		}
	}
}
