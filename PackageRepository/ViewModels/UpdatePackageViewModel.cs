﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PackageRepository.Models;
using PackageRepository.Utils;

namespace PackageRepository.ViewModels {
    public class UpdatePackageViewModel : PackageViewModel {
        [JsonProperty(PropertyName = "_attachments")]
        public IDictionary<string, AttachmentViewModel> Attachments { get; set; }

        protected override IList<ValidationResult> InternalValidate(ValidationContext validationContext) {
            var results = base.InternalValidate(validationContext);

            if (Attachments != null) {
                if (!ValidateAttachments())
                    results.Add(new ValidationResult("Invalid attachments hash encountered"));

                if (DistTags == null || DistTags.Count == 0)
                    results.Add(new ValidationResult("Invalid dist-tags hash encountered"));
            }

            return results;
        }

        private bool ValidateAttachments() {
            return Attachments.Count == Versions.Count
                && Attachments.All(kvp => Versions.Any(v => PackageUtils.GetTarballName(Name, v.Key) == kvp.Key) && IsValidBase64String(kvp.Value.Data));
        }

        private static bool IsValidBase64String(string str) {
            if (string.IsNullOrEmpty(str) || str.Length % 4 != 0)
                return false;

            var pos = str.Length - 1;
            if (str[pos] == '=')
                pos -= (str[pos - 1] == '=' ? 2 : 1);

            for (; pos >= 0; --pos) {
                if (!IsValidBase64Char(str[pos]))
                    return false;
            }

            return true;
        }

        private static bool IsValidBase64Char(char ch) {
            return (ch >= 'a' && ch <= 'z')
                || (ch >= 'A' && ch <= 'Z')
                || (ch >= '0' && ch <= '9')
                || (ch == '+' || ch == '/');
        }
    }
}
