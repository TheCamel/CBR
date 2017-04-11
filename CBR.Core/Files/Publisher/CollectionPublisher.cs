using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Data;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace CBR.Core.Files.Publisher
{
	public class CollectionPublisher
	{
		public string GroupBy { get; set; }
		public string SortBy { get; set; }
		public List<string> Columns { get; set; }
		public string FileOutput { get; set; }

		public ICollection DataCollection { get; set; }

		public bool Publish(string templateFile)
		{
			ICollectionView cv = CollectionViewSource.GetDefaultView(DataCollection);
			cv.SortDescriptions.Add(new SortDescription(SortBy, ListSortDirection.Descending));
			cv.GroupDescriptions.Add(new PropertyGroupDescription(GroupBy));
			cv.Refresh();
			cv.MoveCurrentToFirst();

			List<PropertyInfo> lstProps = new List<PropertyInfo>();
			Type tp = cv.CurrentItem.GetType();

			DataTable table = new DataTable("VALUES");
			
			foreach (string prop in Columns)
			{
				PropertyInfo pi = tp.GetProperty(prop);
				lstProps.Add( pi );
				table.Columns.Add(prop, pi.PropertyType);
			}

			foreach (object obj in cv)
			{
				DataRow dr = table.NewRow();
				table.Rows.Add(dr);

				foreach (PropertyInfo prop in lstProps)
				{
					dr[prop.Name] = prop.GetValue(obj, null);
				}
			}
			

			HtmlPublisher hp = new HtmlPublisher();
			hp.Initialize(templateFile, FileOutput, table);

			return true;
		}
	}
}
