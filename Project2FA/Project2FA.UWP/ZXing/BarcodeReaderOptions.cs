namespace ZXing.Net.UWP
{
    public record BarcodeReaderOptions
    {
    	public bool AutoRotate { get; set; }

    	public bool TryHarder { get; set; }

    	public bool TryInverted { get; set; }

    	public BarcodeFormat Formats { get; set; }

    	public bool Multiple { get; set; }

    	public bool UseCode39ExtendedMode { get; set; }
    }
}
