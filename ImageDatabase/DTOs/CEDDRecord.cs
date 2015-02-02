using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.DTOs
{
    [Serializable]
    public class CEDDRecord : ImageRecord
    {
        public double[] CEDDDiscriptor { get; set; }     
    }
}
