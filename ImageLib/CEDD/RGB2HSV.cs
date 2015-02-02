
// 
/*
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * 
 * Savvas Chatzichristofis
 * Download the latest version from http://www.chatzichristofis.info
 * 
 * Details regarding these descriptors can be found at the following papers: (in other words, if you use these descriptors in your scientific work, we kindly ask you to cite one or more of the following papers  )
 *
 * S. A. Chatzichristofis and Y. S. Boutalis, “CEDD: COLOR AND EDGE DIRECTIVITY DESCRIPTOR – A COMPACT DESCRIPTOR FOR IMAGE INDEXING AND RETRIEVAL.”, «6th International Conference in advanced research on Computer Vision Systems (ICVS)», Lecture Notes in Computer Science (LNCS), pp.312-322, May 12 to 15, 2008, Santorini, Greece.
 *
 * S. A. Chatzichristofis, Y. S. Boutalis and M. Lux, “SELECTION OF THE PROPER COMPACT COMPOSIDE DESCRIPTOR FOR IMPROVING CONTENT BASED IMAGE RETRIEVAL.”, «The Sixth IASTED International Conference on Signal Processing, Pattern Recognition and Applications (SPPRA)», ACTA PRESS, pp.134-140, February 17 to 19, 2009, Innsbruck, Austria.
 * 
 * 
 * 
 * VER 1.01 April 1st 2013
 * schatzic@ee.duth.gr
 *

 */

using System;
using System.Collections.Generic;
using System.Text;

namespace CEDD_Descriptor
{
    class RGB2HSV
    {
        public int[] ApplyFilter(int red, int green, int blue)
        {
            int[] Results = new int[3];
            int HSV_H=0;
            int HSV_S = 0;
            int HSV_V = 0; 

            double MaxHSV = (double)(Math.Max(red, Math.Max(green, blue)));
            double MinHSV = (double)(Math.Min(red, Math.Min(green, blue)));

            //Παραγωγη Του V του HSV
            HSV_V = (int)(MaxHSV);



            //Παραγωγη Του S του HSV
            HSV_S = 0;
            if (MaxHSV != 0) HSV_S = (int)(255 - 255 * (MinHSV / MaxHSV));



            //Παραγωγη Του H
            if (MaxHSV != MinHSV)
            {
                int IntegerMaxHSV = (int)(MaxHSV);

                if (IntegerMaxHSV == red && green >= blue)
                {
                    HSV_H = (int)(60 * (green - blue) / (MaxHSV - MinHSV));
                }

                else if (IntegerMaxHSV == red && green < blue)
                {
                    HSV_H = (int)(359 + 60 * (green - blue) / (MaxHSV - MinHSV));
                }
                else if (IntegerMaxHSV == green)
                {
                    HSV_H = (int)(119 + 60 * (blue - red) / (MaxHSV - MinHSV));
                }
                else if (IntegerMaxHSV == blue)
                {
                    HSV_H = (int)(239 + 60 * (red - green) / (MaxHSV - MinHSV));
                }


            }
            else HSV_H = 0;

            Results[0] = HSV_H;
            Results[1] = HSV_S;
            Results[2] = HSV_V;

            return (Results);
        }

    }
}
