using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.Indexers
{
    public enum emAlgo
    {
        Undetermined = 0,
        pHash = 1,
        RBGHistogram = 2,
        bhattacharyya = 3,
        CEDD = 4,
        SURF = 5,
        AccordSurf = 6,
        Locate = 7
    }

    public enum emCEDDAlgo
    {
        Linear = 0,
        BKTree = 1
    }

}
