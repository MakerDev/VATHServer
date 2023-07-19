using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VathServer.Models
{
    public partial class ImageModel : ObservableObject
    {
        public string ImageUrl { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        [ObservableProperty]
        private double _opacity = 1.0;
    }
}
