using Project2FA.Repository.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Project2FA.Services
{
    [JsonSerializable(typeof(Dictionary<string, Dictionary<string, string>>))]
    [JsonSerializable(typeof(NextcloudLoginFlowV2Response))]
    [JsonSerializable(typeof(NextcloudLoginFlowV2Poll))]
    [JsonSerializable(typeof(NextcloudLoginFlowV2Credentials))]
    [JsonSerializable(typeof(DatafileModel))]
    [JsonSerializable(typeof(TwoFACodeModel))]
    [JsonSerializable(typeof(CategoryModel))]
    [JsonSerializable(typeof(FontIdentifikationCollectionModel))]
    [JsonSerializable(typeof(DependencyRootModel))]
    //[JsonSerializable(typeof(List<DependencyRootModel>))]
    [JsonSerializable(typeof(List<DependencyGroupModel>))]
    [JsonSerializable(typeof(List<DependencyModel>))]
    //[JsonSerializable(typeof(ObservableGroupedCollection<string, DependencyModel>))]
    [JsonSerializable(typeof(AegisModel<AegisDecryptedDatabase>))]
    [JsonSerializable(typeof(AegisModel<string>))]
    [JsonSerializable(typeof(List<AndOTPModel<string>>))]
    [JsonSerializable(typeof(List<TwoFASBackup>))]
    [JsonSerializable(typeof(NextcloudLoginFlowV2Response))]
    [JsonSerializable(typeof(NextcloudLoginFlowV2Credentials))]

    public partial class SerializationContext : JsonSerializerContext
    {
    }
}
