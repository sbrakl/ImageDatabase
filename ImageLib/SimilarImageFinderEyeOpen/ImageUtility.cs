using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace EyeOpen.Imaging.Processing
{
	public static class ImageUtility
	{
		public static unsafe double[][] GetRgbProjections(Bitmap bitmap)
		{
			int width = bitmap.Width - 1;
			int num = bitmap.Width - 1;
			double[] numArray = new double[width];
			double[] numArray1 = new double[num];
			BitmapData bitmapDatum = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			int num1 = 0;
			byte* scan0 = (byte*)((void*)bitmapDatum.Scan0);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < width; j++)
				{
					byte num2 = *scan0;
					byte num3 = *(scan0 + 1);
					byte num4 = *(scan0 + 2);
					num1 = (byte)(0.2126 * (double)num4 + 0.7152 * (double)num3 + 0.0722 * (double)num2);
					numArray[j] = numArray[j] + (double)num1;
					numArray1[i] = numArray1[i] + (double)num1;
					scan0 = scan0 + 4;
				}
				scan0 = scan0 + (bitmapDatum.Stride - bitmapDatum.Width * 4);
			}
			ImageUtility.MaximizeScale(ref numArray, (double)num);
			ImageUtility.MaximizeScale(ref numArray1, (double)width);
			double[][] numArray2 = new double[][] { numArray, numArray1 };
			bitmap.UnlockBits(bitmapDatum);
			return numArray2;
		}

		private static void MaximizeScale(ref double[] projection, double max)
		{
			double num = double.MaxValue;
			double num1 = double.MinValue;
			for (int i = 0; i < (int)projection.Length; i++)
			{
				if (projection[i] > 0)
				{
					projection[i] = projection[i] / max;
				}
				if (projection[i] < num)
				{
					num = projection[i];
				}
				if (projection[i] > num1)
				{
					num1 = projection[i];
				}
			}
			if (num1 == 0)
			{
				return;
			}
			for (int j = 0; j < (int)projection.Length; j++)
			{
				if (num1 != 255)
				{
					projection[j] = (projection[j] - num) / (num1 - num);
				}
				else
				{
					projection[j] = 1;
				}
			}
		}

		public static Bitmap ResizeBitmap(Bitmap bitmap, int width, int height)
		{
			Bitmap bitmap1 = new Bitmap(width, height);
			using (Graphics graphic = Graphics.FromImage(bitmap1))
			{
				graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphic.DrawImage(bitmap, 0, 0, width - 1, height - 1);
			}
			return bitmap1;
		}
	}
}