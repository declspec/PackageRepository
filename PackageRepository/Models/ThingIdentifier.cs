using PackageRepository.Enums;

namespace PackageRepository.Models {
    public class ThingIdentifier {
        public long OrganisationId { get; }
        public ThingType Type { get; }
        public string Name { get; }

        public ThingIdentifier(long organistionId, ThingType type, string name) {
            OrganisationId = organistionId;
            Type = type;
            Name = name;
        }

        public override int GetHashCode() {
            var hash = 17;

            hash = hash * 23 + OrganisationId.GetHashCode();
            hash = hash * 23 + Type.GetHashCode();
            hash = hash * 23 + Name.GetHashCode();

            return hash;
        }

        public override bool Equals(object obj) {
            return obj is ThingIdentifier id
                && id.OrganisationId == OrganisationId
                && id.Type == Type
                && id.Name == Name;
        }

        public static bool operator ==(ThingIdentifier id1, ThingIdentifier id2) {
            return ReferenceEquals(id1, null)
                ? ReferenceEquals(id2, null)
                : id1.Equals(id2);
        }

        public static bool operator !=(ThingIdentifier id1, ThingIdentifier id2) {
            return !(id1 == id2);
        }
    }
}
