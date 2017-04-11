using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CBR.Core.Formats.ePUB;
using CBR.Core.Models;
using CBR.Core.Helpers;
using System.Collections;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
    public class ePUBBookViewModel : BookViewModelBase
    {
        #region ----------------CONSTRUCTOR----------------

        public ePUBBookViewModel(Book bk)
			: base(bk)
        {
            this.Icon = "pack://application:,,,/Resources/Images/32x32/book_type/book_type_epub.png";

            if (Data != null)
                Service.LoadBook(Data);

			//Messenger.Default.Register<object>(ViewModelMessages.TocIndexChanged, HandleTocIndexChange);

			//Messenger.Default.Send(ViewModelMessages.TocContentChanged,
			//    (Data.Tag as ePUB).Container.Package.TableOfContent.Items.Cast<object>().ToList());

            RaisePropertyChanged("CurrentPageSource");
		}

		/// <summary>
		/// Child classes can override this method to perform clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();
			
			//Messenger.Default.Unregister<object>(ViewModelMessages.TocIndexChanged, HandleTocIndexChange);
		}

		#endregion

        #region -----------------PROPERTIES-----------------

		private Uri _currentPageSource = null;
        public Uri CurrentPageSource
        {
			get { return _currentPageSource; }
			set
			{
				if (_currentPageSource != value)
				{
					Messenger.Default.Send<PropertyChangedMessage<Uri>>
						(new PropertyChangedMessage<Uri>(_currentPageSource, value, "CurrentPageSource"));

					_currentPageSource = value;
				}
			}
        }

		override public bool HasTableOfContent
		{
			get { return true; }
		}

		override public IList TableOfContent
		{
			get { return (Data.Tag as ePUB).Container.Package.TableOfContent.Items.Cast<object>().ToList(); }
		}

		override public object TableOfContentIndex
		{
			set { CurrentPageSource = new Uri((Data.Tag as ePUB).GetContentFile(value as ePUBNavPoint)); }
		}

        #endregion

		//#region -----------------HANDLERS-----------------

		//private void HandleTocIndexChange( object o )
		//{
		//    if (this.IsSelected)
		//        CurrentPageSource = new Uri((Data.Tag as ePUB).GetContentFile(o as ePUBNavPoint));
		//}

		

		//#endregion
    }
}
