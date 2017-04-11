using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using CBR.Core.Models;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Windows.Documents;

namespace CBR.Core.Services
{
	/// <summary>
	/// Manage XPS documents
	/// </summary>
    public class XpsBookService : BookServiceBase
    {
		/// <summary>
		/// override to load covers
		/// </summary>
		/// <param name="param"></param>
        override internal void LoadCoverThread(object param)
        {
            Book bk = param as Book;

			if (LogHelper.CanDebug())
				LogHelper.Begin("XpsBookService.LoadCoverThread");
			try
			{
				bk.Cover = new XpsHelper().GetXpsThumbnail( bk.FilePath );
			}
			catch (Exception err)
			{
				LogHelper.Manage("XpsBookService.LoadCoverThread", err);
			}
			finally
			{
				LogHelper.End("XpsBookService.LoadCoverThread");
			}  
        }

		/// <summary>
		/// override to load books
		/// </summary>
		/// <param name="bk"></param>
		/// <returns></returns>
        override public object LoadBook(Book bk)
        {
            XpsDocument document = null;
            FixedDocumentSequence fds = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("XpsBookService.LoadBook");
			try
			{
                base.LoadBook(bk);

                document = new XpsDocument(bk.FilePath, FileAccess.Read);
                fds = document.GetFixedDocumentSequence();

                DocumentReference docReference = fds.References.First();
                FixedDocument fixDoc = docReference.GetDocument(false);

                bk.PageCount = fds.DocumentPaginator.PageCount;
                for( int i = 0; i<bk.PageCount-1; i++ )
                    bk.Pages.Add( new Page(bk, i.ToString(), i) );
			}
			catch (Exception err)
			{
				LogHelper.Manage("XpsBookService.LoadBook", err);
			}
			finally
			{
				if (document != null)
					document.Close();

				LogHelper.End("XpsBookService.LoadBook");
			}  
            return fds;
        }
    }
}
