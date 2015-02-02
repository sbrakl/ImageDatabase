using Accord.MachineLearning;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using ImageDatabase.Helper.RandomNumber;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageDatabase.Indexers
{
    /// <summary>
    /// This class is used in building Accord SURF Linear Index
    /// </summary>
    public class LocateIndexer
    {       
        public void IndexFiles(FileInfo[] imageFiles, System.ComponentModel.BackgroundWorker IndexBgWorker,
            Action<string> logWriter,
            LocateSettings locateSetting = null)
        {

            //For Time Profilling
            long extractingTime, kMeanTime = 0, calcBagOfVisualTime = 0;
            Stopwatch sw1;

            SimpleSurfSift.LoCATe descriptorExtractor = new SimpleSurfSift.LoCATe();

            sw1 = Stopwatch.StartNew();
            logWriter("Index started, extracting Descriptors...");

            List<double[]> ListofDescriptorsForCookBook = new List<double[]>();
            List<LoCATeRecord> ListOfAllImageDescriptors = new List<LoCATeRecord>();



            int totalFileCount = imageFiles.Length;
            if (totalFileCount == 0) 
            {
                logWriter("No files to index");
                return;
            };
            for (int i = 0; i < totalFileCount; i++)
            {
                var fi = imageFiles[i];

                using (Bitmap observerImage = (Bitmap)Image.FromFile(fi.FullName))
                {
                    List<double[]> locateDescriptors = descriptorExtractor.extract(observerImage, "SURF");
                    ListOfAllImageDescriptors.Add(new LoCATeRecord
                    {
                        Id = i,
                        ImageName = fi.Name,
                        ImagePath = fi.FullName,
                        LoCATeDescriptors = locateDescriptors
                    });
                    if (locateSetting.IsCodeBookNeedToBeCreated)
                    {
                        if (locateDescriptors.Count > 4)
                        {
                            RandomHelper randNumGenerator = new RandomHelper();
                            List<int> randIndexes = randNumGenerator.GetRandomNumberInRange(0, locateDescriptors.Count, 10d);
                            foreach (int index in randIndexes)
                            {
                                ListofDescriptorsForCookBook.Add(locateDescriptors[index]);
                            }
                        }
                        else
                        {
                            Debug.WriteLine(fi.Name + " skip from index, because it didn't have significant feature");
                        }
                    }
                    
                }
                IndexBgWorker.ReportProgress(i);
            } 
            sw1.Stop();
            extractingTime = Convert.ToInt32(sw1.Elapsed.TotalSeconds);            
            double[][] codeBook = null;
            if (locateSetting.IsCodeBookNeedToBeCreated)
            {
                logWriter("Indexing, Calculating Mean...");
                sw1.Reset(); sw1.Start();
                KMeans kMeans = new KMeans(locateSetting.SizeOfCodeBook);
                kMeans.Compute(ListofDescriptorsForCookBook.ToArray());
                codeBook = kMeans.Clusters.Centroids;
                //------------Save CookBook
                string fullFileName = locateSetting.CodeBookFullPath;
                if (File.Exists(fullFileName))
                    File.Delete(fullFileName);
                using (FileStream fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf
                        = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bf.Serialize(fs, codeBook);
                    fs.Close();
                }
                sw1.Stop();
                kMeanTime = Convert.ToInt32(sw1.Elapsed.TotalSeconds);
            }
            else
            {
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
            }                     

            logWriter("Indexing, Calculating Bag of Visual Words...");
            sw1.Reset(); sw1.Start();
            List<LoCaTeBoWRecord> ListOfImageVisualBagOfWorks = new List<LoCaTeBoWRecord>();
            for (int i = 0; i < ListOfAllImageDescriptors.Count; i++)
            {
                double[] visualWordForImage = createVisualWord(ListOfAllImageDescriptors[i].LoCATeDescriptors, codeBook);
                LoCaTeBoWRecord rec = new LoCaTeBoWRecord
                {
                    Id = ListOfAllImageDescriptors[i].Id,
                    ImageName = ListOfAllImageDescriptors[i].ImageName,
                    ImagePath = ListOfAllImageDescriptors[i].ImagePath, 
                    VisaulWord = visualWordForImage
                };
                ListOfImageVisualBagOfWorks.Add(rec);
                IndexBgWorker.ReportProgress(i);
            }
            logWriter("Indexing, Calculating ltcData...");
            int[] histogramSumOfAllVisualWords = null;            
            //------------Creating sum histogram of all words
            double[][] AllDatas = ListOfImageVisualBagOfWorks.Select(des => des.VisaulWord).ToArray();
            histogramSumOfAllVisualWords = createIndex((double[][])(AllDatas));                
            //------------Creating Image Records Data
            LoCaTeDataSet locateDS = new LoCaTeDataSet
            {
                AllImageRecordSet = ListOfImageVisualBagOfWorks,
                HistogramSumOfAllVisualWords = histogramSumOfAllVisualWords                
            };
            logWriter("Indexing, Saving Image Data...");
            //------------Save CookBook
            string ImageRecordName = Path.Combine(DirectoryHelper.SaveDirectoryPath, "LoCATeImageRecords.bin");
            if (File.Exists(ImageRecordName))
                File.Delete(ImageRecordName);
            using (FileStream fs = new FileStream(ImageRecordName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf
                    = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(fs, locateDS);
                fs.Close();
            }
            sw1.Stop();
            calcBagOfVisualTime = Convert.ToInt32(sw1.Elapsed.TotalSeconds);
            logWriter(string.Format("Extracting: {0} sec, KMeanTime: {1} sec, CalcBagOfVisalTime: {2} sec", extractingTime, kMeanTime, calcBagOfVisualTime));
        }


        public void IndexFilesAsync(FileInfo[] imageFiles, System.ComponentModel.BackgroundWorker IndexBgWorker,
            Action<string> logWriter,
            LocateSettings locateSetting = null)
        {
            throw new NotImplementedException();
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

        private int[] createIndex(double[][] arr)
        {
            int arrLen = arr.Length;
            int len = arr[0].Length;
            int[] index = new int[len];
            Array.Clear(index, 0, index.Length);

            for (int i = 0; i < arrLen; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    if (arr[i][j] != 0)
                        index[j]++;
                }
            }

            return index;
        }

        private double[] doMaths(double[] myData, int[] index, int filesCount)
        {
            double[] tempArr = (double[])myData;
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
                if (tempArr[ii] != 0)
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
