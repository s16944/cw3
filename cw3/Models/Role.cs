using System;
using System.Collections.Generic;

namespace cw3.Models
{
    public partial class Role
    {
        public Role()
        {
            StudentRoles = new HashSet<StudentRoles>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<StudentRoles> StudentRoles { get; set; }
    }
}
