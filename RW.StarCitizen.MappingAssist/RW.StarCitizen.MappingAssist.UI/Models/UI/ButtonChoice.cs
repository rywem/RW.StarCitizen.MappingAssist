namespace RW.StarCitizen.MappingAssist.UI.Models.UI
{
    public class ButtonChoice
    {
        // If set => filter by this button number (and include LALT/RALT combos automatically)
        public int? ButtonNumber { get; set; }

        // If set to "lalt" or "ralt" (and ButtonNumber is null) => filter by that modifier
        public string? Modifier { get; set; }  // "lalt", "ralt" or null

        // Text shown in the ComboBox, e.g. "Button 12 — Red Button" or "LALT"
        public string Display { get; set; } = "";
    }
}

