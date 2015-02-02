using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using LibSimilarImageDotNet;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageDatabase.Query
{
    public class pHashQuery : IImageQuery
    {
        public List<ImageRecord> QueryImage(string queryImagePath, object argument = null)
        {
            List<ImageRecord> rtnImageList = new List<ImageRecord>();

            string queryImageCompressHash;            
            using (Bitmap bmp = new Bitmap(System.Drawing.Image.FromFile(queryImagePath)))
            {
                queryImageCompressHash = SimilarImage.GetCompressedImageHashAsString(bmp);
            }
            BinaryAlgoRepository<List<PHashImageRecord>> repo = new BinaryAlgoRepository<List<PHashImageRecord>>();
            List<PHashImageRecord> AllImage = repo.Load();
            foreach (var imgInfo in AllImage)
            {
                var dist = SimilarImage.CompareHashes(queryImageCompressHash, imgInfo.CompressHash);
                if (dist > 0.8)
                {
                    imgInfo.Distance = dist;
                    rtnImageList.Add(imgInfo);
                }
            }

            rtnImageList = rtnImageList.OrderByDescending(x => x.Distance).ToList();

            return rtnImageList;
        }
    }
}
