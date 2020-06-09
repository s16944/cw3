using System;
using System.Collections.Generic;
using System.Linq;
using cw3.Models;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace cw3.DAL
{
    public class EfDbService : ITransactionalDbService
    {
        private readonly StudiesDbContext _dbContext;

        public EfDbService(StudiesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Student GetStudentByIndexNumber(string indexNumber)
        {
            var student = _dbContext.Student
                .SingleOrDefault(s => s.IndexNumber == indexNumber);
            if (student == default) throw new KeyNotFoundException();
            return student;
        }

        public IEnumerable<Student> GetStudents() =>
            _dbContext.Student.ToList();

        public Student AddStudent(Student student)
        {
            AddStudentInternal(student);
            _dbContext.SaveChanges();
            return student;
        }

        private void AddStudentInternal(Student student)
        {
            _dbContext.Student.Add(student);
        }

        public Student UpdateStudent(Student student)
        {
            var dbStudent = UpdateStudentInternal(student);

            _dbContext.SaveChanges();

            return dbStudent;
        }

        private Student UpdateStudentInternal(Student student)
        {
            var dbStudent = _dbContext.Student.SingleOrDefault(s => s.IndexNumber == student.IndexNumber);
            if (dbStudent == default) throw new KeyNotFoundException();

            dbStudent.FirstName = student.FirstName;
            dbStudent.LastName = student.LastName;
            dbStudent.BirthDate = student.BirthDate;

            return dbStudent;
        }

        public Student RemoveStudent(string indexNumber)
        {
            var dbStudent = RemoveStudentInternal(indexNumber);
            _dbContext.SaveChanges();
            return dbStudent;
        }

        private Student RemoveStudentInternal(string indexNumber)
        {
            var dbStudent = _dbContext.Student.SingleOrDefault(s => s.IndexNumber == indexNumber);
            if (dbStudent == default) throw new KeyNotFoundException();

            _dbContext.Student.Remove(dbStudent);
            return dbStudent;
        }

        public IEnumerable<Enrollment> GetStudentEnrollments(string indexNumber)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Role> GetStudentRoles(Student student)
        {
            var dbStudent = _dbContext.Student
                .Where(s => s.IndexNumber == student.IndexNumber)
                .Include(s => s.StudentRoles)
                .ThenInclude(sr => sr.Role)
                .FirstOrDefault();
            if (dbStudent == default) throw new KeyNotFoundException();

            return dbStudent.StudentRoles.Select(sr => sr.Role)
                .ToList();
        }

        public void AddStudentRefreshToken(Student student, string refreshToken, DateTime validity)
        {
            AddStudentRefreshTokenInternal(student, refreshToken, validity);
            _dbContext.SaveChanges();
        }

        private void AddStudentRefreshTokenInternal(Student student, string refreshToken, DateTime validity)
        {
            var dbStudent = _dbContext.Student
                .Where(s => s.IndexNumber == student.IndexNumber)
                .Include(s => s.RefreshTokens)
                .FirstOrDefault();
            if (dbStudent == default) throw new KeyNotFoundException();

            dbStudent.RefreshTokens.Add(new RefreshTokens {Token = refreshToken, Validity = validity});
        }

        public bool IsRefreshTokenPresent(string refreshToken) =>
            _dbContext.RefreshTokens.Any(r => r.Token == refreshToken);

        public Student GetStudentByRefreshToken(string refreshToken)
        {
            var token = _dbContext.RefreshTokens
                .Where(r => r.Token == refreshToken)
                .Include(r => r.IndexNumberNavigation)
                .FirstOrDefault();
            if (token == default) throw new KeyNotFoundException();

            return token.IndexNumberNavigation;
        }

        public void ReplaceRefreshToken(string oldToken, string newToken, DateTime validity)
        {
            ReplaceRefreshTokenInternal(oldToken, newToken, validity);
            _dbContext.SaveChanges();
        }

        private void ReplaceRefreshTokenInternal(string oldToken, string newToken, DateTime validity)
        {
            var token = _dbContext.RefreshTokens
                .FirstOrDefault(r => r.Token == oldToken);
            if (token == default) throw new KeyNotFoundException();

            token.Token = newToken;
            token.Validity = validity;
        }

        public Studies GetStudiesByName(string name)
        {
            throw new NotImplementedException();
        }

        public Enrollment GetEnrollmentByStudiesIdAndSemester(int studiesId, int semester)
        {
            throw new NotImplementedException();
        }

        public Enrollment AddEnrollment(Enrollment enrollment)
        {
            throw new NotImplementedException();
        }

        public void EnrollStudent(Student student, Enrollment enrollment)
        {
            throw new NotImplementedException();
        }

        public Enrollment PromoteStudents(string studiesName, int semester)
        {
            throw new NotImplementedException();
        }

        public T InTransaction<T>(Func<IDbService, Tuple<bool, T>> operations, Func<T> onError)
        {
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    var (isSuccessful, result) = operations.Invoke(this);
                    if (isSuccessful) transaction.Commit();
                    else transaction.Rollback();
                    return result;
                }
            }
            catch (SqlException exception)
            {
                Console.WriteLine(exception);
                return onError.Invoke();
            }
        }

        private class TransactionDbService : IDbService
        {
            private readonly EfDbService _efDbService;

            public TransactionDbService(EfDbService efDbService)
            {
                _efDbService = efDbService;
            }

            public Student GetStudentByIndexNumber(string indexNumber) =>
                _efDbService.GetStudentByIndexNumber(indexNumber);

            public IEnumerable<Student> GetStudents() =>
                _efDbService.GetStudents();

            public Student AddStudent(Student student)
            {
                _efDbService.AddStudentInternal(student);
                return student;
            }

            public Student UpdateStudent(Student student) =>
                _efDbService.UpdateStudent(student);

            public Student RemoveStudent(string indexNumber) =>
                _efDbService.RemoveStudent(indexNumber);

            public IEnumerable<Enrollment> GetStudentEnrollments(string indexNumber)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<Role> GetStudentRoles(Student student) =>
                _efDbService.GetStudentRoles(student);

            public void AddStudentRefreshToken(Student student, string refreshToken, DateTime validity) =>
                _efDbService.AddStudentRefreshTokenInternal(student, refreshToken, validity);

            public bool IsRefreshTokenPresent(string refreshToken) =>
                _efDbService.IsRefreshTokenPresent(refreshToken);

            public Student GetStudentByRefreshToken(string refreshToken) =>
                _efDbService.GetStudentByRefreshToken(refreshToken);

            public void ReplaceRefreshToken(string oldToken, string newToken, DateTime validity) =>
                _efDbService.ReplaceRefreshTokenInternal(oldToken, newToken, validity);

            public Studies GetStudiesByName(string name)
            {
                throw new NotImplementedException();
            }

            public Enrollment GetEnrollmentByStudiesIdAndSemester(int studiesId, int semester)
            {
                throw new NotImplementedException();
            }

            public Enrollment AddEnrollment(Enrollment enrollment)
            {
                throw new NotImplementedException();
            }

            public void EnrollStudent(Student student, Enrollment enrollment)
            {
                throw new NotImplementedException();
            }

            public Enrollment PromoteStudents(string studiesName, int semester)
            {
                throw new NotImplementedException();
            }
        }
    }
}