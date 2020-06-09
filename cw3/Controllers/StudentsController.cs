using System;
using System.Collections.Generic;
using System.Linq;
using cw3.DAL;
using cw3.DTOs.Response;
using cw3.Mappers;
using cw3.Models;
using Microsoft.AspNetCore.Mvc;

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        private readonly IMapper<Student, StudentResponse> _studentMapper;

        public StudentsController(IDbService dbService, IMapper<Student, StudentResponse> studentMapper)
        {
            _dbService = dbService;
            _studentMapper = studentMapper;
        }

        [HttpGet]
        public IActionResult GetStudent()
        {
            var students = _dbService.GetStudents();
            var response = students.Select(s => _studentMapper.Map(s));
            return Ok(response);
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
                var response = _studentMapper.Map(student);
                return Ok(response);
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