﻿using Windows.Foundation;

namespace ZXing.Net.UWP.Readers;

public record PixelBufferHolder
{
	public Size Size { get; init; }

	public

#if ANDROID
	Java.Nio.ByteBuffer
#elif IOS || MACCATALYST
	CoreVideo.CVPixelBuffer
#elif WINDOWS
	Windows.Graphics.Imaging.SoftwareBitmap
#else
	byte[]
#endif

	Data { get; init; }
}
