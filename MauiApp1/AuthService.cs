using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Microsoft.AspNet.Identity
{
    public class UserManager<TUser, TKey> : IDisposable where TUser : class, IUser<TKey> where TKey : IEquatable<TKey>
    {
        //protected virtual async Task<bool> VerifyPasswordAsync(IUserPasswordStore<TUser, TKey> store, TUser user, string password)
        //{
        //    string hash = await store.GetPasswordHashAsync(user).WithCurrentCulture();
        //    return PasswordHasher.VerifyHashedPassword(hash, password) != PasswordVerificationResult.Failed;
        //}

        public void Dispose()
        {
            //Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        //PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword);
    }
   
}
