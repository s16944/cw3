using cw3.DTOs.Response;
using cw3.Models;

namespace cw3.Mappers
{
    public class EnrollmentToResponseMapper : IMapper<Enrollment, EnrollmentResponse>
    {
        public EnrollmentResponse Map(Enrollment data) => new EnrollmentResponse
        {
            Semester = data.Semester,
            StartDate = data.StartDate.Date,
            Studies = data.IdStudyNavigation.Name
        };
    }
}