using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CBR.Core.Formats.ePUB
{
    public class ePUBHelper
    {
        public class Files
        {
            public const string Mime = "mimetype";

			public const string ContainerRelativFile = "META-INF\\container.xml";
			public const string ContainerFolder = "META-INF";
			public const string ContainerFile = "container.xml";

			public const string PackageRelativFile = "OEBPS\\content.opf";
			public const string PackageFolder = "OEBPS";
			public const string PackageFile = "content.opf";

            public const string NcxToc = "toc.ncx";
        }

        public class XmlMediaTypes
        {
            public const string ePUB = "application/epub+zip";
            public const string OEBPSPackage = "application/oebps-package+xml";
            public const string Content = "application/xhtml+xml";
            public const string Css = "text/css";
            public const string NcxToc = "application/x-dtbncx+xml";
            public const string Images = "image/";
        }

        public class XmlNamespaces
        {
            public const string MetaDC = "http://purl.org/dc/elements/1.1/";
            public const string MetaOPF = "http://www.idpf.org/2007/opf";
            public const string NcxToc = "http://www.daisy.org/z3986/2005/ncx/";
            public const string Opf = "http://www.idpf.org/2007/opf";


        }

        public class XmlAttributes
        {
            public const string container_full_path = "full-path";

            public const string meta_identifier = "identifier";
            public const string meta_title = "title";
            public const string meta_language = "language";
            public const string meta_creator = "creator";
            public const string meta_contributor = "contributor";
            public const string meta_publisher = "publisher";
            public const string meta_subject = "subject";
            public const string meta_description = "description";
            public const string meta_date = "date";
            public const string meta_type = "type";
            public const string meta_format = "format";
            public const string meta_source = "source";
            public const string meta_relation = "relation";
            public const string meta_coverage = "coverage";
            public const string meta_rights = "rights";

			public const string meta_cover = "cover";
        }
    }
}
