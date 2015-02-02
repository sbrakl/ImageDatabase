using Emgu.CV;
using ImageDatabase.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;

using SuperMatrixType = ImageDatabase.DTOs.SurfDataSet;

namespace ImageDatabase.Helper
{
    /// <summary>
    /// SURF Repository to store Surf Descriptors In-Mem and persist to FileSystem
    /// </summary>
    public static class SurfRepository
    {

        private const string _superMatrixDataSet = "SuperMatrixDataSet";
        private const string _SURFRecord2List = "SURFRecord2List";

        private static ObjectCache cache = MemoryCache.Default;

        private static string _saveDirectoryPath;
        private static string SaveDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_saveDirectoryPath))
                    _saveDirectoryPath = DirectoryHelper.SaveDirectoryPath;
                return _saveDirectoryPath;
            }
        }


        public static bool TrackSize { get; set; }

        private static long _cacheSizeinKb;
        public static long CacheSizeinKb
        {
            get { return _cacheSizeinKb; }
        }

        /// <summary>
        /// Insert value into the cache using
        /// appropriate name/value pairs
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="o">Item to be cached</param>
        /// <param name="key">Name of item</param>
        public static void AddSURFRecord2List(List<SURFRecord2> o)
        {
            if (TrackSize)
                _cacheSizeinKb += MemorySize.GetBlobSizeinKb(o);

            cache.Add(
                _SURFRecord2List,
                o,
                DateTime.Now.AddMinutes(1440));
        }

        public static void AddFlannIndex(Emgu.CV.Flann.Index o, string key)
        {
            // NOTE: Apply expiration parameters as you see fit.
            // I typically pull from configuration file.

            // In this example, I want an absolute
            // timeout so changes will always be reflected
            // at that time. Hence, the NoSlidingExpiration.

            if (TrackSize)
                _cacheSizeinKb += MemorySize.GetBlobSizeinKb(o);

            cache.Add(
                key,
                o,
                DateTime.Now.AddMinutes(1440));
        }

        /// <summary>
        /// Remove item from cache
        /// </summary>
        /// <param name="key">Name of cached item</param>
        public static void Remove(string key)
        {
            object o = cache[key];
            if (TrackSize)
            {
                long sizeInKb = MemorySize.GetBlobSizeinKb(o);
                _cacheSizeinKb = CacheSizeinKb - sizeInKb;
            }
            cache.Remove(key);
        }

        public static void ClearEntireCache()
        {
            foreach (var c in cache.AsEnumerable())
            {
                Remove(c.Key);
            }
            _cacheSizeinKb = 0;
        }

        /// <summary>
        /// Check for item in cache
        /// </summary>
        /// <param name="key">Name of cached item</param>
        /// <returns></returns>
        public static bool Exists(string key)
        {
            return cache[key] != null;
        }

        /// <summary>
        /// Retrieve cached item
        /// </summary>
        /// <typeparam name="SURFRecord2"></typeparam>
        /// <param name="key">Name of cached item</param>
        /// <param name="value">Cached value. Default(SURFRecord2) if item doesn't exist.</param>
        /// <returns>SURFRecord2 cache item</returns>
        public static bool GetSURFRecord2(string key, out SURFRecord2 value)
        {
            try
            {
                if (!Exists(key))
                {
                    value = default(SURFRecord2);
                    return false;
                }

                value = (SURFRecord2)cache[key];
            }
            catch
            {
                value = default(SURFRecord2);
                return false;
            }

            return true;
        }

        public static List<SURFRecord2> GetSurfRecordList()
        {
            try
            {
                if (!Exists(_SURFRecord2List))
                {
                    return default(List<SURFRecord2>);
                }

                return (List<SURFRecord2>)cache[_SURFRecord2List];
            }
            catch
            {
                return default(List<SURFRecord2>);
            }
        }

        public static Emgu.CV.Flann.Index GetFlannIndex(string key)
        {
            try
            {
                if (!Exists(key))
                {
                    return default(Emgu.CV.Flann.Index);
                }

                return (Emgu.CV.Flann.Index)cache[key];
            }
            catch
            {
                return default(Emgu.CV.Flann.Index);
            }
        }

        public static bool IsRepositoryInMemoryLoaded(SurfAlgo algo)
        {
            if (algo == SurfAlgo.Linear)
            {
                return Exists(_SURFRecord2List);
            }
            else
            {
                return Exists(_superMatrixDataSet);
            }
        }


        public static void SaveRepository(SurfAlgo algo)
        {
            if (algo == SurfAlgo.Linear)
                SaveRepoForLinear();
            else
                SaveRepoForFlann();
        }

        private static void SaveRepoForLinear()
        {
            List<SURFRecord2> featureSets = GetSurfRecordList();

            string repoFileStoragePath = Path.Combine(SaveDirectoryPath, "observerFeatureSets.bin");

            File.Delete(repoFileStoragePath);

            FileStream stream = File.Create(repoFileStoragePath);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, featureSets);
            stream.Close();
            //Polenter.Serialization.SharpSerializerBinarySettings st = new Polenter.Serialization.SharpSerializerBinarySettings(Polenter.Serialization.BinarySerializationMode.SizeOptimized);
            //Polenter.Serialization.SharpSerializer serializer = new Polenter.Serialization.SharpSerializer(st);
            //serializer.Serialize(featureSets, repoFileStoragePath);
        }


        /// <summary>
        /// Load the cache from the File
        /// </summary>
        /// <param name="fullFileName">File the full file path, else, it will try to load from default file if it exists</param>
        /// <returns></returns>
        public static void LoadRespositoryFromFile(SurfAlgo surfAlgo, string fullFileName = "")
        {
            string repoFileStoragePath = string.Empty;

            string defaultRepoFileStorageName = surfAlgo == SurfAlgo.Linear ? "observerFeatureSets.bin" : "superMatrix.bin";

            if (string.IsNullOrWhiteSpace(fullFileName))
            {
                repoFileStoragePath = Path.Combine(SaveDirectoryPath, defaultRepoFileStorageName);
            }
            else
            {
                repoFileStoragePath = fullFileName;
            }

            if (!File.Exists(repoFileStoragePath))
                throw new ArgumentException(string.Format("Can't find Surf Repository file at {0} :", repoFileStoragePath));

            FileStream stream = File.OpenRead(repoFileStoragePath);
            var formatter = new BinaryFormatter();

            if (surfAlgo == SurfAlgo.Linear)
            {
                List<SURFRecord2> observerFeatureSets = (List<SURFRecord2>)formatter.Deserialize(stream);
                stream.Close();
                AddSURFRecord2List(observerFeatureSets);
            }
            else if (surfAlgo == SurfAlgo.Flaan)
            {
                SuperMatrixType superMatrix = (SuperMatrixType)formatter.Deserialize(stream);
                stream.Close();
                AddSuperMatrixList(superMatrix);
            }


            //Polenter.Serialization.SharpSerializerBinarySettings st = new Polenter.Serialization.SharpSerializerBinarySettings(Polenter.Serialization.BinarySerializationMode.SizeOptimized);
            //Polenter.Serialization.SharpSerializer serializer = new Polenter.Serialization.SharpSerializer(st);
            //List<SURFRecord2> observerFeatureSets = (List<SURFRecord2>)serializer.Deserialize(repoFileStoragePath);

        }


        public static bool IsSurperMatrixLoaded
        {
            get
            {
                bool rtnBool = false;
                if (Exists(_superMatrixDataSet))
                {
                    rtnBool = true;
                }
                else
                {
                    rtnBool = false;
                }
                return false;
            }
        }

        public static void AddSuperMatrixList(SuperMatrixType o)
        {
            // NOTE: Apply expiration parameters as you see fit.
            // I typically pull from configuration file.

            // In this example, I want an absolute
            // timeout so changes will always be reflected
            // at that time. Hence, the NoSlidingExpiration.

            if (TrackSize)
                _cacheSizeinKb += MemorySize.GetBlobSizeinKb(o);

            cache.Add(
                _superMatrixDataSet,
                o,
                DateTime.Now.AddMinutes(1440));
        }

        public static SuperMatrixType GetSurfDataSet()
        {
            try
            {
                if (!Exists(_superMatrixDataSet))
                {
                    return default(SuperMatrixType);
                }

                return (SuperMatrixType)cache[_superMatrixDataSet];
            }
            catch
            {
                return default(SuperMatrixType);
            }
        }

        private static void SaveRepoForFlann()
        {
            if (!Exists(_superMatrixDataSet))
                throw new InvalidOperationException("Can't save. Super Matrix  not added to Repository");

            SuperMatrixType superMatrix = GetSurfDataSet();

            string exeDirectoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string repoFileStoragePath = Path.Combine(exeDirectoryPath, "superMatrix.bin");
            File.Delete(repoFileStoragePath);
            FileStream stream = File.Create(repoFileStoragePath);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, superMatrix);
            stream.Close();
            //Polenter.Serialization.SharpSerializerBinarySettings st = new Polenter.Serialization.SharpSerializerBinarySettings(Polenter.Serialization.BinarySerializationMode.SizeOptimized);
            //Polenter.Serialization.SharpSerializer serializer = new Polenter.Serialization.SharpSerializer(st);
            //serializer.Serialize(featureSets, repoFileStoragePath);
        }

        public static void SaveFlannIndex(Emgu.CV.Flann.Index index)
        {
            string repoFileStoragePath = Path.Combine(SaveDirectoryPath, "flannIndex.bin");
            File.Delete(repoFileStoragePath);
            FileStream stream = File.Create(repoFileStoragePath);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, index);
            stream.Close();
        }

        public static Emgu.CV.Flann.Index LoadFlannIndex()
        {
            Emgu.CV.Flann.Index index = null;
            string repoFileStoragePath = Path.Combine(SaveDirectoryPath, "flannIndex.bin");

            FileStream stream = File.OpenRead(repoFileStoragePath);
            var formatter = new BinaryFormatter();
            index = (Emgu.CV.Flann.Index)formatter.Deserialize(stream);
            stream.Close();

            return index;
            //Polenter.Serialization.SharpSerializerBinarySettings st = new Polenter.Serialization.SharpSerializerBinarySettings(Polenter.Serialization.BinarySerializationMode.SizeOptimized);
            //Polenter.Serialization.SharpSerializer serializer = new Polenter.Serialization.SharpSerializer(st);
            //serializer.Serialize(featureSets, repoFileStoragePath);
        }
    }

}
