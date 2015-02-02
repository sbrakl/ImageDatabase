using EyeOpen.Imaging.Processing;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageDatabase.Indexers
{
    public class RGBProjectionIndexer : IIndexer
    {
        public void IndexFiles(FileInfo[] imageFiles, System.ComponentModel.BackgroundWorker IndexBgWorker, object argument = null)
        {
            List<RGBProjectionRecord> listOfRecords = new List<RGBProjectionRecord>();

            RgbProjections projections = null;
            int totalFileCount = imageFiles.Length;
            for (int i = 0; i < totalFileCount; i++)
            {
                var fi = imageFiles[i];
                using (Bitmap bitmap = ImageUtility.ResizeBitmap(new Bitmap(fi.FullName), 100, 100))
                {
                    projections = new RgbProjections(ImageUtility.GetRgbProjections(bitmap));                    
                }

                RGBProjectionRecord record = new RGBProjectionRecord
                {
                    Id = i,
                    ImageName = fi.Name,
                    ImagePath = fi.FullName,
                    RGBProjection = projections
                };
                listOfRecords.Add(record);
                IndexBgWorker.ReportProgress(i);
            }
            BinaryAlgoRepository<List<RGBProjectionRecord>> repo = new BinaryAlgoRepository<List<RGBProjectionRecord>>();
            repo.Save(listOfRecords);
        }


        public void IndexFilesAsync(FileInfo[] imageFiles, System.ComponentModel.BackgroundWorker IndexBgWorker, object argument = null)
        {
            ConcurrentBag<RGBProjectionRecord> listOfRecords = new ConcurrentBag<RGBProjectionRecord>();

            RgbProjections projections = null;
            int totalFileCount = imageFiles.Length;

            int i = 0; long nextSequence;
            //In the class scope:
            Object lockMe = new Object();

            Parallel.ForEach(imageFiles, currentImageFile =>
            {
                var fi = currentImageFile;
                using (Bitmap bitmap = ImageUtility.ResizeBitmap(new Bitmap(fi.FullName), 100, 100))
                {
                    projections = new RgbProjections(ImageUtility.GetRgbProjections(bitmap));
                }

                lock (lockMe)
                {
                    nextSequence = i++;
                }

                RGBProjectionRecord record = new RGBProjectionRecord
                {
                    Id = nextSequence,
                    ImageName = fi.Name,
                    ImagePath = fi.FullName,
                    RGBProjection = projections
                };

                listOfRecords.Add(record);
             
                IndexBgWorker.ReportProgress(i);  
            });
            BinaryAlgoRepository<List<RGBProjectionRecord>> repo = new BinaryAlgoRepository<List<RGBProjectionRecord>>();
            repo.Save(listOfRecords.ToList());
        }
    }
}
