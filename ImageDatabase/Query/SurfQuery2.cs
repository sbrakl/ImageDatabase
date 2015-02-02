using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace ImageDatabase.Query
{
    public class SurfQuery2
    {
        public List<ImageRecord> QueryImage(string queryImagePath,  SurfSettings surfSetting = null)
        {
            List<ImageRecord> rtnImageList = new List<ImageRecord>();

            var observerFeatureSets = SurfRepository.GetSurfRecordList();

            #region Surf Dectator Region
            double hessianThresh = 500;
            double uniquenessThreshold = 0.8;
            int minGoodMatchPercent = 50;
            
            if (surfSetting != null)
            {
                hessianThresh = surfSetting.HessianThresh.Value;
                uniquenessThreshold = surfSetting.UniquenessThreshold.Value;
                minGoodMatchPercent = surfSetting.GoodMatchThreshold.Value;
            }

            SURFDetector surfDectector = new SURFDetector(hessianThresh, false);
            #endregion

            using (Image<Gray, byte> modelImage = new Image<Gray, byte>(queryImagePath))
            {
                ImageFeature<float>[] modelFeatures = surfDectector.DetectFeatures(modelImage, null);

                if (modelFeatures.Length < 4) throw new InvalidOperationException("Model image didn't have any significant features to detect");

                Features2DTracker<float> tracker = new Features2DTracker<float>(modelFeatures);
                foreach (var surfRecord in observerFeatureSets)
                {
                    string queryImageName = System.IO.Path.GetFileName(queryImagePath);
                    string modelImageName = surfRecord.ImageName;

                    Features2DTracker<float>.MatchedImageFeature[] matchedFeatures = tracker.MatchFeature(surfRecord.observerFeatures, 2);

                    Features2DTracker<float>.MatchedImageFeature[] uniqueFeatures = Features2DTracker<float>.VoteForUniqueness(matchedFeatures, uniquenessThreshold);

                    Features2DTracker<float>.MatchedImageFeature[] uniqueRotOriFeatures = Features2DTracker<float>.VoteForSizeAndOrientation(uniqueFeatures, 1.5, 20);

                    int goodMatchCount = 0;
                    goodMatchCount = uniqueRotOriFeatures.Length;
                    bool isMatch = false;

                    double totalnumberOfModelFeature = modelFeatures.Length;
                    double matchPercentage = ((totalnumberOfModelFeature - (double)goodMatchCount) / totalnumberOfModelFeature);
                    matchPercentage = (1 - matchPercentage) * 100;
                    matchPercentage = Math.Round(matchPercentage);
                    if (matchPercentage >= minGoodMatchPercent)
                    {

                        HomographyMatrix homography =
                            Features2DTracker<float>.GetHomographyMatrixFromMatchedFeatures(uniqueRotOriFeatures);
                        if (homography != null)
                        {
                            isMatch = homography.IsValid(5);
                            if (isMatch)
                            {
                                surfRecord.Distance = matchPercentage;
                                rtnImageList.Add((ImageRecord)surfRecord);
                            }
                        }
                    }

                    //bool isMatch = false;
                    //if (uniqueFeatures.Length > 4)
                    //{
                    //    HomographyMatrix homography =
                    //        Features2DTracker<float>.GetHomographyMatrixFromMatchedFeatures(uniqueRotOriFeatures);
                    //    if (homography != null)
                    //    {
                    //        isMatch = homography.IsValid(5);
                    //    }                        
                    //}

                    //if (isMatch)
                    //{
                    //    surfRecord.Distance = goodMatchCount;
                    //    rtnImageList.Add((ImageRecord)surfRecord);
                    //}

                    //int goodMatchCount = 0;
                    //foreach (Features2DTracker<float>.MatchedImageFeature ms in matchedFeatures)
                    //{ 
                    //    if (ms.SimilarFeatures[0].Distance < uniquenessThreshold) 
                    //        goodMatchCount++;
                    //}                   

                  

                    //double totalnumberOfModelFeature = modelFeatures.Length;
                    //double matchPercentage = ((totalnumberOfModelFeature - (double)goodMatchCount) / totalnumberOfModelFeature);
                    //matchPercentage = (1 - matchPercentage) * 100;
                    //matchPercentage = Math.Round(matchPercentage);
                    //if (matchPercentage >= minGoodMatchPercent)
                    //{
                    //    surfRecord.Distance = matchPercentage;
                    //    rtnImageList.Add((ImageRecord)surfRecord);
                    //}
                }
            }
            rtnImageList = rtnImageList.OrderByDescending(x => x.Distance).ToList();
            return rtnImageList;
        }        
    }
}
