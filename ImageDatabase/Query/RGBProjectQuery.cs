using EyeOpen.Imaging.Processing;
using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageDatabase.Query
{
    public class RGBProjectQuery : IImageQuery
    {
        public List<DTOs.ImageRecord> QueryImage(string queryImagePath, object argument = null)
        {
            List<ImageRecord> rtnImageList = new List<ImageRecord>();

            RgbProjections queryProjections;

            using (Bitmap bitmap = ImageUtility.ResizeBitmap(new Bitmap(queryImagePath), 100, 100))
            {
                queryProjections = new RgbProjections(ImageUtility.GetRgbProjections(bitmap));
            }
            BinaryAlgoRepository<List<RGBProjectionRecord>> repo = new BinaryAlgoRepository<List<RGBProjectionRecord>>();
            List<RGBProjectionRecord> AllImage = repo.Load();
            foreach (var imgInfo in AllImage)
            {
                var dist = imgInfo.RGBProjection.CalculateSimilarity(queryProjections);
                if (dist > 0.8d)
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
