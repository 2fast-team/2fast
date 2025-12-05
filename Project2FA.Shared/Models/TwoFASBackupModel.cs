using CommunityToolkit.Common;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Project2FA.Repository.Models
{
    public sealed class TwoFASBackup
    {
        [JsonPropertyName("services")]
        public List<TwoFASServiceModel> Services { get; set; }

        [JsonPropertyName("groups")]
        public List<TwoFASGroupModel> Groups { get; set; }

        [JsonPropertyName("servicesEncrypted")]
        public string ServicesEncrypted { get; set; }
    }
    public sealed class TwoFASOtpModel
    {
        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("issuer")]
        public string Issuer { get; set; }

        [JsonPropertyName("digits")]
        public int Digits { get; set; }

        [JsonPropertyName("period")]
        public int Period { get; set; }

        [JsonPropertyName("algorithm")]
        public string Algorithm { get; set; }

        [JsonPropertyName("counter")]
        public int Counter { get; set; }

        [JsonPropertyName("tokenType")]
        public string TokenType { get; set; }
    }

    public sealed class TwoFASGroupModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public sealed class TwoFASServiceModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("secret")]
        public string Secret { get; set; }

        [JsonPropertyName("otp")]
        public TwoFASOtpModel Otp { get; set; }

        [JsonPropertyName("groupId")]
        public string GroupId { get; set; }
    }
}
