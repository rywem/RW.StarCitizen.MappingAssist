using System.Windows.Media.Imaging;

namespace RW.StarCitizen.MappingAssist.UI.Models.UI
{
    public class BindingRow
    {
        public string ActionMap { get; set; } = "";
        public string Action { get; set; } = "";
        public string Input { get; set; } = "";

        public string HumanName { get; set; } = "";              // e.g., "Castle Hat - Press"
        public BitmapImage? HumanImage { get; set; } = null;      // thumbnail loaded from Resources/Images
    }
}
