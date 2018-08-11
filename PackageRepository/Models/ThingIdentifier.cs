using PackageRepository.Enums;

namespace PackageRepository.Models {
    public class ThingIdentifier {
        public string Organisation { get; }
        public ThingType Type { get; }
        public string Name { get; }

        public ThingIdentifier(string organistion, ThingType type, string name) {
            Organisation = organistion;
            Type = type;
            Name = name;
        }

        public override int GetHashCode() {
            var hash = 17;

            hash = hash * 23 + Organisation.GetHashCode();
            hash = hash * 23 + Type.GetHashCode();
            hash = hash * 23 + Name.GetHashCode();

            return hash;
        }

        public override bool Equals(object obj) {
            return obj is ThingIdentifier id
                && id.Organisation == Organisation
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
