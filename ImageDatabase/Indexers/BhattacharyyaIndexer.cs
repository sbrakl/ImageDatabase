using BhattacharyyaCompare;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageDatabase.Indexers
{

    public class BhattacharyyaIndexer : IIndexer
    {
        public void IndexFiles(FileInfo[] imageFiles, BackgroundWorker IndexBgWorker, object argument = null)
        {
            List<BhattacharyyaRecord> listOfRecords = new List<BhattacharyyaRecord>();            

            Double[,] normalizedHistogram = new double[16,16];
            int totalFileCount = imageFiles.Length;
            for (int i = 0; i < totalFileCount; i++)
            {
                var fi = imageFiles[i];
                using (Image img = Image.FromFile(fi.FullName))
                {
                    normalizedHistogram = Bhattacharyya.CalculateNormalizedHistogram(img);
                }

                BhattacharyyaRecord record = new BhattacharyyaRecord
                {
                    Id = i,
                    ImageName = fi.Name,
                    ImagePath = fi.FullName,
                    NormalizedHistogram = MultiToSingle(normalizedHistogram)
                };
                listOfRecords.Add(record);
                IndexBgWorker.ReportProgress(i);
            }
            BinaryAlgoRepository<List<BhattacharyyaRecord>> repo = new BinaryAlgoRepository<List<BhattacharyyaRecord>>();
            repo.Save(listOfRecords);
        }

        private static double[] MultiToSingle(double[,] array)
        {
            int index = 0;
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            double[] single = new double[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    single[index] = array[x, y];
                    index++;
                }
            }
            return single;
        }


        public void IndexFilesAsync(FileInfo[] imageFiles, BackgroundWorker IndexBgWorker, object argument = null)
        {
            ConcurrentBag<BhattacharyyaRecord> listOfRecords = new ConcurrentBag<BhattacharyyaRecord>();

            Double[,] normalizedHistogram = new double[16, 16];
            int totalFileCount = imageFiles.Length;

            int i = 0; long nextSequence;
            //In the class scope:
            Object lockMe = new Object();
            

            Parallel.ForEach(imageFiles, currentImageFile =>
            {
                var fi = currentImageFile;
                using (Image img = Image.FromFile(fi.FullName))
                {
                    normalizedHistogram = Bhattacharyya.CalculateNormalizedHistogram(img);
                }

                lock (lockMe)
                {
                    nextSequence = i++;
                }

                BhattacharyyaRecord record = new BhattacharyyaRecord
                {
                    Id = nextSequence,
                    ImageName = fi.Name,
                    ImagePath = fi.FullName,
                    NormalizedHistogram = MultiToSingle(normalizedHistogram)
                };
                listOfRecords.Add(record);
               
                IndexBgWorker.ReportProgress(i);  
            });
            BinaryAlgoRepository<List<BhattacharyyaRecord>> repo = new BinaryAlgoRepository<List<BhattacharyyaRecord>>();
            repo.Save(listOfRecords.ToList());
        }
    }
}
