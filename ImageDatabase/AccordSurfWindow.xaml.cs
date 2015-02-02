using Accord.Imaging;
using Accord.Imaging.Filters;
using AForge;
using Accord.Math;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Documents;
using ImageDatabase.DTOs;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageDatabase
{
    /// <summary>
    /// Interaction logic for AccordSurf.xaml
    /// </summary>
    public partial class AccordSurfWindow : Window
    {
        public string ModelImagePath { get; set; }

        public string ObserverImagePath { get; set; }

        public SurfSettings SurfSetting { get; set; }
        public AccordSurfWindow()
        {
            InitializeComponent();
        }

        public AccordSurfWindow(string modelImagePath, string observerImagePath, SurfSettings setting) : this()
        {
            this.ModelImagePath = modelImagePath;
            this.ObserverImagePath = observerImagePath;
            this.SurfSetting = setting;
        }

        private void AccordSurfCompareWin_Loaded(object sender, RoutedEventArgs e)
        {
            ShowImage();
        }

        protected void ShowImage()
        {
            bool isModelImageMissing = string.IsNullOrEmpty(ModelImagePath);
            bool isObserverImageMissing = string.IsNullOrEmpty(ObserverImagePath);
            if (isModelImageMissing || isObserverImageMissing)
                return;
            Bitmap compareImagebmp = CompareAndDrawImage(new Bitmap(ModelImagePath), 
                                                        new Bitmap(ObserverImagePath), 
                                                        SurfSetting);
            if (compareImagebmp != null)
            {
                MemoryStream ms = new MemoryStream();
                compareImagebmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                imageAccordCompare.Source = bi;
            }
            else
            {
                this.Close();
            }                
        }

        private Bitmap CompareAndDrawImage(Bitmap modelImage, Bitmap observedImage, SurfSettings setting)
        {
            Stopwatch watch1 = new Stopwatch();
            Stopwatch watch2 = new Stopwatch();

            Bitmap returnBitmap;

            watch2.Start();
            watch1.Reset(); watch1.Start();
            double hessianThreshold = setting.HessianThresh.HasValue ? setting.HessianThresh.Value : 500;
            float hessianThreshold2 = (float)hessianThreshold / 1000000;

            Debug.WriteLine("hessianThreshold2: {0}", hessianThreshold2);
            SpeededUpRobustFeaturesDetector surf = new SpeededUpRobustFeaturesDetector(hessianThreshold2);
            List<SpeededUpRobustFeaturePoint> surfPoints1 = surf.ProcessImage(modelImage);
            List<SpeededUpRobustFeaturePoint> surfPoints2 = surf.ProcessImage(observedImage);


            Debug.WriteLine("Surf points count: {0}", surfPoints1.Count);
            Debug.WriteLine("Surf points count: {0}", surfPoints2.Count);
            //long memoryFootprint = MemorySize.GetBlobSizeinKb(surfPoints2);
            //Debug.WriteLine("Surf extractor: {0} kb", memoryFootprint);

            watch1.Stop();
            Debug.WriteLine("Surf Detection tooked {0} ms", watch1.ElapsedMilliseconds);

            watch1.Reset(); watch1.Start();
            // Show the marked points in the original images            
            Bitmap img1mark = new FeaturesMarker(surfPoints1, 2).Apply(modelImage);
            Bitmap img2mark = new FeaturesMarker(surfPoints2, 2).Apply(observedImage);
            // Concatenate the two images together in a single image (just to show on screen)
            Concatenate concatenate = new Concatenate(img1mark);
            returnBitmap = concatenate.Apply(img2mark);
            watch1.Stop();
            Debug.WriteLine("Surf point plotting tooked {0} ms", watch1.ElapsedMilliseconds);


            //watch1.Reset(); watch1.Start();
            //List<IntPoint>[] coretionalMatches = getMatches(surfPoints1, surfPoints2);
            //watch1.Stop();
            //Debug.WriteLine("Correctional Match tooked {0} ms", watch1.ElapsedMilliseconds);

            //// Get the two sets of points
            //IntPoint[] correlationPoints11 = coretionalMatches[0].ToArray();
            //IntPoint[] correlationPoints22 = coretionalMatches[1].ToArray();

            //Debug.WriteLine("Correclation points count: {0}", correlationPoints11.Length);
            //Debug.WriteLine("Correclation points count: {0}", correlationPoints22.Length);        

            Debug.WriteLine("Threshold: {0}", setting.UniquenessThreshold.Value);
            watch1.Reset(); watch1.Start();
            // Step 2: Match feature points using a k-NN
            KNearestNeighborMatching matcher = new KNearestNeighborMatching(2);
            matcher.Threshold = setting.UniquenessThreshold.Value;
            IntPoint[][] matches = matcher.Match(surfPoints1, surfPoints2);
            watch1.Stop();
            Debug.WriteLine("Knn Match tooked {0} ms", watch1.ElapsedMilliseconds);

            // Get the two sets of points
            IntPoint[] correlationPoints1 = matches[0];
            IntPoint[] correlationPoints2 = matches[1];

            Debug.WriteLine("Knn points count: {0}", correlationPoints1.Length);
            Debug.WriteLine("Knn points count: {0}", correlationPoints2.Length);

            //watch1.Reset(); watch1.Start();
            //// Show the marked correlations in the concatenated image
            //PairsMarker pairs = new PairsMarker(
            //    correlationPoints1, // Add image1's width to the X points to show the markings correctly
            //    correlationPoints2.Apply(p => new IntPoint(p.X + modelImage.Width, p.Y)), Color.Blue);

            //returnBitmap = pairs.Apply(returnBitmap);
            //watch1.Stop();
            //Debug.WriteLine("Match pair marking tooked {0} ms", watch1.ElapsedMilliseconds);

            if (correlationPoints1.Length < 4 || correlationPoints2.Length < 4)
            {
                MessageBox.Show("Insufficient points to attempt a fit.");
                return null;
            }

            watch1.Reset(); watch1.Start();
            // Step 3: Create the homography matrix using a robust estimator
            //RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            MatrixH homography = ransac.Estimate(correlationPoints1, correlationPoints2);
            watch1.Stop();
            Debug.WriteLine("Ransac tooked {0} ms", watch1.ElapsedMilliseconds);

            watch1.Reset(); watch1.Start();
            // Plot RANSAC results against correlation results
            IntPoint[] inliers1 = correlationPoints1.Submatrix(ransac.Inliers);
            IntPoint[] inliers2 = correlationPoints2.Submatrix(ransac.Inliers);
            watch1.Stop();
            Debug.WriteLine("Ransac SubMatrix {0} ms", watch1.ElapsedMilliseconds);

            Debug.WriteLine("Ransac points count: {0}", inliers1.Length);
            Debug.WriteLine("Ransac points count: {0}", inliers2.Length);

            watch1.Reset(); watch1.Start();
            PairsMarker inlierPairs = new PairsMarker(
               inliers1, // Add image1's width to the X points to show the markings correctly
               inliers2.Apply(p => new IntPoint(p.X + modelImage.Width, p.Y)), Color.Red);

            returnBitmap = inlierPairs.Apply(returnBitmap);
            watch1.Stop();
            Debug.WriteLine("Ransac plotting tooked {0} ms", watch1.ElapsedMilliseconds);

            watch2.Stop();
            return returnBitmap;
        }

      

    }
}
