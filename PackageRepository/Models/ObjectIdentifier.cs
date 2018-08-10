using PackageRepository.Enums;

namespace PackageRepository.Models {
    public class ObjectIdentifier {
        public int OrganisationId { get; }
        public ObjectType Type { get; }
        public string Name { get; }

        public ObjectIdentifier(int organistionId, ObjectType type, string name) {
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
            return obj is ObjectIdentifier id && id.OrganisationId == OrganisationId && id.Type == Type && id.Name == Name;
        }

        public static bool operator ==(ObjectIdentifier id1, ObjectIdentifier id2) {
            return ReferenceEquals(id1, null)
                ? ReferenceEquals(id2, null)
                : id1.Equals(id2);
        }

        public static bool operator !=(ObjectIdentifier id1, ObjectIdentifier id2) {
            return !(id1 == id2);
        }
    }
}
