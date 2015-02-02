using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using ImageDatabase.Helper.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace ImageDatabase.Query
{
    public class SurfQuery1
    {
        public List<ImageRecord> QueryImage(string queryImagePath, out string messageToLog, SurfSettings surfSetting = null)
        {
            List<ImageRecord> rtnImageList = new List<ImageRecord>();

            #region Diagnostic Region
            Stopwatch sw = new Stopwatch();
            long IndexingTime = 0; long QueryingTime = 0; long LoopTime = 0;            
            #endregion Diagnostic Region           

            SurfDataSet observerDataset = SurfRepository.GetSurfDataSet();
            if (observerDataset == null)
                throw new InvalidOperationException("Can't get the Surf Index, please index first");

            #region Surf Dectator Region
            double hessianThresh = 500;
            double uniquenessThreshold = 0.8;
            
            if (surfSetting != null)
            {
                hessianThresh = surfSetting.HessianThresh.Value;
                uniquenessThreshold = surfSetting.UniquenessThreshold.Value;
            }

            SURFDetector surfDectector = new SURFDetector(hessianThresh, false);
            #endregion Surf Dectator Region


            

            Matrix<float> modelDescriptors;


            using (Image<Gray, byte> modelImage = new Image<Gray, byte>(queryImagePath))
            {
                VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();
                modelDescriptors = surfDectector.DetectAndCompute(modelImage, null, modelKeyPoints);
                if (modelDescriptors.Rows < 4) throw new InvalidOperationException("Model image didn't have any significant features to detect");
                Matrix<float> superMatrix = observerDataset.SuperMatrix;

                sw.Start();                
                Emgu.CV.Flann.Index flannIndex;
                if (!SurfRepository.Exists("flannIndex"))
                {                    
                    flannIndex = new Emgu.CV.Flann.Index(superMatrix, 4);
                    SurfRepository.AddFlannIndex(flannIndex, "flannIndex");                    
                }                    
                else
                    flannIndex = SurfRepository.GetFlannIndex("flannIndex");                

                sw.Stop(); IndexingTime = sw.ElapsedMilliseconds; sw.Reset();

                var indices = new Matrix<int>(modelDescriptors.Rows, 2); // matrix that will contain indices of the 2-nearest neighbors found
                var dists = new Matrix<float>(modelDescriptors.Rows, 2); // matrix that will contain distances to the 2-nearest neighbors found

                sw.Start();
                flannIndex.KnnSearch(modelDescriptors, indices, dists, 2, 24);
                sw.Stop(); QueryingTime = sw.ElapsedMilliseconds; sw.Reset();

                List<SURFRecord1> imageList = observerDataset.SurfImageIndexRecord;
                imageList.ForEach(x => x.Distance = 0);

                //Create Interval Tree for Images
                IntervalTreeHelper.CreateTree(imageList);
                

                sw.Start();
                for (int i = 0; i < indices.Rows; i++)
                {
                    // filter out all inadequate pairs based on distance between pairs
                    if (dists.Data[i, 0] < (uniquenessThreshold * dists.Data[i, 1]))
                    {
                        var img = IntervalTreeHelper.GetImageforRange(indices[i, 0]);
                        if (img != null) img.Distance++;                      
                    }
                }
                sw.Stop(); LoopTime = sw.ElapsedMilliseconds;
                
                string msg = String.Format("Indexing: {0}, Querying: {1}, Looping: {2}", IndexingTime, QueryingTime, LoopTime);
                messageToLog = msg;

                rtnImageList = imageList.Where(x => x.Distance > surfSetting.GoodMatchThreshold).OrderByDescending(x => x.Distance).Select(x => (ImageRecord)x).ToList();
            }

           
            
            return rtnImageList;
        }        
    }
}
