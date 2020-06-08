using System;
using System.Collections.Generic;
using cw3.DAL;
using cw3.Models;
using Microsoft.AspNetCore.Mvc;

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult GetStudent()
        {
            return Ok(_dbService.GetStudents());
        }

        [HttpGet("{index}/enrollments")]
        public IActionResult GetEnrollment(string index)
        {
            return Ok(_dbService.GetStudentEnrollments(index));
        }

        [HttpGet("{index}")]
        public IActionResult GetStudent(string index)
        {
            try
            {
                var student = _dbService.GetStudentByIndexNumber(index);
                return Ok(student);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut]
        public IActionResult CreateStudent(Student student)
        {
            var randomNumber = new Random().Next(1, 20000);
            student.IndexNumber = $"s{randomNumber}";
            _dbService.AddStudent(student);
            return Ok(student);
        }

        [HttpPatch]
        public IActionResult UpdateStudent(Student student)
        {
            try
            {
                _dbService.UpdateStudent(student);
                return Ok("Updated");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{index}")]
        public IActionResult DeleteStudent(string index)
        {
            try
            {
                _dbService.RemoveStudent(index);
                return Ok("Deleted");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}