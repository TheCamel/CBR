using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Imaging;
using CBR.Core.Models;

namespace CBR.Core.Services
{
	public class PdfService : BookServiceBase
	{
		/// <summary>
		/// override to load covers
		/// </summary>
		/// <param name="param"></param>
		override internal void LoadCoverThread(object param)
		{
			Book bk = param as Book;

			if (LogHelper.CanDebug())
				LogHelper.Begin("PdfService.LoadCoverThread");
			try
			{

				string coverFile = null;

				if (coverFile == null)
				{
					// no image or an error, send default one from us
					GetUnknownCover(bk);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("PdfService.LoadCoverThread", err);
			}
			finally
			{
				LogHelper.End("PdfService.LoadCoverThread");
			}
		}
	}
}
