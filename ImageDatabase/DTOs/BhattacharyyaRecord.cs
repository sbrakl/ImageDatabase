using System;

namespace ImageDatabase.DTOs
{
    [Serializable]
    public class BhattacharyyaRecord : ImageRecord
    {
        public double[] NormalizedHistogram { get; set; }        
    }
}
