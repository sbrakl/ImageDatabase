using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ImageDatabase.Indexers
{
    /// <summary>
    /// This Class is used in Linear Surf Compare
    /// </summary>
    public class SurfIndexer2
    {
        public void IndexFiles(FileInfo[] imageFiles, System.ComponentModel.BackgroundWorker IndexBgWorker, 
            Action<string> logWriter,
            SurfSettings surfSetting = null)
        {            
            #region Surf Dectator Region
            double hessianThresh = 500;
            double uniquenessThreshold = 0.8;
           
            if (surfSetting != null)
            {
                hessianThresh = surfSetting.HessianThresh.Value;
                uniquenessThreshold = surfSetting.UniquenessThreshold.Value;
            }

            SURFDetector surfDectector = new SURFDetector(hessianThresh, false);             
            #endregion

            List<SURFRecord2> surfRecord2List = new List<SURFRecord2>();
            Stopwatch sw1, sw2;

            sw1 = Stopwatch.StartNew();
            logWriter("Index started...");
            int totalFileCount = imageFiles.Length;
            for (int i = 0; i < totalFileCount; i++)
            {
                var fi = imageFiles[i];
                using (Image<Gray, byte> observerImage = new Image<Gray, byte>(fi.FullName))
                {
                    ImageFeature<float>[] observerFeatures = surfDectector.DetectFeatures(observerImage, null);
                    
                    if (observerFeatures.Length > 4)
                    {
                        SURFRecord2 record = new SURFRecord2
                        {
                            Id = i,
                            ImageName = fi.Name,
                            ImagePath = fi.FullName,
                            observerFeatures = observerFeatures
                        };
                        surfRecord2List.Add(record);
                    }
                    else
                    {
                        Debug.WriteLine(fi.Name + " skip from index, because it didn't have significant feature");
                    }
                    
                }
                IndexBgWorker.ReportProgress(i);                
            }
            SurfRepository.AddSURFRecord2List(surfRecord2List);
            sw1.Stop();
            logWriter(string.Format("Index Complete, it tooked {0} ms. Saving Repository...", sw1.ElapsedMilliseconds));

            sw2 = Stopwatch.StartNew();
            SurfRepository.SaveRepository(SurfAlgo.Linear);
            sw2.Stop();

            logWriter(string.Format("Index tooked {0} ms. Saving Repository tooked {1} ms", sw1.ElapsedMilliseconds, sw2.ElapsedMilliseconds));
        }


        public void IndexFilesAsync(FileInfo[] imageFiles, System.ComponentModel.BackgroundWorker IndexBgWorker,
            Action<string> logWriter,
            SurfSettings surfSetting = null)
        {
            throw new NotImplementedException();
        }        
        
    }
}
