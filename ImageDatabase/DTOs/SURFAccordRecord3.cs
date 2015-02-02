using Accord.Imaging;
using Emgu.CV.Features2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.DTOs
{
    [Serializable]
    public class SURFAccordRecord3 : ImageRecord
    {
        public List<SpeededUpRobustFeaturePoint> SurfDescriptors { get; set; }
    }
}
