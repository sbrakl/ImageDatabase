using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.DTOs
{
    [Serializable]
    public class SURFRecord1 : ImageRecord
    {
        public int IndexStart { get; set; }
        public int IndexEnd { get; set; }        
    }

    [Serializable]
    public class SurfDataSet
    {
        public List<SURFRecord1> SurfImageIndexRecord { get; set; }
        public Matrix<float> SuperMatrix { get; set; }
    }   
}
