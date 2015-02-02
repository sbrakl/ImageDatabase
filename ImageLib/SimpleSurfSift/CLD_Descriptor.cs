using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SimpleSurfSift
{
    /*
 * Java Version
 * This file is part of Caliph & Emir.
 *
 * Caliph & Emir is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Caliph & Emir is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Caliph & Emir; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright statement:
 * --------------------
 * (c) 2002-2005 by Mathias Lux (mathias@juggle.at)
 * http://www.juggle.at, http://caliph-emir.sourceforge.net
 * 
 * 
 * C# Version
 * This Code is a modification of Caliph& Emir Project.
 * Part of img(Rummager) project
 * © 2006-2008 Savvas Chatzichristofis
 * http://savvash.blogspot.com
 * savvash@gmail.com, schatzic@ee.duth.gr
 * If you use this code please cite:
 * Mathias Lux, S. A. Chatzichristofis, "LIRe: Lucene Image Retrieval - An Extensible Java CBIR Library", ACM International Conference on Multimedia 2008, Vancouver, BC, Canada October 27 – 31, 2008, Open Source Application Competition.

 * 
 * */


    class CLD_Descriptor
    {

        // static final boolean debug = true;
        protected int[,] shape;
        protected int _ySize, _xSize;
        protected Bitmap srcImg;

        protected static int[] availableCoeffNumbers = { 1, 3, 6, 10, 15, 21, 28, 64 };

        public int[] YCoeff, CbCoeff, CrCoeff;

        protected int numCCoeff = 3, numYCoeff = 6;

        protected static int[] arrayZigZag = {
            0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5,
            12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63
    };

        protected static double[,] arrayCosin = {
            {
                    3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01,
                    3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01
            },
            {
                    4.903926e-01, 4.157348e-01, 2.777851e-01, 9.754516e-02,
                    -9.754516e-02, -2.777851e-01, -4.157348e-01, -4.903926e-01
            },
            {
                    4.619398e-01, 1.913417e-01, -1.913417e-01, -4.619398e-01,
                    -4.619398e-01, -1.913417e-01, 1.913417e-01, 4.619398e-01
            },
            {
                    4.157348e-01, -9.754516e-02, -4.903926e-01, -2.777851e-01,
                    2.777851e-01, 4.903926e-01, 9.754516e-02, -4.157348e-01
            },
            {
                    3.535534e-01, -3.535534e-01, -3.535534e-01, 3.535534e-01,
                    3.535534e-01, -3.535534e-01, -3.535534e-01, 3.535534e-01
            },
            {
                    2.777851e-01, -4.903926e-01, 9.754516e-02, 4.157348e-01,
                    -4.157348e-01, -9.754516e-02, 4.903926e-01, -2.777851e-01
            },
            {
                    1.913417e-01, -4.619398e-01, 4.619398e-01, -1.913417e-01,
                    -1.913417e-01, 4.619398e-01, -4.619398e-01, 1.913417e-01
            },
            {
                    9.754516e-02, -2.777851e-01, 4.157348e-01, -4.903926e-01,
                    4.903926e-01, -4.157348e-01, 2.777851e-01, -9.754516e-02
            }
    };
        protected static int[,] weightMatrix = new int[3, 64];
        protected Bitmap colorLayoutImage;

        public void Apply(Bitmap srcImg)
        {

            this.srcImg = srcImg;
            _ySize = srcImg.Height;
            _xSize = srcImg.Width;
            init();
        }

        private void init()
        {
            shape = new int[3, 64];
            YCoeff = new int[64];
            CbCoeff = new int[64];
            CrCoeff = new int[64];
            colorLayoutImage = null;
            extract();
        }

        private void extract()
        {

            createShape();

            int[] Temp1 = new int[64];
            int[] Temp2 = new int[64];
            int[] Temp3 = new int[64];

            for (int i = 0; i < 64; i++)
            {
                Temp1[i] = shape[0, i];
                Temp2[i] = shape[1, i];
                Temp3[i] = shape[2, i];
            }


            Fdct(Temp1);
            Fdct(Temp2);
            Fdct(Temp3);


            for (int i = 0; i < 64; i++)
            {
                shape[0, i] = Temp1[i];
                shape[1, i] = Temp2[i];
                shape[2, i] = Temp3[i];
            }


            YCoeff[0] = quant_ydc(shape[0, 0] >> 3) >> 1;
            CbCoeff[0] = quant_cdc(shape[1, 0] >> 3);
            CrCoeff[0] = quant_cdc(shape[2, 0] >> 3);

            //quantization and zig-zagging
            for (int i = 1; i < 64; i++)
            {
                YCoeff[i] = quant_ac((shape[0, (arrayZigZag[i])]) >> 1) >> 3;
                CbCoeff[i] = quant_ac(shape[1, (arrayZigZag[i])]) >> 3;
                CrCoeff[i] = quant_ac(shape[2, (arrayZigZag[i])]) >> 3;
            }


        }


        private void createShape()
        {
            int y_axis, x_axis;
            int i, k, j;
            long[,] sum = new long[3, 64];
            int[] cnt = new int[64];
            double yy = 0.0;
            int R, G, B;

            //init of the blocks
            for (i = 0; i < 64; i++)
            {
                cnt[i] = 0;
                sum[0, i] = 0;
                sum[1, i] = 0;
                sum[2, i] = 0;
                shape[0, i] = 0;
                shape[1, i] = 0;
                shape[2, i] = 0;
            }

            ///

            PixelFormat fmt = (srcImg.PixelFormat == PixelFormat.Format8bppIndexed) ?
                              PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;

            BitmapData srcData = srcImg.LockBits(
               new Rectangle(0, 0, _xSize, _ySize),
               ImageLockMode.ReadOnly, fmt);

            int offset = srcData.Stride - srcData.Width * 3;


            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer();

                for (int yi = 0; yi < _ySize; yi++)
                {
                    for (int xi = 0; xi < _xSize; xi++, src += 3)
                    {

                        R = src[2];
                        G = src[1];
                        B = src[0];


                        y_axis = (int)(yi / (_ySize / 8.0));
                        x_axis = (int)(xi / (_xSize / 8.0));

                        k = (y_axis << 3) + x_axis;

                        //RGB to YCbCr, partition and average-calculation
                        yy = (0.299 * R + 0.587 * G + 0.114 * B) / 256.0;
                        sum[0, k] += (int)(219.0 * yy + 16.5); // Y
                        sum[1, k] += (int)(224.0 * 0.564 * (B / 256.0 * 1.0 - yy) + 128.5); // Cb
                        sum[2, k] += (int)(224.0 * 0.713 * (R / 256.0 * 1.0 - yy) + 128.5); // Cr
                        cnt[k]++;


                    }

                    src += offset;


                }

            }

            srcImg.UnlockBits(srcData);


            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    for (k = 0; k < 3; k++)
                    {
                        if (cnt[(i << 3) + j] != 0)
                            shape[k, (i << 3) + j] = (int)(sum[k, (i << 3) + j] / cnt[(i << 3) + j]);
                        else
                            shape[k, (i << 3) + j] = 0;
                    }
                }
            }
        }

        private static void Fdct(int[] shapes)
        {
            int i, j, k;
            double s;
            double[] dct = new double[64];

            //calculation of the cos-values of the second sum
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    s = 0.0;
                    for (k = 0; k < 8; k++)
                        s += arrayCosin[j, k] * shapes[8 * i + k];
                    dct[8 * i + j] = s;
                }
            }

            for (j = 0; j < 8; j++)
            {
                for (i = 0; i < 8; i++)
                {
                    s = 0.0;
                    for (k = 0; k < 8; k++)
                        s += arrayCosin[i, k] * dct[8 * k + j];
                    shapes[8 * i + j] = (int)Math.Floor(s + 0.499999);
                }
            }
        }

        private static int quant_ydc(int i)
        {
            int j;
            if (i > 192)
                j = 112 + ((i - 192) >> 2);
            else if (i > 160)
                j = 96 + ((i - 160) >> 1);
            else if (i > 96)
                j = 32 + (i - 96);
            else if (i > 64)
                j = 16 + ((i - 64) >> 1);
            else
                j = i >> 2;

            return j;
        }

        private static int quant_cdc(int i)
        {
            int j;
            if (i > 191)
                j = 63;
            else if (i > 160)
                j = 56 + ((i - 160) >> 2);
            else if (i > 144)
                j = 48 + ((i - 144) >> 1);
            else if (i > 112)
                j = 16 + (i - 112);
            else if (i > 96)
                j = 8 + ((i - 96) >> 1);
            else if (i > 64)
                j = (i - 64) >> 2;
            else
                j = 0;

            return j;
        }


        private static int quant_ac(int i)
        {
            int j;

            if (i > 255)
                i = 255;

            if (i < -256)
                i = -256;
            if ((Math.Abs(i)) > 127)
                j = 64 + ((Math.Abs(i)) >> 2);
            else if ((Math.Abs(i)) > 63)
                j = 32 + ((Math.Abs(i)) >> 1);
            else
                j = Math.Abs(i);
            j = (i < 0) ? -j : j;

            j += 128;


            return j;
        }



        /**
          * Nicht alle Werte sind laut MPEG-7 erlaubt ....
          */
        private static int getRightCoeffNumber(int num)
        {
            int val = 0;
            if (num <= 1)
                val = 1;
            else if (num <= 3)
                val = 3;
            else if (num <= 6)
                val = 6;
            else if (num <= 10)
                val = 10;
            else if (num <= 15)
                val = 15;
            else if (num <= 21)
                val = 21;
            else if (num <= 28)
                val = 28;
            else if (num > 28) val = 64;
            return val;
        }






        private static Bitmap YCrCb2RGB(int[,] rgbSmallImage)
        {
            Bitmap br = new Bitmap(240, 240, PixelFormat.Format24bppRgb);

            double rImage, gImage, bImage;

            ///
            BitmapData srcData = br.LockBits(new Rectangle(0, 0, 240, 240),
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int offset = srcData.Stride - srcData.Width * 3;


            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer();

                int i = 0;
                for (int y = 1; y <= 240; y++)
                {
                    i = 0;
                    if (y >= 30) i = 8;
                    if (y >= 60) i = 16;
                    if (y >= 90) i = 24;
                    if (y >= 120) i = 32;
                    if (y >= 150) i = 40;
                    if (y >= 180) i = 48;
                    if (y >= 210) i = 56;

                    for (int x = 0; x < 8; x++)
                    {

                        for (int j = 0; j < 30; j++)
                        {
                            rImage = ((rgbSmallImage[0, i] - 16.0) * 256.0) / 219.0;
                            gImage = ((rgbSmallImage[1, i] - 128.0) * 256.0) / 224.0;
                            bImage = ((rgbSmallImage[2, i] - 128.0) * 256.0) / 224.0;

                            src[2] = (byte)Math.Max(0, (int)((rImage) + (1.402 * bImage) + 0.5)); //R
                            src[1] = (byte)Math.Max(0, (int)((rImage) + (-0.34413 * gImage) + (-0.71414 * bImage) + 0.5));  //G
                            src[0] = (byte)Math.Max(0, (int)((rImage) + (1.772 * gImage) + 0.5)); //B
                            src += 3;
                        }
                        i++;

                    }

                    src += offset;


                }

            }

            br.UnlockBits(srcData);

            ////


            return br;
        }

        public Bitmap getColorLayoutImage()
        {
            if (colorLayoutImage != null)
                return colorLayoutImage;
            else
            {
                int[,] smallReImage = new int[3, 64];

                // inverse quantization and zig-zagging
                smallReImage[0, 0] = IquantYdc((YCoeff[0]));
                smallReImage[1, 0] = IquantCdc((CbCoeff[0]));
                smallReImage[2, 0] = IquantCdc((CrCoeff[0]));

                for (int i = 1; i < 64; i++)
                {
                    smallReImage[0, (arrayZigZag[i])] = IquantYac((YCoeff[i]));
                    smallReImage[1, (arrayZigZag[i])] = IquantCac((CbCoeff[i]));
                    smallReImage[2, (arrayZigZag[i])] = IquantCac((CrCoeff[i]));
                }

                // inverse Discrete Cosine Transform

                int[] Temp1 = new int[64];
                int[] Temp2 = new int[64];
                int[] Temp3 = new int[64];

                for (int i = 0; i < 64; i++)
                {
                    Temp1[i] = smallReImage[0, i];
                    Temp2[i] = smallReImage[1, i];
                    Temp3[i] = smallReImage[2, i];

                }

                Idct(Temp1);
                Idct(Temp2);
                Idct(Temp3);

                for (int i = 0; i < 64; i++)
                {
                    smallReImage[0, i] = Temp1[i];
                    smallReImage[1, i] = Temp2[i];
                    smallReImage[2, i] = Temp3[i];
                }


                // YCrCb to RGB
                colorLayoutImage = YCrCb2RGB(smallReImage);
                return colorLayoutImage;
            }
        }

        private static void Idct(int[] iShapes)
        {
            int u, v, k;
            double s;
            double[] dct = new double[64];

            //calculation of the cos-values of the second sum
            for (u = 0; u < 8; u++)
            {
                for (v = 0; v < 8; v++)
                {
                    s = 0.0;
                    for (k = 0; k < 8; k++)
                        s += arrayCosin[k, v] * iShapes[8 * u + k];
                    dct[8 * u + v] = s;
                }
            }

            for (v = 0; v < 8; v++)
            {
                for (u = 0; u < 8; u++)
                {
                    s = 0.0;
                    for (k = 0; k < 8; k++)
                        s += arrayCosin[k, u] * dct[8 * k + v];
                    iShapes[8 * u + v] = (int)Math.Floor(s + 0.499999);
                }
            }
        }


        private static int IquantYdc(int i)
        {
            int j;
            i = i << 1;
            if (i > 112)
                j = 194 + ((i - 112) << 2);
            else if (i > 96)
                j = 162 + ((i - 96) << 1);
            else if (i > 32)
                j = 96 + (i - 32);
            else if (i > 16)
                j = 66 + ((i - 16) << 1);

            else
                j = i << 2;

            return j << 3;
        }

        private static int IquantCdc(int i)
        {
            int j;
            if (i > 63)
                j = 192;
            else if (i > 56)
                j = 162 + ((i - 56) << 2);
            else if (i > 48)
                j = 145 + ((i - 48) << 1);
            else if (i > 16)
                j = 112 + (i - 16);
            else if (i > 8)
                j = 97 + ((i - 8) << 1);
            else if (i > 0)
                j = 66 + (i << 2);
            else
                j = 64;
            return j << 3;
        }

        private static int IquantYac(int i)
        {
            int j;
            i = i << 3;
            i -= 128;
            if (i > 128)
                i = 128;
            if (i < -128)
                i = -128;
            if ((Math.Abs(i)) > 96)
                j = ((Math.Abs(i)) << 2) - 256;
            else if ((Math.Abs(i)) > 64)
                j = ((Math.Abs(i)) << 1) - 64;
            else
                j = Math.Abs(i);
            j = (i < 0) ? -j : j;

            return j << 1;
        }

        private static int IquantCac(int i)
        {
            int j;
            i = i << 3;
            i -= 128;
            if (i > 128)
                i = 128;
            if (i < -128)
                i = -128;
            if ((Math.Abs(i)) > 96)
                j = ((Math.Abs(i) << 2) - 256);
            else if ((Math.Abs(i)) > 64)
                j = ((Math.Abs(i) << 1) - 64);
            else
                j = Math.Abs(i);
            j = (i < 0) ? -j : j;

            return j;
        }


    }

}