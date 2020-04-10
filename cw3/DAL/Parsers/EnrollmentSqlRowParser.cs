using System.Data.SqlClient;
using cw3.Models;

namespace cw3.DAL.Parsers
{
    public class EnrollmentSqlRowParser : SqlRowParser<Enrollment>
    {
        public Enrollment Parse(SqlDataReader reader, int currentIndex) =>
            new Enrollment
            {
                Semester = int.Parse(reader["Semester"].ToString()),
                Studies = reader["Name"].ToString()
            };
    }
}