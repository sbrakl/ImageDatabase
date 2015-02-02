using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSurfSift
{
    public class SCD_Local
    {
        public List<double[]> extract(Bitmap image, string detector)
        {
            SCD_Descriptor scdLocal = new SCD_Descriptor();
            Bitmap bmpImage = new Bitmap(image);

            createPoints pointsCreator = new createPoints();
            List<Keypoint> keypointsList = null;
            if (detector == "SURF")
                keypointsList = pointsCreator.usingSurf(image);
            else if (detector == "SIFT")
                keypointsList = pointsCreator.usingSift(image);
            else
                throw new Exception("Cannot recognize Detector");

            #region SCD_Local
            Rectangle cloneRect;
            Bitmap bmpCrop;
            double[] scdDescriptor = new double[256];
            List<double[]> tilesDescriptors = new List<double[]>();

            foreach (Keypoint myKeypoint in keypointsList)
            {
                cloneRect = new Rectangle((int)(myKeypoint.X - (int)myKeypoint.Size / 2), (int)(myKeypoint.Y - (int)myKeypoint.Size / 2), (int)myKeypoint.Size, (int)myKeypoint.Size);
                bmpCrop = new Bitmap(bmpImage.Clone(cloneRect, bmpImage.PixelFormat));

                scdLocal.Apply(new Bitmap(bmpCrop), 256, 0);
                scdDescriptor = scdLocal.Norm4BitHistogram;
                tilesDescriptors.Add(scdDescriptor);
            }
            #endregion

            return tilesDescriptors;
        }

    }
}