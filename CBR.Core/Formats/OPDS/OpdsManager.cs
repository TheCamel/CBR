using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;
using CBR.Core.Helpers;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using CBR.Core.Files;
using CBR.Core.Services;
using CBR.Core.Models;

namespace CBR.Core.Formats.OPDS
{
	public class OpdsManager
	{
		private static Dictionary<string, DocumentType> _appTypes = new Dictionary<string, DocumentType>()
			{
				{"application/epub+zip", DocumentType.ePUB },
				{"text/html", DocumentType.HTML },
				{"application/pdf", DocumentType.PDF },
				{"application/x-rar-compressed", DocumentType.RARBased },
				{"application/zip", DocumentType.ZIPBased },
				{"application/vnd.ms-xpsdocument", DocumentType.XPS }
			};

		public bool CanFromCache(Uri uri)
		{
			string file = GetCachedFileName(uri);
			if (File.Exists(file))
			{
				if (File.GetLastWriteTime(file) < DateTime.Now.AddDays(WorkspaceService.Instance.Settings.Feed.CacheDuration))
					return true;
			}

			return false;
		}

		public OpdsFeed ConvertFeed(Uri uri)
		{
			string file = GetCachedFileName(uri);

			using (Stream cache = ReadCacheFile(file))
			{
				return ConvertToModel(cache);
			}
		}

		public OpdsFeed ConvertFeed(Stream result, Uri uri)
		{
			using (Stream cache = SaveFeedCache(result, uri))
			{
				return ConvertToModel(cache);
			}
		}

		private OpdsFeed ConvertToModel(Stream cache)
		{
			XDocument doc = XDocument.Load(cache);

			OpdsFeed fvm = new OpdsFeed();
			fvm.Author = ParseItemAndSub(doc.Root.Elements(), "author", "name");
			fvm.PageTitle = ParseValue(doc.Root.Elements(), "title");
			fvm.Icon = ConverToUri(ParseValue(doc.Root.Elements(), "icon"));
			fvm.WebUrl = ParseUrlByRel(doc.Root.Elements(), "alternate");
			fvm.SearchUrl = ParseUrlByRel(doc.Root.Elements(), "search");
			fvm.NextUrl = ParseUrlByRel(doc.Root.Elements(), "next");
			fvm.PreviousUrl = ParseUrlByRel(doc.Root.Elements(), "previous");
			if (fvm.NextUrl != null || fvm.PreviousUrl != null)
			{
				fvm.TotalResults = Convert.ToInt32(ParseValue(doc.Root.Elements(), "totalResults"));
				fvm.ItemPerPage = Convert.ToInt32(ParseValue(doc.Root.Elements(), "itemsPerPage" ));
			}

			//fvm.StartUrl = new Uri( ParseItemAndSub(doc.Root.Elements(), "author", "uri")+ParseLinkByRel(doc.Root.Elements(), "start") );

			fvm.Items = new List<OpdsItemBase>();

			List<XElement> elemList = doc.Root.Elements().Where(i => i.Name.LocalName == "entry").ToList();
			foreach (XElement elem in elemList)
			{
				if (fvm.NextUrl == null && fvm.PreviousUrl == null) //not on a book, then category
				{
					OpdsCategory cat = new OpdsCategory();
					cat.Title = ParseValue( elem.Elements(), "title");
					cat.Link = ParseUrlByType(elem.Elements(), "application/atom+xml");
					cat.Updated = ConvertToDateTime(ParseValue(elem.Elements(), "updated"));
					//Thumbnail = ConvertToBitmapImage( item.Elements().First(i => i.Name.LocalName == "link" &&
						//  i.Attributes().First(a => a.Name.LocalName == "type").Value == "image/png").Attribute("href").Value	)
					cat.Content = ParseValue( elem.Elements(), "content");

					fvm.Items.Add(cat);
				}
				else
				{
					OpdsItem book = new OpdsItem();
					book.Title = ParseValue(elem.Elements(), "title");
					book.AuthorName = ParseItemAndSub( elem.Elements(), "author", "name");
					book.AuthorWeb = ConverToUri(ParseItemAndSub(elem.Elements(), "author", "uri"));
					book.Language = ParseValue(elem.Elements(), "language");
					book.Updated = ConvertToDateTime(ParseValue(elem.Elements(), "updated"));

					book.Content = ParseValue(elem.Elements(), "content").Replace("<br />", Environment.NewLine);
					if(string.IsNullOrEmpty(book.Content))
						book.Content = ParseValue(elem.Elements(), "summary").Replace("<br />", Environment.NewLine);

					book.Categories = elem.Elements().Where(i => i.Name.LocalName == "category").Select(m => m.Attributes().First(a => a.Name.LocalName == "label").Value).ToList();
					book.AlternateUrl = ParseUrlByRel( elem.Elements(), "alternate");
					book.SameAuthorUrl = ParseUrlByRel( elem.Elements(), "related");

					book.ThumbnailUrl = ParseUrlByRel(elem.Elements(), "http://opds-spec.org/image/thumbnail");

					//all download links
					List<XElement> elemDown = elem.Elements().Where(i => i.Name.LocalName == "link" &&
								i.Attributes().First(a => a.Name.LocalName == "rel").Value == "http://opds-spec.org/acquisition" ).ToList();

					book.Downloads = new List<OpdsDownload>();
					foreach( XElement download in elemDown )
					{
						if( _appTypes.Keys.Contains(download.Attributes().First(a => a.Name.LocalName == "type").Value) )
							book.Downloads.Add(new OpdsDownload()
							{
								Type = _appTypes[download.Attributes().First(a => a.Name.LocalName == "type").Value],
								Link = new Uri(download.Attributes().First(a => a.Name.LocalName == "href").Value),
								Title = book.Title
							});
					}
					fvm.Items.Add(book);
				}
			}
			return fvm;
		}

		


		#region ----------------PARSING METHODS----------------

		private string ParseValue(IEnumerable<XElement> children, string elementName)
		{
			try
			{
				return children.First(i => i.Name.LocalName == elementName).Value;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		private Uri ParseUrlByType(IEnumerable<XElement> children, string urlType)
		{
			try
			{
				return new Uri(children.First(i => i.Name.LocalName == "link" &&
								 i.Attributes().First(a => a.Name.LocalName == "type").Value.Contains(urlType)).Attribute("href").Value);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private Uri ParseUrlByRel(IEnumerable<XElement> children, string urlRel)
		{
			try
			{
				return new Uri(children.First(i => i.Name.LocalName == "link" &&
								 i.Attributes().First(a => a.Name.LocalName == "rel").Value == urlRel).Attribute("href").Value);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private string ParseLinkByRel(IEnumerable<XElement> children, string urlRel)
		{
			try
			{
				return children.First(i => i.Name.LocalName == "link" &&
								 i.Attributes().First(a => a.Name.LocalName == "rel").Value == urlRel).Attribute("href").Value;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private string ParseItemAndSub(IEnumerable<XElement> children, string itemName, string subName)
		{
			try
			{
				return children.First(i => i.Name.LocalName == itemName).Elements().First(i => i.Name.LocalName == subName).Value;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		#endregion

		#region ----------------CONVERT METHODS----------------

		private Uri ConverToUri(string data)
		{
			if (!string.IsNullOrEmpty(data))
				return new Uri(data);
			else
				return null;
		}
		/// <summary>
		/// opds type to intern document type
		/// </summary>
		/// <param name="typ"></param>
		/// <returns></returns>
		private DocumentType ConvertToType(string typ)
		{
			if (_appTypes.ContainsKey(typ))
				return _appTypes[typ];
			else
				return DocumentType.None;
		}

		/// <summary>
		/// string to date
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		private DateTime ConvertToDateTime(string date)
		{
			return Convert.ToDateTime( date );
		}

		/// <summary>
		/// string to image
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private BitmapImage ConvertToBitmapImage(string data)
		{
			string stream = data.Split(',')[1];
			using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(stream)))
			{
				return StreamToImage.GetImageFromStream( ms, 0 );
			}
		}

		#endregion

		#region ----------------CACHE MANAGEMENT----------------

		/// <summary>
		/// Save the given stream under uri file name and return the cache content
		/// </summary>
		/// <param name="result"></param>
		/// <param name="uri"></param>
		/// <returns></returns>
		private Stream SaveFeedCache(Stream result, Uri uri)
		{
			string file = GetCachedFileName(uri);
			
			XmlDocument xd = new XmlDocument();
			xd.Load( result );
			xd.Save(file);

			return ReadCacheFile(file);
		}

		/// <summary>
		/// Read a cached file as a stream
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		private Stream ReadCacheFile(string file)
		{
			StreamReader sr = new StreamReader(file, Encoding.UTF8);
			return sr.BaseStream;
		}

		/// <summary>
		/// Construct a file name based on uri and parameters
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		private string GetCachedFileName(Uri uri)
		{
			string parameters = GetParams(uri.OriginalString);
			string file = uri.Host + uri.LocalPath.Replace('/', '_') + parameters + ".rss";
			return DirectoryHelper.Combine(CBRFolders.Cache, file);
		}

		/// <summary>
		/// extract the parameter from an uri string for file name compliance
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		private string GetParams(string s)
		{
			string parameters = string.Empty;

			NameValueCollection nvc = ParseQueryString(s);

			for (int i = 0; i < nvc.Count; i++)
				parameters += nvc.AllKeys[i] + nvc.Get(i);
 
			return parameters;
		}

		/// <summary>
		/// Parse a query string to extract key/value from the uri
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		private NameValueCollection ParseQueryString(string s) 
		{ 
			NameValueCollection nvc = new NameValueCollection(); 
 
			// remove anything other than query string from url 
			if(s.Contains("?")) 
			{ 
				s = s.Substring(s.IndexOf('?') + 1); 
			}
 
			foreach (string vp in Regex.Split(s, "&")) 
			{ 
				string[] singlePair = Regex.Split(vp, "="); 
				if (singlePair.Length == 2) 
				{ 
					nvc.Add(singlePair[0], singlePair[1]); 
				} 
			} 
			return nvc;
		}
		#endregion
	}
}
