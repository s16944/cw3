using cw3.DTOs.Response;
using cw3.Models;

namespace cw3.Mappers
{
    public class StudentToResponseMapper : IMapper<Student, StudentResponse>
    {
        public StudentResponse Map(Student data) => new StudentResponse
        {
            IndexNumber = data.IndexNumber,
            FirstName = data.FirstName,
            LastName = data.LastName,
            BirthDate = data.BirthDate
        };

    }
}