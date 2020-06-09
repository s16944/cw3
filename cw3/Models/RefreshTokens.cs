using System;
using System.Collections.Generic;

namespace cw3.Models
{
    public partial class RefreshTokens
    {
        public int Id { get; set; }
        public string IndexNumber { get; set; }
        public string Token { get; set; }
        public DateTime? Validity { get; set; }

        public virtual Student IndexNumberNavigation { get; set; }
    }
}
