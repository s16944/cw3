using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using cw3.DAL.Parsers;
using cw3.Models;

namespace cw3.DAL
{
    public class MsSqlDbService : IDbService
    {
        private const string ConnectionString = "Data Source=db-mssql;Initial Catalog=s16944;Integrated Security=True";

        private readonly SqlRowParser<Student> _studentParser;
        private readonly SqlRowParser<Enrollment> _enrollmentParser;

        public MsSqlDbService(SqlRowParser<Student> studentParser, SqlRowParser<Enrollment> enrollmentParser)
        {
            _studentParser = studentParser;
            _enrollmentParser = enrollmentParser;
        }

        public IEnumerable<Student> GetStudents()
        {
            const string sqlQuery =
                "SELECT IndexNumber, FirstName, LastName, BirthDate, Name, Semester " +
                "FROM Student " +
                "INNER JOIN Enrollment E on Student.IdEnrollment = E.IdEnrollment " +
                "INNER JOIN Studies S on E.IdStudy = S.IdStudy";

            return GetAll(sqlQuery, _studentParser);
        }

        private IEnumerable<T> GetAll<T>(string sqlQuery, SqlRowParser<T> parser) => 
            GetAll(command => command.CommandText = sqlQuery, parser);

        private IEnumerable<T> GetAll<T>(Action<SqlCommand> sqlCommandInit, SqlRowParser<T> parser)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                sqlCommandInit.Invoke(command);

                connection.Open();

                var reader = command.ExecuteReader();
                var result = new List<T>();
                while (reader.Read())
                {
                    var item = parser.Parse(reader, result.Count);
                    result.Add(item);
                }

                return result;
            }
        }

        public IEnumerable<Enrollment> GetStudentEnrollments(string indexNumber)
        {
            var sqlQuery =
                "SELECT Semester, Name " +
                "FROM Student " +
                "INNER JOIN Enrollment E on Student.IdEnrollment = E.IdEnrollment " +
                "INNER JOIN Studies S on E.IdStudy = S.IdStudy " +
                "WHERE IndexNumber = @indexNumber";

            return GetAll((command) =>
            {
                command.CommandText = sqlQuery;
                command.Parameters.AddWithValue("indexNumber", indexNumber);
            }, _enrollmentParser);
        }
    }
}