using System;
using System.Collections.Generic;
using System.IO;

namespace EyeOpen.Imaging.Processing
{
	public class SimilarityImages : IComparer<SimilarityImages>, IComparable
	{
		private ComparableImage source;

		private ComparableImage destination;

		private double similarity;

		public ComparableImage Destination
		{
			get
			{
				return this.destination;
			}
		}

		public double Similarity
		{
			get
			{
				return Math.Round(this.similarity * 100, 1);
			}
		}

		public ComparableImage Source
		{
			get
			{
				return this.source;
			}
		}

		public SimilarityImages(ComparableImage source, ComparableImage destination, double similarity)
		{
			this.source = source;
			this.destination = destination;
			this.similarity = similarity;
		}

		public int Compare(SimilarityImages x, SimilarityImages y)
		{
			return x.similarity.CompareTo(y.similarity);
		}

		public int CompareTo(object obj)
		{
			return this.Compare(this, (SimilarityImages)obj);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || this.GetType() != obj.GetType())
			{
				return false;
			}
			SimilarityImages similarityImage = (SimilarityImages)obj;
			if (!this.Source.File.FullName.Equals(similarityImage.Source.File.FullName, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (!this.Destination.File.FullName.Equals(similarityImage.Destination.File.FullName, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return string.Format("{0};{1}", this.Source.File.FullName, this.Destination.File.FullName).GetHashCode();
		}

		public static int operator ==(SimilarityImages value, SimilarityImages compare)
		{
			return value.CompareTo(compare);
		}

		public static int operator >(SimilarityImages value, SimilarityImages compare)
		{
			return value.CompareTo(compare);
		}

		public static int operator !=(SimilarityImages value, SimilarityImages compare)
		{
			return value.CompareTo(compare);
		}

		public static int operator <(SimilarityImages value, SimilarityImages compare)
		{
			return value.CompareTo(compare);
		}

		public override string ToString()
		{
			return string.Format("{0}, {1} --> {2}", this.source.File.Name, this.destination.File.Name, this.similarity);
		}
	}
}