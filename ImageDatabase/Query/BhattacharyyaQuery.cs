using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageDatabase.Query
{
    public class BhattacharyyaQuery : IImageQuery
    {
        public List<ImageRecord> QueryImage(string queryImagePath, object argument = null)
        {
            List<ImageRecord> rtnImageList = new List<ImageRecord>();

            double[,] queryHistogram;            
            using (Image img = Image.FromFile(queryImagePath))
            {
                queryHistogram = BhattacharyyaCompare.Bhattacharyya.CalculateNormalizedHistogram(img);
            }
            BinaryAlgoRepository<List<BhattacharyyaRecord>> repo = new BinaryAlgoRepository<List<BhattacharyyaRecord>>();
            List<BhattacharyyaRecord> AllImage = repo.Load();
            foreach (var imgInfo in AllImage)
            {
                double[,] norHist = SingleToMulti(imgInfo.NormalizedHistogram);
                var dist = BhattacharyyaCompare.Bhattacharyya.CompareHistogramPercDiff(queryHistogram, norHist);
                if (dist < 3)
                {
                    imgInfo.Distance = dist;
                    rtnImageList.Add(imgInfo);
                }
            }
            rtnImageList = rtnImageList.OrderBy(x => x.Distance).ToList();
            return rtnImageList;
        }

        private static double[,] SingleToMulti(double[] array)
        {
            int index = 0;
            int sqrt = (int)Math.Sqrt(array.Length);
            double[,] multi = new double[sqrt, sqrt];
            for (int y = 0; y < sqrt; y++)
            {
                for (int x = 0; x < sqrt; x++)
                {
                    multi[x, y] = array[index];
                    index++;
                }
            }
            return multi;
        }
    }
}
