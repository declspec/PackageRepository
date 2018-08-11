using PackageRepository.Enums;

namespace PackageRepository.Models {
    public class Thing {
        public long Id { get; set; }
        public long OrganisationId { get; set; }
        public ThingType Type { get; set; }
        public string Name { get; set; }
    }
}
