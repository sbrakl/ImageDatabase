using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.Helper
{
    public static class MemorySize
    {
        public static long GetBlobSizeinKb(object o)
        {
            long size = 0; 
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, o);
                size = s.Length;
            }
            long sizeInKb = size / 1024;
            return sizeInKb;
        }

        public static int Hits { get; set; }
    }
}
