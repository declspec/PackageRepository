using Newtonsoft.Json;
using PackageRepository.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PackageRepository.ViewModels {
    public class UpdatePackageViewModel {
        [Required]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "dist-tags")]
        public IDictionary<string, string> DistTags { get; set; }
        [Required]
        public IDictionary<string, Manifest> Versions { get; set; }
        [JsonProperty(PropertyName = "_attachments")]
        public IDictionary<string, AttachmentViewModel> Attachments { get; set; }
    }
}
