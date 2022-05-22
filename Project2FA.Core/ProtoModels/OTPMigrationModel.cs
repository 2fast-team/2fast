//based on https://github.com/Rookiestyle/KeePassOTP/blob/a1aed0a74fc897b406f10e6a5b48f30af68536ea/src/GoogleAuthenticatorImport/GoogleAuthenticatorImport.cs
#region Designer generated code
#pragma warning disable 0612, 0618, 1591, 3021
namespace Project2FA.Core.ProtoModels
{

	[global::ProtoBuf.ProtoContract()]
	public partial class OTPMigrationModel : global::ProtoBuf.IExtensible
	{
		private global::ProtoBuf.IExtension __pbn__extensionData;
		global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
		{
			return global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);
		}
		public OTPMigrationModel()
		{
			otp_parameters = new global::System.Collections.Generic.List<OtpParameters>();
			OnConstructor();
		}

		partial void OnConstructor();

		[global::ProtoBuf.ProtoMember(1)]
		public global::System.Collections.Generic.List<OtpParameters> otp_parameters { get; set; }

		[global::ProtoBuf.ProtoMember(2, Name = @"version")]
		public int Version { get; set; }

		[global::ProtoBuf.ProtoMember(3, Name = @"batch_size")]
		public int BatchSize { get; set; }

		[global::ProtoBuf.ProtoMember(4, Name = @"batch_index")]
		public int BatchIndex { get; set; }

		[global::ProtoBuf.ProtoMember(5, Name = @"batch_id")]
		public int BatchId { get; set; }

		[global::ProtoBuf.ProtoContract()]
		public partial class OtpParameters : global::ProtoBuf.IExtensible
		{
			private global::ProtoBuf.IExtension __pbn__extensionData;
			global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
			{
				return global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);
			}
			public OtpParameters()
			{
				Name = "";
				Issuer = "";
				OnConstructor();
			}

			partial void OnConstructor();

			[global::ProtoBuf.ProtoMember(1, Name = @"secret")]
			public byte[] Secret { get; set; }

			[global::ProtoBuf.ProtoMember(2, Name = @"name")]
			[global::System.ComponentModel.DefaultValue("")]
			public string Name { get; set; }

			[global::ProtoBuf.ProtoMember(3, Name = @"issuer")]
			[global::System.ComponentModel.DefaultValue("")]
			public string Issuer { get; set; }

			[global::ProtoBuf.ProtoMember(4, Name = @"algorithm")]
			public OTPMigrationModel.Algorithm Algorithm { get; set; }

			[global::ProtoBuf.ProtoMember(5, Name = @"digits")]
			public OTPMigrationModel.DigitCount Digits { get; set; }

			[global::ProtoBuf.ProtoMember(6, Name = @"type")]
			public OTPMigrationModel.OtpType Type { get; set; }

			[global::ProtoBuf.ProtoMember(7, Name = @"counter")]
			public long Counter { get; set; }

		}

		[global::ProtoBuf.ProtoContract()]
		public enum Algorithm
		{
			[global::ProtoBuf.ProtoEnum(Name = @"ALGORITHM_UNSPECIFIED")]
			AlgorithmUnspecified = 0,
			[global::ProtoBuf.ProtoEnum(Name = @"ALGORITHM_SHA1")]
			AlgorithmSha1 = 1,
			[global::ProtoBuf.ProtoEnum(Name = @"ALGORITHM_SHA256")]
			AlgorithmSha256 = 2,
			[global::ProtoBuf.ProtoEnum(Name = @"ALGORITHM_SHA512")]
			AlgorithmSha512 = 3,
			[global::ProtoBuf.ProtoEnum(Name = @"ALGORITHM_MD5")]
			AlgorithmMd5 = 4,
		}

		[global::ProtoBuf.ProtoContract()]
		public enum DigitCount
		{
			[global::ProtoBuf.ProtoEnum(Name = @"DIGIT_COUNT_UNSPECIFIED")]
			DigitCountUnspecified = 0,
			[global::ProtoBuf.ProtoEnum(Name = @"DIGIT_COUNT_SIX")]
			DigitCountSix = 1,
			[global::ProtoBuf.ProtoEnum(Name = @"DIGIT_COUNT_EIGHT")]
			DigitCountEight = 2,
		}

		[global::ProtoBuf.ProtoContract()]
		public enum OtpType
		{
			[global::ProtoBuf.ProtoEnum(Name = @"OTP_TYPE_UNSPECIFIED")]
			OtpTypeUnspecified = 0,
			[global::ProtoBuf.ProtoEnum(Name = @"OTP_TYPE_HOTP")]
			OtpTypeHotp = 1,
			[global::ProtoBuf.ProtoEnum(Name = @"OTP_TYPE_TOTP")]
			OtpTypeTotp = 2,
		}

	}

}

#pragma warning restore 0612, 0618, 1591, 3021
#endregion
