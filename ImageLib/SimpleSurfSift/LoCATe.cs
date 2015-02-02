using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSurfSift
{
    public class LoCATe
    {
        public List<double[]> extract(Bitmap image, string detector)
        {
            CEDD cedd = new CEDD();
            Bitmap bmpImage = new Bitmap(image);

            createPoints pointsCreator = new createPoints();
            List<Keypoint> keypointsList = null;
            if (detector == "SURF")
                keypointsList = pointsCreator.usingSurf(image);
            else if (detector == "SIFT")
                keypointsList = pointsCreator.usingSift(image);
            else
                throw new Exception("Cannot recognize Detector");

            #region LoCATe
            Rectangle cloneRect;            
            double[] ceddDescriptor;
            List<double[]> tilesDescriptors = new List<double[]>();

            Object thisLock = new Object(); 
            Parallel.ForEach(keypointsList, myKeypoint =>
            {
                Bitmap bmpCrop;
                cloneRect = new Rectangle((int)(myKeypoint.X - (int)myKeypoint.Size / 2), (int)(myKeypoint.Y - (int)myKeypoint.Size / 2), (int)myKeypoint.Size, (int)myKeypoint.Size);
                lock (thisLock)
                {
                    bmpCrop = new Bitmap(bmpImage.Clone(cloneRect, bmpImage.PixelFormat));
                }
                
                ceddDescriptor = cedd.Apply(new Bitmap(bmpCrop));
                lock (thisLock)
                {
                    tilesDescriptors.Add(ceddDescriptor);
                }
            });

            //foreach (Keypoint myKeypoint in keypointsList)
            //{
            //    cloneRect = new Rectangle((int)(myKeypoint.X - (int)myKeypoint.Size / 2), (int)(myKeypoint.Y - (int)myKeypoint.Size / 2), (int)myKeypoint.Size, (int)myKeypoint.Size);
            //    bmpCrop = new Bitmap(bmpImage.Clone(cloneRect, bmpImage.PixelFormat));

            //    ceddDescriptor = cedd.Apply(new Bitmap(bmpCrop));
                
            //}
            #endregion

            return tilesDescriptors;
        }

    }
}