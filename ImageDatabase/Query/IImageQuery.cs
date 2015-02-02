using System;
namespace ImageDatabase.Query
{
    interface IImageQuery
    {
        System.Collections.Generic.List<ImageDatabase.DTOs.ImageRecord> QueryImage(
            string queryImagePath,
            object argument = null);
    }
}
