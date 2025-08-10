using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class MailBodyGenerate
    {
        private static string GenerateBaseTemplate(string title, string content)
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
            <div class='header'>{title}</div>
            {content}
        </div>
    </body>
    </html>";
        }

        public static string BodyCreateAccount(string email, string password)
        {
            var content = $@"
            <div class='info'>Tài khoản được cấp của bạn: </div>
            <div class='info'><strong>Email:</strong> {email}</div>
            <div class='info'><strong>Mật khẩu:</strong> {password}</div>
            <div class='info'>Vui lòng đăng nhập bằng thông tin trên tại đường dẫn dưới đây:</div>
            <a class='btn' href=''>Đăng nhập ngay</a>";

            return GenerateBaseTemplate("Chào mừng !", content);
        }

        public static string BodyCreateForgotPassword(string token, string resetLink)
        {
            var content = $@"
            <div class='info'>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</div>
            <div class='info'><strong>Mã:</strong> {token}</div>
            <div class='info'>Vui lòng nhấp vào nút bên dưới để đặt lại mật khẩu. Link này sẽ hết hạn sau 24 giờ:</div>
            <a class='btn' href='{resetLink}'>Đặt lại mật khẩu</a>
            <div class='info' style='margin-top: 20px; font-size: 12px; color: #666;'>
                Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
            </div>";

            return GenerateBaseTemplate("Yêu cầu đặt lại mật khẩu!", content);
        }
        public static string BodyCreateConfirmEmail(string email, string resetLink)
        {
            var content = $@"
            <div class='info'>Chúng tôi đã nhận được yêu cầu xác thực tài khoản của bạn.</div>
            <div class='info'><strong>Email:</strong> {email}</div>
            <div class='info'>Vui lòng nhấp vào nút bên dưới để xác thực tài khoản. Link này sẽ hết hạn sau 24 giờ:</div>
            <a class='btn' href='{resetLink}'>Xác thực tài khoản</a>
            <div class='info' style='margin-top: 20px; font-size: 12px; color: #666;'>
                Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
            </div>";

            return GenerateBaseTemplate("Yêu cầu xác thực tài khoản!", content);
        }
        public static string BodyCreateOrder(string email, string orderLink)
        {
            var content = $@"
    <div class='info'>Cảm ơn bạn đã đặt hàng tại hệ thống của chúng tôi.</div>
    <div class='info'><strong>Email đặt hàng:</strong> {email}</div>
    <div class='info'>Đơn hàng của bạn đã được tạo thành công. Vui lòng nhấp vào nút bên dưới để xem chi tiết đơn hàng:</div>
    <a class='btn' href='{orderLink}'>Xem đơn hàng</a>
    <div class='info' style='margin-top: 20px; font-size: 12px; color: #666;'>
        Nếu bạn không phải là người thực hiện đơn hàng này, vui lòng liên hệ với chúng tôi để được hỗ trợ.
    </div>";

            return GenerateBaseTemplate("Xác nhận tạo đơn hàng!", content);
        }

        public static string BodyOrderApproved(string email, string orderLink)
        {
            var content = $@"
    <div class='info'>Đơn hàng của bạn đã được duyệt.</div>
    <div class='info'><strong>Email đặt hàng:</strong> {email}</div>
    <div class='info'>Bạn có thể kiểm tra trạng thái đơn hàng, thông tin nhận hàng và chi tiết sản phẩm bằng cách nhấp vào nút dưới đây:</div>
    <a class='btn' href='{orderLink}'>Xem đơn hàng</a>
    <div class='info' style='margin-top: 20px; font-size: 12px; color: #666;'>
        Nếu bạn có bất kỳ thắc mắc hoặc cần hỗ trợ, vui lòng liên hệ với chúng tôi qua thông tin trên website.
    </div>";

            return GenerateBaseTemplate("Đơn hàng đã được duyệt!", content);
        }


    }
}
