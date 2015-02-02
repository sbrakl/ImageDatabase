using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using LibSimilarImageDotNet;
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
    public class PHashIndexer : IIndexer
    {
        public void IndexFiles(FileInfo[] imageFiles, BackgroundWorker IndexBgWorker, object argument = null)
        {
            List<PHashImageRecord> listOfRecords = new List<PHashImageRecord>();
            string compressHash = string.Empty;
            int totalFileCount = imageFiles.Length;
            for (int i = 0; i < totalFileCount; i++)
            {
                var fi = imageFiles[i];
                using (Bitmap bmp = new Bitmap(Image.FromFile(fi.FullName)))
                {
                    compressHash = SimilarImage.GetCompressedImageHashAsString(bmp);
                }

                PHashImageRecord record = new PHashImageRecord
                {
                    Id = i,
                    ImageName = fi.Name,
                    ImagePath = fi.FullName,
                    CompressHash = compressHash
                };
                listOfRecords.Add(record);
                IndexBgWorker.ReportProgress(i);
            }
            BinaryAlgoRepository<List<PHashImageRecord>> repo = new BinaryAlgoRepository<List<PHashImageRecord>>();
            repo.Save(listOfRecords);
        }
        public void IndexFilesAsync(FileInfo[] imageFiles, BackgroundWorker IndexBgWorker, object argument = null)
        {
            ConcurrentBag<PHashImageRecord> listOfRecords = new ConcurrentBag<PHashImageRecord>();

            string compressHash = string.Empty;
            int totalFileCount = imageFiles.Length;

            int i = 0; long nextSequence;
            //In the class scope: long nextSequence;
            Object lockMe = new Object();            

            Parallel.ForEach(imageFiles, currentImageFile =>
            {
                var fi = currentImageFile;
                using (Bitmap bmp = new Bitmap(Image.FromFile(fi.FullName)))
                {
                    compressHash = SimilarImage.GetCompressedImageHashAsString(bmp);
                }

                lock (lockMe)
                {
                    nextSequence = i++;
                }

                PHashImageRecord record = new PHashImageRecord
                {
                    Id = nextSequence,
                    ImageName = fi.Name,
                    ImagePath = fi.FullName,
                    CompressHash = compressHash
                };

                listOfRecords.Add(record);
               
                IndexBgWorker.ReportProgress(i); 
            });
            BinaryAlgoRepository<List<PHashImageRecord>> repo = new BinaryAlgoRepository<List<PHashImageRecord>>();
            repo.Save(listOfRecords.ToList());
        }
    }
}
