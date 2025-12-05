using CommunityToolkit.Mvvm.Collections;
using Project2FA.Repository.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Project2FA.Services
{
    [JsonSerializable(typeof(DatafileModel))]
    [JsonSerializable(typeof(TwoFACodeModel))]
    [JsonSerializable(typeof(CategoryModel))]
    [JsonSerializable(typeof(FontIdentifikationCollectionModel))]
    [JsonSerializable(typeof(DependencyModel))]
    [JsonSerializable(typeof(List<DependencyModel>))]
    [JsonSerializable(typeof(ObservableGroupedCollection<string, DependencyModel>))]
    [JsonSerializable(typeof(AegisModel<AegisDecryptedDatabase>))]
    [JsonSerializable(typeof(AegisModel<string>))]
    [JsonSerializable(typeof(List<AndOTPModel<string>>))]
    [JsonSerializable(typeof(List<TwoFASBackup>))]

    public partial class SerializationContext : JsonSerializerContext
    {
    }
}
