using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Extensions
{
    public static class MailBodyGenerate
    {
        private static readonly string LINKCHANGEPASSWORD = "";
        public static string BodyCreateAccount(string email, string password, string rolename)
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
            <div class='info'><strong>Vai trò:</strong> {rolename}</div>
            <div class='info'>Vui lòng đăng nhập bằng thông tin trên tại đường dẫn dưới đây:</div>
            <a class='btn' href='{LINKCHANGEPASSWORD}'>Đăng nhập ngay</a>
        </div>
    </body>
    </html>";
        }
        public static string BodyResetPassword(string email, string password)
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
            <div class='header'>RESET mật khẩu</div>
            <div class='info'>Tài khoản đã được đổi mật khẩu: </div>
            <div class='info'><strong>Email:</strong> {email}</div>
            <div class='info'><strong>Mật khẩu:</strong> {password}</div>           
            
            <a class='btn' href='{LINKCHANGEPASSWORD}'>Đổi mật khẩu</a>
        </div>
    </body>
    </html>";
        }
    }
}
