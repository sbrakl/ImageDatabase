using Accord.Imaging;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace ImageDatabase.Indexers
{
    /// <summary>
    /// This class is used in building Accord SURF Linear Index
    /// </summary>
    public class SurfIndexer4
    {
        
        public void IndexFiles(FileInfo[] imageFiles, System.ComponentModel.BackgroundWorker IndexBgWorker,
            Action<string> logWriter,
            SurfSettings surfSetting = null)
        {

            //For Time Profilling
            long readingTime, indexingTime = 0 , saveingTime = 0;

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
            
            Stopwatch sw1;

            sw1 = Stopwatch.StartNew();
            logWriter("Index started...");

            string fullFileName = Path.Combine(DirectoryHelper.SaveDirectoryPath, "SurfAccordLinear.bin");
            if (File.Exists(fullFileName))
                File.Delete(fullFileName);
            using (FileStream fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf
                    = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                int totalFileCount = imageFiles.Length;
                for (int i = 0; i < totalFileCount; i++)
                {
                    var fi = imageFiles[i];

                    using (Bitmap observerImage = (Bitmap)Image.FromFile(fi.FullName))
                    {
                        List<SpeededUpRobustFeaturePoint> observerImageSurfPoints = surf.ProcessImage(observerImage);

                        if (observerImageSurfPoints.Count > 4)
                        {
                            SURFAccordRecord3 record = new SURFAccordRecord3
                            {
                                Id = i,
                                ImageName = fi.Name,
                                ImagePath = fi.FullName,
                                SurfDescriptors = observerImageSurfPoints 
                            };
                            bf.Serialize(fs, record);
                        }
                        else
                        {
                            Debug.WriteLine(fi.Name + " skip from index, because it didn't have significant feature");
                        }
                    }
                    IndexBgWorker.ReportProgress(i);
                }
                fs.Close();
            }
            
            sw1.Stop();
            readingTime = sw1.ElapsedMilliseconds;
            logWriter(string.Format("Reading Surb Complete, it tooked {0} ms. Saving Repository...", readingTime));
                        
                        
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
