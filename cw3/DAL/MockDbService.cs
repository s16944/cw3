using System;
using System.Collections.Generic;
using cw3.Models;

namespace cw3.DAL
{
    public class MockDbService : IDbService
    {
        private static readonly IEnumerable<Student> Students;
        private static readonly IEnumerable<Enrollment> Enrollments;

        static MockDbService()
        {
            Students = new List<Student>
            {
                new Student {FirstName = "Jan", LastName = "Kowalski"},
                new Student {FirstName = "Anna", LastName = "Malewski"},
                new Student {FirstName = "Andrzej", LastName = "Andrzejewicz"}
            };
            Enrollments = new List<Enrollment>
            {
                new Enrollment {Semester = 1, IdStudyNavigation = new Studies {Name = "Studies1"}},
                new Enrollment {Semester = 2, IdStudyNavigation = new Studies {Name = "Studies2"}},
                new Enrollment {Semester = 3, IdStudyNavigation = new Studies {Name = "Studies3"}}
            };
        }

        public Student GetStudentByIndexNumber(string indexNumber)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Student> GetStudents() => Students;
        public Student AddStudent(Student student)
        {
            throw new NotImplementedException();
        }

        public Student UpdateStudent(Student student)
        {
            throw new NotImplementedException();
        }

        public Student RemoveStudent(string indexNumber)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Enrollment> GetStudentEnrollments(string indexNumber) => Enrollments;
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
}