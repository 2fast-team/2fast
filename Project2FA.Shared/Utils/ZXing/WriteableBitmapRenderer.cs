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
using Windows.UI.Text;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

#if WINDOWS_UWP
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace Project2FA.ZXing.Rendering
{
    /// <summary>
    /// Renders a <see cref="BitMatrix" /> to a <see cref="WriteableBitmap" />
    /// </summary>
    public class WriteableBitmapRenderer : IBarcodeRenderer<WriteableBitmap>
    {
        private static readonly FontFamily DefaultFontFamily = new FontFamily("Arial");

        //
        // Summary:
        //     Gets or sets the foreground color.
        //
        // Value:
        //     The foreground color.
        public Color Foreground { get; set; }

        //
        // Summary:
        //     Gets or sets the background color.
        //
        // Value:
        //     The background color.
        public Color Background { get; set; }

        //
        // Summary:
        //     Gets or sets the font family.
        //
        // Value:
        //     The font family.
        public FontFamily FontFamily { get; set; }

        //
        // Summary:
        //     Gets or sets the size of the font.
        //
        // Value:
        //     The size of the font.
        public double FontSize { get; set; }

        //
        // Summary:
        //     Initializes a new instance of the ZXing.Rendering.WriteableBitmapRenderer class.
        public WriteableBitmapRenderer()
        {
            Foreground = Colors.Black;
            Background = Colors.White;
            FontFamily = DefaultFontFamily;
            FontSize = 10.0;
        }

        //
        // Summary:
        //     Renders the specified matrix.
        //
        // Parameters:
        //   matrix:
        //     The matrix.
        //
        //   format:
        //     The format.
        //
        //   content:
        //     The content.
        public WriteableBitmap Render(BitMatrix matrix, BarcodeFormat format, string content)
        {
            return Render(matrix, format, content, null);
        }

        //
        // Summary:
        //     Renders the specified matrix.
        //
        // Parameters:
        //   matrix:
        //     The matrix.
        //
        //   format:
        //     The format.
        //
        //   content:
        //     The content.
        //
        //   options:
        //     The options.
        public virtual WriteableBitmap Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options)
        {
            int width = matrix.Width;
            int height = matrix.Height;
            int num = (((options == null || !options.PureBarcode) && !string.IsNullOrEmpty(content) && (format == BarcodeFormat.CODE_39 || format == BarcodeFormat.CODE_128 || format == BarcodeFormat.EAN_13 || format == BarcodeFormat.EAN_8 || format == BarcodeFormat.CODABAR || format == BarcodeFormat.ITF || format == BarcodeFormat.UPC_A || format == BarcodeFormat.MSI || format == BarcodeFormat.PLESSEY)) ? 16 : 0);
            int num2 = 1;
            if (options != null)
            {
                if (options.Width > width)
                {
                    width = options.Width;
                }

                if (options.Height > height)
                {
                    height = options.Height;
                }

                num2 = width / matrix.Width;
                if (num2 > height / matrix.Height)
                {
                    num2 = height / matrix.Height;
                }
            }

            byte[] array = new byte[4] { Foreground.B, Foreground.G, Foreground.R, Foreground.A };
            byte[] array2 = new byte[4] { Background.B, Background.G, Background.R, Background.A };
            WriteableBitmap writeableBitmap = new WriteableBitmap(width, height);
            using (Stream stream = writeableBitmap.PixelBuffer.AsStream())
            {
                for (int i = 0; i < matrix.Height - num; i++)
                {
                    for (int j = 0; j < num2; j++)
                    {
                        for (int k = 0; k < matrix.Width; k++)
                        {
                            byte[] buffer = (matrix[k, i] ? array : array2);
                            for (int l = 0; l < num2; l++)
                            {
                                stream.Write(buffer, 0, 4);
                            }
                        }

                        for (int m = num2 * matrix.Width; m < width; m++)
                        {
                            stream.Write(array2, 0, 4);
                        }
                    }
                }

                for (int n = matrix.Height * num2 - num; n < height; n++)
                {
                    for (int num3 = 0; num3 < width; num3++)
                    {
                        stream.Write(array2, 0, 4);
                    }
                }
            }

            writeableBitmap.Invalidate();
            return writeableBitmap;
        }
    }
}
