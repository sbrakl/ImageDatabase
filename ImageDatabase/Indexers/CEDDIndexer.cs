using CEDD_Descriptor;
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
    public class CEDDIndexer : IIndexer
    {
        public void IndexFiles(FileInfo[] imageFiles, BackgroundWorker IndexBgWorker, object argument = null)
        {
            List<CEDDRecord> listOfRecords = new List<CEDDRecord>();

            double[] ceddDiscriptor = null;
            int totalFileCount = imageFiles.Length;
            CEDD cedd = new CEDD();
            for (int i = 0; i < totalFileCount; i++)
            {
                var fi = imageFiles[i];
                using (Bitmap bmp = new Bitmap(Image.FromFile(fi.FullName)))
                {
                    ceddDiscriptor = cedd.Apply(bmp);
                }

                CEDDRecord record = new CEDDRecord
                {
                    Id = i,
                    ImageName = fi.Name,
                    ImagePath = fi.FullName,
                    CEDDDiscriptor = ceddDiscriptor
                };
                listOfRecords.Add(record);
                IndexBgWorker.ReportProgress(i);
            }
            BinaryAlgoRepository<List<CEDDRecord>> repo = new BinaryAlgoRepository<List<CEDDRecord>>();
            repo.Save(listOfRecords);
        }


        public void IndexFilesAsync(FileInfo[] imageFiles, BackgroundWorker IndexBgWorker, object argument = null)
        {
            ConcurrentBag<CEDDRecord> listOfRecords = new ConcurrentBag<CEDDRecord>();

            double[] ceddDiscriptor = null;
            int totalFileCount = imageFiles.Length;
            CEDD cedd = new CEDD();

            int i = 0; long nextSequence;
            //In the class scope:
            Object lockMe = new Object(); 

            Parallel.ForEach(imageFiles, currentImageFile =>
            {
                var fi = currentImageFile;
                using (Bitmap bmp = new Bitmap(Image.FromFile(fi.FullName)))
                {
                    ceddDiscriptor = cedd.Apply(bmp);
                }
                
                lock (lockMe)
                {
                    nextSequence = i++;
                }

                CEDDRecord record = new CEDDRecord
                {
                    Id = nextSequence,
                    ImageName = fi.Name,
                    ImagePath = fi.FullName,
                    CEDDDiscriptor = ceddDiscriptor
                };
                listOfRecords.Add(record);               
                IndexBgWorker.ReportProgress(i);                
            });
            BinaryAlgoRepository<List<CEDDRecord>> repo = new BinaryAlgoRepository<List<CEDDRecord>>();
            repo.Save(listOfRecords.ToList());
        }
    }
}
