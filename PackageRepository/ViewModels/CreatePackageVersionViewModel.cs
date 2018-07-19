﻿using Newtonsoft.Json;
using PackageRepository.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PackageRepository.ViewModels {
    public class CreatePackageVersionViewModel {
        [Required]
        public string Name { get; set; }
        [Required]
        [JsonProperty(PropertyName = "dist-tags")]
        public IDictionary<string, string> DistTags { get; set; }
        [Required]
        public IDictionary<string, Manifest> Versions { get; set; }
        [Required]
        [JsonProperty(PropertyName = "_attachments")]
        public IDictionary<string, AttachmentViewModel> Attachments { get; set; }
    }
}
