/*
* Copyright 2017 ZXing.Net authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Text;
using ZXing;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
#if WINDOWS_UWP
using Windows.UI.Xaml.Media.Imaging;
#else
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace Project2FA.ZXing
{
    //
    // Summary:
    //     class which represents the luminance values for a bitmap object of a WriteableBitmap
    //     class
    public class BitmapLuminanceSource : BaseLuminanceSource
    {
        //
        // Summary:
        //     Initializes a new instance of the ZXing.BitmapLuminanceSource class.
        //
        // Parameters:
        //   width:
        //     The width.
        //
        //   height:
        //     The height.
        protected BitmapLuminanceSource(int width, int height)
            : base(width, height)
        {
        }

        //
        // Summary:
        //     initializing constructor
        //
        // Parameters:
        //   writeableBitmap:
        public BitmapLuminanceSource(WriteableBitmap writeableBitmap)
            : base(writeableBitmap.PixelWidth, writeableBitmap.PixelHeight)
        {
            int pixelHeight = writeableBitmap.PixelHeight;
            int pixelWidth = writeableBitmap.PixelWidth;
            byte[] array = writeableBitmap.PixelBuffer.ToArray(0u, (int)writeableBitmap.PixelBuffer.Length);
            if (array.Length != writeableBitmap.PixelBuffer.Length)
            {
                throw new InvalidOperationException($"The WriteableBitmap instance isn't correct initialized. The PixelBuffer length is {writeableBitmap.PixelBuffer.Length}, but the resulting data array is {array.Length}");
            }

            int num = 0;
            int num2 = pixelWidth * pixelHeight * 4;
            if (array.Length != writeableBitmap.PixelBuffer.Length)
            {
                throw new InvalidOperationException($"The WriteableBitmap instance isn't correct initialized. The PixelBuffer length is {writeableBitmap.PixelBuffer.Length}, but it should be {num2} (height * width * 4)");
            }

            for (int i = 0; i < num2; i += 4)
            {
                Color color = Color.FromArgb(array[i], array[i + 1], array[i + 2], array[i + 3]);
                luminances[num] = (byte)(19562 * color.R + 38550 * color.G + 7424 * color.B >> 16);
                num++;
            }
        }

        //
        // Summary:
        //     Should create a new luminance source with the right class type. The method is
        //     used in methods crop and rotate.
        //
        // Parameters:
        //   newLuminances:
        //     The new luminances.
        //
        //   width:
        //     The width.
        //
        //   height:
        //     The height.
        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
        {
            return new BitmapLuminanceSource(width, height)
            {
                luminances = newLuminances
            };
        }
    }
}
