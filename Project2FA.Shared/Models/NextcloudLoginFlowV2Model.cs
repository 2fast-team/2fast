using System.Text.Json.Serialization;

namespace Project2FA.Repository.Models
{
    /// <summary>
    /// Response from POST /index.php/login/v2
    /// </summary>
    public class NextcloudLoginFlowV2Response
    {
        [JsonPropertyName("poll")]
        public NextcloudLoginFlowV2Poll Poll { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; }
    }

    public class NextcloudLoginFlowV2Poll
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; }
    }

    /// <summary>
    /// Response from polling POST /login/v2/poll once the user has authenticated.
    /// </summary>
    public class NextcloudLoginFlowV2Credentials
    {
        [JsonPropertyName("server")]
        public string Server { get; set; }

        [JsonPropertyName("loginName")]
        public string LoginName { get; set; }

        [JsonPropertyName("appPassword")]
        public string AppPassword { get; set; }
    }
}