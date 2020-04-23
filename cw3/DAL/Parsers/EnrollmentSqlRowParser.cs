using System;
using System.Data.SqlClient;
using cw3.Models;

namespace cw3.DAL.Parsers
{
    public class EnrollmentSqlRowParser : SqlRowParser<Enrollment>
    {
        private readonly SqlRowParser<Studies> _studiesSqlRowParser;

        public EnrollmentSqlRowParser(SqlRowParser<Studies> studiesSqlRowParser)
        {
            _studiesSqlRowParser = studiesSqlRowParser;
        }

        public Enrollment Parse(SqlDataReader reader, int currentIndex) =>
            new Enrollment
            {
                IdEnrollment = int.Parse(reader["IdEnrollment"].ToString()),
                Semester = int.Parse(reader["Semester"].ToString()),
                StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                Studies = _studiesSqlRowParser.Parse(reader, 0)
            };
    }
}