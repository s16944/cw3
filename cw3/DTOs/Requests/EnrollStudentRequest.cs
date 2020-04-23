using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace cw3.DTOs.Requests
{
    public class EnrollStudentRequest
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression("^s\\d+$")]
        public string IndexNumber { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Studies { get; set; }
    }
}