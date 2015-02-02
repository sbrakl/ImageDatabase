using ImageDatabase.DTOs;
using IntervalTreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.Helper.Tree
{
    public static class IntervalTreeHelper
    {
        private static IntervalTree<int, int>  _imageTree;

        public static IntervalTree<int, int> ImageTree
        {
            get { return _imageTree; }
            set { _imageTree = value; }
        }

        private static List<SURFRecord1> ImageList { get; set; }

        public static void CreateTree(List<SURFRecord1> imageList)
        {
            ImageList = imageList;
            _imageTree = new IntervalTree<int, int>();
            for(int i = 0; i < imageList.Count; i++)
            {
                var rec = imageList[i];
                _imageTree.AddInterval(rec.IndexStart, rec.IndexEnd, i);
            }
        }

        public static SURFRecord1 GetImageforRange(int value)
        {
            SURFRecord1 rec = null;
            if (_imageTree == null)
                return rec;
            var indexList = _imageTree.Get(value, StubMode.ContainsStartThenEnd);
            if (indexList.Count > 1  || indexList.Count == 0)
                return rec;
            var imageIndex = indexList[0];
            rec = ImageList[imageIndex];
            return rec;
        }
    }
}
