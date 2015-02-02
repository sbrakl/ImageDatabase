using ImageDatabase.DTOs;
using ImageDatabase.Helper;
using ImageDatabase.Helper.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace ImageDatabase.Query
{
    public class CEDDQuery2 : IImageQuery
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
            BKTree<CEDDTreeNode> ceddTree = null;
            if (!CacheHelper.Get<BKTree<CEDDTreeNode>>("CeddIndexTree", out ceddTree))           
            {
                CEDDRepository<BKTree<CEDDTreeNode>> repo = new CEDDRepository<BKTree<CEDDTreeNode>>();
                ceddTree = repo.Load();
                if (ceddTree == null)
                    throw new InvalidOperationException("Please index CEDD with BK-Tree before querying the Image");
                CacheHelper.Add<BKTree<CEDDTreeNode>>(ceddTree, "CeddIndexTree");
            }             
            sw.Stop();
            Debug.WriteLine("Load tooked {0} ms", sw.ElapsedMilliseconds);


            CEDDTreeNode queryNode = new CEDDTreeNode
            {
                Id = 0,
                ImagePath = queryImagePath,
                CEDDDiscriptor = queryCeddDiscriptor
            };
            
            sw.Reset(); sw.Start();
            Dictionary<CEDDTreeNode, Int32> result = ceddTree.query(queryNode, goodMatchDistance);
            sw.Stop();
            Debug.WriteLine("Query tooked {0} ms", sw.ElapsedMilliseconds);

            foreach (KeyValuePair<CEDDTreeNode, Int32> ceddNode in result)
            {
                ImageRecord rec = new ImageRecord
                {
                    Id = ceddNode.Key.Id,
                    ImageName = ceddNode.Key.ImageName,
                    ImagePath = ceddNode.Key.ImagePath,
                    Distance = ceddNode.Value
                };
                rtnImageList.Add(rec);
            }

            rtnImageList = rtnImageList.OrderBy(x => x.Distance).ToList();
            return rtnImageList;
        }
    }
}
