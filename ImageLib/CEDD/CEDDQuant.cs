
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
    class CEDDQuant
    {

        private double[] QuantTable =
                    {180.19686541079636,23730.024499150866,61457.152912541605,113918.55437576842,179122.46400035513,260980.3325940354,341795.93301552488,554729.98648386425 };


        double[] QuantTable2 =
                    { 209.25176965926232, 22490.5872862417345, 60250.8935141849988, 120705.788057580583, 181128.08709063051, 234132.081356900555, 325660.617733105708, 520702.175858657472 };

        double[] QuantTable3 =
                    { 405.4642173212585, 4877.9763319071481, 10882.170090625908, 18167.239081219657, 27043.385568785292, 38129.413201299016, 52675.221316293857, 79555.402607004813 };

        double[] QuantTable4 =
                    { 405.4642173212585, 4877.9763319071481, 10882.170090625908, 18167.239081219657, 27043.385568785292, 38129.413201299016, 52675.221316293857, 79555.402607004813 };


        double[] QuantTable5 =
                    { 968.88475977695578, 10725.159033657819, 24161.205360376698, 41555.917344385321, 62895.628446402261, 93066.271379694881, 136976.13317822068, 262897.86056221306 };

        double[] QuantTable6 =
                    { 968.88475977695578, 10725.159033657819, 24161.205360376698, 41555.917344385321, 62895.628446402261, 93066.271379694881, 136976.13317822068, 262897.86056221306 };


        public double[] Apply(double[] Local_Edge_Histogram)
        {
            double[] Edge_HistogramElement = new double[Local_Edge_Histogram.Length];
            double[] ElementsDistance = new double[8];
            double Max = 1;

            for (int i = 0; i < 24; i++)
            {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++)
                {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable[j]/1000000);
                }
                Max = 1;
                for (int j = 0; j < 8; j++)
                {
                    if (ElementsDistance[j] < Max)
                    {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }

               
            }


            for (int i = 24; i < 48; i++)
            {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++)
                {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable2[j] / 1000000);
                }
                Max = 1;
                for (int j = 0; j < 8; j++)
                {
                    if (ElementsDistance[j] < Max)
                    {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }



            for (int i = 48; i < 72; i++)
            {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++)
                {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable3[j] / 1000000);
                }
                Max = 1;
                for (int j = 0; j < 8; j++)
                {
                    if (ElementsDistance[j] < Max)
                    {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }



            for (int i = 72; i < 96; i++)
            {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++)
                {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable4[j] / 1000000);
                }
                Max = 1;
                for (int j = 0; j < 8; j++)
                {
                    if (ElementsDistance[j] < Max)
                    {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 96; i < 120; i++)
            {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++)
                {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable5[j] / 1000000);
                }
                Max = 1;
                for (int j = 0; j < 8; j++)
                {
                    if (ElementsDistance[j] < Max)
                    {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }



            for (int i = 120; i < 144; i++)
            {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++)
                {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable6[j] / 1000000);
                }
                Max = 1;
                for (int j = 0; j < 8; j++)
                {
                    if (ElementsDistance[j] < Max)
                    {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }






            return Edge_HistogramElement;
        }
    }
}
