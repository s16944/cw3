using cw3.DAL;
using cw3.DTOs.Requests;
using cw3.Mappers;
using cw3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/enrollments/promotions")]
    public class PromotionsController : ControllerBase
    {
        private readonly ITransactionalDbService _dbService;
        private readonly IMapper<EnrollStudentRequest, Student> _enrollStudentToStudentMapper;

        public PromotionsController(ITransactionalDbService dbService,
            IMapper<EnrollStudentRequest, Student> enrollStudentToStudentMapper)
        {
            _dbService = dbService;
            _enrollStudentToStudentMapper = enrollStudentToStudentMapper;
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {
            try
            {
                var enrollment = _dbService.PromoteStudents(request.Studies, request.Semester);
                return Created("api/enrollments", enrollment);
            }
            catch (NoSuchStudiesException)
            {
                return NotFound("No such studies");
            }
            catch (NoSuchEnrollmentException)
            {
                return NotFound("No such enrollment");
            }
        }
    }
}