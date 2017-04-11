using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using CBR.Core.Helpers;

namespace CBR.Core.Formats.ePUB
{
	/// <summary>
	/// Models an epub book
	/// </summary>
	public class ePUB
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		public ePUB()
		{
		}

		/// <summary>
		/// Constructor with file parameter
		/// </summary>
		/// <param name="filePath"></param>
		public ePUB(string fullFileName)
		{
			FilePath = fullFileName;
		}

		/// <summary>
		/// Constructor with file parameter
		/// </summary>
		/// <param name="filePath"></param>
		public ePUB(string fullFileName, string expandFolder)
		{
			FilePath = fullFileName;
			ExpandFolder = expandFolder;
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		[Browsable(true)]
		[Description("FilePath")]
		public string FilePath { get; set; }

		[Browsable(true)]
		[Description("ExpandFolder")]
		public string ExpandFolder { get; set; }

		[Browsable(true)]
		[Description("IsDirty")]
		public bool IsDirty { get; set; }

		[Browsable(true)]
		[Description("Container")]
		public ePUBContainer Container { get; set; }

        public List<ePUBNavPoint> TocShortcut
        {
            get { return Container.Package.TableOfContent.Items.ToList(); }
        }
		#endregion

		#region -----------------METHODS-----------------

		public string GetContentFile(ePUBNavPoint point)
		{
			string result = GetRoot();
			result = Path.Combine(result, point.Content);
			return result;
		}

        public string GetCoverFile()
		{
			string result = GetRoot();

			try
			{
				// if metadata identify a cover item
				if (Container.Package.Metadata.Meta.Count(p => p.Name == ePUBHelper.XmlAttributes.meta_cover) > 0)
				{
					ePUBMetaItem meta = Container.Package.Metadata.Meta.Where(p => p.Name == ePUBHelper.XmlAttributes.meta_cover).First();

					result = Path.Combine(result,
						Container.Package.Manifest.Items.Where(p => p.MediaType.Contains(ePUBHelper.XmlMediaTypes.Images)
								&& p.Id == meta.Value).First().hRefForPath);

					return result;
				}

				//search any item with "cover" and image type
				{
					IEnumerable<ePUBManifestItem> coverImages = Container.Package.Manifest.Items.Where(
						p => p.MediaType.Contains(ePUBHelper.XmlMediaTypes.Images) );
		            if (coverImages.Count() > 1)
		            {
						foreach (ePUBManifestItem item in coverImages)
						{
							if (item.hRef.Contains("cover") || item.Id.Contains("cover"))
							{
								result = Path.Combine(result, item.hRefForPath);
								return result;
							}
						}
		            }
				}
				// take the first image
				{
					result = Path.Combine(result,
						Container.Package.Manifest.Items.Where(p => p.MediaType.Contains(ePUBHelper.XmlMediaTypes.Images)).First().hRefForPath);

					return result;
				}
			}
			catch (System.Exception)
			{
				return null;
			}
		}

        public string GetTOCFile()
		{
			string result = GetRoot();

			result = Path.Combine(result,
				Container.Package.Manifest.Items.Where(p => p.MediaType == ePUBHelper.XmlMediaTypes.NcxToc).First().hRefForPath);
			
			return result;
		}

		public string GetRoot()
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(ExpandFolder))
				result = Path.Combine(ExpandFolder, Container.Package.RelativPackageFolder);
			else
				result = Container.Package.RelativPackageFolder;

			return result;
		}

		public string GetContent(string href)
		{
			return File.ReadAllText(
				System.IO.Path.Combine(GetRoot(), href), Encoding.UTF8);
		}

		public string GetDocumentPath(string href)
		{
			return System.IO.Path.Combine(GetRoot(), href);
		}

		#endregion
	}
}
