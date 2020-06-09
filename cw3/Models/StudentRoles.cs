using System;
using System.Collections.Generic;

namespace cw3.Models
{
    public partial class StudentRoles
    {
        public int Id { get; set; }
        public int? RoleId { get; set; }
        public string IndexNumber { get; set; }

        public virtual Student IndexNumberNavigation { get; set; }
        public virtual Role Role { get; set; }
    }
}
