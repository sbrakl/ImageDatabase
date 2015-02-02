using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


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

namespace CEDD_Descriptor
{
    public class CEDD
    {
        public struct MaskResults
        {
            public double Mask1;
            public double Mask2;
            public double Mask3;
            public double Mask4;
            public double Mask5;

        }

        public struct Neighborhood 
        {
            public double Area1;
            public double Area2;
            public double Area3;
            public double Area4;
            
            // Area 1       Area2
            // Area 3       Area4

        }

        public double T0;
        public double T1;
        public double T2;
        public double T3;
        public bool Compact;

        public CEDD(double Th0, double Th1, double Th2, double Th3,bool CompactDescriptor)
        {
            this.T0 = Th0;
            this.T1 = Th1;
            this.T2 = Th2;
            this.T3 = Th3;
            this.Compact = CompactDescriptor;


        }

        public CEDD()
        {
            this.T0 = 14;
            this.T1 = 0.68;
            this.T2 = 0.98;
            this.T3 = 0.98;
         

        }

        // Extract the descriptor
        public double[] Apply(Bitmap srcImg)
        {            
            Fuzzy10Bin Fuzzy10 = new Fuzzy10Bin(false);
            Fuzzy24Bin Fuzzy24 = new Fuzzy24Bin(false);
            RGB2HSV HSVConverter = new RGB2HSV();
            int[] HSV = new int[3];

            double[] Fuzzy10BinResultTable = new double[10];
            double[] Fuzzy24BinResultTable = new double[24];
            double[] CEDD = new double[144];
           

            int width = srcImg.Width;
            int height = srcImg.Height;

           
            double[,] ImageGrid = new double[width, height];
            double[,] PixelCount = new double[2, 2];
            int[,] ImageGridRed = new int[width, height];
            int[,] ImageGridGreen = new int[width, height];
            int[,] ImageGridBlue = new int[width, height];
            int NumberOfBlocks = 1600; // blocks
            int Step_X =  (int)Math.Floor(width / Math.Sqrt(NumberOfBlocks));
            int Step_Y = (int)Math.Floor(height / Math.Sqrt(NumberOfBlocks));

            if ((Step_X % 2) != 0)
            {
                Step_X = Step_X - 1;
            }
            if ((Step_Y % 2) != 0)
            {
                Step_Y = Step_Y - 1;
            }


            if (Step_Y < 2) Step_Y = 2;
            if (Step_X < 2) Step_X = 2;

            int[] Edges = new int[6]; 

            MaskResults MaskValues;
            Neighborhood PixelsNeighborhood;

           PixelFormat fmt = (srcImg.PixelFormat == PixelFormat.Format8bppIndexed) ?
                        PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;
        
            for (int i = 0; i < 144; i++)
            {

                CEDD[i] = 0;

            }

            //****************
            //Incase below unsafe code gives error, uncomment the slow GetPixel code and 
            //comment the unsafe code
            //****************
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        byte red = srcImg.GetPixel(x, y).R;
            //        byte green = srcImg.GetPixel(x, y).G;
            //        byte blue = srcImg.GetPixel(x, y).B;
            //        ImageGrid[x, y] = (0.299f * red + 0.587f * green + 0.114f * red);
            //        ImageGridRed[x, y] = (int)red;
            //        ImageGridGreen[x, y] = (int)green;
            //        ImageGridBlue[x, y] = (int)blue;
            //    }
            //}


            BitmapData srcData = srcImg.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly, fmt);

            int offset = srcData.Stride - ((fmt == PixelFormat.Format8bppIndexed) ? width : width * 3);

            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer();                

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src += 3)
                    {


                        ImageGrid[x, y] = (0.299f * src[2] + 0.587f * src[1] + 0.114f * src[0]);
                        ImageGridRed[x, y] = (int)src[2];
                        ImageGridGreen[x, y] = (int)src[1];
                        ImageGridBlue[x, y] = (int)src[0];


                    }

                    src += offset;


                }

            }

            srcImg.UnlockBits(srcData);


            int[] CororRed = new int[Step_Y * Step_X];
            int[] CororGreen = new int[Step_Y * Step_X];
            int[] CororBlue = new int[Step_Y * Step_X];

            int[] CororRedTemp = new int[Step_Y * Step_X];
            int[] CororGreenTemp = new int[Step_Y * Step_X];
            int[] CororBlueTemp = new int[Step_Y * Step_X];
                    
            int MeanRed , MeanGreen , MeanBlue = 0;
            int T = -1;


            int TempSum = 0;
            double Max = 0;

            int TemoMAX_X = Step_X * (int)Math.Sqrt(NumberOfBlocks);
            int TemoMAX_Y = Step_Y * (int)Math.Sqrt(NumberOfBlocks); ;


            for (int y = 0; y < TemoMAX_Y; y += Step_Y)
            {

                for (int x = 0; x < TemoMAX_X; x += Step_X)
                {


                    MeanRed = 0;
                    MeanGreen = 0;
                    MeanBlue = 0;
                    PixelsNeighborhood.Area1 = 0;
                    PixelsNeighborhood.Area2 = 0;
                    PixelsNeighborhood.Area3 = 0;
                    PixelsNeighborhood.Area4 = 0;
                    Edges[0] = -1;
                    Edges[1] = -1;
                    Edges[2] = -1;
                    Edges[3] = -1;
                    Edges[4] = -1;
                    Edges[5] = -1;

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            PixelCount[i, j] = 0;
                        }

                    }

                   TempSum = 0;

                    for (int i = y; i < y + Step_Y; i ++)
                    {
                        for (int j = x; j < x+ Step_X; j ++)
                        {
                            // Color Information
                            CororRed[TempSum] = ImageGridRed[j, i];
                            CororGreen[TempSum] = ImageGridGreen[j, i];
                            CororBlue[TempSum] = ImageGridBlue[j, i];

                            CororRedTemp[TempSum] = ImageGridRed[j, i];
                            CororGreenTemp[TempSum] = ImageGridGreen[j, i];
                            CororBlueTemp[TempSum] = ImageGridBlue[j, i];

                            TempSum++;


                            // Texture Information

                            if (j < (x + Step_X / 2) && i < (y + Step_Y / 2)) PixelsNeighborhood.Area1 += (ImageGrid[j, i]);
                            if (j >= (x + Step_X / 2) && i < (y + Step_Y / 2)) PixelsNeighborhood.Area2 += (ImageGrid[j, i]);
                            if (j < (x + Step_X / 2) && i >= (y + Step_Y / 2)) PixelsNeighborhood.Area3 += (ImageGrid[j, i]);
                            if (j >= (x + Step_X / 2) && i >= (y + Step_Y / 2)) PixelsNeighborhood.Area4 += (ImageGrid[j, i]);



                        }

                    }


                    PixelsNeighborhood.Area1 = (int)(PixelsNeighborhood.Area1 * (4.0 / (Step_X * Step_Y)));

                    PixelsNeighborhood.Area2 = (int)(PixelsNeighborhood.Area2 * (4.0 / (Step_X * Step_Y)));

                    PixelsNeighborhood.Area3 = (int)(PixelsNeighborhood.Area3 * (4.0 / (Step_X * Step_Y)));

                    PixelsNeighborhood.Area4 = (int)(PixelsNeighborhood.Area4 * (4.0 / (Step_X * Step_Y)));


                    MaskValues.Mask1 = Math.Abs(PixelsNeighborhood.Area1 * 2 + PixelsNeighborhood.Area2 * -2 + PixelsNeighborhood.Area3 * -2 + PixelsNeighborhood.Area4 * 2);
                    MaskValues.Mask2 = Math.Abs(PixelsNeighborhood.Area1 * 1 + PixelsNeighborhood.Area2 * 1 + PixelsNeighborhood.Area3 * -1 + PixelsNeighborhood.Area4 * -1);
                    MaskValues.Mask3 = Math.Abs(PixelsNeighborhood.Area1 * 1 + PixelsNeighborhood.Area2 * -1 + PixelsNeighborhood.Area3 * 1 + PixelsNeighborhood.Area4 * -1);
                    MaskValues.Mask4 = Math.Abs(PixelsNeighborhood.Area1 * Math.Sqrt(2) + PixelsNeighborhood.Area2 * 0 + PixelsNeighborhood.Area3 * 0 + PixelsNeighborhood.Area4 * -Math.Sqrt(2));
                    MaskValues.Mask5 = Math.Abs(PixelsNeighborhood.Area1 * 0 + PixelsNeighborhood.Area2 * Math.Sqrt(2) + PixelsNeighborhood.Area3 * -Math.Sqrt(2) + PixelsNeighborhood.Area4 * 0);


                   Max = Math.Max(MaskValues.Mask1, Math.Max(MaskValues.Mask2, Math.Max(MaskValues.Mask3, Math.Max(MaskValues.Mask4, MaskValues.Mask5))));

                    MaskValues.Mask1 = MaskValues.Mask1 / Max;
                    MaskValues.Mask2 = MaskValues.Mask2 / Max;
                    MaskValues.Mask3 = MaskValues.Mask3 / Max;
                    MaskValues.Mask4 = MaskValues.Mask4 / Max;
                    MaskValues.Mask5 = MaskValues.Mask5 / Max;


                     T = -1;

                    if (Max < T0)
                    {
                        Edges[0] = 0;
                        T = 0;
                    }
                    else
                    {
                        T = -1;

                        if (MaskValues.Mask1 > T1)
                        {
                            T++;
                            Edges[T] = 1;
                        }
                        if (MaskValues.Mask2 > T2)
                        {
                            T++;
                            Edges[T] = 2;
                        }
                        if (MaskValues.Mask3 > T2)
                        {
                            T++;
                            Edges[T] = 3;
                        }
                        if (MaskValues.Mask4 > T3)
                        {
                            T++;
                            Edges[T] = 4;
                        }
                        if (MaskValues.Mask5 > T3)
                        {
                            T++;
                            Edges[T] = 5;
                        }

                    }




                   for (int i = 0; i < (Step_Y * Step_X); i++)
                    {
                        MeanRed += CororRed[i];
                        MeanGreen += CororGreen[i];
                        MeanBlue += CororBlue[i];
                    }

                    MeanRed = Convert.ToInt32(MeanRed / (Step_Y * Step_X));
                    MeanGreen = Convert.ToInt32(MeanGreen / (Step_Y * Step_X));
                    MeanBlue = Convert.ToInt32(MeanBlue / (Step_Y * Step_X));

                    HSV = HSVConverter.ApplyFilter(MeanRed, MeanGreen, MeanBlue);



                    if (this.Compact == false)
                    {
                        Fuzzy10BinResultTable = Fuzzy10.ApplyFilter(HSV[0], HSV[1], HSV[2], 2);
                        Fuzzy24BinResultTable = Fuzzy24.ApplyFilter(HSV[0], HSV[1], HSV[2], Fuzzy10BinResultTable, 2);


                        for (int i = 0; i <= T; i++)
                        {
                            for (int j = 0; j < 24; j++)
                            {

                                if (Fuzzy24BinResultTable[j] > 0) CEDD[24 * Edges[i] + j] += Fuzzy24BinResultTable[j];

                            }

                        }
                    }
                    else
                    {

                        Fuzzy10BinResultTable = Fuzzy10.ApplyFilter(HSV[0], HSV[1], HSV[2], 2);

                        for (int i = 0; i <= T; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {

                                if (Fuzzy10BinResultTable[j] > 0) CEDD[10 * Edges[i] + j] += Fuzzy10BinResultTable[j];

                            }

                        }
                    }






                }
                
            }



            double Sum = 0;

            for (int i = 0; i < 144; i++)
            {
               

                    Sum += CEDD[ i];
            }

            for (int i = 0; i < 144; i++)
            {


                CEDD[i] = CEDD[i]/Sum;
            }


            CEDDQuant Quantization = new CEDDQuant();
            
            CEDD = Quantization.Apply(CEDD);

            return (CEDD);

        }

        //Compare CEDD Discriptor using TanimotoClassifier algorimthm
        public static double Compare(double[] Table1, double[] Table2)
        {
            double Result = 0;
            double Temp1 = 0;
            double Temp2 = 0;

            double TempCount1 = 0, TempCount2 = 0, TempCount3 = 0;

            for (int i = 0; i < Table1.Length; i++)
            {
                Temp1 += Table1[i];
                Temp2 += Table2[i];
            }

            if (Temp1 == 0 || Temp2 == 0) Result = 100;
            if (Temp1 == 0 && Temp2 == 0) Result = 0;

            if (Temp1 > 0 && Temp2 > 0)
            {
                for (int i = 0; i < Table1.Length; i++)
                {
                    TempCount1 += (Table1[i] / Temp1) * (Table2[i] / Temp2);
                    TempCount2 += (Table2[i] / Temp2) * (Table2[i] / Temp2);
                    TempCount3 += (Table1[i] / Temp1) * (Table1[i] / Temp1);

                }

                Result = (100 - 100 * (TempCount1 / (TempCount2 + TempCount3 - TempCount1))); //Tanimoto
            }

            return (Result);

        }

    }
}
