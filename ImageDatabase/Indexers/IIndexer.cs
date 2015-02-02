using System;
namespace ImageDatabase.Indexers
{
    public interface IIndexer
    {
        void IndexFiles(System.IO.FileInfo[] imageFiles, 
            System.ComponentModel.BackgroundWorker IndexBgWorker,
            object argument = null);

        void IndexFilesAsync(System.IO.FileInfo[] imageFiles,
            System.ComponentModel.BackgroundWorker IndexBgWorker,
            object argument = null);
    }
}
