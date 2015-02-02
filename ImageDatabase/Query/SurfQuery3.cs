using Accord.Imaging;
using Accord.MachineLearning.Structures;
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
    public class SurfQuery3
    {
        public List<ImageRecord> QueryImage(string queryImagePath, out string messageToLog, SurfSettings surfSetting = null)
        {
            List<ImageRecord> rtnImageList = new List<ImageRecord>();

            #region Diagnostic Region
            Stopwatch sw = new Stopwatch();
            long _loadingTime, _modelImageDectionlong, _queryingTime, _treeQuery, _loopTime = 0;
            #endregion Diagnostic Region

            #region Get KD-Tree Index
            sw.Reset(); sw.Start();
            //--------------Getting Indexed Records
            SurfAccordDataSet surfAccordDataset;
            bool isExist= CacheHelper.Get<SurfAccordDataSet>("SurfAccordDataSet", out surfAccordDataset);
            if (!isExist)
            {
                string repoFileStoragePath = Path.Combine(DirectoryHelper.SaveDirectoryPath, "SurfAccordDataSet.bin");
                if (!File.Exists(repoFileStoragePath))
                {
                    string exMsg = string.Format("Can't get the Surf Index at {0}, please index first", repoFileStoragePath);
                    throw new FileNotFoundException(exMsg);
                }
                using (FileStream s = File.OpenRead(repoFileStoragePath))
                {
                    //Polenter.Serialization.SharpSerializerBinarySettings bs =
                    //    new Polenter.Serialization.SharpSerializerBinarySettings(Polenter.Serialization.BinarySerializationMode.SizeOptimized);                
                    //Polenter.Serialization.SharpSerializer formatter = new Polenter.Serialization.SharpSerializer(bs);
                    //formatter.Serialize(surfAccordDataset, s);
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter
                        = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    surfAccordDataset = (SurfAccordDataSet)formatter.Deserialize(s);
                    s.Close();
                }
                CacheHelper.Add<SurfAccordDataSet>(surfAccordDataset, "SurfAccordDataSet");
            }           
            if (surfAccordDataset == null)
                throw new InvalidOperationException("Can't get the Surf Index, please index first");
            sw.Stop();
            _loadingTime = sw.ElapsedMilliseconds;
            #endregion

            #region Surf Dectator Region
            double hessianThresh = 500;
            double uniquenessThreshold = 0.8;
            int goodMatchDistThreshold = 0;

            if (surfSetting != null)
            {
                hessianThresh = surfSetting.HessianThresh.Value;
                uniquenessThreshold = surfSetting.UniquenessThreshold.Value;
                goodMatchDistThreshold = surfSetting.GoodMatchThreshold.Value;
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
            //------------Search Images
            Accord.MachineLearning.Structures.KDTree<int> tree = surfAccordDataset.IndexedTree;
            double[][] listofQueryDescriptors = modelImageSurfPoints.Select(ft => ft.Descriptor).ToArray();
            double[] myscores = new double[listofQueryDescriptors.Length];
            int[] labels = Enumerable.Repeat(-1, listofQueryDescriptors.Length).ToArray();                       
            for (int i = 0; i < listofQueryDescriptors.Length; i++)            
            {
                KDTreeNodeCollection<int> neighbors = tree.ApproximateNearest(listofQueryDescriptors[i],2, 90d);
                //KDTreeNodeCollection<int> neighbors = tree.Nearest(listofQueryDescriptors[i], uniquenessThreshold, 2);
                Dictionary<int, double> keyValueStore = new Dictionary<int, double>();
                double similarityDist = 0;
                foreach (KDTreeNodeDistance<int> point in neighbors)
                {
                    int label = point.Node.Value;
                    double d = point.Distance;

                    // Convert to similarity measure
                    if (keyValueStore.ContainsKey(label))
                    {
                        similarityDist = keyValueStore[label];
                        similarityDist += 1.0 / (1.0 + d);
                        keyValueStore[label] = similarityDist;
                    }
                    else
                    {
                        similarityDist = 1.0 / (1.0 + d);
                        keyValueStore.Add(label, similarityDist);
                    }
                }
                if (keyValueStore.Count > 0)
                {
                    int maxIndex = keyValueStore.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                    labels[i] = maxIndex;
                    double sumOfAllValues = keyValueStore.Values.Sum();
                    myscores[i] = keyValueStore[maxIndex] / sumOfAllValues;
                }
            }
            sw.Stop();
            _queryingTime = sw.ElapsedMilliseconds;

            sw.Reset(); sw.Start();
            List<SURFRecord1> listOfSurfImages = surfAccordDataset.SurfImageIndexRecord;
            //----------Create Interval Tree from ImageMetaData
            IntervalTreeLib.IntervalTree<SURFRecord1, int> intervalTree;
            bool isTreeExist = CacheHelper.Get<IntervalTreeLib.IntervalTree<SURFRecord1, int>>("SurfAccordIntervalTree", out intervalTree);
            if (!isTreeExist)
            {
                intervalTree = new IntervalTreeLib.IntervalTree<SURFRecord1, int>();
                foreach (var record in listOfSurfImages)
                {
                    intervalTree.AddInterval(record.IndexStart, record.IndexEnd, record);
                }
                CacheHelper.Add<IntervalTreeLib.IntervalTree<SURFRecord1, int>>(intervalTree, "SurfAccordIntervalTree");
            }

            //--------------Matching Target image similarity
            for (int i = 0; i < listofQueryDescriptors.Length; i++)
            {
                int rowNum = labels[i];
                if (rowNum == -1) continue;
                double dist = myscores[i];
                SURFRecord1 rec = intervalTree.Get(rowNum, IntervalTreeLib.StubMode.ContainsStartThenEnd).FirstOrDefault();
                rec.Distance++;
            }
            sw.Stop();            
            _loopTime = sw.ElapsedMilliseconds;
            #endregion

            string msg = String.Format("Loading: {0}, Model detection: {1}, Querying: {2}, Looping: {3}",
                                                _loadingTime, _modelImageDectionlong, _queryingTime, _loopTime);                                                                
            messageToLog = msg;
            rtnImageList = listOfSurfImages.Where(rec => rec.Distance > goodMatchDistThreshold)
                                    .OrderByDescending(rec => rec.Distance)
                                    .ToList<ImageRecord>();

            return rtnImageList;
        }
    }
}
