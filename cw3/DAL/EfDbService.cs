using System;
using System.Collections.Generic;
using System.Linq;
using cw3.Models;

namespace cw3.DAL
{
    public class EfDbService : IDbService
    {
        private readonly StudiesDbContext _dbContext;

        public EfDbService(StudiesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Student GetStudentByIndexNumber(string indexNumber)
        {
            var student = _dbContext.Student
                .Where(s => s.IndexNumber == indexNumber)
                .SelectStudentOnlyFields()
                .SingleOrDefault();
            if(student == default) throw new KeyNotFoundException();
            return student;
        }

        public IEnumerable<Student> GetStudents() =>
            _dbContext.Student
                .SelectStudentOnlyFields()
                .ToList();

        public Student AddStudent(Student student)
        {
            _dbContext.Student.Add(student);
            _dbContext.SaveChanges();
            return student;
        }

        public Student UpdateStudent(Student student)
        {
            var dbStudent = _dbContext.Student.SingleOrDefault(s => s.IndexNumber == student.IndexNumber);
            if (dbStudent == default) throw new KeyNotFoundException();

            dbStudent.FirstName = student.FirstName;
            dbStudent.LastName = student.LastName;
            dbStudent.BirthDate = student.BirthDate;

            _dbContext.SaveChanges();

            return dbStudent;
        }

        public Student RemoveStudent(string indexNumber)
        {
            var dbStudent = _dbContext.Student.SingleOrDefault(s => s.IndexNumber == indexNumber);
            if (dbStudent == default) throw new KeyNotFoundException();

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
            throw new NotImplementedException();
        }

        public void AddStudentRefreshToken(Student student, string refreshToken, DateTime validity)
        {
            throw new NotImplementedException();
        }

        public bool IsRefreshTokenPresent(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Student GetStudentByRefreshToken(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public void ReplaceRefreshToken(string oldToken, string newToken, DateTime validity)
        {
            throw new NotImplementedException();
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
    }

    public static class StudentExtensions
    {
        public static IQueryable<Student> SelectStudentOnlyFields(this IQueryable<Student> students) =>
            students.Select(s => new Student
            {
                IndexNumber = s.IndexNumber, FirstName = s.FirstName, LastName = s.LastName, BirthDate = s.BirthDate
            });
    }
}