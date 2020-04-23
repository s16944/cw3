using System;
using System.Collections.Generic;
using cw3.Models;

namespace cw3.DAL
{
    public class MockDbService : IDbService
    {
        private static IEnumerable<Student> _students;
        private static IEnumerable<Enrollment> _enrollments;

        static MockDbService()
        {
            _students = new List<Student>
            {
                new Student {IdStudent = 1, FirstName = "Jan", LastName = "Kowalski"},
                new Student {IdStudent = 2, FirstName = "Anna", LastName = "Malewski"},
                new Student {IdStudent = 3, FirstName = "Andrzej", LastName = "Andrzejewicz"}
            };
            _enrollments = new List<Enrollment>
            {
                new Enrollment {Semester = 1, Studies = new Studies {Name = "Studies1"}},
                new Enrollment {Semester = 2, Studies = new Studies {Name = "Studies2"}},
                new Enrollment {Semester = 3, Studies = new Studies {Name = "Studies3"}}
            };
        }

        public Student GetStudentByIndexNumber(string indexNumber)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Student> GetStudents() => _students;

        public IEnumerable<Enrollment> GetStudentEnrollments(string indexNumber) => _enrollments;
        
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
    }
}