using System;
using System.Data.SqlClient;
using cw3.Models;

namespace cw3.DAL.Parsers
{
    public class StudentSqlRowParser : SqlRowParser<Student>
    {
        public Student Parse(SqlDataReader reader, int currentIndex) =>
            new Student
            {
                IdStudent = currentIndex,
                IndexNumber = reader["IndexNumber"].ToString(),
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                BirthDate = DateTime.Parse(reader["BirthDate"].ToString())
            };
    }
}