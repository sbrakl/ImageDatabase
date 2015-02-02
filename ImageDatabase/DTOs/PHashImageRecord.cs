using System;

namespace ImageDatabase.DTOs
{
    [Serializable]
    public class PHashImageRecord : ImageRecord
    {       
        public string CompressHash { get; set; }
    }
}
