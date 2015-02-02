using ImageDatabase.DTOs;
using ImageDatabase.Helper.Tree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.Helper
{
    public class BinaryAlgoRepository<T> where T : IList
    {
        public bool Save(T listOfRecords)
        {
            bool rtnValue = false;

            string fullFilePath = GetFileNameBasedOnType<T>();

            if (File.Exists(fullFilePath))
                File.Delete(fullFilePath);
            using (FileStream stream = File.Create(fullFilePath))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, listOfRecords);
                stream.Close();
            }
            
            return rtnValue;
        }

        public T Load()
        {
            T rtnList = default(T);

            string fullFilePath = GetFileNameBasedOnType<T>();

            using (FileStream stream = File.OpenRead(fullFilePath))
            {
                var formatter = new BinaryFormatter();
                rtnList = (T)formatter.Deserialize(stream);
                stream.Close();
            }

            return rtnList;
        }

        private string GetFileNameBasedOnType<T>()
        {            
            string fileName = string.Empty;
            Type t = typeof(T);
            if (t == typeof(List<PHashImageRecord>))
            {
                fileName = "pHashIndex.bin";
            }
            else if (t == typeof(List<RGBProjectionRecord>))
            {
                fileName = "rgbHistoIndex.bin";
            }
            else if (t == typeof(List<BhattacharyyaRecord>))
            {
                fileName = "BhattacharyyaIndex.bin";
            }
            else if (t == typeof(List<CEDDRecord>))
            {
                fileName = "CEDDIndex.bin";
            }
            if (string.IsNullOrEmpty(fileName))
            {
                string msg = string.Format("Generic type '{0}' not supported", typeof(T).Name);
                throw new InvalidCastException(msg);
            }
            string fullFilePath = Path.Combine(DirectoryHelper.SaveDirectoryPath, fileName);
            return fullFilePath;
        }
    }

    public class CEDDRepository<T> where T : BKTree<CEDDTreeNode>
    {
        public bool Save(T listOfRecords)
        {
            bool rtnValue = false;

            string fullFilePath = GetFileNameBasedOnType<T>();

            if (File.Exists(fullFilePath))
                File.Delete(fullFilePath);
            using (FileStream stream = File.Create(fullFilePath))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, listOfRecords);
                stream.Close();
            }

            return rtnValue;
        }

        public T Load()
        {
            T rtnList = default(T);

            string fullFilePath = GetFileNameBasedOnType<T>();

            using (FileStream stream = File.OpenRead(fullFilePath))
            {
                var formatter = new BinaryFormatter();
                rtnList = (T)formatter.Deserialize(stream);
                stream.Close();
            }

            return rtnList;
        }

        private string GetFileNameBasedOnType<T>()
        {
            string fileName = string.Empty;
            Type t = typeof(T);
            if (t == typeof(BKTree<CEDDTreeNode>))
            {
                fileName = "CeddTreeIndex.bin";
            }           
            if (string.IsNullOrEmpty(fileName))
            {
                string msg = string.Format("Generic type '{0}' not supported", typeof(T).Name);
                throw new InvalidCastException(msg);
            }
            string fullFilePath = Path.Combine(DirectoryHelper.SaveDirectoryPath, fileName);
            return fullFilePath;
        }
    }
}
