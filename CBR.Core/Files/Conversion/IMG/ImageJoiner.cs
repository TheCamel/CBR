using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using CBR.Core.Helpers;

namespace CBR.Core.Files.Conversion
{
	internal class ImageJoiner
	{
		/** the byte array of the extracted images */
		private List<byte[]> _ImageBytes = new List<byte[]>();
		public List<byte[]> NewImageBytes
		{
			get { return _ImageBytes; }
		}
		/** the file names of the extracted images */
		private List<string> _imageNames = new List<string>();
		public List<string> NewImageNames
		{
			get { return _imageNames; }
		}

		/// <summary>
		/// if needed, will merge the images base on page in the image names
		/// </summary>
		/// <param name="imageBytes"></param>
		/// <param name="imageNames"></param>
		/// <returns></returns>
		public bool Merge(List<byte[]> imageBytes, List<string> imageNames)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("ImageJoiner.Merge");
			try
			{
				int startIndex, endIndex, pageIndex;

				//group by page index
				startIndex = endIndex = 0;
				pageIndex = 1;
				for (int nameIndex = 0; nameIndex < imageNames.Count; nameIndex++)
				{
					string temp = imageNames[nameIndex].Substring( 0, 4 );
					if (Convert.ToInt32(temp) == pageIndex )
						endIndex = nameIndex;
					else
					{
						MergeGroup(imageBytes, imageNames, startIndex, endIndex, pageIndex);
						startIndex = endIndex = nameIndex;
						pageIndex++;
					}
				}

				//last range
				MergeGroup(imageBytes, imageNames, startIndex, endIndex, pageIndex);
			}
			catch (Exception err)
			{
				LogHelper.Manage("ImageJoiner.Merge", err);
			}
			finally
			{
				LogHelper.End("ImageJoiner.Merge");
			}  
			return true;
		}

		/// <summary>
		/// will merge a group of images into one
		/// </summary>
		/// <param name="imageBytes"></param>
		/// <param name="imageNames"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="index"></param>
		public void MergeGroup(List<byte[]> imageBytes, List<string> imageNames, int start, int end, int index)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("ImageJoiner.MergeGroup");
			try
			{
				List<BitmapImage> bmps = new List<BitmapImage>();
				double maxWidth = 0, maxHeight = 0, position = 0;

				for (int i = start; i <= end; i++)
				{
					MemoryStream ms = new MemoryStream(imageBytes[i]);
					BitmapImage myImage = new BitmapImage();
					myImage.BeginInit();
					myImage.StreamSource = ms;
					myImage.CacheOption = BitmapCacheOption.None;
					myImage.EndInit();

					bmps.Add(myImage);
					maxWidth = Math.Max(myImage.Width, maxWidth);
					maxHeight += myImage.Height;
				}

				RenderTargetBitmap temp = new RenderTargetBitmap((int)maxWidth, (int)maxHeight, 96d, 96d, PixelFormats.Pbgra32);

				DrawingVisual dv = new DrawingVisual();
				using (DrawingContext ctx = dv.RenderOpen())
				{
					foreach (BitmapImage bi in bmps)
					{
						ctx.DrawImage(bi, new System.Windows.Rect(0, position, bi.Width, bi.Height));
						position += bi.Height;
					}
					ctx.Close();
				}

				temp.Render(dv);

				NewImageNames.Add(string.Format("{0:0000}.jpg", index));
				NewImageBytes.Add(StreamToImage.BufferFromImage(temp));

				bmps.Clear();
			}
			catch (Exception err)
			{
				LogHelper.Manage("ImageJoiner.MergeGroup", err);
			}
			finally
			{
				LogHelper.End("ImageJoiner.MergeGroup");
			}  
		}
	}
}
