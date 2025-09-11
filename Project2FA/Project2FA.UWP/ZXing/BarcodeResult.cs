using System.Collections.Generic;
using Windows.Foundation;

namespace ZXing.Net.UWP
{
	public record BarcodeResult
	{
		public byte[] Raw { get; set; }

		public string Value { get; set; }

		public BarcodeFormat Format { get; set; }

		public IReadOnlyDictionary<MetadataType, object> Metadata { get; set; }

		public Point[] PointsOfInterest { get; set; }
	}
}
