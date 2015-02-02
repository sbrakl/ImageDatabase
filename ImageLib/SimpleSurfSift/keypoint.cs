using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSurfSift
{
    public class Keypoint
    {
        public float X;
        public float Y;
        public float Size;

        public Keypoint(float xx, float yy, float s)
        {
            X = xx;
            Y = yy;
            Size = s;
        }
    }
}
