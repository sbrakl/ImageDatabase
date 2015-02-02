using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyeOpen.Imaging.Processing;

namespace ImageDatabase.DTOs
{
    [Serializable]
    public class RGBProjectionRecord : ImageRecord
    {
        public RgbProjections RGBProjection { get; set; }       
    }
}
