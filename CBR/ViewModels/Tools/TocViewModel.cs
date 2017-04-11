using System.Collections;
using System.Collections.Generic;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using GalaSoft.MvvmLight.Messaging;
using CBR.ViewModels.Messages;
using CBR.Core.Formats.ePUB;

namespace CBR.ViewModels
{
	public class TocViewModel : ToolViewModel
	{
		#region ----------------CONSTRUCTOR----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public TocViewModel()
            : base(CultureManager.Instance.GetLocalization("ByCode", "TocView.Title", "Table of content"))
		{
            this.ContentId = "TocViewModel";
			base.Icon = "pack://application:,,,/Resources/Images/32x32/book/book.png";
			base.IsVisible = false;

			CultureManager.Instance.UICultureChanged += new CultureEventArrived(Instance_UICultureChanged);

			Messenger.Default.Register<DocumentViewModel>(this, ViewModelMessages.DocumentActivChanged, HandleActiveDocumentChange);
			Messenger.Default.Register<TocChangedNotification>(this, HandleTocContentChange);
		}

		/// <summary>
		/// Child classes can override this method to perform clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();
	
			CultureManager.Instance.UICultureChanged -= new CultureEventArrived(Instance_UICultureChanged);

			Messenger.Default.Unregister<DocumentViewModel>(this, ViewModelMessages.DocumentActivChanged, HandleActiveDocumentChange);
			Messenger.Default.Unregister<TocChangedNotification>(this, HandleTocContentChange);
		}

		#endregion

		new public List<ePUBNavPoint> Data
		{
			get { return base.Data as List<ePUBNavPoint>; }
			set
			{
				base.Data = value;
			}
		}

		#region -----------------HANDLERS-----------------

		private void Instance_UICultureChanged(object sender, CultureEventArgs e)
		{
            DisplayName = CultureManager.Instance.GetLocalization("ByCode", "TocView.Title", "Table of content");
		}

		private void HandleTocContentChange(TocChangedNotification o)
		{
            base.IsVisible = o.Content != null  ? true : false;

            Data = (List<ePUBNavPoint>)o.Content;

            RaisePropertyChanged("TableOfContent");
        }

		private void HandleActiveDocumentChange(DocumentViewModel o)
		{
			if (o is BookViewModelBase)
			{
                BookViewModelBase bbk = o as BookViewModelBase;
                if (bbk.HasTableOfContent)
                    HandleTocContentChange(new TocChangedNotification(bbk.TableOfContent));
			}
			else HandleTocContentChange(new TocChangedNotification(null));
		}

		#endregion
	}
}
