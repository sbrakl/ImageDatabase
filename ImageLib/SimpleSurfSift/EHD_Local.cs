using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;


namespace SimpleSurfSift
{
    public class EHD_Local
    {
        public List<double[]> extract(Bitmap image, string detector)
        {
            EHD_Descriptor ehdLocal = new EHD_Descriptor(11);
            Bitmap bmpImage = new Bitmap(image);

            createPoints pointsCreator = new createPoints();
            List<Keypoint> keypointsList = null;
            if (detector == "SURF")
                keypointsList = pointsCreator.usingSurf(image);
            else if (detector == "SIFT")
                keypointsList = pointsCreator.usingSift(image);
            else
                throw new Exception("Cannot recognize Detector");

            #region EHD_Local
            Rectangle cloneRect;
            Bitmap bmpCrop;
            double[] ehdDescriptor = new double[80];
            List<double[]> tilesDescriptors = new List<double[]>();
            foreach (Keypoint myKeypoint in keypointsList)
            {
                cloneRect = new Rectangle((int)(myKeypoint.X - (int)myKeypoint.Size / 2), (int)(myKeypoint.Y - (int)myKeypoint.Size / 2), (int)myKeypoint.Size, (int)myKeypoint.Size);
                bmpCrop = new Bitmap(bmpImage.Clone(cloneRect, bmpImage.PixelFormat));

                ehdDescriptor = ehdLocal.Apply(new Bitmap(bmpCrop));
                ehdDescriptor = ehdLocal.Quant(ehdDescriptor);
                tilesDescriptors.Add(ehdDescriptor);
            }
            #endregion

            return tilesDescriptors;
        }

    }
}