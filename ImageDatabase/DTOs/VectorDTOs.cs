using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.DTOs
{
    public delegate void WriteLog(string msg);

    public class SurfSettings
    {
        public double? HessianThresh;
        public double? UniquenessThreshold;
        public int? GoodMatchThreshold;
        public SurfAlgo Algorithm;
    }

    public class LocateSettings
    {
        public bool IsCodeBookNeedToBeCreated { get; set; }
        public string CodeBookFullPath { get; set; }
        public int SizeOfCodeBook { get; set; }
        public bool isLteSchemeNeedToAppy { get; set; }
        public double GoodThresholdDistance { get; set; }
        public bool IsExtendedSearch { get; set; }
    }

    public class FastSettings
    {
        public int? ThreshholdPixel;
        public double? UniquenessThreshold;
    }

    public class ObserverImage
    {
        public string ImageName;
        public Image<Gray, byte> Image;
        public string ImageFullPath;
    }

    public enum SurfAlgo
    {
        Linear,
        Flaan
    }
}
