using Accord.Imaging;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageDatabase.Indexers
{
    /// <summary>
    /// This class is used in building Accord KD-Tree Index
    /// </summary>
    public class SurfIndexer3
    {
        

        public void IndexFiles(FileInfo[] imageFiles, System.ComponentModel.BackgroundWorker IndexBgWorker,
            Action<string> logWriter,
            SurfSettings surfSetting = null)
        {

            //For Time Profilling
            long readingTime, indexingTime, saveingTime;

            #region Surf Dectator Region
            double hessianThresh = 500;
            double uniquenessThreshold = 0.8;

            if (surfSetting != null)
            {
                hessianThresh = surfSetting.HessianThresh.Value;
                uniquenessThreshold = surfSetting.UniquenessThreshold.Value;
            }
            float hessianThreshold2 = (float)hessianThresh / 1000000;
            SpeededUpRobustFeaturesDetector surf = new SpeededUpRobustFeaturesDetector(hessianThreshold2);
            #endregion

            int rows = 0;
            
            List<SURFRecord1> observerSurfImageIndexList = new List<SURFRecord1>();
            List<SpeededUpRobustFeaturePoint> listOfAllObserverImagesSurfPoints = new List<SpeededUpRobustFeaturePoint>();
            Stopwatch sw1;

            sw1 = Stopwatch.StartNew();
            logWriter("Index started...");
            int totalFileCount = imageFiles.Length;
            for (int i = 0; i < totalFileCount; i++)
            {
                var fi = imageFiles[i];
                using (Bitmap observerImage = (Bitmap)Image.FromFile(fi.FullName))
                {
                    List<SpeededUpRobustFeaturePoint> observerImageSurfPoints = surf.ProcessImage(observerImage);

                    if (observerImageSurfPoints.Count > 4)
                    {
                        int initRow = rows; int endRows = rows + observerImageSurfPoints.Count - 1;

                        SURFRecord1 record = new SURFRecord1
                        {
                            Id = i,
                            ImageName = fi.Name,
                            ImagePath = fi.FullName,
                            IndexStart = rows,
                            IndexEnd = endRows
                        };

                        observerSurfImageIndexList.Add(record);

                        listOfAllObserverImagesSurfPoints.AddRange(observerImageSurfPoints);

                        rows = endRows + 1;
                    }
                    else
                    {
                        Debug.WriteLine(fi.Name + " skip from index, because it didn't have significant feature");
                    }                    
                }
                IndexBgWorker.ReportProgress(i);
            }
            sw1.Stop();
            readingTime = sw1.ElapsedMilliseconds;
            logWriter(string.Format("Reading Surb Complete, it tooked {0} ms. Saving Repository...", readingTime));
            
            //------------Initialize Tree from Data
            sw1.Reset(); sw1.Start();
            double[][] superMatrix = listOfAllObserverImagesSurfPoints.Select(c => c.Descriptor).ToArray();
            int[] outputs = new int[superMatrix.Length];
            for (int i = 0; i < outputs.Length; i++)
                outputs[i] = i;

            Accord.MachineLearning.Structures.KDTree<int> tree =
                  Accord.MachineLearning.Structures.KDTree.FromData(superMatrix,
                          outputs,
                          Accord.Math.Distance.Euclidean, inPlace: true);
            sw1.Stop();
            indexingTime = sw1.ElapsedMilliseconds;
            logWriter(string.Format("Intializing KD Tree: {0}", indexingTime));


            //--------------Saving Indexed Records            
            sw1.Reset(); sw1.Start();            
            SurfAccordDataSet surfAccordDataset = new SurfAccordDataSet
            {
                SurfImageIndexRecord = observerSurfImageIndexList,
                IndexedTree = tree
            };
            string repoFileStoragePath = Path.Combine(DirectoryHelper.SaveDirectoryPath, "SurfAccordDataSet.bin");
            if (File.Exists(repoFileStoragePath))
                File.Delete(repoFileStoragePath);
            using (FileStream s = File.Create(repoFileStoragePath))
            {
                //Polenter.Serialization.SharpSerializerBinarySettings bs =
                //    new Polenter.Serialization.SharpSerializerBinarySettings(Polenter.Serialization.BinarySerializationMode.SizeOptimized);                
                //Polenter.Serialization.SharpSerializer formatter = new Polenter.Serialization.SharpSerializer(bs);
                //formatter.Serialize(surfAccordDataset, s);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter
                    = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(s, surfAccordDataset);
                s.Close();
            }
            sw1.Stop();
            saveingTime = sw1.ElapsedMilliseconds;
            logWriter(string.Format("Saving Surf Accord Dataset: {0}", saveingTime));


            //Invalidating Cache
            CacheHelper.Remove("SurfAccordDataSet");
            CacheHelper.Remove("SurfAccordIntervalTree");
                        
            logWriter(string.Format("Reading: {0} ms, Indexing: {1} ms, Saving Indexed data {2}", readingTime, indexingTime, saveingTime));
        }


        public void IndexFilesAsync(FileInfo[] imageFiles, System.ComponentModel.BackgroundWorker IndexBgWorker,
            Action<string> logWriter,
            SurfSettings surfSetting = null)
        {
            throw new NotImplementedException();
        }

        public void CalculateSurfDescriptor(Image img, SurfSettings surfSetting)
        {

        }


    }
}
