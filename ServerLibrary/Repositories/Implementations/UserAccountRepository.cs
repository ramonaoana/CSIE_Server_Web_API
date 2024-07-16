using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServerLibrary.Repositories.Implementations
{
    public class UserAccountRepository(IOptions<JwtSection> config, AppDbContext appDbContext) : IUserAccount
    {
        public async Task<GeneralResponse> CreateAsync(Register user)
        {
            if (user is null) return new GeneralResponse(false, "Model is empty");

            var checkUser = await FindUserByEmail(user.Email!);
            if (checkUser != null) return new GeneralResponse(false, "User registered already");

            //Save user
            var applicationUser = await AddToDatabase(new User()
            {
                UserEmail = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
            });

            var checkProfessorRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(_ => _.Name!.Equals(Constants.Professor));
            SystemRole response = new();
            if (checkProfessorRole is null)
            {
                response = await AddToDatabase(new SystemRole() { Name = Constants.Professor });
                await AddToDatabase(new UserSystemRole() { RoleId = response.RoleId, UserId = applicationUser.UserId });
            }
            else
            {
                await AddToDatabase(new UserSystemRole() { RoleId = checkProfessorRole.RoleId, UserId = applicationUser.UserId });
            }
            return new GeneralResponse(true, "Account created!");

            var checkStudentRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(_ => _.Name!.Equals(Constants.Student));
            SystemRole responseStudent = new();
            if (checkStudentRole is null)
            {
                responseStudent = await AddToDatabase(new SystemRole() { Name = Constants.Student });
                await AddToDatabase(new UserSystemRole() { RoleId = response.RoleId, UserId = applicationUser.UserId });
            }
            else
            {
                await AddToDatabase(new UserSystemRole() { RoleId = checkStudentRole.RoleId, UserId = applicationUser.UserId });
            }
            return new GeneralResponse(true, "Account created!");
        }

        private async Task<T> AddToDatabase<T>(T model)
        {
            var result = appDbContext.Add(model!);
            await appDbContext.SaveChangesAsync();
            return (T)result.Entity;
        }

        private async Task<User> FindUserByEmail(string email) =>
            await appDbContext.Users.FirstOrDefaultAsync(_ => _.UserEmail!.ToLower()!.Equals(email!.ToLower()));

        public async Task<LoginResponse> SignInAsync(Login user)
        {
            if (user is null) return new LoginResponse(false, "Model is empty");

            var applicationUser = await FindUserByEmail(user.Email!);
            if (applicationUser is null) return new LoginResponse(false, "User not found");

            //Verify password
            if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
                return new LoginResponse(false, "Email/Password not valid");

            var getUserRole = await FindUserRole(applicationUser.UserId);
            if (getUserRole is null) return new LoginResponse(false, "user role not found");

            var getRoleName = await FindRoleName(getUserRole.RoleId);
            if (getUserRole is null) return new LoginResponse(false, "user role not found");

            string jwtToken = GenerateToken(applicationUser, getRoleName!.Name!);
            string refreshToken = GenerateRefreshToken();

            var findUser = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.UserId == applicationUser.UserId);
            if (findUser is not null)
            {
                findUser!.Token = refreshToken;
                await appDbContext.SaveChangesAsync();
            }
            else
            {
                await AddToDatabase(new RefreshTokenInfo() { Token = refreshToken, UserId = applicationUser.UserId });
            }
            return new LoginResponse(true, "Login successfully", jwtToken, refreshToken, applicationUser.UserId);
        }



        private string GenerateToken(User user, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.UserEmail!),
                new Claim(ClaimTypes.Role, role!)
            };
            var token = new JwtSecurityToken(
                issuer: config.Value.Issuer,
                audience: config.Value.Audience,
                claims: userClaims,
                expires: DateTime.UtcNow.AddHours(5),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<UserSystemRole> FindUserRole(int userId) => await appDbContext.UserSystemRole.FirstOrDefaultAsync(_ => _.UserId == userId);
        private async Task<SystemRole> FindRoleName(int roleId) => await appDbContext.SystemRoles.FirstOrDefaultAsync(_ => _.RoleId == roleId);

        private async Task<Student> FindStudent(int userId) => await appDbContext.Students.FirstOrDefaultAsync(_ => _.UserId == userId);
        private async Task<Professor> FindProfessor(int userId) => await appDbContext.Professors.FirstOrDefaultAsync(_ => _.UserId == userId);
        private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));



        public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
        {
            if (token is null) return new LoginResponse(false, "Model is empty");

            var findToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.Token!.Equals(token.Token));
            if (findToken is null) return new LoginResponse(false, "Refresh token is required");

            //get user details
            var user = await appDbContext.Users.FirstOrDefaultAsync(_ => _.UserId == findToken.UserId);
            if (user is null) return new LoginResponse(false, "Refesh token could not be generated because user not found");

            var userRole = await FindUserRole(user.UserId);
            var roleName = await FindRoleName(userRole.RoleId);
            string jwtToken = GenerateToken(user, roleName.Name!);
            string refreshToken = GenerateRefreshToken();

            var updateRefreshToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.UserId == user.UserId);
            if (updateRefreshToken is null) return new LoginResponse(false, "Refesh token could not be generated because user has not signed in");

            updateRefreshToken.Token = refreshToken;
            await appDbContext.SaveChangesAsync();
            return new LoginResponse(true, "Token refreshed successfully", jwtToken, refreshToken);
        }

        public async Task<Person> GetProfile(int user)
        {
            var getUserRole = await FindUserRole(user);
            var getRoleName = await FindRoleName(getUserRole.RoleId);
            Person person = new Person();
            if (getRoleName.Name.Equals("Student"))
            {
                var student = await FindStudent(user);
                person.PersonId = student.StudentId;
                person.Type = Constants.Student;
                person.AddressId = student.AddressId;
                person.FirstName = student.FirstName;
                person.DateOfBirth = student.DateOfBirth;
                person.PlaceOfBirth = student.PlaceOfBirth;
                person.LastName = student.LastName;
                person.EmailAddress = student.PersonalEmailAddress;
                person.PhoneNumber = student.PhoneNumber;
                person.CNP = student.CNP;

            }
            else if (getRoleName.Name.Equals(Constants.Student))
            {
                var professor = await FindProfessor(user);
                person.PersonId = professor.ProfessorId;
                person.Type = Constants.Professor;
                person.AddressId = professor.AddressId;
                person.DateOfBirth = professor.DateOfBirth;
                person.PlaceOfBirth = professor.PlaceOfBirth;
                person.FirstName = professor.FirstName;
                person.LastName = professor.LastName;
                person.EmailAddress = professor.PersonalEmailAddress;
                person.PhoneNumber = professor.PhoneNumber;
                person.CNP = professor.CNP;
                person.AcademicTitle = professor.AcademicTitle;
            }
            return person;
        }

        private async Task<User> FindUser(int userId) => await appDbContext.Users.FirstOrDefaultAsync(_ => _.UserId == userId);

        public async Task<GeneralResponse> LogOut(int userId)
        {
            try
            {
                var refreshToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(r => r.UserId == userId);

                if (refreshToken != null)
                {
                    appDbContext.RefreshTokenInfos.Remove(refreshToken);
                    await appDbContext.SaveChangesAsync();
                    return new GeneralResponse(true, "Logout successful");
                }
                else
                {
                    return new GeneralResponse(false, "Refresh token not found for the user");
                }
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<GeneralResponse> InsertUsers(List<UserAccount> users)
        {
            foreach (var user in users)
            {
                var applicationUser = await AddToDatabase(new User()
                {
                    UserEmail = user.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
                });
            }
            return new GeneralResponse(true, "All users inserted successfully.");
        }

        public async Task<GeneralResponse> InsertProfessors(List<ProfessorAccount> professors)
        {
            foreach (var professor in professors)
            {
                var prof = await AddToDatabase(new Professor()
                {
                    UserId = professor.UserId,
                    FirstName = professor.FirstName,
                    LastName = professor.LastName,
                    DateOfBirth = professor.DateOfBirth,
                    PlaceOfBirth = professor.PlaceOfBirth,
                    PersonalEmailAddress = professor.PersonalEmailAddress,
                    PhoneNumber = professor.PhoneNumber,
                    CNP = professor.CNP,
                    AddressId = professor.AddressId,
                    AcademicTitle = professor.AcademicTitle
                });
            }
            return new GeneralResponse(true, "All professors inserted successfully.");
        }
    }
}
