using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.DTOs
{
    [Serializable]
    public class ImageRecord
    {
        public long Id { get; set; }
        public string ImageName { get; set; }
        public string ImagePath { get; set; }
        public double Distance { get; set; }

        public ImageRecord Clone()
        {
            System.IO.MemoryStream m = new System.IO.MemoryStream();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter b = 
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            b.Serialize(m, this);
            m.Position = 0;
            return (ImageRecord)b.Deserialize(m);
        }
    }
}
