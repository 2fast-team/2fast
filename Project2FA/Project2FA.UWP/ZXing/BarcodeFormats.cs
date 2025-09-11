using System;
using System.Collections.Generic;

namespace ZXing.Net.UWP
{
    public static class BarcodeFormats
    {
    	internal static IList<BarcodeFormat> ToZXingList(this BarcodeFormat formats)
    	{
    		var items = new List<BarcodeFormat>();

    		foreach (BarcodeFormat enumValue in Enum.GetValues(typeof(BarcodeFormat)))
    		{
    			if (formats.HasFlag(enumValue))
    				items.Add((BarcodeFormat)enumValue);
    		}

    		return items;
    	}


    	/// <summary>
    	/// UPC_A | UPC_E | EAN_13 | EAN_8 | CODABAR | CODE_39 | CODE_93 | CODE_128 | ITF | RSS_14 | RSS_EXPANDED
    	/// without MSI (to many false-positives) and IMB (not enough tested, and it looks more like a 2D)
    	/// </summary>
    	public static BarcodeFormat OneDimensional =>
    		BarcodeFormat.UPC_A
    		| BarcodeFormat.UPC_E
    		| BarcodeFormat.EAN_13
    		| BarcodeFormat.EAN_8
    		| BarcodeFormat.CODABAR
    		| BarcodeFormat.CODE_39
    		| BarcodeFormat.CODE_93
    		| BarcodeFormat.CODE_128
    		| BarcodeFormat.ITF
    		| BarcodeFormat.RSS_14
    		| BarcodeFormat.RSS_EXPANDED;

    	public static BarcodeFormat TwoDimensional =>
    		BarcodeFormat.AZTEC
    		| BarcodeFormat.DATA_MATRIX
    		| BarcodeFormat.ITF
    		| BarcodeFormat.MAXICODE
    		| BarcodeFormat.PDF_417
    		| BarcodeFormat.QR_CODE
    		| BarcodeFormat.UPC_EAN_EXTENSION
    		| BarcodeFormat.MSI
    		| BarcodeFormat.PLESSEY
    		| BarcodeFormat.IMB;
    		// Seems to have a lot of false positives, so leave it out of this group
    		//| BarcodeFormat.PharmaCode;

    	public static BarcodeFormat All =>
    		BarcodeFormat.AZTEC
    		| BarcodeFormat.CODABAR
    		| BarcodeFormat.CODE_39
    		| BarcodeFormat.CODE_93
    		| BarcodeFormat.CODE_128
    		| BarcodeFormat.DATA_MATRIX
    		| BarcodeFormat.EAN_8
    		| BarcodeFormat.EAN_13
    		| BarcodeFormat.ITF
    		| BarcodeFormat.MAXICODE
    		| BarcodeFormat.PDF_417
    		| BarcodeFormat.QR_CODE
    		| BarcodeFormat.RSS_14
    		| BarcodeFormat.RSS_EXPANDED
    		| BarcodeFormat.UPC_A
    		| BarcodeFormat.UPC_E
    		| BarcodeFormat.UPC_EAN_EXTENSION
    		| BarcodeFormat.MSI
    		| BarcodeFormat.PLESSEY
    		| BarcodeFormat.IMB;
    		// Seems to have a lot of false positives, so leave it out of this group
    		//| BarcodeFormat.PharmaCode;
    }
}
