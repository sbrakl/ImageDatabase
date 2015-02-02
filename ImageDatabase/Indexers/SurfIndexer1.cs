using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
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
    /// This class is used in building Flann Index
    /// </summary>
    public class SurfIndexer1
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

            int rows = 0;

            Matrix<float> superMatrix = null;
            List<SURFRecord1> observerSurfImageIndexList = new List<SURFRecord1>();

            Stopwatch sw1, sw2;

            sw1 = Stopwatch.StartNew();
            logWriter("Index started...");
            int totalFileCount = imageFiles.Length;
            for (int i = 0; i < totalFileCount; i++)
            {
                var fi = imageFiles[i];
                using (Image<Gray, byte> observerImage = new Image<Gray, byte>(fi.FullName))
                {
                    VectorOfKeyPoint observerKeyPoints = new VectorOfKeyPoint();
                    Matrix<float> observerDescriptor = surfDectector.DetectAndCompute(observerImage, null, observerKeyPoints);
                    
                    if (observerDescriptor.Rows > 4)
                    {
                        int initRow = rows; int endRows = rows + observerDescriptor.Rows - 1;

                        SURFRecord1 record = new SURFRecord1
                        {
                            Id = i,
                            ImageName = fi.Name,
                            ImagePath = fi.FullName,
                            IndexStart = rows,
                            IndexEnd = endRows
                        };

                        observerSurfImageIndexList.Add(record);

                        if (superMatrix == null)
                            superMatrix = observerDescriptor;
                        else
                            superMatrix = superMatrix.ConcateVertical(observerDescriptor);

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
            logWriter(string.Format("Index Complete, it tooked {0} ms. Saving Repository...", sw1.ElapsedMilliseconds));           
            SurfDataSet surfDataset = new SurfDataSet
            {
                SurfImageIndexRecord = observerSurfImageIndexList,
                SuperMatrix = superMatrix
            };
            sw2 = Stopwatch.StartNew();
            SurfRepository.AddSuperMatrixList(surfDataset);
            SurfRepository.SaveRepository(SurfAlgo.Flaan);
            sw2.Stop();            

            logWriter(string.Format("Index tooked {0} ms. Saving Repository tooked {1} ms", sw1.ElapsedMilliseconds, sw2.ElapsedMilliseconds));
        }


        public void IndexFilesAsync(FileInfo[] imageFiles, System.ComponentModel.BackgroundWorker IndexBgWorker, object argument = null)
        {
            throw new NotImplementedException();
        }

        public void CalculateSurfDescriptor(Image img, SurfSettings surfSetting)
        {

        }


    }
}
