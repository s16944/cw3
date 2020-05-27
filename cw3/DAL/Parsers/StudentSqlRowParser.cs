using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
                BirthDate = DateTime.Parse(reader["BirthDate"].ToString()),
                Password = HasPasswordColumn(reader) ? reader["Password"].ToString() : default
            };

        private static bool HasPasswordColumn(SqlDataReader reader) => reader.GetColumnSchema()
            .Any(e => e.ColumnName == "Password");
    }
}