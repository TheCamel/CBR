using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace CBR.Core.Files.Publisher
{
	public class HtmlPublisher : IPublisher
	{
		const string _tag_title = "<%TAG_TITLE%>";
		const string _tag_itemheader = "<%TAG_ITEMHEADER%>";
		const string _tag_itemdata = "<%TAG_ITEMDATA%>";
		const string _tag_item = "<%TAG_ITEM%>";
		StreamWriter _OutputStream;

		public void Initialize(string templateFilePath, string outputFile, DataTable data)
		{
			_OutputStream = File.CreateText(outputFile);

			ParseTemplate(templateFilePath, _OutputStream, data);

			_OutputStream.Flush();
			_OutputStream.Close();
		}

		private void ParseTemplate(string templateFilePath, StreamWriter outputStream, DataTable data)
		{
			using (StreamReader sr = File.OpenText(templateFilePath))
			{
				string temp;
				do
				{
					temp = sr.ReadLine();

					if (temp.Contains(_tag_title))
					{
						outputStream.WriteLine(temp.Replace(_tag_title, "Title"));
					}
					else if (temp.Contains(_tag_itemheader))
					{
						foreach (DataColumn col in data.Columns)
						{
							_OutputStream.WriteLine( temp.Replace(_tag_itemheader, col.ColumnName) );
						}
					}
					else if (temp.Contains(_tag_item))
					{
						//take the start
						string linestart = temp.Replace(_tag_item, "");

						temp = sr.ReadLine();
						string item = temp;
						
						temp = sr.ReadLine();
						string lineend = temp.Replace(_tag_item, "");

						//if (line.Contains(_tag_itemdata))
						{
							foreach (DataRow dr in data.Rows)
							{
								_OutputStream.WriteLine(linestart);
								foreach (DataColumn col in data.Columns)
								{
									_OutputStream.WriteLine( item.Replace(_tag_itemdata, dr[col.ColumnName].ToString()) );
								}
								_OutputStream.WriteLine(lineend);
							}
						}
					}
					else
						outputStream.WriteLine(temp);

				}
				while (!string.IsNullOrEmpty(temp));
			}
		}

	}
}
