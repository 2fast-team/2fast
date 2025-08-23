﻿using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ZXing.Net.UWP;

public class SoftwareBitmapLuminanceSource : BaseLuminanceSource
{
	public SoftwareBitmapLuminanceSource(SoftwareBitmap softwareBitmap) : base(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight)
		=> CalculateLuminance(softwareBitmap);

	protected SoftwareBitmapLuminanceSource(int width, int height) : base(width, height)
	{
	}

	protected SoftwareBitmapLuminanceSource(byte[] luminanceArray, int width, int height) : base(luminanceArray, width, height)
	{
	}

	protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
		=> new SoftwareBitmapLuminanceSource(width, height) { luminances = newLuminances };

	void CalculateLuminance(SoftwareBitmap bitmap)
	{
		if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Gray8)
        {
	        using SoftwareBitmap convertedSoftwareBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Gray8);
	        convertedSoftwareBitmap.CopyToBuffer(luminances.AsBuffer());
        }
        else
        {
            bitmap.CopyToBuffer(luminances.AsBuffer());
        }
	}
}