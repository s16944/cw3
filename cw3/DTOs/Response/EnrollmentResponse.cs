using System;
using cw3.Models;

namespace cw3.DTOs.Response
{
    public class EnrollmentResponse
    {
        public int Semester { get; set; }
        public DateTime StartDate { get; set; }
        public string Studies { get; set; }
    }
}