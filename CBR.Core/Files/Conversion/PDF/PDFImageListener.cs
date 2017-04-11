using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace CBR.Core.Files.Conversion
{
	internal class PDFImageListener : IRenderListener
	{
		/** the byte array of the extracted images */
		private List<byte[]> _ImageBytes;
		public List<byte[]> ImageBytes
		{
			get { return _ImageBytes; }
		}
		/** the file names of the extracted images */
		private List<string> _imageNames;
		public List<string> ImageNames
		{
			get { return _imageNames; }
		}

		public int PageIndex { get; set; }

		// ---------------------------------------------------------------------------
		/**
		 * Creates a RenderListener that will look for images.
		 */
		public PDFImageListener()
		{
			_ImageBytes = new List<byte[]>();
			_imageNames = new List<string>();
			//_images = new List<Image>();
		}
		// ---------------------------------------------------------------------------
		/**
		 * @see com.itextpdf.text.pdf.parser.RenderListener#beginTextBlock()
		 */
		public void BeginTextBlock() { }
		// ---------------------------------------------------------------------------     
		/**
		 * @see com.itextpdf.text.pdf.parser.RenderListener#endTextBlock()
		 */
		public void EndTextBlock() { }
		// ---------------------------------------------------------------------------     
		/**
		 * @see com.itextpdf.text.pdf.parser.RenderListener#renderImage(
		 *     com.itextpdf.text.pdf.parser.ImageRenderInfo)
		 */
		public void RenderImage(ImageRenderInfo renderInfo)
		{
			PdfImageObject image = renderInfo.GetImage();
			//PdfName filter = (PdfName)image.Get(PdfName.FILTER);

			_imageNames.Add(string.Format("{0:0000}_{1:0000}.{2}", PageIndex, _imageNames.Count, image.GetImageBytesType().FileExtension));
			_ImageBytes.Add(image.GetImageAsBytes());
		}
		// ---------------------------------------------------------------------------     
		/**
		  * @see com.itextpdf.text.pdf.parser.RenderListener#renderText(
		  *     com.itextpdf.text.pdf.parser.TextRenderInfo)
		  */
		public void RenderText(TextRenderInfo renderInfo) { }

	}
}
