using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USB_Forensic.Model
{
    public class UsbDevice
    {
        public string FullName { get; set; }
        public string FriendlyName { get; set; }
        public string Id { get; set; }
        public string[] ValueNames { get; set; }
        public string[] SubKeyNames { get; set; }
    }
}
