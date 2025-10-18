using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RW.StarCitizen.MappingAssist.UI.Models.UI
{
    public class DeviceChoice
    {
        public string Type { get; set; } = "";
        public int Instance { get; set; }
        public string Product { get; set; } = "";
        public string Prefix { get; set; } = "";      // e.g., kb1_, ms1_, js2_
        public string DisplayName { get; set; } = ""; // e.g., "Joystick #2 (Virpil Left)"
    }
}

