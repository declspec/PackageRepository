using System;
using System.Collections.Generic;
using System.Text;

namespace GetPkg.Npm {
    public class Token {
        public string Id { get; }
        public long? UserId { get; }

        internal Token(string id)
            : this(id, null) { }

        internal Token(string id, long? userId) {
            Id = id;
            UserId = userId;
        }
    }
}
