using Accord.Imaging;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageDatabase.Query
{
    /// <summary>
    /// Class which Queries the Image in Linear fashion
    /// </summary>
    public class SurfQuery4
    {
        public List<ImageRecord> QueryImage(string queryImagePath, out string messageToLog, SurfSettings surfSetting = null)
        {
            List<ImageRecord> rtnImageList = new List<ImageRecord>();

            #region Diagnostic Region
            Stopwatch sw = new Stopwatch();
            Stopwatch sw1 = new Stopwatch();
            long _loadingTime = 0, _modelImageDectionlong = 0, _queryingTime = 0, _matchingTime = 0;
            #endregion Diagnostic Region

            #region Surf Dectator Region
            double hessianThresh = 500;
            double uniquenessThreshold = 0.8;
            int minGoodMatchPercent = 0;

            if (surfSetting != null)
            {
                hessianThresh = surfSetting.HessianThresh.Value;
                uniquenessThreshold = surfSetting.UniquenessThreshold.Value;
                minGoodMatchPercent = surfSetting.GoodMatchThreshold.Value;
            }
            float hessianThreshold2 = (float)hessianThresh / 1000000;
            SpeededUpRobustFeaturesDetector surf = new SpeededUpRobustFeaturesDetector(hessianThreshold2);
            #endregion Surf Dectator Region

            #region Get Model Dectection and Validation
            sw.Reset(); sw.Start();
            List<SpeededUpRobustFeaturePoint> modelImageSurfPoints;
            using (Bitmap modelImage = (Bitmap)Image.FromFile(queryImagePath))
            {
                modelImageSurfPoints = surf.ProcessImage(modelImage);
            }

            if (modelImageSurfPoints == null
                || modelImageSurfPoints.Count < 4)
            {
                throw new InvalidOperationException("Insuffucient interesting point in query image, try another query image");
            }
            sw.Stop();
            _modelImageDectionlong = sw.ElapsedMilliseconds;
            #endregion

            #region Search Images
            sw.Reset(); sw.Start();
            string fullFileName = Path.Combine(DirectoryHelper.SaveDirectoryPath, "SurfAccordLinear.bin");
            if (!File.Exists(fullFileName))
            {
                string exMsg = string.Format("Can't get the Surf Index at {0}, please index first", fullFileName);
                throw new FileNotFoundException(fullFileName);
            }
            using (FileStream fs = new FileStream(fullFileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf
                   = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                long fileLength = fs.Length;
                while (fs.Position < fileLength)
                {
                    SURFAccordRecord3 record = (SURFAccordRecord3)bf.Deserialize(fs);
                    KNearestNeighborMatching matcher = new KNearestNeighborMatching(2);
                    matcher.Threshold = uniquenessThreshold;
                    sw1.Start();
                    AForge.IntPoint[][] matches = matcher.Match(modelImageSurfPoints, record.SurfDescriptors);
                    sw1.Stop();
                    var countOfMatchPoint = matches[0].Length;
                    if (countOfMatchPoint > 0)
                    {
                        double totalnumberOfModelFeature = modelImageSurfPoints.Count;
                        double matchPercentage = ((totalnumberOfModelFeature - (double)countOfMatchPoint) / totalnumberOfModelFeature);
                        matchPercentage = (1 - matchPercentage) * 100;
                        matchPercentage = Math.Round(matchPercentage);
                        if (matchPercentage >= minGoodMatchPercent)
                        {
                            record.Distance = matchPercentage;
                            rtnImageList.Add(record.Clone());
                        }                        
                    }
                    record = null;
                }
                fs.Close();
            }
            sw.Stop();
            _matchingTime = sw1.ElapsedMilliseconds;
            _queryingTime = sw.ElapsedMilliseconds;
            #endregion

            string msg = String.Format("Loading: {0}, Model detection: {1}, Querying: {2}, Matching: {3}",
                                                _loadingTime, _modelImageDectionlong, _queryingTime, _matchingTime);
            messageToLog = msg;

            if (rtnImageList.Count > 0)
            {
                rtnImageList = rtnImageList.OrderByDescending(rec => rec.Distance)
                                       .ToList<ImageRecord>();
            }
            return rtnImageList;
        }
    }

    public static class DoubleExtension
    {
        public static double PercentageWithMax(this double value, double Max)
        {
            double rtnValue = (value / Max) * 100;
            return rtnValue;
        }
    }
}
