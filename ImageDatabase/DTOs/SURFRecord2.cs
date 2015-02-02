using Emgu.CV.Features2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.DTOs
{
    [Serializable]
    public class SURFRecord2 : ImageRecord
    {
        public ImageFeature<float>[] observerFeatures { get; set; }
    }
}
