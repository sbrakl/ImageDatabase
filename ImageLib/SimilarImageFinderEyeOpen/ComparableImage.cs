using System;
using System.Drawing;
using System.IO;

namespace EyeOpen.Imaging.Processing
{
	public class ComparableImage
	{
		private FileInfo file;

		private RgbProjections projections;

		public FileInfo File
		{
			get
			{
				return this.file;
			}
		}

		public RgbProjections Projections
		{
			get
			{
				return this.projections;
			}
		}

		public ComparableImage(FileInfo file)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
			if (!file.Exists)
			{
				throw new ArgumentNullException("file");
			}
			this.file = file;
			using (Bitmap bitmap = ImageUtility.ResizeBitmap(new Bitmap(file.FullName), 100, 100))
			{
				this.projections = new RgbProjections(ImageUtility.GetRgbProjections(bitmap));
			}
		}

		public double CalculateSimilarity(ComparableImage compare)
		{
			return this.projections.CalculateSimilarity(compare.projections);
		}

		public override string ToString()
		{
			return this.file.Name;
		}
	}
}