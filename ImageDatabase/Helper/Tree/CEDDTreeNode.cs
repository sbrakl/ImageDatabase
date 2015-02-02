using ImageDatabase.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.Helper.Tree
{
    [Serializable]
    public class CEDDTreeNode : BKTreeNode
    {
        public int Id { get; set; }        
        public string ImageName { get; set; }
        public string ImagePath { get; set; }
        public double Distance { get; set; }
        public double[] CEDDDiscriptor { get; set; }
        protected override Int32 calculateDistance(BKTreeNode node)
        {
            MemorySize.Hits++;
            double[] ceddDiscriptor1 = this.CEDDDiscriptor;
            double[] ceddDiscriptor2 = ((CEDDTreeNode)node).CEDDDiscriptor;
            double dist = CEDD_Descriptor.CEDD.Compare(ceddDiscriptor1, ceddDiscriptor2);            
            return Convert.ToInt32(dist);
        }
    }
}
