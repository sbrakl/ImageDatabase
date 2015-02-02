using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.Helper
{
    public class DirectoryHelper
    {
        public static string MyApplicationDirectory
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); ;
            }
        }

        public static string PictureDirectory
        {
            get
            {
                string appDirectory = MyApplicationDirectory;
                string picDirectory = string.Empty;
                DirectoryInfo di = new DirectoryInfo(appDirectory);
                DirectoryInfo picDirInfo = null;
                DirectoryInfo currDir = di;
                while (true)
                {
                    picDirInfo = currDir.Parent.EnumerateDirectories().Where(d => d.Name.ToLower() == "pic").SingleOrDefault();
                    if (picDirInfo != null)
                        break;
                    else
                        currDir = currDir.Parent;

                    //If root directory, exist
                    if (currDir.Parent == null)
                        break;
                }
                if (picDirInfo != null)
                {                    
                    picDirectory = picDirInfo.FullName;
                }
                return picDirectory;
            }
        }

        public static string PictureObserverDirectory
        { 
            get
            {
                string picDir = PictureDirectory;
                string obsrDir = Path.Combine(picDir, "Observer");
                return obsrDir;
            }
            
        }

        private static string _saveDirectoryPath;
        public static string SaveDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_saveDirectoryPath))
                {
                    _saveDirectoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    _saveDirectoryPath = Path.Combine(_saveDirectoryPath, "Data");
                    if (!Directory.Exists(_saveDirectoryPath))
                        Directory.CreateDirectory(_saveDirectoryPath);
                }
                return _saveDirectoryPath;
            }
        }

        public static string CodebookFullPath(int sizeOfCodeBook, int numberOfImages)
        {
            return string.Format("{2}\\LoCATeCookbook_{0}_{1}.cb", sizeOfCodeBook, numberOfImages, SaveDirectoryPath);
        }

        public static string LocateImageRecordsPath(int sizeOfCodeBook, int numberOfImages)
        {
            return string.Format("{2}\\LoCATeImageRecords{0}_{1}.bin", sizeOfCodeBook, numberOfImages, SaveDirectoryPath);
        }
    }
}
