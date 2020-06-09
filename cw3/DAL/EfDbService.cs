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

        public Student GetStudentByIndexNumber(string indexNumber) => _dbContext.Student
                .SingleOrDefault(s => s.IndexNumber == indexNumber);

        public IEnumerable<Student> GetStudents() => _dbContext.Student.ToList();

        public Student AddStudent(Student student)
        {
            _dbContext.Student.Add(student);
            _dbContext.SaveChanges();
            return student;
        }

        public Student UpdateStudent(Student student)
        {
            var dbStudent = _dbContext.Student.Single(s => s.IndexNumber == student.IndexNumber);

            dbStudent.FirstName = student.FirstName;
            dbStudent.LastName = student.LastName;
            dbStudent.BirthDate = student.BirthDate;

            _dbContext.SaveChanges();

            return dbStudent;
        }

        public Student RemoveStudent(string indexNumber)
        {
            var dbStudent = _dbContext.Student.Single(s => s.IndexNumber == indexNumber);
            _dbContext.Student.Remove(dbStudent);
            _dbContext.SaveChanges();
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
                .First();

            return dbStudent.StudentRoles.Select(sr => sr.Role)
                .ToList();
        }

        public void AddStudentRefreshToken(Student student, string refreshToken, DateTime validity)
        {
            var dbStudent = _dbContext.Student
                .Where(s => s.IndexNumber == student.IndexNumber)
                .Include(s => s.RefreshTokens)
                .First();

            dbStudent.RefreshTokens.Add(new RefreshTokens {Token = refreshToken, Validity = validity});
            _dbContext.SaveChanges();
        }

        public bool IsRefreshTokenPresent(string refreshToken) =>
            _dbContext.RefreshTokens.Any(r => r.Token == refreshToken);

        public Student GetStudentByRefreshToken(string refreshToken)
        {
            var token = _dbContext.RefreshTokens
                .Where(r => r.Token == refreshToken)
                .Include(r => r.IndexNumberNavigation)
                .First();
            return token.IndexNumberNavigation;
        }

        public void ReplaceRefreshToken(string oldToken, string newToken, DateTime validity)
        {
            var token = _dbContext.RefreshTokens
                .First(r => r.Token == oldToken);

            token.Token = newToken;
            token.Validity = validity;
            
            _dbContext.SaveChanges();
        }
        
        public Studies GetStudiesByName(string name) => _dbContext.Studies.SingleOrDefault(s => s.Name == name);

        public Enrollment GetEnrollmentByStudiesIdAndSemester(int studiesId, int semester) =>
            _dbContext.Enrollment
                .SingleOrDefault(e => e.IdStudy == studiesId && e.Semester == semester);

        public Enrollment AddEnrollment(Enrollment enrollment)
        {
            _dbContext.Enrollment.Add(enrollment);
            _dbContext.SaveChanges();
            return enrollment;
        }

        public void EnrollStudent(Student student, Enrollment enrollment)
        {
            student.IdEnrollment = enrollment.IdEnrollment;
            _dbContext.Student.Add(student);
            _dbContext.SaveChanges();
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
    }
}