using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USB_Forensic.Model
{
    public class GeneralEvent
    {
        public DateTimeOffset Timestamp { get; set; }
        public UserData User { get; set; }

    }
}
