using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhattacharyyaCompare
{
    public class Bhattacharyya
    {

        public static double[,] CalculateNormalizedHistogram(Image img)
        {
            var normalizedHistogram = new double[16, 16];

            double histSum1 = 0.0;
            byte[,] img1GrayscaleValues = img.GetGrayScaleValues();

            foreach (var value in img1GrayscaleValues) { histSum1 += value; }

            for (int x = 0; x < img1GrayscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < img1GrayscaleValues.GetLength(1); y++)
                {
                    normalizedHistogram[x, y] = (double)img1GrayscaleValues[x, y] / histSum1;
                }
            }

            return normalizedHistogram;
        }

        public static double CompareHistogramPercDiff(double[,] normalizedHistogram1, double[,] normalizedHistogram2)
        {
            double bCoefficient = 0.0;
            for (int x = 0; x < normalizedHistogram2.GetLength(0); x++)
            {
                for (int y = 0; y < normalizedHistogram2.GetLength(1); y++)
                {
                    double histSquared = normalizedHistogram1[x, y] * normalizedHistogram2[x, y];
                    bCoefficient += Math.Sqrt(histSquared);
                }
            }

            double dist1 = 1 - bCoefficient;
            dist1 = Math.Round(dist1, 8);
            double distance = Math.Sqrt(dist1);
            distance = Math.Round(distance, 8);

            var percentage = distance * 100;
            percentage = Math.Round(percentage, 3);

            return percentage;
        }

        public static double Difference(Image img1, Image img2)
        {
            byte[,] img1GrayscaleValues = img1.GetGrayScaleValues();
            byte[,] img2GrayscaleValues = img2.GetGrayScaleValues();

            var normalizedHistogram1 = new double[16, 16];
            var normalizedHistogram2 = new double[16, 16];

            double histSum1 = 0.0;
            double histSum2 = 0.0;

            foreach (var value in img1GrayscaleValues) { histSum1 += value; }
            foreach (var value in img2GrayscaleValues) { histSum2 += value; }


            for (int x = 0; x < img1GrayscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < img1GrayscaleValues.GetLength(1); y++)
                {
                    normalizedHistogram1[x, y] = (double)img1GrayscaleValues[x, y] / histSum1;
                }
            }
            for (int x = 0; x < img2GrayscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < img2GrayscaleValues.GetLength(1); y++)
                {
                    normalizedHistogram2[x, y] = (double)img2GrayscaleValues[x, y] / histSum2;
                }
            }

            double bCoefficient = 0.0;
            for (int x = 0; x < img2GrayscaleValues.GetLength(0); x++)
            {
                for (int y = 0; y < img2GrayscaleValues.GetLength(1); y++)
                {
                    double histSquared = normalizedHistogram1[x, y] * normalizedHistogram2[x, y];
                    bCoefficient += Math.Sqrt(histSquared);
                }
            }

            double dist1 = 1.0 - bCoefficient;
            dist1 = Math.Round(dist1, 8);
            double distance = Math.Sqrt(dist1);
            distance = Math.Round(distance, 8);
            return distance;

        }
    }   
}
