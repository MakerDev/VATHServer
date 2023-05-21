using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;

namespace VathServer.Platforms.Windows
{
    public class ScreenSizeHelper
    {
        public static double GetDiagonalScreenSizeInInches()
        {
            var displayInformation = DisplayInformation.GetForCurrentView();
            var screenSize = new Size(displayInformation.ScreenWidthInRawPixels, displayInformation.ScreenHeightInRawPixels);
            var diagonalSizeInInches = Math.Sqrt(screenSize.Width * screenSize.Width + screenSize.Height * screenSize.Height) / displayInformation.RawDpiX;

            return diagonalSizeInInches;
        }
    }
}
