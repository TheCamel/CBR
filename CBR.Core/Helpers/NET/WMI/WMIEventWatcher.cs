using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace CBR.Core.Helpers
{
    #region ----------------EVENTS----------------

    public enum WMIActions
    {
        Added,
        Removed
    }

    public class WMIEventArgs : EventArgs
    {
        public LogicalDiskInfo Disk { get; set; }
        public WMIActions EventType { get; set; }
    }

    public delegate void WMIEventArrived(object sender, WMIEventArgs e);

    #endregion

    /// <summary>
    /// clas that watch over wmi event to register to usb compliant devices
    /// </summary>
    public class WMIEventWatcher
    {
        #region ----------------CONSTRUCTOR----------------

        /// <summary>
        /// constructor
        /// </summary>
        public WMIEventWatcher()
        {
            Devices = GetExistingDevices();
        }

        #endregion

        #region ----------------INTERNALS----------------

        /// <summary>
        /// internal watcher for add event
        /// </summary>
        private ManagementEventWatcher addedWatcher = null;

        /// <summary>
        /// internal watcher for remove event
        /// </summary>
        private ManagementEventWatcher removedWatcher = null;

        #endregion

        #region ----------------PROPERTIES----------------

        /// <summary>
        /// List of founded devices
        /// </summary>
        public List<LogicalDiskInfo> Devices { get; set; }

        #endregion

        #region ----------------EVENTS----------------

        /// <summary>
        /// signaled when watched events occurs
        /// </summary>
        public event WMIEventArrived EventArrived;

        #endregion

        #region ----------------METHODS----------------

        /// <summary>
        /// start watching for logical device events
        /// </summary>
        public void StartWatchUSB()
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("WMIEventWatcher.StartWatchUSB");
			try
			{
                addedWatcher = new ManagementEventWatcher("SELECT * FROM __InstanceCreationEvent WITHIN 5 WHERE TargetInstance ISA \"Win32_USBControllerDevice\"");
                addedWatcher.EventArrived += new EventArrivedEventHandler(HandleAddedEvent);
                addedWatcher.Start();

                removedWatcher = new ManagementEventWatcher("SELECT * FROM __InstanceDeletionEvent WITHIN 5 WHERE TargetInstance ISA \"Win32_USBControllerDevice\"");
                removedWatcher.EventArrived += new EventArrivedEventHandler(HandleRemovedEvent);
                removedWatcher.Start();
			}
			catch (Exception err)
			{
				LogHelper.Manage("WMIEventWatcher.StartWatchUSB", err);
			}
			finally
			{
				LogHelper.End("WMIEventWatcher.StartWatchUSB");
			}  
        }

        /// <summary>
        /// stop watching for logical device events
        /// </summary>
        public void StopWatchUSB()
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("WMIEventWatcher.StopWatchUSB");
			try
			{
                // Stop listening for events
                if (addedWatcher != null)
                    addedWatcher.Stop();

                if (removedWatcher != null)
                    removedWatcher.Stop();
			}
			catch (Exception err)
			{
				LogHelper.Manage("WMIEventWatcher.StopWatchUSB", err);
			}
			finally
			{
				LogHelper.End("WMIEventWatcher.StopWatchUSB");
			}  
        }

        /// <summary>
        /// handle the add event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleAddedEvent(object sender, EventArrivedEventArgs e)
        {
            LogicalDiskInfo device = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("WMIEventWatcher.HandleAddedEvent");
			
            try
            {
                ManagementBaseObject targetInstance = e.NewEvent["TargetInstance"] as ManagementBaseObject;
#if DEBUG
                DebugPrint(targetInstance);
#endif
                // get the device name in the dependent property Dependent: extract the last part
                // \\FR-L25676\root\cimv2:Win32_PnPEntity.DeviceID="USBSTOR\\DISK&VEN_USB&PROD_FLASH_DISK&REV_1100\\AA04012700076941&0"
                string PNP_deviceID = Convert.ToString(targetInstance["Dependent"]).Split('=').Last().Replace("\"", "").Replace("\\", "");
                string device_name = Convert.ToString(targetInstance["Dependent"]).Split('\\').Last().Replace("\"", "");

                // query that device entity
                ObjectQuery query = new ObjectQuery(string.Format("Select * from Win32_PnPEntity Where DeviceID like \"%{0}%\"", device_name));

                // check if match usb removable disk
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    ManagementObjectCollection entities = searcher.Get();

                    //first loop to check USBSTOR
                    foreach (var entity in entities)
                    {
                        string service = Convert.ToString(entity["Service"]);
                        
                        if (service == "USBSTOR")
                            device = new LogicalDiskInfo();
                    }

                    if (device != null)
                    {
                        foreach (var entity in entities)
                        {
                            string service = Convert.ToString(entity["Service"]);
                            if (service == "disk")
                            {
                                GetDiskInformation(device, device_name);

                                Devices.Add(device);
                                if (EventArrived != null)
                                    EventArrived(this, new WMIEventArgs() { Disk = device, EventType = WMIActions.Added });
                            }
                        }
                    }
                }
            }
			catch (Exception err)
			{
				LogHelper.Manage("WMIEventWatcher.HandleAddedEvent", err);
			}
			finally
			{
				LogHelper.End("WMIEventWatcher.HandleAddedEvent");
			}  
        }

        private ManagementObjectSearcher GetDiskInformation(LogicalDiskInfo device, string device_name)
        {
            ManagementObjectSearcher searcher2 = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("WMIEventWatcher.GetDiskInformation");
			try
			{
				ObjectQuery query1 = new ObjectQuery("SELECT * FROM Win32_DiskDrive where PNPDeviceID like '%" + device_name + "%'");
				searcher2 = new ManagementObjectSearcher(query1);

				ManagementObjectCollection disks = searcher2.Get();
				foreach (var disk in disks)
				{
#if DEBUG
                DebugPrint(disk);
#endif
					//Use the disk drive device id to find associated partition
					ObjectQuery query2 = new ObjectQuery("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + Convert.ToString(disk["DeviceID"]).Replace("\"", "\\") + "'} WHERE AssocClass=Win32_DiskDriveToDiskPartition");
					searcher2 = new ManagementObjectSearcher(query2);

					ManagementObjectCollection partitions = searcher2.Get();
					foreach (var part in partitions)
					{
#if DEBUG
                    DebugPrint(part);
#endif
						//Use partition device id to find logical disk
						ObjectQuery query3 = new ObjectQuery("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + Convert.ToString(part["DeviceID"]).Replace("\"", "\\") + "'} WHERE AssocClass=Win32_LogicalDiskToPartition");
						searcher2 = new ManagementObjectSearcher(query3);

						ManagementObjectCollection logic = searcher2.Get();
						foreach (var disklogic in logic)
						{
#if DEBUG
                        DebugPrint(disklogic);
#endif
							ParseDiskDriveInfo(device, disk);
							ParseDiskLogicalInfo(device, disklogic);
						}
					}
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("WMIEventWatcher.GetDiskInformation", err);
			}
			finally
			{
				LogHelper.End("WMIEventWatcher.GetDiskInformation");
			}  
            
            return searcher2;
        }

        /// <summary>
        /// handle the remove event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleRemovedEvent(object sender, EventArrivedEventArgs e)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("WMIEventWatcher.HandleRemovedEvent");
			try
			{
				ManagementBaseObject targetInstance = e.NewEvent["TargetInstance"] as ManagementBaseObject;
#if DEBUG
                DebugPrint(targetInstance);
#endif
				string PNP_deviceID = Convert.ToString(targetInstance["Dependent"]).Split('=').Last().Replace("\"", "").Replace("\\", "");

                LogicalDiskInfo device = Devices.Find(x => x.PNPDeviceID == PNP_deviceID);
				if (device != null)
				{
					Devices.Remove(device);
					if (EventArrived != null)
						EventArrived(this, new WMIEventArgs() { Disk = device, EventType = WMIActions.Removed });
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("WMIEventWatcher.HandleRemovedEvent", err);
			}
			finally
			{
				LogHelper.End("WMIEventWatcher.HandleRemovedEvent");
			}  
        }

        /// <summary>
        /// when starting, get a list of all existing logical disk
        /// </summary>
        /// <returns></returns>
        private List<LogicalDiskInfo> GetExistingDevices()
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("WMIEventWatcher.GetExistingDevices");
			try
			{
                List<LogicalDiskInfo> devices = new List<LogicalDiskInfo>();

				ObjectQuery diskQuery = new ObjectQuery("Select * from Win32_DiskDrive where InterfaceType='USB'");

				foreach (ManagementObject drive in new ManagementObjectSearcher(diskQuery).Get())
				{
					ObjectQuery partQuery = new ObjectQuery(
						String.Format("associators of {{Win32_DiskDrive.DeviceID='{0}'}} where AssocClass = Win32_DiskDriveToDiskPartition", drive["DeviceID"])
						);

#if DEBUG
                    DebugPrint(drive);
#endif
					foreach (ManagementObject partition in new ManagementObjectSearcher(partQuery).Get())
					{
						// associate partitions with logical disks (drive letter volumes)
						ObjectQuery logicalQuery = new ObjectQuery(
						String.Format("associators of {{Win32_DiskPartition.DeviceID='{0}'}} where AssocClass = Win32_LogicalDiskToPartition", partition["DeviceID"])
						);

#if DEBUG
                        DebugPrint(partition);
#endif
						foreach (ManagementObject logical in new ManagementObjectSearcher(logicalQuery).Get())
						{
#if DEBUG
                            DebugPrint(logical);
#endif
                            LogicalDiskInfo disk = new LogicalDiskInfo();

							ParseDiskDriveInfo(disk, drive);
							ParseDiskLogicalInfo(disk, logical);

							devices.Add(disk);
						}
					}
				}

				return devices;
			}
			catch (Exception err)
			{
				LogHelper.Manage("WMIEventWatcher.GetExistingDevices", err);
				return null;
			}
			finally
			{
				LogHelper.End("WMIEventWatcher.GetExistingDevices");
			}  
        }

        private void ParseDiskDriveInfo(LogicalDiskInfo disk, ManagementBaseObject drive)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("WMIEventWatcher.ParseDiskDriveInfo");
			try
			{
				disk.PNPDeviceID = drive["PNPDeviceID"].ToString().Replace("\\", "");
				disk.Model = drive["Model"].ToString();
				disk.Caption = drive["Caption"].ToString();
			}
			catch (Exception err)
			{
				LogHelper.Manage("WMIEventWatcher.ParseDiskDriveInfo", err);
			}
			finally
			{
				LogHelper.End("WMIEventWatcher.ParseDiskDriveInfo");
			}  
        }

        private void ParseDiskLogicalInfo(LogicalDiskInfo disk, ManagementBaseObject logical)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("WMIEventWatcher.ParseDiskLogicalInfo");
			try
			{
				disk.Name = logical["Name"].ToString();
				disk.Path = logical["Name"].ToString();

				disk.VolumeLabel = logical["VolumeName"].ToString();
				disk.AvailableFreeSpace = (long)logical["FreeSpace"];
				disk.TotalSize = (long)logical["Size"];
			}
			catch (Exception err)
			{
				LogHelper.Manage("WMIEventWatcher.ParseDiskLogicalInfo", err);
			}
			finally
			{
				LogHelper.End("WMIEventWatcher.ParseDiskLogicalInfo");
			}  
        }

        /// <summary>
        /// internal function to trace properties
        /// </summary>
        /// <param name="e"></param>
        private void DebugPrint(ManagementBaseObject e)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("ClassPath : {0}", e.ClassPath);
            Console.WriteLine("Site : {0}", e.Site);

            foreach (PropertyData prop in e.Properties)
                Console.WriteLine("PROPERTY : {0} - {1}", prop.Name, prop.Value);

            foreach (PropertyData prop in e.SystemProperties)
                Console.WriteLine("SYSTEM : {0} - {1}", prop.Name, prop.Value);

            foreach (QualifierData prop in e.Qualifiers)
                Console.WriteLine("QUALIFIER : {0} - {1}", prop.Name, prop.Value);

            Console.WriteLine("--------------------------------------------------------------");
        }

        #endregion
    }
}
