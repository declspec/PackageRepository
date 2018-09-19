﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace GetPkg.Npm.Http.ViewModels {
    public class AttachmentViewModel {
        [JsonProperty(PropertyName = "content_type")]
        public string ContentType { get; set; }
        [Required]
        public string Data { get; set; }
    }
}
