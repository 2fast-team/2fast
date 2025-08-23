using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Project2FA.Services
{
    // List<AndOTPModel<string>>
    [JsonSerializable(typeof(DatafileModel))]
    [JsonSerializable(typeof(FontIdentifikationCollectionModel))]
    [JsonSerializable(typeof(DependencyModel))]
    [JsonSerializable(typeof(List<DependencyModel>))]
    [JsonSerializable(typeof(AegisModel<AegisDecryptedDatabase>))]
    [JsonSerializable(typeof(AegisModel<string>))]
    [JsonSerializable(typeof(List<AndOTPModel<string>>))]
    public partial class SerializationContext : JsonSerializerContext
    {
    }
}
