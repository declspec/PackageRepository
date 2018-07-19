using Newtonsoft.Json;

namespace PackageRepository.ViewModels {
    public class AttachmentViewModel
    {
        [JsonProperty(PropertyName = "content_type")]
        public string ContentType { get; set; }
        public string Data { get; set; }
    }
}
