using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;


namespace SimpleSurfSift
{
    public class CLD_Local
    {
        public List<double[]> extract(Bitmap image, string detector)
        {
            CLD_Descriptor cldLocal = new CLD_Descriptor();
            Bitmap bmpImage = new Bitmap(image);

            createPoints pointsCreator = new createPoints();
            List<Keypoint> keypointsList = null;
            if (detector == "SURF")
                keypointsList = pointsCreator.usingSurf(image);
            else if (detector == "SIFT")
                keypointsList = pointsCreator.usingSift(image);
            else
                throw new Exception("Cannot recognize Detector");

            #region CLD_Local
            Rectangle cloneRect;
            Bitmap bmpCrop;
            double[] result;
            int[] Y = new int[64];
            int[] Cb = new int[64];
            int[] Cr = new int[64];
            List<double[]> tilesDescriptors = new List<double[]>();
            foreach (Keypoint myKeypoint in keypointsList)
            {
                result = new double[3 * 64];
                cloneRect = new Rectangle((int)(myKeypoint.X - (int)myKeypoint.Size / 2), (int)(myKeypoint.Y - (int)myKeypoint.Size / 2), (int)myKeypoint.Size, (int)myKeypoint.Size);
                bmpCrop = new Bitmap(bmpImage.Clone(cloneRect, bmpImage.PixelFormat));

                cldLocal.Apply(new Bitmap(bmpCrop));
                Y = cldLocal.YCoeff;
                Cb = cldLocal.CbCoeff;
                Cr = cldLocal.CrCoeff;
                for (int i = 0; i < 64; i++ )
                {
                    result[i] = Y[i];
                    result[64 + i] = Cb[i];
                    result[2 * 64 + i] = Cr[i];

                }
                tilesDescriptors.Add(result);
            }
            #endregion

            return tilesDescriptors;
        }

    }
}