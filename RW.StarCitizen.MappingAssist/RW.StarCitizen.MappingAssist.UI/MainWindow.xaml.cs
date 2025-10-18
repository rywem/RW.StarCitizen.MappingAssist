using Microsoft.Win32;
using RW.StarCitizen.MappingAssist.Devices;
// UI models (ours below)
using RW.StarCitizen.MappingAssist.UI.Models.UI;
// XML model + parser (your namespaces)
using RW.StarCitizen.MappingAssist.UI.Models.XML;      // ActionMapsModel, etc.
using RW.StarCitizen.MappingAssist.UI.Parsers;         // ActionMapsParser
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RW.StarCitizen.MappingAssist.UI
{
    public partial class MainWindow : Window
    {
        private const string DefaultMappingsDir =
            @"C:\Program Files\Roberts Space Industries\StarCitizen\LIVE\user\client\0\controls\mappings";

        public ActionMapsModel? LoadedMaps { get; private set; }

        // Bindables
        public List<DeviceChoice> DeviceChoices { get; } = new();
        public DeviceChoice? SelectedDevice { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<BindingRow> BindingRows { get; }
            = new System.Collections.ObjectModel.ObservableCollection<BindingRow>();

        public System.Collections.ObjectModel.ObservableCollection<ButtonChoice> ButtonChoices { get; }
            = new System.Collections.ObjectModel.ObservableCollection<ButtonChoice>();

        public ButtonChoice? SelectedButtonChoice { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void BtnLoadMapping_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    Title = "Open Star Citizen Mapping XML",
                    Filter = "ActionMap XML (*.xml)|*.xml|All files (*.*)|*.*",
                    CheckFileExists = true,
                    InitialDirectory = Directory.Exists(DefaultMappingsDir)
                        ? DefaultMappingsDir
                        : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                };

                if (dlg.ShowDialog(this) == true)
                {
                    string xml = File.ReadAllText(dlg.FileName);
                    LoadedMaps = ActionMapsParser.Parse(xml);

                    TxtSelectedFile.Text = dlg.FileName;
                    TxtStatus.Text = $"Loaded profile \"{LoadedMaps.ProfileName}\" with {LoadedMaps.ActionMaps.Count} action maps.";

                    BuildDeviceChoices();
                    if (DeviceChoices.Count > 0)
                    {
                        SelectedDevice = DeviceChoices[0];
                        CmbDevices.SelectedItem = SelectedDevice;
                        BuildButtonChoices();
                        TxtPrefix.Text = SelectedDevice.Prefix;
                        RefreshBindingRows();
                        UpdateDeviceImage();
                    }
                    else
                    {
                        BindingRows.Clear();
                        BindingsGrid.Items.Refresh();
                        TxtPrefix.Text = "";
                        ClearDeviceImage();
                    }
                }
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Failed to load mapping.";
                MessageBox.Show(this,
                    $"Error loading or parsing XML:\n\n{ex.Message}",
                    "Load Mapping",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #region choice builders
        private void BuildButtonChoices()
        {
            ButtonChoices.Clear();

            // Always offer "All"
            ButtonChoices.Add(new ButtonChoice { Display = "All buttons" });

            // If it's a Virpil device, include buttons 1..31 with human names
            bool isVirpil = SelectedDevice != null &&
                            (SelectedDevice.Product?.IndexOf("VPC", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;

            if (isVirpil)
            {
                for (int i = 1; i <= 31; i++)
                {
                    var info = VirpilMappings.Get(i);
                    var label = info != null ? $"Button {i} — {info.Name}" : $"Button {i}";
                    ButtonChoices.Add(new ButtonChoice { ButtonNumber = i, Display = label });
                }
            }
            else
            {
                // Unknown joystick/keyboard/mouse: at least offer numbers 1..31
                for (int i = 1; i <= 31; i++)
                    ButtonChoices.Add(new ButtonChoice { ButtonNumber = i, Display = $"Button {i}" });
            }

            // Standalone modifier filters
            ButtonChoices.Add(new ButtonChoice { Modifier = "lalt", Display = "LALT" });
            ButtonChoices.Add(new ButtonChoice { Modifier = "ralt", Display = "RALT" });

            // Default to "All"
            SelectedButtonChoice = ButtonChoices.FirstOrDefault();
            CmbButtonFilter.ItemsSource = ButtonChoices;
            CmbButtonFilter.SelectedItem = SelectedButtonChoice;
        }
        private void BuildDeviceChoices()
        {
            DeviceChoices.Clear();
            if (LoadedMaps == null) return;

            foreach (var opt in LoadedMaps.OptionsBlocks)
            {
                var type = (opt.Type ?? "").Trim().ToLowerInvariant();
                var inst = opt.Instance;
                string prefix = type switch
                {
                    "keyboard" => $"kb{inst}_",
                    "mouse" => $"ms{inst}_",
                    "joystick" => $"js{inst}_",
                    _ => $"{type}{inst}_"
                };

                // Friendly label + hints
                string brandHint = "";
                var product = opt.Product ?? string.Empty;
                if (product.Contains("vJoy", StringComparison.OrdinalIgnoreCase))
                    brandHint = " (vJoy)";
                else if (product.Contains("LEFT VPC Stick", StringComparison.OrdinalIgnoreCase))
                    brandHint = " (Virpil Left)";
                else if (product.Contains("RIGHT VPC Stick", StringComparison.OrdinalIgnoreCase))
                    brandHint = " (Virpil Right)";

                string typeLabel = char.ToUpper(type.FirstOrDefault()) + type.Substring(1);
                var display = $"{typeLabel} #{inst}{brandHint}".Trim();

                DeviceChoices.Add(new DeviceChoice
                {
                    Type = type,
                    Instance = inst,
                    Product = product,
                    Prefix = prefix,
                    DisplayName = display
                });
            }

            // Unique + ordered
            var unique = DeviceChoices
                .GroupBy(d => (d.Type, d.Instance))
                .Select(g => g.First())
                .OrderBy(d => d.Type)
                .ThenBy(d => d.Instance)
                .ToList();

            DeviceChoices.Clear();
            DeviceChoices.AddRange(unique);

            CmbDevices.ItemsSource = null;
            CmbDevices.ItemsSource = DeviceChoices;
        }

        #endregion 
        #region Handlers
        private void CmbButtonFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectedButtonChoice = CmbButtonFilter.SelectedItem as ButtonChoice;
            RefreshBindingRows(); // rebuild rows with the new filter
        }

        private void CmbDevices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectedDevice = CmbDevices.SelectedItem as DeviceChoice;
            TxtPrefix.Text = SelectedDevice?.Prefix ?? "";
            BuildButtonChoices();
            RefreshBindingRows();
            UpdateDeviceImage();
        }
        private void BindingsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = BindingsGrid.SelectedItem as BindingRow;
            if (row == null) { ClearDeviceImage(); return; }

            // Pull button number from the selected row's Input (e.g., js1_lalt+button12)
            var btn = TryExtractButtonNumber(row.Input);
            if (!btn.HasValue) { ClearDeviceImage(); return; }

            // Look up the Virpil mapping and load that exact file
            var info = VirpilMappings.Get(btn.Value);
            if (info == null) { ClearDeviceImage(); return; }

            LoadRightSideImageFromResources(info.ImageFile, info.Name);
        }
        #endregion

        #region Refreshs
        private void RefreshBindingRows()
        {
            BindingRows.Clear();

            if (LoadedMaps == null || SelectedDevice == null)
            {
                TxtStatus.Text = "No device selected.";
                return;
            }

            string prefix = SelectedDevice.Prefix;
            bool isVirpil = SelectedDevice.Product.IndexOf("VPC", StringComparison.OrdinalIgnoreCase) >= 0;

            int? filterButton = SelectedButtonChoice?.ButtonNumber;
            string? filterMod = SelectedButtonChoice?.Modifier?.ToLowerInvariant(); // "lalt" | "ralt" | null

            foreach (var map in LoadedMaps.ActionMaps)
            {
                foreach (var action in map.Actions)
                {
                    foreach (var rb in action.Rebinds)
                    {
                        var input = (rb.Input ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(input)) continue;

                        // Scope to selected device
                        if (!input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

                        // Apply filter:
                        //  A) explicit modifier-only filter (LALT/RALT) -> show all using that modifier
                        //  B) button filter -> include unmodified AND LALT+buttonNN AND RALT+buttonNN
                        if (filterMod == "lalt" && input.IndexOf("lalt+", StringComparison.OrdinalIgnoreCase) < 0) continue;
                        if (filterMod == "ralt" && input.IndexOf("ralt+", StringComparison.OrdinalIgnoreCase) < 0) continue;

                        if (filterButton.HasValue)
                        {
                            var extracted = TryExtractButtonNumber(input);
                            if (extracted != filterButton.Value) continue; // ensures Button 1 != Button 10
                                                                           // modifier state (none/lalt/ralt) is all allowed here
                        }

                        string humanName = "";
                        System.Windows.Media.Imaging.BitmapImage? humanImg = null;

                        var buttonNum = TryExtractButtonNumber(input);
                        if (buttonNum.HasValue && isVirpil)
                        {
                            var info = VirpilMappings.Get(buttonNum.Value);
                            if (info != null)
                            {
                                humanName = info.Name;

                                // tiny icon in the grid (optional; harmless if missing)
                                try
                                {
                                    var uri = new Uri($"pack://siteoforigin:,,,/Resources/Images/{info.ImageFile}", UriKind.Absolute);
                                    humanImg = new System.Windows.Media.Imaging.BitmapImage(uri);
                                }
                                catch { /* ignore */ }
                            }
                        }

                        BindingRows.Add(new BindingRow
                        {
                            ActionMap = map.Name,
                            Action = action.Name,
                            Input = input,
                            HumanName = humanName,
                            HumanImage = humanImg
                        });
                    }
                }
            }

            TxtStatus.Text =
                $"Showing {BindingRows.Count} rows for {SelectedDevice.DisplayName} — " +
                (filterButton.HasValue ? $"Button {filterButton}" :
                 filterMod == "lalt" ? "LALT" :
                 filterMod == "ralt" ? "RALT" : "All buttons");
        }
        /* private void RefreshBindingRows()
         {
             BindingRows.Clear();
             if (LoadedMaps == null || SelectedDevice == null)
             {
                 BindingsGrid.ItemsSource = BindingRows;
                 BindingsGrid.Items.Refresh();
                 return;
             }

             string prefix = SelectedDevice.Prefix;
             bool isVirpil = SelectedDevice.Product.IndexOf("VPC", StringComparison.OrdinalIgnoreCase) >= 0;

             string appBase = AppDomain.CurrentDomain.BaseDirectory;
             string imagesDir = System.IO.Path.Combine(appBase, "Resources", "Images");

             foreach (var map in LoadedMaps.ActionMaps)
             {
                 foreach (var action in map.Actions)
                 {
                     foreach (var rb in action.Rebinds)
                     {
                         var input = (rb.Input ?? string.Empty).Trim();
                         if (string.IsNullOrWhiteSpace(input)) continue;

                         if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                         {
                             string humanName = "";
                             BitmapImage? humanImg = null;

                             var buttonNum = TryExtractButtonNumber(input);
                             if (buttonNum.HasValue && isVirpil)
                             {
                                 var info = VirpilMappings.Get(buttonNum.Value);
                                 if (info != null)
                                 {
                                     humanName = info.Name;

                                     // Try load the per-button image (Resources/Images/<filename>)
                                     string full = System.IO.Path.Combine(imagesDir, info.ImageFile);
                                     if (File.Exists(full))
                                     {
                                         var bmp = new BitmapImage();
                                         bmp.BeginInit();
                                         bmp.CacheOption = BitmapCacheOption.OnLoad;
                                         bmp.UriSource = new Uri(full, UriKind.Absolute);
                                         bmp.EndInit();
                                         humanImg = bmp;
                                     }
                                 }
                             }

                             BindingRows.Add(new BindingRow
                             {
                                 ActionMap = map.Name,
                                 Action = action.Name,
                                 Input = input,
                                 HumanName = humanName,
                                 HumanImage = humanImg
                             });
                         }
                     }
                 }
             }

             BindingsGrid.ItemsSource = BindingRows;
             BindingsGrid.Items.Refresh();

             TxtStatus.Text = $"Showing {BindingRows.Count} bindings for {SelectedDevice.DisplayName} ({SelectedDevice.Prefix}).";
             // Auto-select first row so you see an image right away
             if (BindingRows.Count > 0)
             {
                 BindingsGrid.SelectedIndex = 0; // will trigger BindingsGrid_SelectionChanged
             }
         }*/
        #endregion


        private void LoadRightSideImageFromResources(string fileName, string caption)
        {
            try
            {
                // Images must be at: <appdir>/Resources/Images/<fileName>
                // Set each image: Build Action=Content, Copy to Output=Copy if newer
                var uri = new Uri($"pack://siteoforigin:,,,/Resources/Images/{fileName}", UriKind.Absolute);
                DeviceImage.Source = new BitmapImage(uri);
                ImgCaption.Text = caption;
            }
            catch
            {
                ClearDeviceImage();
                TxtStatus.Text = $"Failed to load image: {fileName}";
            }
        }
        private static int? TryExtractButtonNumber(string input)
        {
            // Examples:
            // js1_button23
            // js2_lalt+button12
            // js3_button1
            // Return the trailing "buttonNN" number if present
            // Simple, fast parsing without regex:
            var token = "button";
            int idx = input.LastIndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                int start = idx + token.Length;
                if (start < input.Length)
                {
                    var numStr = new string(input.Skip(start).TakeWhile(char.IsDigit).ToArray());
                    if (int.TryParse(numStr, out int n))
                        return n;
                }
            }
            return null;
        }

        private void UpdateDeviceImage()
        {
            if (SelectedDevice == null)
            {
                ClearDeviceImage();
                return;
            }

            // Choose an image filename based on product/type
            // Put these files in: [AppBase]/Resources/Images/
            string? filename = null;
            var prod = SelectedDevice.Product ?? "";

            if (prod.Contains("LEFT VPC Stick", StringComparison.OrdinalIgnoreCase))
                filename = "virpil_left.jpg";    // <-- supply this
            else if (prod.Contains("RIGHT VPC Stick", StringComparison.OrdinalIgnoreCase))
                filename = "virpil_right.jpg";   // <-- supply this
            else if (prod.Contains("vJoy", StringComparison.OrdinalIgnoreCase))
                filename = "vjoy.jpg";
            else if (SelectedDevice.Type == "keyboard")
                filename = "keyboard.jpg";
            else if (SelectedDevice.Type == "mouse")
                filename = "mouse.jpg";

            if (filename == null)
            {
                ClearDeviceImage();
                return;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = System.IO.Path.Combine(baseDir, "Resources", "Images", filename);

            if (File.Exists(fullPath))
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
                bmp.EndInit();
                DeviceImage.Source = bmp;

                ImgCaption.Text = System.IO.Path.GetFileNameWithoutExtension(filename).Replace('_', ' ');
            }
            else
            {
                ClearDeviceImage();
            }
        }

        private void ClearDeviceImage()
        {
            DeviceImage.Source = null;
            ImgCaption.Text = "No image available";
        }
    }
}
