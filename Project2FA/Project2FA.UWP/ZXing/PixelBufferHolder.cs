using Windows.Foundation;

namespace ZXing.Net.UWP.Readers
{
    public record PixelBufferHolder
    {
    	public Size Size { get; set; }

    	public

#if ANDROID
    	Java.Nio.ByteBuffer
#elif IOS || MACCATALYST
    	CoreVideo.CVPixelBuffer
#elif WINDOWS_UWP
    	Windows.Graphics.Imaging.SoftwareBitmap
#else
    	byte[]
#endif

    	Data { get; set; }
    }
}
