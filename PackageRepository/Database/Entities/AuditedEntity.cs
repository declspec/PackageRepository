using System;
using System.Collections.Generic;
using System.Text;

namespace PackageRepository.Database.Entities {
    public class AuditedEntity {
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateModified { get; set; }
    }
}
