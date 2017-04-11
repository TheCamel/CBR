using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using CBR.Core.Helpers;
using SevenZip;

namespace CBR.Core.Formats.ePUB
{
    internal class ePUBManager
	{
		#region -----------------PUBLICS-----------------
		
		public ePUB ParseExtracted(string filePath, string extractFolder)
        {
            try
            {
				ePUB docPUB = new ePUB(filePath, extractFolder);

                //read META-INF container.xml
                ParseContainer(docPUB);

                // read content.OPF file
				ParsePackage(docPUB);

                // parse the table of content
				ParseTOC(docPUB);

                return docPUB;
            }
            catch (Exception err)
            {
				LogHelper.Manage("ePUBManager:ParseExtracted", err);
                return null;
            }
        }

		#endregion

		public ePUB ParseFile(string filePath)
		{
			SevenZipExtractor temp = null;
			try
			{
				ePUB docPUB = new ePUB(filePath);

				temp = ZipHelper.Instance.GetExtractor(filePath);

				// find container.xml and parse it
				ArchiveFileInfo fil = temp.ArchiveFileData.Where(p => !p.IsDirectory && p.FileName == ePUBHelper.Files.ContainerRelativFile).First();
				using (MemoryStream stream = new MemoryStream())
				{
					temp.ExtractFile(fil.FileName, stream);
					ParseContainer(docPUB, stream);
				}

				// find OPF package file and parse it
				fil = temp.ArchiveFileData.Where(p => !p.IsDirectory && p.FileName == docPUB.Container.Package.SysRelativFilePath).First();
				using (MemoryStream stream = new MemoryStream())
				{
					temp.ExtractFile(fil.FileName, stream);
					ParsePackage(docPUB, stream);
				}

				// find the toc file and parse it
				fil = temp.ArchiveFileData.Where(p => !p.IsDirectory && p.FileName == docPUB.GetTOCFile()).First();
				using (MemoryStream stream = new MemoryStream())
				{
					temp.ExtractFile(fil.FileName, stream);
					ParseTOC(docPUB, stream);
				}

				return docPUB;
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ParseFile", err);
				return null;
			}
			finally
			{
				ZipHelper.Instance.ReleaseExtractor(temp);
			}
		}

		public ePUB ParseFileForCoverOnly(string filePath)
		{
			SevenZipExtractor temp = null;
			try
			{
				ePUB docPUB = new ePUB(filePath);

				temp = ZipHelper.Instance.GetExtractor(filePath);

				// find container.xml and parse it
				ArchiveFileInfo fil = temp.ArchiveFileData.Where(p => !p.IsDirectory && p.FileName == ePUBHelper.Files.ContainerRelativFile).First();
				using (MemoryStream stream = new MemoryStream())
				{
					temp.ExtractFile(fil.FileName, stream);
					ParseContainer(docPUB, stream);
				}

				// find OPF package file and parse it
				fil = temp.ArchiveFileData.Where(p => !p.IsDirectory && p.FileName == docPUB.Container.Package.SysRelativFilePath).First();
				using (MemoryStream stream = new MemoryStream())
				{
					temp.ExtractFile(fil.FileName, stream);
					ParsePackage(docPUB, stream);
				}

				return docPUB;
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ParseFile", err);
				return null;
			}
			finally
			{
				ZipHelper.Instance.ReleaseExtractor(temp);
			}
		}

		#region -----------------INTERNALS-----------------

		#region -----------------container-----------------
		
		private void ParseContainer(ePUB docPUB, Stream content)
		{
			try
			{
				XmlDocument doc = GetDocumentWithNoValidation(content);

				XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
				ResolveNamespaces(nsmgr, doc.DocumentElement);

				XmlNode rootfileNode = doc.SelectSingleNode("//ROOT:rootfiles/ROOT:rootfile", nsmgr);

				docPUB.Container = new ePUBContainer();
				docPUB.Container.Package = new ePUBPackage(TryGetAttributeString(rootfileNode, ePUBHelper.XmlAttributes.container_full_path));
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ParseContainer", err);
			}
		}

        private void ParseContainer(ePUB docPUB)
        {
            try
            {
				using (FileStream fs = File.Open(Path.Combine(docPUB.ExpandFolder, ePUBHelper.Files.ContainerRelativFile), FileMode.Open, FileAccess.Read))
                {
                    ParseContainer(docPUB, fs);
                }
            }
            catch (Exception err)
            {
				LogHelper.Manage("ePUBManager:ParseContainer", err);
            }
        }

		#endregion

		#region -----------------package-----------------

		private void ParsePackage(ePUB docPUB, Stream content)
		{
			try
			{
				//reading the OPF
				XmlDocument doc = GetDocumentWithNoValidation(content);

				XmlNode root = doc.DocumentElement; //<package>

				// resolve <package>
				XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
				ResolveNamespaces(nsmgr, root);

				// resolve <metadata>
				ResolveNamespaces(nsmgr, root.SelectSingleNode("//ROOT:metadata", nsmgr));

				//shortcut
				ePUBPackage pack = docPUB.Container.Package;

				pack.Title = root.SelectSingleNode("//ROOT:metadata/dc:title", nsmgr).InnerText;
				pack.Identifier = root.SelectSingleNode("//ROOT:metadata/dc:identifier", nsmgr).InnerText;

				////try get the cover id
				//XmlNode xmlNod = root.SelectSingleNode("//ROOT:metadata/ROOT:meta[@name='cover']", nsmgr);
				//pack.CoverId = TryGetAttributeString(xmlNod, "content");

				pack.Metadata = new ePUBMetadata();

				//read the meta list
				XmlNodeList listNode = root.SelectNodes("//ROOT:metadata/ROOT:meta", nsmgr);
				foreach (XmlNode node in listNode)
				{
					pack.Metadata.Meta.Add(new ePUBMetaItem(node.Attributes["name"].Value, node.Attributes["content"].Value));
				}
				listNode = root.SelectNodes("//ROOT:metadata/dc:*", nsmgr);
				foreach (XmlNode node in listNode)
				{
					ePUBMetaDcItem item = new ePUBMetaDcItem(node.LocalName, node.InnerText);
					
					foreach (XmlAttribute attrib in node.Attributes)
					{
						if( attrib.Prefix == "opf")
							item.SetOpfAttribute(attrib.LocalName, attrib.Value);
						else
							item.SetAttribute(attrib.LocalName, attrib.Value);
					}
					pack.Metadata.MetaDC.Add(item);
				}

				//read manifest items
				listNode = root.SelectNodes("//ROOT:manifest/ROOT:item", nsmgr);

				foreach (XmlNode node in listNode)
				{
					pack.Manifest.Items.Add(new ePUBManifestItem()
					{
						Id = TryGetAttributeString(node, "id"),
						hRef = TryGetAttributeString(node, "href"),
                        hRefForPath = TryGetAttributeString(node, "href").Replace("/", "\\"),
                        XamlId = "XamlBody" + pack.Manifest.Items.Count(),
						MediaType = TryGetAttributeString(node, "media-type")
					});
				}

				//read <spine>
				XmlNode xmlNode = root.SelectSingleNode("//ROOT:spine", nsmgr);
				pack.Spine.TableOfContentId = TryGetAttributeString(xmlNode, "toc");

				listNode = root.SelectNodes("//ROOT:spine/ROOT:itemref", nsmgr);
				foreach (XmlNode node in listNode)
				{
					ePUBSpineItem item = new ePUBSpineItem(TryGetAttributeString(node, "idref"), TryGetAttributeString(node, "linear"));
					pack.Spine.Items.Add( item );
				}

				//read <guide>
				listNode = root.SelectNodes("//ROOT:guide/ROOT:reference", nsmgr);
				foreach (XmlNode node in listNode)
				{
					ePUBGuideItem item = new ePUBGuideItem()
					{
						hRef = TryGetAttributeString(node, "href"),
						Type = TryGetAttributeString(node, "type"),
						Title = TryGetAttributeString(node, "title")
					};
					pack.Guide.Items.Add(item);
				}

				////if no cover search for an image
				//if (string.IsNullOrEmpty(pack.CoverId) || pack.ManifestItems.Count(p => p.Id == pack.CoverId) <= 0)
				//{
				//    IEnumerable<ManifestItem> img = pack.ManifestItems.Where(p => p.MediaType == "image/jpeg");
				//    if (img.Count() > 1)
				//    {
				//        foreach (ManifestItem item in img)
				//        {
				//            if (item.Id.Contains("cover") || item.Reference.Contains("cover"))
				//            {
				//                pack.CoverId = item.Id;
				//                break;
				//            }
				//        }
				//    }
				//    else
				//    {
				//        foreach (ManifestItem item in img)
				//        {
				//            pack.CoverId = item.Id;
				//            break;
				//        }
				//    }

				//}

				
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ParsePackage", err);
			}
		}

		private void ParsePackage(ePUB docPUB)
		{
			try
			{
				using (FileStream fs = File.Open(Path.Combine(docPUB.ExpandFolder, docPUB.Container.Package.SysRelativFilePath), FileMode.Open, FileAccess.Read))
				{
					ParsePackage(docPUB, fs);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ParsePackage", err);
			}
		}

		#endregion

		#region -----------------toc-----------------

		private void ParseTOC(ePUB docPUB, Stream content)
		{
			try
			{
				//reading 
				XmlDocument doc = GetDocumentWithNoValidation(content);

				XmlNode root = doc.DocumentElement; //<ncx>

				// resolve <ncx>
				XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
				ResolveNamespaces(nsmgr, root);

				ePUBTableNCX toc = docPUB.Container.Package.TableOfContent;
				toc.Title = root.SelectSingleNode("//ROOT:docTitle/ROOT:text", nsmgr).InnerText;

				XmlNodeList listNode;
				listNode = root.SelectNodes("child::ROOT:navMap/ROOT:navPoint", nsmgr);

				List<ePUBNavPoint> navMapList = new List<ePUBNavPoint>();
				foreach (XmlNode navPoint in listNode)
				{
					ePUBNavPoint item = new ePUBNavPoint()
					{
						Id = TryGetAttributeString(navPoint, "id"),
						PlayOrder = TryGetAttributeInt(navPoint, "playOrder"),
						Label = navPoint.SelectSingleNode("child::ROOT:navLabel/ROOT:text", nsmgr).InnerText,
						Content = TryGetAttributeString(navPoint.SelectSingleNode("child::ROOT:content", nsmgr), "src")
                    };
                    string[] parts = item.Content.Split('#');
                    item.PageSource = parts[0];
                    if (parts.Count() == 2)
                        item.TargetId = parts[2];
                    item.XamlId = docPUB.Container.Package.Manifest.Items.Single(p => p.hRef == item.PageSource).XamlId;

                    //is there any bookmark?
                    if (item.Content.IndexOf('#')!=-1)
						item.Content = item.Content.Split('#')[1];

					navMapList.Add(item);

					item.Items = ParseNavRecursif(docPUB, navPoint, nsmgr);
				}
				toc.Items = navMapList;
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ParseTOC", err);
			}
		}

		private void ParseTOC(ePUB docPUB)
		{
			try
			{
				using (FileStream fs = File.Open(docPUB.GetTOCFile(), FileMode.Open, FileAccess.Read))
				{
					ParseTOC(docPUB, fs);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ParseTOC", err);
			}
		}

		private List<ePUBNavPoint> ParseNavRecursif(ePUB docPUB, XmlNode node, XmlNamespaceManager nsmgr)
		{
			try
			{
				XmlNodeList listNode = node.SelectNodes("child::ROOT:navPoint", nsmgr);

				List<ePUBNavPoint> navMapList = new List<ePUBNavPoint>();
				foreach (XmlNode navPoint in listNode)
				{
					ePUBNavPoint item = new ePUBNavPoint()
					{
						Id = TryGetAttributeString(navPoint, "id"),
						PlayOrder = TryGetAttributeInt(navPoint, "playOrder"),
						Label = navPoint.SelectSingleNode("child::ROOT:navLabel/ROOT:text", nsmgr).InnerText,
						Content = TryGetAttributeString(navPoint.SelectSingleNode("child::ROOT:content", nsmgr), "src")
					};
					
					//is there any bookmark?
					if (item.Content.IndexOf('#') != -1)
                        item.Content = item.Content.Split('#')[1];

                    navMapList.Add(item);

					item.Items = ParseNavRecursif(docPUB, navPoint, nsmgr);
				}

				return navMapList;
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ParseNavRecursif", err);
				return null;
			}
		}

		#endregion

		#endregion

		#region -----------------XML HELPERS-----------------

		private XmlDocument GetDocumentWithNoValidation(Stream content)
		{
			try
			{
				content.Position = 0;

				XmlDocument doc = new XmlDocument();

				XmlReaderSettings settings = new XmlReaderSettings();
				settings.DtdProcessing = DtdProcessing.Ignore;
				using (XmlReader reader = XmlReader.Create(content, settings))
				{
					doc.Load(reader);
				}

				return doc;
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:GetDocumentWithNoValidation", err);
				return null;
			}
		}

		private XmlDocument GetDocumentWithNoValidation(string filePath)
		{
			try
			{
				using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
				{
					return GetDocumentWithNoValidation(fs);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:GetDocumentWithNoValidation", err);
				return null;
			}
		}

		private void ResolveNamespaces(XmlNamespaceManager xmlManager, XmlNode xmlElement)
		{
			try
			{
				ResolveNamespace(xmlManager, xmlElement, "xmlns", "ROOT");
				ResolveNamespace(xmlManager, xmlElement, "xmlns:opf", "opf");
				ResolveNamespace(xmlManager, xmlElement, "xmlns:dc", "dc");
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ResolveNamespaces", err);
			}
		}

		private void ResolveNamespace(XmlNamespaceManager xmlManager, XmlNode xmlElement, string xmlNamespace, string xmlTag)
		{
			try
			{
				// Create an XmlNamespaceManager to resolve the default namespace
				if (xmlElement.Attributes[xmlNamespace] != null)
				{
					string xmlns = xmlElement.Attributes[xmlNamespace].Value;
					xmlManager.AddNamespace(xmlTag, xmlns);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ResolveNamespace", err);
			}
		}

		private XmlAttribute TryGetAttribute(XmlNode node, string attributeName)
		{
			try
			{
				return node.Attributes[attributeName];
			}
			catch 
			{
				return null;
			}
		}

		private string TryGetAttributeString(XmlNode node, string attributeName)
		{
			try
			{
				string result = node.Attributes[attributeName].Value;
				if (result.IndexOf("%") != -1)
					result = Uri.UnescapeDataString (result);

				return result;
			}
			catch
			{
				return string.Empty;
			}
		}

		private int TryGetAttributeInt(XmlNode node, string attributeName)
		{
			try
			{
				return Convert.ToInt32( node.Attributes[attributeName].Value );
			}
			catch
			{
				return Int32.MinValue;
			}
		}
		
		#endregion

		#region -----------------WRITING-----------------

		public void Write(ePUB docPUB)
		{
			try
			{
				//write ePUB is nothing

				//write mime-type
				using (StreamWriter outfile = new StreamWriter(Path.Combine(docPUB.ExpandFolder, ePUBHelper.Files.Mime)))
				{
					outfile.Write(ePUBHelper.XmlMediaTypes.ePUB);
				}

				//write META-INF//container.xml
				WriteContainer(docPUB);

				// write content.OPF file
				WritePackage(docPUB);

				// parse the table of content
				//ParseTOC(docPUB);
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBManager:ParseFolder", err);
			}
		}

		public bool Check(ePUB docPUB)
		{
			return true;
		}

		public void Compile(ePUB docPUB)
		{
			//write
			Write(docPUB);

			//zip it
			Compress(docPUB);
		}

		public void Compress(ePUB docPUB)
		{
		}

		public void WriteContainer(ePUB docPUB)
		{
			string path = Path.Combine(docPUB.ExpandFolder, ePUBHelper.Files.ContainerFolder);

			//check or create metainf
			DirectoryHelper.Check(path);

			//write container.xml
			using (StreamWriter outfile = new StreamWriter(Path.Combine(path, ePUBHelper.Files.ContainerFile)))
			{
				outfile.WriteLine("<?xml version=\"1.0\"?>");
				outfile.WriteLine("<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">");
				outfile.WriteLine("<rootfiles>");
				outfile.WriteLine(string.Format("<rootfile full-path=\"{0}/{1}\"",
					docPUB.Container.Package.RelativPackageFolder, docPUB.Container.Package.PackageFileName));
				outfile.WriteLine("media-type=\"application/oebps-package+xml\" />");
				outfile.WriteLine("</rootfiles>");
				outfile.WriteLine("</container>");
			}
		}

		public void WritePackage(ePUB docPUB)
		{
		}

		#endregion
	}
}
