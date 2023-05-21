using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace VathServer.Platforms.iOS
{
    public class ScreenSizeHelper
    {
        public static double GetDiagonalScreenSizeInInches()
        {
            var mainScreen = UIScreen.MainScreen;
            var bounds = mainScreen.Bounds;
            var width = bounds.Width;
            var height = bounds.Height;
            var scale = mainScreen.Scale;

            var ppi = 163; // Assuming a standard ppi value for iOS devices
            var diagonalSizeInPoints = Math.Sqrt(width * width + height * height);
            var diagonalSizeInInches = diagonalSizeInPoints / ppi / scale;

            return diagonalSizeInInches;
        }
    }
}
