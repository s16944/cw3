using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using cw3.DAL;
using cw3.DTOs.Requests;
using cw3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private IConfiguration Configuration { get; set; }
        private readonly ITransactionalDbService _dbService;

        public AuthController(ITransactionalDbService dbService, IConfiguration configuration)
        {
            _dbService = dbService;
            Configuration = configuration;
        }

        [HttpPost]
        public IActionResult Login(LoginRequest request) => _dbService.InTransaction(transaction =>
        {
            var student = transaction.GetStudentByIndexNumber(request.Login);
            if (request.Password != student.Password)
                return Tuple.Create<bool, IActionResult>(false, Unauthorized("Invalid credentials"));

            var token = GetStudentToken(student, transaction);

            var refreshToken = Guid.NewGuid();
            transaction.AddStudentRefreshToken(student, refreshToken.ToString(), CreateRefreshTokenValidity());

            return Tuple.Create(true, CreateResult(token, refreshToken));
        }, () => StatusCode(500));

        private JwtSecurityToken GetStudentToken(Student student, IDbService dbService)
        {
            var roles = dbService.GetStudentRoles(student);
            var claims = CreateClaims(student, roles);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials
            );
        }

        private static DateTime CreateRefreshTokenValidity() => DateTime.Now.AddDays(7);

        private static IEnumerable<Claim> CreateClaims(Student student, IEnumerable<Role> roles)
        {
            var standardClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, student.IndexNumber),
                new Claim(ClaimTypes.Name, $"{student.FirstName} {student.LastName}")
            };

            return roles
                .Select(role => new Claim(ClaimTypes.Role, role.Name))
                .Concat(standardClaims)
                .ToArray();
        }

        private IActionResult CreateResult(SecurityToken token, Guid refreshToken) =>
            Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });

        [HttpGet("refresh/{refreshToken}")]
        public IActionResult RefreshToken(string refreshToken) => _dbService.InTransaction(transaction =>
        {
            if (!transaction.IsRefreshTokenPresent(refreshToken))
                return Tuple.Create<bool, IActionResult>(false, Unauthorized("Unknown refresh token"));

            var student = transaction.GetStudentByRefreshToken(refreshToken);
            var token = GetStudentToken(student, transaction);

            var newRefreshToken = Guid.NewGuid();
            transaction.ReplaceRefreshToken(refreshToken, newRefreshToken.ToString(), CreateRefreshTokenValidity());

            return Tuple.Create(true, CreateResult(token, newRefreshToken));
        }, () => StatusCode(500));
    }
}