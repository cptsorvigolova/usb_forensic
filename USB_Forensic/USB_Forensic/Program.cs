using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JumpList.Automatic;
using JumpList.Custom;
using Microsoft.Win32;
using USB_Forensic.Model;

namespace USB_Forensic
{
    class Program
    {
        static void Main(string[] args)
        {
            //var reg = Microsoft.Win32.Registry.LocalMachine;
            //var devices = new List<UsbDevice>();
            //var devProps = new List<RegistryKey>();
            //var usbstor = reg.OpenSubKey(Constants.RegistryKeyUSBSTOR);
            //foreach (var deviceName in usbstor.GetSubKeyNames())
            //{
            //    var device = usbstor.OpenSubKey(deviceName);
            //    var devIds = device.GetSubKeyNames();
            //    foreach (var devId in devIds)
            //    {
            //        var specDev = device.OpenSubKey(devId);
            //        devices.Add(
            //            new UsbDevice
            //            {
            //                FriendlyName = specDev.GetValue("FriendlyName")?.ToString(),
            //                Id = devId,
            //                FullName = specDev.Name,
            //                ValueNames = specDev.GetValueNames(),
            //                SubKeyNames = specDev.GetSubKeyNames()
            //            }
            //        ); 
            //    }
            //}
            string SystemRoot = Environment.GetEnvironmentVariable("SystemRoot");
            var amcachePath = Path.Combine(SystemRoot, @"AppCompat\Programs\Amcache.hve");
            var parser = new Amcache.AmcacheNew(amcachePath, true, false);

            var kernelEvtxPath = Path.Combine(SystemRoot, @"System32\winevt\Logs\Microsoft-Windows-Kernel-PnP%4Configuration.evtx");
            var partitionEvtxPath = Path.Combine(SystemRoot, @"System32\winevt\Logs\Microsoft-Windows-Partition%4Diagnostic.evtx");
            var evtxReader = new EventLogReader(kernelEvtxPath, PathType.FilePath);
            var evtx = evtxReader.ReadEvent();

            var recentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
            var lnkFile = Directory.GetFiles(recentDirectory);
            var lnk = Lnk.Lnk.LoadFile(lnkFile[0]);

            var autoDest = Path.Combine(recentDirectory, "AutomaticDestinations");
            var custDest = Path.Combine(recentDirectory, "CustomDestinations");
            var autoFiles = Directory.GetFiles(autoDest, "*.automaticDestinations-ms", SearchOption.AllDirectories);
            var custFiles = Directory.GetFiles(custDest, "*.customDestinations-ms", SearchOption.AllDirectories);
            foreach (var fname in autoFiles)
            {
                var raw = File.ReadAllBytes(fname);
                var a = new AutomaticDestination(raw, fname);
            }            
            foreach (var fname in custFiles)
            {
                var raw = File.ReadAllBytes(fname);
                var a = new CustomDestination(raw, fname);
            }


            Console.ReadLine();
        }
    }
}
