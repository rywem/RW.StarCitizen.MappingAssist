using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RW.StarCitizen.MappingAssist.Devices
{

    // RW.StarCitizen.MappingAssist.UI.Models.UI.VirpilMappings.cs

    public sealed class VirpilButtonInfo
    {
        public string ImageFile { get; init; } = "";
        public string Name { get; init; } = "";
    }

    public static class VirpilMappings
    {
        private static readonly Dictionary<int, VirpilButtonInfo> _map = new()
        {
            { 1,  new() { ImageFile = "FlipTrigger.jpg",  Name = "Flip Trigger - Toggle" } },
            { 2,  new() { ImageFile = "FlipTrigger.jpg",  Name = "Flip Trigger - Press" } },
            { 3,  new() { ImageFile = "FlipTrigger.jpg",  Name = "Main Trigger - Press" } },
            { 4,  new() { ImageFile = "MainTrigger.jpg",  Name = "Main Trigger Deep Press" } },
            { 5,  new() { ImageFile = "MiniStick.jpg",    Name = "Mini Stick - Press" } },
            { 6,  new() { ImageFile = "BlackButton.jpg",  Name = "Black Button" } },
            { 7,  new() { ImageFile = "CastleHat.jpg",    Name = "Castle Hat - Press" } },
            { 8,  new() { ImageFile = "CastleHat.jpg",    Name = "Castle Hat - Up" } },
            { 9,  new() { ImageFile = "CastleHat.jpg",    Name = "Castle Hat - Right" } },
            { 10, new() { ImageFile = "CastleHat.jpg",    Name = "Castle Hat - Down" } },
            { 11, new() { ImageFile = "CastleHat.jpg",    Name = "Castle Hat - Left" } },
            { 12, new() { ImageFile = "RedButton.jpg",    Name = "Red Button" } },
            { 13, new() { ImageFile = "PyramidHat.jpg",   Name = "Pyramid Hat - Press" } },
            { 14, new() { ImageFile = "PyramidHat.jpg",   Name = "Pyramid Hat - Up" } },
            { 15, new() { ImageFile = "PyramidHat.jpg",   Name = "Pyramid Hat - Right" } },
            { 16, new() { ImageFile = "PyramidHat.jpg",   Name = "Pyramid Hat - Down" } },
            { 17, new() { ImageFile = "PyramidHat.jpg",   Name = "Pyramid Hat - Left" } },
            { 18, new() { ImageFile = "SideSwitch.jpg",   Name = "Side Switch - Press" } },
            { 19, new() { ImageFile = "SideSwitch.jpg",   Name = "Side Switch - Forward" } },
            { 20, new() { ImageFile = "SideSwitch.jpg",   Name = "Side Switch - Back" } },
            { 21, new() { ImageFile = "ThumbWheel.jpg",   Name = "Thumb Wheel - Press" } },
            { 22, new() { ImageFile = "ThumbWheel.jpg",   Name = "Thumb Wheel - Deep Press" } },
            { 23, new() { ImageFile = "ThumbWheel.jpg",   Name = "Thumb Wheel - Down" } },
            { 24, new() { ImageFile = "ThumbWheel.jpg",   Name = "Thumb Wheel - Up" } },
            { 25, new() { ImageFile = "ThumbHat.jpg",     Name = "Thumb Hat - Press" } },
            { 26, new() { ImageFile = "ThumbHat.jpg",     Name = "Thumb Hat - Forward" } },
            { 27, new() { ImageFile = "ThumbHat.jpg",     Name = "Thumb Hat - Right" } },
            { 28, new() { ImageFile = "ThumbHat.jpg",     Name = "Thumb Hat - Back" } },
            { 29, new() { ImageFile = "ThumbHat.jpg",     Name = "Thumb Hat - Left" } },
            { 30, new() { ImageFile = "PinkyButton.jpg",  Name = "Pinky Button" } },
            { 31, new() { ImageFile = "BrakeLever.jpg",   Name = "Brake Lever - Slider and Button" } },
        };

        public static VirpilButtonInfo? Get(int buttonNumber) =>
            _map.TryGetValue(buttonNumber, out var v) ? v : null;
    }
}


