using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;

namespace CBR.Core.Models
{
    /// <summary>
    /// Information to display in recent file list (MRU)
    /// </summary>
    public class RecentFileInfo
    {
        /// <summary>
        /// the folder
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// the file name with extension
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Should it allways stay in the list ?
        /// </summary>
        public bool IsPined { get; set; }
        /// <summary>
        /// last time access
        /// </summary>
        public DateTime LastAccess { get; set; }
    }
}
