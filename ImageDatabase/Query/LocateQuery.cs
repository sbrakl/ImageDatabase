using Accord.Math;
using AutoMapper;
using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using ImageDatabase.Helper.Tree;
using SimpleSurfSift;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageDatabase.Query
{
    /// <summary>
    /// Class which Queries the Image in Linear fashion
    /// </summary>
    public class LocateQuery
    {
        public List<ImageRecord> QueryImage(string queryImagePath, out string messageToLog,
            LocateSettings locateSetting = null)
        {
            List<ImageRecord> rtnImageList = new List<ImageRecord>();

            #region Diagnostic Region
            Stopwatch sw = new Stopwatch();
            Stopwatch sw1 = new Stopwatch();
            long _loadingTime = 0, _queryingMAVTime = 0, _matchingTime = 0;
            #endregion Diagnostic Region

            #region Reading Cookbook
            sw1.Reset(); sw1.Start();
            //-----Reading Cookbook
            double[][] codeBook = null;
            string fullFileName = locateSetting.CodeBookFullPath;
            if (!File.Exists(fullFileName))
            {
                string msg = string.Format("Couldn't find {0}, Please Index before querying with Locate", fullFileName);
                throw new InvalidOperationException(msg);
            }

            using (FileStream fs = new FileStream(fullFileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf
                    = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                codeBook = (double[][])bf.Deserialize(fs);
                fs.Close();
            }
            #endregion

            #region Reading Image Data
            //----Read Image Data
            LoCaTeDataSet locateDS = null;
            fullFileName = Path.Combine(DirectoryHelper.SaveDirectoryPath, "LoCATeImageRecords.bin");
            if (!File.Exists(fullFileName))
            {
                string msg = string.Format("Couldn't find {0}, Please Index before querying with Locate", fullFileName);
                throw new InvalidOperationException(msg);
            }

            using (FileStream fs = new FileStream(fullFileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf
                    = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                locateDS = (LoCaTeDataSet)bf.Deserialize(fs);
                fs.Close();
            }
            sw1.Stop();
            _loadingTime = sw1.ElapsedMilliseconds;
            #endregion

            #region Query Image Detection and Visual Words
            //---Reading Query Image
            sw1.Reset(); sw1.Start();
            LoCATe descriptor = new LoCATe();
            List<double[]> queryDescriptors = descriptor.extract(new Bitmap(queryImagePath), "SURF");
            double[] queryVisualWord = createVisualWord(queryDescriptors, codeBook);
            int totalFileCount = locateDS.AllImageRecordSet.Count;

            if (locateSetting.isLteSchemeNeedToAppy)
            {
                queryVisualWord = doMaths(queryVisualWord, locateDS.HistogramSumOfAllVisualWords, totalFileCount);
            }
            sw1.Stop();
            _queryingMAVTime = sw1.ElapsedMilliseconds;
            #endregion

            List<LoCaTeBoWRecord> AllImageRecordSet = locateDS.AllImageRecordSet;
            double[][] AllDatas = AllImageRecordSet.Select(rec => rec.VisaulWord).ToArray();

            #region Searching of Image
            sw1.Reset(); sw1.Start();
            if (locateSetting.isLteSchemeNeedToAppy)
            {
                //-----------Creating ltc Data                
                double[][] ltcData = new double[totalFileCount][];
                for (int i = 0; i < AllDatas.Length; i++)
                {
                    ltcData[i] = doMaths((double[])(AllDatas[i]).Clone(), locateDS.HistogramSumOfAllVisualWords, totalFileCount);
                }
                AllDatas = ltcData;
            }

            double EucDistance;
            for (int i = 0; i < totalFileCount; i++)
            {
                EucDistance = Accord.Math.Distance.Euclidean(queryVisualWord, AllDatas[i]);
                AllImageRecordSet[i].Distance = EucDistance;
            }

            List<LoCaTeRanker> listofImageWRanker = new List<LoCaTeRanker>();
            if (locateSetting.IsExtendedSearch)
            {
                //Take first 25 images
                IEnumerable<LoCaTeBoWRecord> first25Image = AllImageRecordSet.OrderBy(rec => rec.Distance)
                                    .Where(rec => rec.Distance < locateSetting.GoodThresholdDistance)
                                    .Take(25);

                //Map to Locate Ranker list
                Mapper.CreateMap<LoCaTeBoWRecord, LoCaTeRanker>();
                Mapper.CreateMap<ImageRecord, LoCaTeRanker>();
                listofImageWRanker = Mapper.Map<IEnumerable<LoCaTeBoWRecord>, List<LoCaTeRanker>>(first25Image);

                //Calculate locate range
                int totalImageCount = listofImageWRanker.Count;
                for (int i = 0; i < totalImageCount; i++)
                {
                    double percentage = (1 -  (listofImageWRanker[i].Distance / 1)) * 100;
                    listofImageWRanker[i].LocateRank = Convert.ToInt32(percentage);
                }

                //Perform SURF query
                listofImageWRanker = PerformExtendedSurfSearch(queryImagePath, listofImageWRanker);
                int totalimageBeforeCEDD = listofImageWRanker.Count;
                //listofImageWRanker = listofImageWRanker.OrderByDescending(rec => rec.SurfRank).ToList();

                //Perform CEDD query
                CEDDQuery2 queryCEDD = new CEDDQuery2();
                var imageListbyCEDD = queryCEDD.QueryImage(queryImagePath).Take(25).ToList();
                totalImageCount = imageListbyCEDD.Count;
                for (int i = 0; i < totalImageCount; i++)
                {
                    long id = imageListbyCEDD[i].Id;
                    double percentage = (1 - (imageListbyCEDD[i].Distance / 35)) * 100;
                    var img2 = listofImageWRanker.Where(img => img.Id == id).SingleOrDefault();
                    if (img2 == null)
                    {
                        var newImage = Mapper.Map<ImageRecord, LoCaTeRanker>(imageListbyCEDD[i]);
                        newImage.CEDDRank = Convert.ToInt32(percentage);
                        listofImageWRanker.Add(newImage);
                    }    
                    else
                    {
                        img2.CEDDRank = Convert.ToInt32(percentage);
                    }
                    
                }

                totalImageCount = listofImageWRanker.Count;
                for (int i = 0; i < totalImageCount; i++ )
                {
                    var imageRank = listofImageWRanker[i];
                    if (imageRank.SurfRank > 0 )
                    {
                        imageRank.CombinedRank = 100 - (totalimageBeforeCEDD - imageRank.SurfRank);
                    }
                    else
                    {
                        imageRank.CombinedRank = (0.5 * imageRank.CEDDRank) + (0.4 * imageRank.LocateRank);
                    }
                    imageRank.Distance = imageRank.CombinedRank;
                }
                   
                      
                 
                listofImageWRanker = listofImageWRanker.OrderByDescending(rec => rec.CombinedRank).ToList();
                rtnImageList = listofImageWRanker.ToList<ImageRecord>();
            }
            else
            {
                rtnImageList = AllImageRecordSet.OrderBy(rec => rec.Distance)
                                    .Where(rec => rec.Distance < locateSetting.GoodThresholdDistance)
                                    .Take(25)
                                    .ToList<ImageRecord>();
            }


            sw1.Stop();
            _matchingTime = sw1.ElapsedMilliseconds;
            #endregion

            messageToLog = string.Format("Loading time {0} ms, Query image processing {1} ms, Matching {2} ms",
                _loadingTime, _queryingMAVTime, _matchingTime);
            return rtnImageList;
        }

        private List<LoCaTeRanker> PerformExtendedSurfSearch(string queryImagePath, List<LoCaTeRanker> ImageList)
        {
            //If no images are found in Locate, return
            if (ImageList.Count == 0)
                return ImageList;

            SURFDetector surfDectector = new SURFDetector(1000, false);
            Matrix<float> superMatrix = null;
            List<SURFRecord1> observerSurfImageIndexList = new List<SURFRecord1>();

            #region Computing Model Descriptors
            Matrix<float> modelDescriptors;
            VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();
            using (Image<Gray, byte> modelImage = new Image<Gray, byte>(queryImagePath))
            {
                modelDescriptors = surfDectector.DetectAndCompute(modelImage, null, modelKeyPoints);
            }
            #endregion

            #region Computing Surf Descriptors
            int rows = 0;

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            ConcurrentDictionary<long, Matrix<float>> obserableSurfPoints = new ConcurrentDictionary<long, Matrix<float>>(concurrencyLevel, ImageList.Count);

            Parallel.ForEach(ImageList, img =>
            {
                string imagePath = img.ImagePath;
                using (Image<Gray, byte> observerImage = new Image<Gray, byte>(imagePath))
                {
                    VectorOfKeyPoint observerKeyPoints = new VectorOfKeyPoint();
                    Matrix<float> observerDescriptors = surfDectector.DetectAndCompute(observerImage, null, observerKeyPoints);
                    obserableSurfPoints.TryAdd(img.Id, observerDescriptors);
                }
            });

            foreach (var rec in ImageList)
            {
                Matrix<float> observerDescriptors = obserableSurfPoints[rec.Id];
                 if (superMatrix != null)
                     superMatrix = superMatrix.ConcateVertical(observerDescriptors);
                 else
                     superMatrix = observerDescriptors;

                 int initRow = rows; int endRows = rows + observerDescriptors.Rows - 1;
                 observerSurfImageIndexList.Add(new SURFRecord1
                 {
                     Id = rec.Id,
                     ImageName = rec.ImageName,
                     ImagePath = rec.ImagePath,
                     IndexStart = rows,
                     IndexEnd = endRows,
                     Distance = 0
                 });
                 rows = endRows + 1;
            }

            //foreach (var rec in ImageList)
            //{
            //    string imagePath = rec.ImagePath;
            //    using (Image<Gray, byte> observerImage = new Image<Gray, byte>(imagePath))
            //    {
            //        VectorOfKeyPoint observerKeyPoints = new VectorOfKeyPoint();

            //        Matrix<float> observerDescriptors = surfDectector.DetectAndCompute(observerImage, null, observerKeyPoints);
            //        if (superMatrix != null)
            //            superMatrix = superMatrix.ConcateVertical(observerDescriptors);
            //        else
            //            superMatrix = observerDescriptors;

            //        int initRow = rows; int endRows = rows + observerDescriptors.Rows - 1;
            //        observerSurfImageIndexList.Add(new SURFRecord1
            //        {
            //            Id = rec.Id,
            //            ImageName = rec.ImageName,
            //            ImagePath = rec.ImagePath,
            //            IndexStart = rows,
            //            IndexEnd = endRows,
            //            Distance = 0
            //        });
            //        rows = endRows + 1;
            //    }
            //} 
            #endregion

            Emgu.CV.Flann.Index flannIndex = new Emgu.CV.Flann.Index(superMatrix, 4);
            var indices = new Matrix<int>(modelDescriptors.Rows, 2); // matrix that will contain indices of the 2-nearest neighbors found
            var dists = new Matrix<float>(modelDescriptors.Rows, 2); // matrix that will contain distances to the 2-nearest neighbors found
            flannIndex.KnnSearch(modelDescriptors, indices, dists, 2, 24);

            IntervalTreeHelper.CreateTree(observerSurfImageIndexList);
            for (int i = 0; i < indices.Rows; i++)
            {
                // filter out all inadequate pairs based on distance between pairs
                if (dists.Data[i, 0] < (0.3 * dists.Data[i, 1]))
                {
                    var img = IntervalTreeHelper.GetImageforRange(indices[i, 0]);
                    if (img != null) img.Distance++;
                }
            }
            int maxMatch = Convert.ToInt32(observerSurfImageIndexList.Select(rec => rec.Distance).Max());
            observerSurfImageIndexList = observerSurfImageIndexList.OrderByDescending(rec => rec.Distance).ToList();
            int totalImageCount = observerSurfImageIndexList.Count;
            for (int i = 0; i < totalImageCount; i++)
            {
                long id = observerSurfImageIndexList[i].Id;
                var img2 = ImageList.Where(img => img.Id == id).SingleOrDefault();
                if (img2 != null)
                {
                    double countofMatch = observerSurfImageIndexList[i].Distance;
                    if (countofMatch > 0)
                    {
                        img2.SurfRank = (totalImageCount - i);
                    }
                }

            }

            return ImageList;
        }

        private double[] createVisualWord(List<double[]> imageDescriptors, double[][] codebook)
        {
            int clusters = codebook.Length;
            int k = imageDescriptors.Count;
            double[] visualWord = new double[clusters];
            Array.Clear(visualWord, 0, visualWord.Length);

            for (int i = 0; i < k; i++)
            {
                double min = Accord.Math.Distance.Euclidean(codebook[0], imageDescriptors[i]);
                int possition = 0;
                for (int j = 0; j < clusters; j++)
                {
                    double distance = Accord.Math.Distance.Euclidean(codebook[j], imageDescriptors[i]);
                    if (distance < min)
                    {
                        min = distance;
                        possition = j;
                    }
                }
                visualWord[possition]++;
            }
            return visualWord;
        }

        private double[] doMaths(double[] myData, int[] index, int filesCount)
        {
            double[] tempArr = myData;
            int tempArrLen = tempArr.Length;
            double EuclidianNorm = 0;


            for (int ii = 0; ii < tempArrLen; ii++)
            {
                if (tempArr[ii] != 0)
                {
                    tempArr[ii] = 1 + Math.Log(tempArr[ii], 10);
                }
            }


            for (int ii = 0; ii < tempArrLen; ii++)
            {
                if (tempArr[ii] != 0 && index[ii] != 0)
                {
                    tempArr[ii] = Math.Log(((double)filesCount / (double)index[ii]), 10) * tempArr[ii];
                }
            }

            EuclidianNorm = 0;
            for (int ii = 0; ii < tempArrLen; ii++)
            {
                EuclidianNorm += Math.Pow(tempArr[ii], 2);
            }
            double temp = Math.Sqrt(EuclidianNorm);
            for (int ii = 0; ii < tempArrLen; ii++)
            {
                if (tempArr[ii] != 0)
                {
                    tempArr[ii] = tempArr[ii] / temp;
                }
            }


            return tempArr;
        }
    }
}
