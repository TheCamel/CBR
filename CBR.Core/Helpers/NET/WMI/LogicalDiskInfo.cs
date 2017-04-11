using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CBR.Core.Helpers
{
    public class LogicalDiskInfo
    {
        public string Caption { get; set; }
        public string Name { get; set; }
        public string VolumeLabel { get; set; }
        public long AvailableFreeSpace { get; set; }
        public string DriveFormat { get; set; }
        public DriveType DriveType { get; set; }
        public long TotalSize { get; set; }

        public string PNPDeviceID { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Path { get; set; }
    }
}
