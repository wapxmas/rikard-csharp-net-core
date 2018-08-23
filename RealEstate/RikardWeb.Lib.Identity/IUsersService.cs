using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RikardWeb.Lib.Identity
{
    public interface IUsersService<T> where T : IdentityUser
    {
        Task<bool> IsPhoneNumberExists(string phoneNumber, string userId = null);
        void AddProlongationPayment(string userId, double amount, string type, string description);
        T GetUserByIdSync(string userId);
    }
}
