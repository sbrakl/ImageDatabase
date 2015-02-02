using System;
using System.Collections.Generic;

namespace EyeOpen.Imaging.Processing
{
    [Serializable]
	public class RgbProjections
	{
		private double[] horizontalProjection;

        private double[] verticalProjection;

		public double[] HorizontalProjection 
		{
			get
			{
				return this.horizontalProjection;
			}
            set
            {
                horizontalProjection = value;
            }
		}

		public double[] VerticalProjection
		{
			get
			{
				return this.verticalProjection;
			}
            set
            {
                verticalProjection = value;
            }
		}

        public RgbProjections()
        {
        }

		public RgbProjections(double[][] projections) : this(projections[0], projections[1])
		{
		}

		internal RgbProjections(double[] horizontalProjection, double[] verticalProjection)
		{
			this.horizontalProjection = horizontalProjection;
			this.verticalProjection = verticalProjection;
		}

		private static double CalculateProjectionSimilarity(double[] source, double[] compare)
		{
			if ((int)source.Length != (int)compare.Length)
			{
				throw new ArgumentException();
			}
			Dictionary<double, int> nums = new Dictionary<double, int>();
			for (int i = 0; i < (int)source.Length; i++)
			{
				double num = source[i] - compare[i];
				num = Math.Round(num, 2);
				num = Math.Abs(num);
				if (!nums.ContainsKey(num))
				{
					nums.Add(num, 1);
				}
				else
				{
					nums[num] = nums[num] + 1;
				}
			}
			double key = 0;
			foreach (KeyValuePair<double, int> keyValuePair in nums)
			{
				key = key + keyValuePair.Key * (double)keyValuePair.Value;
			}
			key = key / (double)((int)source.Length);
			key = (0.5 - key) * 2;
			return key;
		}

		public double CalculateSimilarity(RgbProjections compare)
		{
			double num = RgbProjections.CalculateProjectionSimilarity(this.horizontalProjection, compare.horizontalProjection);
			double num1 = RgbProjections.CalculateProjectionSimilarity(this.verticalProjection, compare.verticalProjection);
			return Math.Max(num, num1);
		}
	}
}