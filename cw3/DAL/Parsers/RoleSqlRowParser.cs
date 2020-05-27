using System.Data.SqlClient;
using cw3.Models;

namespace cw3.DAL.Parsers
{
    public class RoleSqlRowParser : SqlRowParser<Role>
    {
        public Role Parse(SqlDataReader reader, int currentIndex) =>
            new Role {Name = reader["name"].ToString()};
    }
}