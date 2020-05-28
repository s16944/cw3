using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using cw3.DAL.Parsers;
using cw3.Models;

namespace cw3.DAL
{
    public class MsSqlDbService : ITransactionalDbService
    {
        private const string ConnectionString = "Data Source=db-mssql;Initial Catalog=s16944;Integrated Security=True";

        private readonly SqlRowParser<Student> _studentParser;
        private readonly SqlRowParser<Enrollment> _enrollmentParser;
        private readonly SqlRowParser<Studies> _studiesParser;
        private readonly SqlRowParser<Role> _roleParser;

        public MsSqlDbService(SqlRowParser<Student> studentParser,
            SqlRowParser<Enrollment> enrollmentParser,
            SqlRowParser<Studies> studiesParser,
            SqlRowParser<Role> roleParser)
        {
            _studentParser = studentParser;
            _enrollmentParser = enrollmentParser;
            _studiesParser = studiesParser;
            _roleParser = roleParser;
        }

        public T InTransaction<T>(Func<IDbService, Tuple<bool, T>> operations, Func<T> onError) =>
            ExecuteCommand((connection, command) =>
            {
                var transaction = connection.BeginTransaction();
                try
                {
                    command.Transaction = transaction;
                    var transactionService = new MsSqlDbTransactionService(this, command);
                    var (isSuccessful, result) = operations.Invoke(transactionService);
                    if (isSuccessful) transaction.Commit();
                    else transaction.Rollback();
                    return result;
                }
                catch (SqlException exception)
                {
                    Console.WriteLine(exception);
                    transaction.Rollback();
                    return onError.Invoke();
                }
            });

        private T ExecuteCommand<T>(Func<SqlConnection, SqlCommand, T> useCommand)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();
                return useCommand.Invoke(connection, command);
            }
        }

        public Student GetStudentByIndexNumber(string indexNumber) =>
            ExecuteCommand(command => GetStudentByIndexNumber(command, indexNumber));

        private Student GetStudentByIndexNumber(SqlCommand command, string indexNumber)
        {
            const string sqlQuery =
                "SELECT IndexNumber, FirstName, LastName, BirthDate, Password " +
                "FROM Student " +
                "WHERE IndexNumber = @indexNumber";

            command.Parameters.Clear();
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("indexNumber", indexNumber);

            return GetFirst(command, _studentParser);
        }

        private T GetFirst<T>(SqlCommand command, SqlRowParser<T> parser)
        {
            using (var reader = command.ExecuteReader())
            {
                return reader.Read() ? parser.Parse(reader, 0) : default(T);
            }
        }

        private T ExecuteCommand<T>(Func<SqlCommand, T> useCommand) =>
            ExecuteCommand((connection, command) => useCommand.Invoke(command));

        public IEnumerable<Student> GetStudents() => ExecuteCommand(GetStudents);

        private IEnumerable<Student> GetStudents(SqlCommand command)
        {
            const string sqlQuery =
                "SELECT IndexNumber, FirstName, LastName, BirthDate, Name, Semester " +
                "FROM Student " +
                "INNER JOIN Enrollment E on Student.IdEnrollment = E.IdEnrollment " +
                "INNER JOIN Studies S on E.IdStudy = S.IdStudy";

            command.CommandText = sqlQuery;

            return GetAll(command, _studentParser);
        }

        private IEnumerable<T> GetAll<T>(SqlCommand command, SqlRowParser<T> parser)
        {
            using (var reader = command.ExecuteReader())
            {
                var result = new List<T>();
                while (reader.Read())
                {
                    var item = parser.Parse(reader, result.Count);
                    result.Add(item);
                }

                return result;
            }
        }

        public IEnumerable<Enrollment> GetStudentEnrollments(string indexNumber) =>
            ExecuteCommand(command => GetStudentEnrollments(command, indexNumber));

        private IEnumerable<Enrollment> GetStudentEnrollments(SqlCommand command, string indexNumber)
        {
            const string sqlQuery =
                "SELECT Student.IdEnrollment, Semester, Name, StartDate, E.IdStudy " +
                "FROM Student " +
                "INNER JOIN Enrollment E on Student.IdEnrollment = E.IdEnrollment " +
                "INNER JOIN Studies S on E.IdStudy = S.IdStudy " +
                "WHERE IndexNumber = @indexNumber";

            command.Parameters.Clear();
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("indexNumber", indexNumber);

            return GetAll(command, _enrollmentParser);
        }

        public IEnumerable<Role> GetStudentRoles(Student student) =>
            ExecuteCommand(command => GetStudentRoles(command, student));

        private IEnumerable<Role> GetStudentRoles(SqlCommand command, Student student)
        {
            const string sqlQuery =
                "SELECT name " +
                "From StudentRoles " +
                "INNER JOIN ROLES R on StudentRoles.roleId = R.id " +
                "WHERE indexNumber = @indexNumber";

            command.Parameters.Clear();
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("indexNumber", student.IndexNumber);

            return GetAll(command, _roleParser);
        }

        public void AddStudentRefreshToken(Student student, string refreshToken, DateTime validity) =>
            ExecuteCommand(command => AddStudentRefreshToken(command, student, refreshToken, validity));

        private object AddStudentRefreshToken(
            SqlCommand command,
            Student student,
            string refreshToken,
            DateTime validity)
        {
            const string sqlQuery =
                "INSERT INTO RefreshTokens(id, indexNumber, token, validity) " +
                "SELECT ISNULL(Max(id) + 1, 1), @indexNumber, @token, @validity " +
                "FROM RefreshTokens";

            command.Parameters.Clear();
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("indexNumber", student.IndexNumber);
            command.Parameters.AddWithValue("token", refreshToken);
            command.Parameters.AddWithValue("validity", validity);

            command.ExecuteNonQuery();

            return default;
        }

        public bool IsRefreshTokenPresent(string refreshToken) =>
            ExecuteCommand(command => IsRefreshTokensPresent(command, refreshToken));

        private bool IsRefreshTokensPresent(SqlCommand command, string refreshToken)
        {
            const string sqlQuery =
                "SELECT 1 " +
                "FROM RefreshTokens " +
                "WHERE token = @refreshToken AND validity > GETDATE()";

            command.Parameters.Clear();
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("refreshToken", refreshToken);

            return command.ExecuteScalar() != null;
        }

        public Student GetStudentByRefreshToken(string refreshToken) =>
            ExecuteCommand(command => GetStudentByRefreshToken(command, refreshToken));

        private Student GetStudentByRefreshToken(SqlCommand command, string refreshToken)
        {
            const string sqlQuery =
                "SELECT Student.IndexNumber, FirstName, LastName, IdEnrollment, BirthDate " +
                "FROM Student " +
                "INNER JOIN RefreshTokens R ON Student.IndexNumber = R.indexNumber " +
                "WHERE token = @refreshToken";

            command.Parameters.Clear();
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("refreshToken", refreshToken);

            return GetFirst(command, _studentParser);
        }

        public void ReplaceRefreshToken(string oldToken, string newToken, DateTime validity) =>
            ExecuteCommand(command => ReplaceRefreshToken(command, oldToken, newToken, validity));
        private object ReplaceRefreshToken(SqlCommand command, string oldToken, string newToken, DateTime validity)
        {
            const string sqlQuery =
                "UPDATE RefreshTokens " +
                "SET token = @newToken, validity = @validity " +
                "WHERE token = @oldToken";

            command.Parameters.Clear();
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("oldToken", oldToken);
            command.Parameters.AddWithValue("newToken", newToken);
            command.Parameters.AddWithValue("validity", validity);

            command.ExecuteNonQuery();
            
            return default;
        }

        public Studies GetStudiesByName(string name) => ExecuteCommand(command => GetStudiesByName(command, name));

        private Studies GetStudiesByName(SqlCommand command, string name)
        {
            const string sqlQuery =
                "SELECT IdStudy, Name " +
                "FROM Studies " +
                "WHERE Name = @name";

            command.Parameters.Clear();
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("name", name);

            return GetFirst(command, _studiesParser);
        }

        public Enrollment GetEnrollmentByStudiesIdAndSemester(int studiesId, int semester) => ExecuteCommand(command =>
            GetEnrollmentByStudiesIdAndSemester(command, studiesId, semester));

        private Enrollment GetEnrollmentByStudiesIdAndSemester(SqlCommand command, int studiesId, int semester)
        {
            const string sqlQuery =
                "SELECT IdEnrollment, Semester, StartDate, S.IdStudy, Name " +
                "FROM Studies S " +
                "INNER JOIN Enrollment E on S.IdStudy = E.IdStudy " +
                "WHERE S.IdStudy = @studiesId AND Semester = @semester";

            command.Parameters.Clear();
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("studiesId", studiesId);
            command.Parameters.AddWithValue("semester", semester);

            return GetFirst(command, _enrollmentParser);
        }

        public Enrollment AddEnrollment(Enrollment enrollment) =>
            ExecuteCommand(command => AddEnrollment(command, enrollment));

        private Enrollment AddEnrollment(SqlCommand command, Enrollment enrollment)
        {
            const string insertCommand =
                "INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate) " +
                "output INSERTED.IdEnrollment " +
                "SELECT Max(IdEnrollment) + 1, @semester, @idStudy, @startDate " +
                "FROM Enrollment";

            command.Parameters.Clear();
            command.CommandText = insertCommand;
            command.Parameters.AddWithValue("semester", enrollment.Semester);
            command.Parameters.AddWithValue("idStudy", enrollment.Studies.IdStudies);
            command.Parameters.AddWithValue("startDate", enrollment.StartDate.Date);

            var modified = (int) command.ExecuteScalar();
            enrollment.IdEnrollment = modified;
            return enrollment;
        }

        public void EnrollStudent(Student student, Enrollment enrollment) =>
            ExecuteCommand(command => EnrollStudent(command, student, enrollment));

        private object EnrollStudent(SqlCommand command, Student student, Enrollment enrollment)
        {
            const string insertCommand =
                "INSERT INTO Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) " +
                "VALUES (@indexNumber, @firstName, @lastName, @birthDate, @idEnrollment)";

            command.Parameters.Clear();
            command.CommandText = insertCommand;
            command.Parameters.AddWithValue("indexNumber", student.IndexNumber);
            command.Parameters.AddWithValue("firstName", student.FirstName);
            command.Parameters.AddWithValue("lastName", student.LastName);
            command.Parameters.AddWithValue("birthDate", student.BirthDate);
            command.Parameters.AddWithValue("idEnrollment", enrollment.IdEnrollment);
            command.ExecuteNonQuery();

            return default(object);
        }

        public Enrollment PromoteStudents(string studiesName, int semester) => ExecuteCommand(command =>
        {
            command.CommandText = "PromoteStudents";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Studies", studiesName);
            command.Parameters.AddWithValue("@Semester", semester);

            try
            {
                return GetFirst(command, _enrollmentParser);
            }
            catch (SqlException exception)
            {
                switch (exception.Number)
                {
                    case 50_001:
                        throw new NoSuchStudiesException();
                    case 50_002:
                        throw new NoSuchEnrollmentException();
                    default:
                        throw;
                }
            }
        });

        private class MsSqlDbTransactionService : IDbService
        {
            private readonly MsSqlDbService _service;
            private readonly SqlCommand _sqlCommand;

            public MsSqlDbTransactionService(MsSqlDbService service, SqlCommand sqlCommand)
            {
                _service = service;
                _sqlCommand = sqlCommand;
            }

            public Student GetStudentByIndexNumber(string indexNumber) =>
                _service.GetStudentByIndexNumber(_sqlCommand, indexNumber);

            public IEnumerable<Student> GetStudents() =>
                _service.GetStudents(_sqlCommand);

            public IEnumerable<Enrollment> GetStudentEnrollments(string indexNumber) =>
                _service.GetStudentEnrollments(_sqlCommand, indexNumber);

            public IEnumerable<Role> GetStudentRoles(Student student) =>
                _service.GetStudentRoles(_sqlCommand, student);

            public void AddStudentRefreshToken(Student student, string refreshToken, DateTime validity) =>
                _service.AddStudentRefreshToken(_sqlCommand, student, refreshToken, validity);

            public bool IsRefreshTokenPresent(string refreshToken) =>
                _service.IsRefreshTokensPresent(_sqlCommand, refreshToken);

            public Student GetStudentByRefreshToken(string refreshToken) =>
                _service.GetStudentByRefreshToken(_sqlCommand, refreshToken);

            public void ReplaceRefreshToken(string oldToken, string newToken, DateTime validity) =>
                _service.ReplaceRefreshToken(_sqlCommand, oldToken, newToken, validity);

            public Studies GetStudiesByName(string name) =>
                _service.GetStudiesByName(_sqlCommand, name);

            public Enrollment GetEnrollmentByStudiesIdAndSemester(int studiesId, int semester) =>
                _service.GetEnrollmentByStudiesIdAndSemester(_sqlCommand, studiesId, semester);

            public Enrollment AddEnrollment(Enrollment enrollment) =>
                _service.AddEnrollment(_sqlCommand, enrollment);

            public void EnrollStudent(Student student, Enrollment enrollment) =>
                _service.EnrollStudent(_sqlCommand, student, enrollment);

            public Enrollment PromoteStudents(string studiesName, int semester) =>
                throw new NotSupportedException();
        }
    }
}