using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using CBR.ViewModels;

namespace CBR.Components.Selectors
{
    public class SysObjectItemStyleSelector : StyleSelector
    {
        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is SysDriveViewModel)
                return this.DriveStyle;
            else if (item is SysDirectoryViewModel)
                return this.DirectoryStyle;
            else if (item is SysFileViewModel)
                return this.FileStyle;

            return base.SelectStyle(item, container);
        }

        public Style DirectoryStyle
        {
            get;
            set;
        }
        public Style FileStyle
        {
            get;
            set;
        }
        public Style DriveStyle
        {
            get;
            set;
        }
    }
}

