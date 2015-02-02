using Accord.MachineLearning.Structures;
using System;
using System.Collections.Generic;

namespace ImageDatabase.DTOs
{
    [Serializable]
    public class SurfAccordDataSet
    {
        public List<SURFRecord1> SurfImageIndexRecord { get; set; }
        public KDTree<int> IndexedTree { get; set; }
    }   
}
