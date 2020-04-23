using System;
using System.Collections.Generic;
using cw3.Models;

namespace cw3.DAL
{
    public interface IDbService
    {
        Student GetStudentByIndexNumber(string indexNumber);
        IEnumerable<Student> GetStudents();
        IEnumerable<Enrollment> GetStudentEnrollments(string indexNumber);
        Studies GetStudiesByName(string name);
        Enrollment GetEnrollmentByStudiesIdAndSemester(int studiesId, int semester);
        Enrollment AddEnrollment(Enrollment enrollment);
        void EnrollStudent(Student student, Enrollment enrollment);
        Enrollment PromoteStudents(string studiesName, int semester);
    }
    
    public class NoSuchStudiesException : Exception {}
    
    public class NoSuchEnrollmentException : Exception {}
    
    public interface ITransactionalDbService : IDbService
    {
        T InTransaction<T>(Func<IDbService, Tuple<bool, T>> operations, Func<T> onError);
    }
}