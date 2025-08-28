using Microsoft.AspNetCore.Identity;
using Entities.EntityModel;
using Domain.Helper.Constants;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Seeds
{
    public static class UserSeeds
    {
        private static readonly (string FullName, string Address, string PhoneNumber, string UserName, string Email, string Role)[]
            seedUsers = new[]
        {
            ("Admin","Hà Nội", "099999999", "admin","ad@lcfms.org",RoleConstant.CompanyAdmin),
            ("Ngô Thị Hồng", "Quận Tân Bình, TP.HCM", "0382128679", "worker1", "worker1@gmail.com", RoleConstant.Worker),
            ("Trần Minh Đức", "Quận Bình Thạnh, TP.HCM", "0982123765", "feedroom1", "feedroom1@gmail.com", RoleConstant.FeedRoomStaff),
            ("Nguyễn Văn An", "Quận 1, TP.HCM", "0982123458", "companyadmin1", "companyadmin1@gmail.com", RoleConstant.CompanyAdmin),
            ("Trần Thị Hạnh", "Quận 2, TP.HCM", "0582172322", "techstaff3", "techstaff3@gmail.com", RoleConstant.TechnicalStaff),
            ("Võ Anh Tuấn", "Quận 4, TP.HCM", "0382123656", "customer2", "customer2@gmail.com", RoleConstant.Customer),
            ("Phạm Quang Cường", "Quận 5, TP.HCM", "0332123452", "breedingroom1", "breedingroom1@gmail.com", RoleConstant.BreedingRoomStaff),
            ("Nguyễn Thị Mai", "Quận 8, TP.HCM", "0382172946", "customer3", "customer3@gmail.com", RoleConstant.Customer),
            ("Phan Thị Hương", "Quận Gò Vấp, TP.HCM", "0382125633", "customer1", "customer1@gmail.com", RoleConstant.Customer),
            ("Lê Thị Bích", "Quận 3, TP.HCM", "0982122312", "techstaff1", "techstaff1@gmail.com", RoleConstant.TechnicalStaff),
            ("Nguyễn Văn Bình", "Quận Tân Phú, TP.HCM", "0882125641", "worker2", "worker2@gmail.com", RoleConstant.Worker),
            ("Lê Thị Hoa", "Quận Bình Tân, TP.HCM", "0382172347", "worker3", "worker3@gmail.com", RoleConstant.Worker),
            ("Phan Minh Đức", "Quận 7, TP.HCM", "0582172153", "techstaff2", "techstaff2@gmail.com", RoleConstant.TechnicalStaff),
            ("Huỳnh Văn Long", "Quận Phú Nhuận, TP.HCM", "0582172222", "salesstaff1", "salesstaff1@gmail.com", RoleConstant.SalesStaff),
            ("Đỗ Thu Phương", "Quận 10, TP.HCM", "0982121234", "medicineroom1", "medicineroom1@gmail.com", RoleConstant.MedicineRoomStaff)
        };
       
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            const string defaultPassword = "@Password1";

            foreach (var userData in seedUsers)
            {
                var user = await userManager.FindByEmailAsync(userData.Email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = userData.UserName,
                        Email = userData.Email,
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true,
                        PhoneNumber = userData.PhoneNumber,
                        FullName = userData.FullName,
                        Address = userData.Address
                    };

                    var result = await userManager.CreateAsync(user, defaultPassword);
                    if (result.Succeeded)
                    {
                        if (!await roleManager.RoleExistsAsync(userData.Role))
                        {
                            await roleManager.CreateAsync(new Role { Name = userData.Role });
                        }
                        await userManager.AddToRoleAsync(user, userData.Role);
                    }
                    else
                    {
                        // Xử lý lỗi ở đây nếu cần
                    }
                }
                else
                {
                    if (!await userManager.IsInRoleAsync(user, userData.Role))
                    {
                        await userManager.AddToRoleAsync(user, userData.Role);
                    }
                }
            }
        }
    }
}
