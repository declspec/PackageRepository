using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace GetPkg.Npm.Http.ViewModels {
    public class PackageViewModel : IValidatableObject {
        [JsonProperty(PropertyName = "_id")]
        public string Id => Name;
        [JsonProperty(PropertyName = "_rev")]
        public string Revision { get; set; }

        public string Name { get; set; }
        [JsonProperty(PropertyName = "dist-tags")]
        public IDictionary<string, string> DistTags { get; set; }
        public IDictionary<string, Manifest> Versions { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            return InternalValidate(validationContext);
        }

        protected virtual IList<ValidationResult> InternalValidate(ValidationContext validationContext) {
            var results = new List<ValidationResult>();

            if (string.IsNullOrEmpty(Name))
                results.Add(new ValidationResult("Missing package name"));

            if (Versions == null || Versions.Count == 0)
                results.Add(new ValidationResult("Missing versions"));
            else {
                if (Versions.Any(kvp => kvp.Value.Version != kvp.Key || kvp.Value.Name != Name))
                    results.Add(new ValidationResult("Invalid version hash encountered"));
            }

            return results;
        }
    }
}
