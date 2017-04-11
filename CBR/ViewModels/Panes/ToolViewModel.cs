using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CBR.ViewModels
{
	public abstract class ToolViewModel : PaneViewModel
	{
		#region -----------------CONSTRUCTOR-----------------

		protected ToolViewModel( string title )
		{
			DisplayName = title;
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		#region IsVisible

		private bool _isVisible = true;
		public bool IsVisible
		{
			get { return _isVisible; }
			set
			{
				if (_isVisible != value)
				{
					_isVisible = value;
					RaisePropertyChanged("IsVisible");
				}
			}
		}

		#endregion

		#endregion
	}
}
