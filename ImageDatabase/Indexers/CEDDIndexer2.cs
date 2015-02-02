using CEDD_Descriptor;
using ImageDatabase.Helper;
using ImageDatabase.Helper.Tree;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace ImageDatabase.Indexers
{
    public class CEDDIndexer2 : IIndexer
    {
        public void IndexFiles(FileInfo[] imageFiles, BackgroundWorker IndexBgWorker, object argument = null)
        {
            BKTree<CEDDTreeNode> ceddtree = new BKTree<CEDDTreeNode>();

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

                CEDDTreeNode ceddTreeNode = new CEDDTreeNode
                {
                    Id = i,
                    ImageName = fi.Name,
                    ImagePath = fi.FullName,
                    CEDDDiscriptor = ceddDiscriptor
                };
                ceddtree.add(ceddTreeNode);
                IndexBgWorker.ReportProgress(i);
            }
            CEDDRepository<BKTree<CEDDTreeNode>> repo = new CEDDRepository<BKTree<CEDDTreeNode>>();
            repo.Save(ceddtree);
            CacheHelper.Remove("CeddIndexTree");
        }
        
        public void IndexFilesAsync(FileInfo[] imageFiles, BackgroundWorker IndexBgWorker, object argument = null)
        {
            throw new NotImplementedException();
        }
    }
}
