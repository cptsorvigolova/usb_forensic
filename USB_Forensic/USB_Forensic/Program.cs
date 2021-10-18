using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
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
        private static string SystemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private static string RecentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
        private static string LogPath = Path.Combine(SystemRoot, @"Sysnative\winevt\Logs\");
        private static string LogPath32 = Path.Combine(SystemRoot, @"System32\winevt\Logs\");

        static void Main(string[] args)
        {
            var users = GetUsers();
            var evtxs = ReadEvtx();
            var acts = new Dictionary<UserData, List<EventLogRecord>>();
            acts = users.ToDictionary(x => x, y => evtxs.Where(ev => string.Compare(ev?.UserId?.Value, y.Sid, StringComparison.OrdinalIgnoreCase) == 0).ToList());
            ReadAmcache();
            Console.ReadLine();
        }

        public static List<UserData> GetUsers()
        {
            var users = new List<DirectoryEntry>();
            DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            var ids = new List<string>();
            foreach (DirectoryEntry child in localMachine.Children)
            {
                if (child.SchemaClassName == "User")
                {
                    users.Add(child);
                    NTAccount f = new NTAccount(child.Name);
                    SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
                    ids.Add(s.ToString());
                }
            }
            var result = users.Zip(ids, (us, id) => new UserData { Username = us.Name, Sid = id }).ToList();
            return result;
        }

        public static void ReadUsbDevices()
        {
            var reg = Microsoft.Win32.Registry.LocalMachine;
            var devices = new List<UsbDevice>();
            var devProps = new List<RegistryKey>();
            var usbstor = reg.OpenSubKey(Constants.RegistryKeyUSBSTOR);
            foreach (var deviceName in usbstor.GetSubKeyNames())
            {
                var device = usbstor.OpenSubKey(deviceName);
                var devIds = device.GetSubKeyNames();
                foreach (var devId in devIds)
                {
                    var specDev = device.OpenSubKey(devId);
                    devices.Add(
                        new UsbDevice
                        {
                            FriendlyName = specDev.GetValue("FriendlyName")?.ToString(),
                            Id = devId,
                            FullName = specDev.Name,
                            ValueNames = specDev.GetValueNames(),
                            SubKeyNames = specDev.GetSubKeyNames()
                        }
                    );
                }
            }

            foreach (var e in devices)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void ReadJumpList()
        {
            var autoDest = Path.Combine(RecentDirectory, "AutomaticDestinations");
            var custDest = Path.Combine(RecentDirectory, "CustomDestinations");
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
        }

        public static void ReadLnk()
        {
            var lnkFile = Directory.GetFiles(RecentDirectory);
            var lnk = Lnk.Lnk.LoadFile(lnkFile[0]);
        }

        public static void ReadAmcache()
        {
            var amcachePath = Path.Combine(SystemRoot, @"AppCompat\Programs\Amcache.hve");
            var parser = new Amcache.AmcacheNew(amcachePath, true, false);
            var unassoc = parser.UnassociatedFileEntries;
            var shortcuts = parser.ShortCuts;
        }

        public static List<EventLogRecord> ReadEvtx()
        {
            var allLogs = Directory.GetFiles(LogPath).Select(x => Path.GetFileName(x));
            allLogs = File.ReadAllLines("LogSources.txt");
            var logsToRead = new[]
            {
                @"Microsoft-Windows-Kernel-PnP%4Configuration.evtx",
                @"Microsoft-Windows-Partition%4Diagnostic.evtx",
                @"Microsoft-Windows-User Profile Service%4Operational.evtx",
                @"System.evtx",
                @"Security.evtx"
            };
            var logs = new List<EventLogRecord>();
            foreach (var log in allLogs)
            {
                var evtxPath = Path.Combine(LogPath32, log);
                var evtxReader = new EventLogReader(evtxPath, PathType.FilePath);
                while (true)
                {
                    try
                    {
                        var eventLog = evtxReader.ReadEvent() as EventLogRecord;
                        if (eventLog != null)
                            logs.Add(eventLog);
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }
            }
            return logs;
        }

        public static void ReadSetupapi()
        {
            var setupapi = File.ReadAllText(Path.Combine(SystemRoot, @"INF\setupapi.dev.log"));
        }
    }
}
