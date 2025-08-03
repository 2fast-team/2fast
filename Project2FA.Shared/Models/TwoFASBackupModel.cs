using CommunityToolkit.Common;
using Newtonsoft.Json;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.Repository.Models
{
    internal sealed class TwoFASBackup
    {
        [JsonProperty(PropertyName = "services")]
        public List<TwoFASServiceModel> Services { get; set; }

        [JsonProperty(PropertyName = "groups")]
        public List<TwoFASGroupModel> Groups { get; set; }

        [JsonProperty(PropertyName = "servicesEncrypted")]
        public string ServicesEncrypted { get; set; }
    }
    internal sealed class TwoFASOtpModel
    {
        [JsonProperty(PropertyName = "account")]
        public string Account { get; set; }

        [JsonProperty(PropertyName = "issuer")]
        public string Issuer { get; set; }

        [JsonProperty(PropertyName = "digits")]
        public int Digits { get; set; }

        [JsonProperty(PropertyName = "period")]
        public int Period { get; set; }

        [JsonProperty(PropertyName = "algorithm")]
        public string Algorithm { get; set; }

        [JsonProperty(PropertyName = "counter")]
        public int Counter { get; set; }

        [JsonProperty(PropertyName = "tokenType")]
        public string TokenType { get; set; }
    }

    internal sealed class TwoFASGroupModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    internal sealed class TwoFASServiceModel
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "secret")]
        public string Secret { get; set; }

        [JsonProperty(PropertyName = "otp")]
        public OtpHashMode HashMode { get; set; }

        [JsonProperty(PropertyName = "groupId")]
        public string GroupId { get; set; }
    }
}
