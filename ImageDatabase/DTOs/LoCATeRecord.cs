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
    public class LoCATeRecord: ImageRecord
    {
        public List<double[]> LoCATeDescriptors { get; set; }
    }

    [Serializable]
    public class LoCaTeBoWRecord : ImageRecord
    {
        public double[] VisaulWord { get; set; }        
    }

    [Serializable]
    public class LoCaTeRanker : ImageRecord
    {
        public int LocateRank { get; set; }
        public int SurfRank { get; set; }
        public int CEDDRank { get; set; }
        public double CombinedRank { get; set; }       
    }

    [Serializable]
    public class LoCaTeDataSet
    {
        public List<LoCaTeBoWRecord> AllImageRecordSet {get; set;}        
        public int[] HistogramSumOfAllVisualWords { get; set; }
    }
}
