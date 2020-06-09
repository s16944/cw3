using System;
using System.Collections.Generic;
using System.Linq;
using cw3.Models;
using System.Data.SqlClient;
using Castle.Core.Internal;
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
            var student = _dbContext.Student
                .Where(s => s.IndexNumber == indexNumber)
                .Include(s => s.IdEnrollmentNavigation)
                .First();

            return new[] {student.IdEnrollmentNavigation};
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
            var id = _dbContext.Enrollment.Max(e => e.IdEnrollment) + 1;
            enrollment.IdEnrollment = id;
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
            _dbContext.Database.BeginTransaction();

            var studies = _dbContext.Studies
                .Include(s => s.Enrollment)
                .SingleOrDefault(s => s.Name == studiesName);
            if (studies == default) throw new NoSuchStudiesException();

            var currentEnrollment = studies.Enrollment.FirstOrDefault(e => e.Semester == semester);
            if (currentEnrollment == default) throw new NoSuchEnrollmentException();

            var newEnrollment = studies.Enrollment.FirstOrDefault(e => e.Semester == semester + 1);
            if (newEnrollment == default)
            {
                var id = _dbContext.Enrollment.Max(e => e.IdEnrollment) + 1;
                newEnrollment = new Enrollment {IdEnrollment = id, Semester = semester + 1, StartDate = DateTime.Now};
                studies.Enrollment.Add(newEnrollment);
                _dbContext.SaveChanges();
            }

            _dbContext.Entry(currentEnrollment).Collection(e => e.Student).Load();
            foreach (var student in currentEnrollment.Student.ToList())
            {
                student.IdEnrollmentNavigation = newEnrollment;
            }

            _dbContext.SaveChanges();
            _dbContext.Database.CommitTransaction();

            return newEnrollment;
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