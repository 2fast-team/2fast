using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Common;
using OtpNet;

//based on https://github.com/stratumauth/app/blob/e495a8f9b18a29d1dc00a4d45b77529dc3fbe3a7/Stratum.Core/src/Converter/AegisBackupConverter.cs#L23
// Copyright (C) 2022 jmh
// SPDX-License-Identifier: GPL-3.0-only

namespace Project2FA.Repository.Models
{
    internal sealed class AegisModel<T>
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("header")]
        public AegisHeader Header { get; set; }

        [JsonPropertyName("db")]
        public T Database { get; set; }
    }

    internal sealed class AegisKeyParams
    {
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }

    internal sealed class AegisHeader
    {
        [JsonPropertyName("slots")]
        public List<AegisSlot> Slots { get; set; }

        [JsonPropertyName("params")]
        public AegisKeyParams Params { get; set; }
    }

    internal enum AegisSlotType
    {
        Raw = 0,
        Password = 1,
        Biometric = 2
    }

    internal sealed class AegisSlot
    {
        [JsonPropertyName("type")]
        public AegisSlotType Type { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("key_params")]
        public AegisKeyParams KeyParams { get; set; }

        [JsonPropertyName("salt")]
        public string Salt { get; set; }

        [JsonPropertyName("n")]
        public int N { get; set; }

        [JsonPropertyName("r")]
        public int R { get; set; }

        [JsonPropertyName("p")]
        public int P { get; set; }
    }

    internal sealed class AegisDecryptedDatabase
    {
        [JsonPropertyName("entries")]
        public List<AegisEntry> Entries { get; set; }
    }

    internal sealed class AegisEntry
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("issuer")]
        public string Issuer { get; set; }

        [JsonPropertyName("group")]
        public string Group { get; set; }

        //[JsonPropertyName("icon")]
        //[JsonConverter(typeof(ByteArrayConverter))]
        //public byte[] Icon { get; set; }

        [JsonPropertyName("info")]
        public AegisEntryInfo Info { get; set; }
    }

    internal sealed class AegisEntryInfo
    {
        [JsonPropertyName("secret")]
        public string Secret { get; set; }

        [JsonPropertyName("algo")]
        public string Algorithm { get; set; }

        [JsonPropertyName("digits")]
        public int Digits { get; set; }

        [JsonPropertyName("period")]
        public int Period { get; set; }

        [JsonPropertyName("counter")]
        public int Counter { get; set; }

        [JsonPropertyName("pin")]
        public string Pin { get; set; }
    }
}
