/*
 * Copyright 2012 ZXing.Net authors
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
using Windows.Graphics.Imaging;
using ZXing;
#if WINDOWS_UWP
using Windows.UI.Xaml.Media.Imaging;
#else
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace Project2FA.ZXing
{
    /// <summary>
    /// A smart class to decode the barcode inside a bitmap object
    /// </summary>
    public class BarcodeReader : BarcodeReader<WriteableBitmap>, IBarcodeReader
    {
        private static readonly Func<SoftwareBitmap, LuminanceSource> defaultCreateLuminanceSourceSoftwareBitmap = (SoftwareBitmap bitmap) => new SoftwareBitmapLuminanceSource(bitmap);

        private readonly Func<SoftwareBitmap, LuminanceSource> createLuminanceSourceSoftwareBitmap;

        private static readonly Func<WriteableBitmap, LuminanceSource> defaultCreateLuminanceSource = (WriteableBitmap bitmap) => new BitmapLuminanceSource(bitmap);

        //
        // Summary:
        //     Optional: Gets or sets the function to create a luminance source object for a
        //     bitmap. If null a platform specific default LuminanceSource is used
        //
        // Value:
        //     The function to create a luminance source object.
        protected Func<SoftwareBitmap, LuminanceSource> CreateLuminanceSourceSoftwareBitmap => createLuminanceSourceSoftwareBitmap ?? defaultCreateLuminanceSourceSoftwareBitmap;

        //
        // Summary:
        //     Initializes a new instance of the ZXing.BarcodeReader class.
        //
        // Parameters:
        //   reader:
        //     Sets the reader which should be used to find and decode the barcode. If null
        //     then MultiFormatReader is used
        //
        //   createLuminanceSource:
        //     Sets the function to create a luminance source object for a bitmap. If null,
        //     an exception is thrown when Decode is called
        //
        //   createBinarizer:
        //     Sets the function to create a binarizer object for a luminance source. If null
        //     then HybridBinarizer is used
        public BarcodeReader(Reader reader, Func<SoftwareBitmap, LuminanceSource> createLuminanceSource, Func<LuminanceSource, Binarizer> createBinarizer)
            : this(reader, createLuminanceSource, createBinarizer, null)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the ZXing.BarcodeReader class.
        //
        // Parameters:
        //   reader:
        //     Sets the reader which should be used to find and decode the barcode. If null
        //     then MultiFormatReader is used
        //
        //   createLuminanceSource:
        //     Sets the function to create a luminance source object for a bitmap. If null,
        //     an exception is thrown when Decode is called
        //
        //   createBinarizer:
        //     Sets the function to create a binarizer object for a luminance source. If null
        //     then HybridBinarizer is used
        //
        //   createRGBLuminanceSource:
        //     Sets the function to create a luminance source object for a rgb raw byte array.
        public BarcodeReader(Reader reader, Func<SoftwareBitmap, LuminanceSource> createLuminanceSource, Func<LuminanceSource, Binarizer> createBinarizer, Func<byte[], int, int, RGBLuminanceSource.BitmapFormat, LuminanceSource> createRGBLuminanceSource)
            : base(reader, createBinarizer, createRGBLuminanceSource)
        {
            createLuminanceSourceSoftwareBitmap = createLuminanceSource;
        }

        //
        // Summary:
        //     Decodes the specified barcode bitmap.
        //
        // Parameters:
        //   barcodeBitmap:
        //     The barcode bitmap.
        //
        // Returns:
        //     the result data or null
        public Result Decode(SoftwareBitmap barcodeBitmap)
        {
            if (CreateLuminanceSourceSoftwareBitmap == null)
            {
                throw new InvalidOperationException("You have to declare a luminance source delegate.");
            }

            if (barcodeBitmap == null)
            {
                throw new ArgumentNullException("barcodeBitmap");
            }

            LuminanceSource luminanceSource = CreateLuminanceSourceSoftwareBitmap(barcodeBitmap);
            return Decode(luminanceSource);
        }

        //
        // Summary:
        //     Decodes the specified barcode bitmap.
        //
        // Parameters:
        //   barcodeBitmap:
        //     The barcode bitmap.
        //
        // Returns:
        //     the result data or null
        public Result[] DecodeMultiple(SoftwareBitmap barcodeBitmap)
        {
            if (CreateLuminanceSourceSoftwareBitmap == null)
            {
                throw new InvalidOperationException("You have to declare a luminance source delegate.");
            }

            if (barcodeBitmap == null)
            {
                throw new ArgumentNullException("barcodeBitmap");
            }

            LuminanceSource luminanceSource = CreateLuminanceSourceSoftwareBitmap(barcodeBitmap);
            return DecodeMultiple(luminanceSource);
        }

        //
        // Summary:
        //     Initializes a new instance of the ZXing.BarcodeReader class.
        public BarcodeReader()
            : this(null, defaultCreateLuminanceSource, null)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the ZXing.BarcodeReader class.
        //
        // Parameters:
        //   reader:
        //     Sets the reader which should be used to find and decode the barcode. If null
        //     then MultiFormatReader is used
        //
        //   createLuminanceSource:
        //     Sets the function to create a luminance source object for a bitmap. If null,
        //     an exception is thrown when Decode is called
        //
        //   createBinarizer:
        //     Sets the function to create a binarizer object for a luminance source. If null
        //     then HybridBinarizer is used
        public BarcodeReader(Reader reader, Func<WriteableBitmap, LuminanceSource> createLuminanceSource, Func<LuminanceSource, Binarizer> createBinarizer)
            : base(reader, createLuminanceSource ?? defaultCreateLuminanceSource, createBinarizer)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the ZXing.BarcodeReader class.
        //
        // Parameters:
        //   reader:
        //     Sets the reader which should be used to find and decode the barcode. If null
        //     then MultiFormatReader is used
        //
        //   createLuminanceSource:
        //     Sets the function to create a luminance source object for a bitmap. If null,
        //     an exception is thrown when Decode is called
        //
        //   createBinarizer:
        //     Sets the function to create a binarizer object for a luminance source. If null
        //     then HybridBinarizer is used
        //
        //   createRGBLuminanceSource:
        //     Sets the function to create a luminance source object for a rgb raw byte array.
        public BarcodeReader(Reader reader, Func<WriteableBitmap, LuminanceSource> createLuminanceSource, Func<LuminanceSource, Binarizer> createBinarizer, Func<byte[], int, int, RGBLuminanceSource.BitmapFormat, LuminanceSource> createRGBLuminanceSource)
            : base(reader, createLuminanceSource ?? defaultCreateLuminanceSource, createBinarizer, createRGBLuminanceSource)
        {
        }
    }
}