using Newtonsoft.Json;

namespace PackageRepository.ViewModels.Npm {
    public class LoginViewModel {
        [JsonProperty(PropertyName = "_id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
