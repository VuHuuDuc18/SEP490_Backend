using Domain.DTOs.Request.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class DateTimeExcutor
    {
        public static (DateTime from, DateTime to) TimeRangeSetting(StatisticsOrderRequest request)
        {
            if (request.From != null || request.To != null)
            {
                //request.From = (DateTime)request.From;
                if (request.To > DateTime.Now || request.To == null)
                {
                    request.To = DateTime.Now;
                }
                if (request.From > DateTime.Now)
                {
                    throw new Exception("Ngày bắt đầu phải trước hiện tại");
                }
                if (request.To < request.From)
                {
                    return ((DateTime)request.To, (DateTime)request.From);
                }
            }
            else
            {
                if (request.Year != null)
                {
                    request.From = new DateTime(request.Year, 1, 1);
                    request.To = new DateTime(request.Year + 1, 1, 1);
                }
                if (request.Quater != null)
                {
                    request.From = new DateTime(request.Year, (((int)request.Quater - 1) * 3 + 1), 1);
                    request.To = new DateTime(request.Year, ((int)request.Quater * 3) + 1, 1);
                }
                if (request.Month != null)
                {
                    request.From = new DateTime(request.Year, (int)request.Month, 1);
                    request.To = new DateTime(request.Year, (int)request.Month + 1, 1);
                }

            }
            return ((DateTime)request.From, (DateTime)request.To);
        }
    }
}
