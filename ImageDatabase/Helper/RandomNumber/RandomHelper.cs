using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDatabase.Helper.RandomNumber
{
    public class RandomHelper: MersenneTwister
    {
        public RandomHelper(): base(Convert.ToUInt32(DateTime.Now.Second))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min">Minimum Number</param>
        /// <param name="max">Maximum Number</param>
        /// <param name="percentage">Percantage in number (0-100)</param>
        /// <returns></returns>
        public List<int> GetRandomNumberInRange(int min, int max, double percentage)
        {
            List<int> rtnNumbers = new List<int>();

            int diff = max - min;
            int totalNumbersToBeGenerate = Convert.ToInt32(Math.Ceiling(diff * (percentage / 100d)));
            for (int i = 0; i < totalNumbersToBeGenerate; i++ )
            {
                int randNum = (int)this.NextUInt((uint)min, (uint)max);
                rtnNumbers.Add(randNum);
            }
            rtnNumbers.Sort();
            return rtnNumbers;
        }        
    }
}
