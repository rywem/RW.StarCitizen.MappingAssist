using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace RW.StarCitizen.MappingAssist.UI.Models.XML
{
    // Models.cs

    public class ActionMapsModel
    {
        public string Version { get; set; }
        public string OptionsVersion { get; set; }
        public string RebindVersion { get; set; }
        public string ProfileName { get; set; }

        public CustomisationUIHeader Header { get; set; }

        public List<DeviceOptionsBlock> DeviceOptions { get; set; } = new();
        public List<OptionsBlock> OptionsBlocks { get; set; } = new();

        public List<ActionMap> ActionMaps { get; set; } = new();
        public bool HasModifiersNode { get; set; } // present even if empty <modifiers />
    }

    public class CustomisationUIHeader
    {
        public string Label { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        public List<DeviceEntry> Devices { get; set; } = new();
        public List<string> Categories { get; set; } = new();
    }

    public class DeviceEntry
    {
        /// <summary>
        /// "keyboard", "mouse", or "joystick"
        /// </summary>
        public string Type { get; set; }
        public int Instance { get; set; }
    }

    public class DeviceOptionsBlock
    {
        public string Name { get; set; }
        public List<DeviceOption> Options { get; set; } = new();
    }

    public class DeviceOption
    {
        public string Input { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new(); // e.g., saturation="0.79..."
    }

    public class OptionsBlock
    {
        public string Type { get; set; }        // keyboard / joystick
        public int Instance { get; set; }       // 1, 2, 3...
        public string Product { get; set; }     // "RIGHT VPC Stick WarBRD  {...}"

        /// <summary>
        /// Child tunables like <flight_move_pitch exponent="1.3"/>
        /// We store them generically so you can bind/edit without knowing future schema changes.
        /// </summary>
        public List<OptionParam> Parameters { get; set; } = new();
    }

    public class OptionParam
    {
        public string Name { get; set; } // e.g., flight_move_pitch
        public Dictionary<string, string> Attributes { get; set; } = new();
    }

    public class ActionMap
    {
        public string Name { get; set; }                    // e.g., "spaceship_movement"
        public List<ActionBinding> Actions { get; set; } = new();
    }

    public class ActionBinding
    {
        public string Name { get; set; } // e.g., "v_yaw"
        public List<Rebind> Rebinds { get; set; } = new();  // multiple <rebind input="..."/>
    }

    public class Rebind
    {
        public string Input { get; set; } // e.g., "js2_x", "js1_lalt+button6", "kb1_lalt+l"
    }
}


