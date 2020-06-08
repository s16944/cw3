using System;
using cw3.DAL;
using cw3.DTOs.Requests;
using cw3.Mappers;
using cw3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly ITransactionalDbService _dbService;
        private readonly IMapper<EnrollStudentRequest, Student> _enrollStudentToStudentMapper;

        public EnrollmentsController(ITransactionalDbService dbService,
            IMapper<EnrollStudentRequest, Student> enrollStudentToStudentMapper)
        {
            _dbService = dbService;
            _enrollStudentToStudentMapper = enrollStudentToStudentMapper;
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(EnrollStudentRequest request) =>
            _dbService.InTransaction(transactionService =>
            {
                var result = EnrollStudentTransaction(transactionService, request);
                return new Tuple<bool, IActionResult>(result is CreatedResult, result);
            }, () => StatusCode(500));

        private IActionResult EnrollStudentTransaction(IDbService transactionService, EnrollStudentRequest request)
        {
            var studies = transactionService.GetStudiesByName(request.Studies);
            if (studies == null) return BadRequest("No such studies");

            var enrollment =
                transactionService
                    .GetEnrollmentByStudiesIdAndSemester(studies.IdStudy, 1)
                ?? transactionService
                    .AddEnrollment(new Enrollment {Semester = 1, StartDate = DateTime.Now, IdStudyNavigation = studies});

            if (transactionService.GetStudentByIndexNumber(request.IndexNumber) != null)
                return Conflict("Student with such index already exists");

            var student = _enrollStudentToStudentMapper.Map(request);
            transactionService.EnrollStudent(student, enrollment);

            enrollment.StartDate = enrollment.StartDate.Date;
            return Created("api/enrollments", enrollment);
        }
    }
}