using cw3.DTOs.Requests;
using cw3.Models;

namespace cw3.Mappers
{
    public class EnrollStudentToStudentMapper : IMapper<EnrollStudentRequest, Student>
    {
        public Student Map(EnrollStudentRequest data) => new Student
        {
            IndexNumber = data.IndexNumber,
            FirstName = data.FirstName,
            LastName = data.LastName,
            BirthDate = data.BirthDate
        };
    }
}