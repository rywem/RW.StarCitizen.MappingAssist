using RW.StarCitizen.MappingAssist.UI.Models.XML;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RW.StarCitizen.MappingAssist.UI.Parsers
{

    public static class ActionMapsParser
    {
        public static ActionMapsModel Parse(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("XML string is empty.", nameof(xml));

            var doc = XDocument.Parse(xml);
            var root = doc.Root ?? throw new InvalidOperationException("Missing root node.");

            var model = new ActionMapsModel
            {
                Version = (string)root.Attribute("version"),
                OptionsVersion = (string)root.Attribute("optionsVersion"),
                RebindVersion = (string)root.Attribute("rebindVersion"),
                ProfileName = (string)root.Attribute("profileName")
            };

            // Header
            var header = root.Element("CustomisationUIHeader");
            if (header != null)
            {
                var h = new CustomisationUIHeader
                {
                    Label = (string)header.Attribute("label"),
                    Description = (string)header.Attribute("description"),
                    Image = (string)header.Attribute("image")
                };

                // devices
                var devices = header.Element("devices");
                if (devices != null)
                {
                    foreach (var d in devices.Elements())
                    {
                        // element name is device type
                        var type = d.Name.LocalName; // keyboard/mouse/joystick
                        if (int.TryParse((string)d.Attribute("instance"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var instance))
                        {
                            h.Devices.Add(new DeviceEntry { Type = type, Instance = instance });
                        }
                    }
                }

                // categories
                var categories = header.Element("categories");
                if (categories != null)
                {
                    foreach (var c in categories.Elements("category"))
                    {
                        var label = (string)c.Attribute("label") ?? string.Empty;
                        h.Categories.Add(label);
                    }
                }

                model.Header = h;
            }

            // deviceoptions (can be multiple blocks)
            foreach (var dop in root.Elements("deviceoptions"))
            {
                var block = new DeviceOptionsBlock
                {
                    Name = (string)dop.Attribute("name")
                };

                foreach (var opt in dop.Elements("option"))
                {
                    var devOpt = new DeviceOption
                    {
                        Input = (string)opt.Attribute("input"),
                        Attributes = opt.Attributes()
                                        .Where(a => a.Name.LocalName != "input")
                                        .ToDictionary(a => a.Name.LocalName, a => a.Value)
                    };
                    block.Options.Add(devOpt);
                }

                model.DeviceOptions.Add(block);
            }

            // options blocks (per device/type)
            foreach (var optBlock in root.Elements("options"))
            {
                var block = new OptionsBlock
                {
                    Type = (string)optBlock.Attribute("type"),
                    Product = (string)optBlock.Attribute("Product")
                };

                if (int.TryParse((string)optBlock.Attribute("instance"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var instance))
                    block.Instance = instance;

                // arbitrary child elements become OptionParam entries
                foreach (var child in optBlock.Elements())
                {
                    var param = new OptionParam
                    {
                        Name = child.Name.LocalName,
                        Attributes = child.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value)
                    };
                    block.Parameters.Add(param);
                }

                model.OptionsBlocks.Add(block);
            }

            // modifiers presence
            model.HasModifiersNode = root.Elements("modifiers").Any();

            // actionmaps
            foreach (var amap in root.Elements("actionmap"))
            {
                var actionMap = new ActionMap
                {
                    Name = (string)amap.Attribute("name")
                };

                foreach (var action in amap.Elements("action"))
                {
                    var act = new ActionBinding
                    {
                        Name = (string)action.Attribute("name")
                    };

                    foreach (var rb in action.Elements("rebind"))
                    {
                        act.Rebinds.Add(new Rebind
                        {
                            Input = (string)rb.Attribute("input")
                        });
                    }

                    actionMap.Actions.Add(act);
                }

                model.ActionMaps.Add(actionMap);
            }

            return model;
        }
    }
}
