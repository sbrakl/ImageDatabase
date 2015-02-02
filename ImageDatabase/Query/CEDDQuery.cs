using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace ImageDatabase.Query
{
    public class CEDDQuery : IImageQuery
    {
        public List<ImageRecord> QueryImage(string queryImagePath, object argument = null)
        {
            List<ImageRecord> rtnImageList = new List<ImageRecord>();
            CEDD_Descriptor.CEDD cedd = new CEDD_Descriptor.CEDD();
            
            int goodMatchDistance = 35;
            if (argument != null && argument is Int32)
                goodMatchDistance = (int)argument;


            double[] queryCeddDiscriptor;            
            using (Bitmap bmp = new Bitmap(Image.FromFile(queryImagePath)))
            {
                queryCeddDiscriptor = cedd.Apply(bmp);
            }

            Stopwatch sw = Stopwatch.StartNew();
            BinaryAlgoRepository<List<CEDDRecord>> repo = new BinaryAlgoRepository<List<CEDDRecord>>();
            List<CEDDRecord> AllImage = (List<CEDDRecord>)repo.Load();
            sw.Stop();
            Debug.WriteLine("Load tooked {0} ms", sw.ElapsedMilliseconds);

            sw.Reset(); sw.Start();
           
            foreach (var imgInfo in AllImage)
            {
                double[] ceddDiscriptor = imgInfo.CEDDDiscriptor;
                var dist = CEDD_Descriptor.CEDD.Compare(queryCeddDiscriptor, ceddDiscriptor);
                if (dist < goodMatchDistance)
                {
                    imgInfo.Distance = dist;
                    rtnImageList.Add(imgInfo);
                }
            }
            sw.Stop();
            Debug.WriteLine("Query tooked {0} ms", sw.ElapsedMilliseconds);
            rtnImageList = rtnImageList.OrderBy(x => x.Distance).ToList();
            return rtnImageList;
        }
    }
}
