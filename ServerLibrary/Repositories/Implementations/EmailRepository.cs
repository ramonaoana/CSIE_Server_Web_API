using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using System.Net;
using System.Net.Mail;

namespace ServerLibrary.Repositories.Implementations
{
    public class EmailRepository(AppDbContext appDbContext) : IEmail
    {
        public async Task<GeneralResponse> SendEmail(EmailDataMethod data)
        {
            var user = await FindUser(data.Id);
            bool flag = false;
            string message = "";
            try
            {
                var fromAddress = new MailAddress(user.UserEmail);
                var toAddress = new MailAddress("testdepartamentsecretariat@gmail.com");
                var fromPassword = user.Password;
                var subject = "CSIEBOT";
                var body = data.Message;

                using (var mess = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                    };
                    await smtp.SendMailAsync(mess);
                }

                flag = true;
                message = "Email sent successfully.";
            }
            catch (Exception ex)
            {
                flag = false;
                message = "Failed to send email: " + ex.Message;

            }
            return new GeneralResponse(flag, message);
        }
        public async Task<User> FindUser(int userId) => await appDbContext.Users.FirstOrDefaultAsync(_ => _.UserId == userId);
    }
}
