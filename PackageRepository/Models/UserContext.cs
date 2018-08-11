using System.Collections.Generic;

namespace PackageRepository.Models {
    public class UserContext {
        public long UserId { get; }
        public long OrganisationId { get; }
        public IList<long> Teams { get; }

        public UserContext(long userId, long organisationId, IList<long> teamIds) {
            UserId = userId;
            OrganisationId = organisationId;
            Teams = teamIds;
        }
    }
}
