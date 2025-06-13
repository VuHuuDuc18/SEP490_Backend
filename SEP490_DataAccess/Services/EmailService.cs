using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailCreateAccountAsync(string Email, string password)
        {
            try
            {
                // create message
                var message = new MimeMessage();
                message.Sender = new MailboxAddress(_config["MailSettings:DisplayName"], _config["MailSettings:EmailFrom"]);
                message.To.Add(MailboxAddress.Parse(Email));
                message.Subject = "THÔNG BÁO THÔNG TIN TÀI KHOẢN HỆ THỐNG LCFM SYSTEM";

                var builder = new BodyBuilder();

                
                builder.HtmlBody = BodyCreatAccount(Email, password);
                

                message.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.Connect(_config["MailSettings:SmtpHost"], Convert.ToInt32(_config["MailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
                smtp.Authenticate(_config["MailSettings:SmtpUser"], _config["MailSettings:SmtpPass"]);
                await smtp.SendAsync(message);
                smtp.Disconnect(true);

            }
            catch (System.Exception ex)
            { }
        }

        public string BodyCreatAccount(string email, string password)
        {
            return $@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                padding: 20px;
                color: #333;
            }}
            .container {{
                background-color: #fff;
                border-radius: 8px;
                padding: 20px;
                max-width: 600px;
                margin: auto;
                box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            }}
            .header {{
                font-size: 24px;
                font-weight: bold;
                color: #4CAF50;
                margin-bottom: 20px;
            }}
            .info {{
                margin-bottom: 10px;
                font-size: 16px;
            }}
            .btn {{
                display: inline-block;
                margin-top: 20px;
                padding: 10px 20px;
                background-color: #4CAF50;
                color: white;
                text-decoration: none;
                border-radius: 5px;
                font-weight: bold;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>Chào mừng !</div>
            <div class='info'>Tài khoản được cấp của bạn: </div>
            <div class='info'><strong>Email:</strong> {email}</div>
            <div class='info'><strong>Mật khẩu:</strong> {password}</div>
            <div class='info'>Vui lòng đăng nhập bằng thông tin trên tại đường dẫn dưới đây:</div>
            <a class='btn' href=''>Đăng nhập ngay</a>
        </div>
    </body>
    </html>";
        }

    }


}