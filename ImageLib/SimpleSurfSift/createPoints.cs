using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace SimpleSurfSift
{
    public class createPoints
    {
        public List<Keypoint> usingSurf(Bitmap image)
        {
            SURFDetector surf = new SURFDetector(750, false);
            Image<Gray, Byte> modelImage = new Image<Gray, byte>(new Bitmap(image));
            VectorOfKeyPoint modelKeyPoints = surf.DetectKeyPointsRaw(modelImage, null);
            MKeyPoint[] keypoints = modelKeyPoints.ToArray();

            Keypoint key;
            List<Keypoint> keypointsList = new List<Keypoint>();
            foreach (MKeyPoint keypoint in keypoints)
            {
                key = new Keypoint(keypoint.Point.X, keypoint.Point.Y, keypoint.Size);
                keypointsList.Add(key);
            }

            return keypointsList;
        }

        public List<Keypoint> usingSift(Bitmap image)
        {
            SIFTDetector sift = new SIFTDetector();
            Image<Gray, Byte> modelImage = new Image<Gray, byte>(new Bitmap(image));
            VectorOfKeyPoint modelKeyPoints = sift.DetectKeyPointsRaw(modelImage, null);
            MKeyPoint[] keypoints = modelKeyPoints.ToArray();

            Keypoint key;
            List<Keypoint> keypointsList = new List<Keypoint>();

            foreach (MKeyPoint keypoint in keypoints)
            {
                key = new Keypoint(keypoint.Point.X, keypoint.Point.Y, keypoint.Size);
                keypointsList.Add(key);
            }

            return keypointsList;
        }
    }
}
