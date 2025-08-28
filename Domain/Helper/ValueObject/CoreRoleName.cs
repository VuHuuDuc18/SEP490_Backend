using Domain.Helper.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Helper.ValueObjects
{
    public static class CoreRoleName
    {
        public static List<String> RoleNames = new List<String>() { RoleConstant.CompanyAdmin, RoleConstant.TechnicalStaff, RoleConstant.BreedingRoomStaff, RoleConstant.MedicineRoomStaff, RoleConstant.FeedRoomStaff, RoleConstant.Worker, RoleConstant.SalesStaff, RoleConstant.Customer };
    }
}
