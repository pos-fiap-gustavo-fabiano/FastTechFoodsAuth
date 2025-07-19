using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastTechFoodsAuth.Application.DTOs
{
    public class AuthResultDto
    {
        [JsonPropertyName("access_token")]
        public string Access_Token { get; set; }
        public string RefreshToken { get; set; }
        public UserDto User { get; set; }
    }
}
