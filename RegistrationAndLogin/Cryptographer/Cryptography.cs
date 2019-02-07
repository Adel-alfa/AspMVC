using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace RegistrationAndLogin
{
    public class Cryptography
    {
        public static string GenerateKeyHash(string Password)
        {

            if (string.IsNullOrEmpty(Password)) return null;

            byte[] result = new byte[36];
            byte[] salt = new byte[16];
            byte[] hash = new byte[20];

            new RNGCryptoServiceProvider().GetBytes(salt);
            var hasBytes = new Rfc2898DeriveBytes(Password, salt, 10000);
            hash = hasBytes.GetBytes(20);
            Array.Copy(salt, 0, result, 0, 16);
            Array.Copy(hash, 0, result, 16, 20);

            return Convert.ToBase64String(result);
        }
        public static bool ValidatePasswords(string hashPassword, string Password)
        {
            if ((string.IsNullOrEmpty(hashPassword)) || (string.IsNullOrEmpty(Password))) return false;
            byte[] salt = new byte[16];

            byte[] hashPassBytes = Convert.FromBase64String(hashPassword);

            Array.Copy(hashPassBytes, 0, salt, 0, 16);

            using (var hashBytes = new Rfc2898DeriveBytes(Password, salt, 10000))
            {

                byte[] newKey = hashBytes.GetBytes(20);
                int equal = 1;
                for (int i = 0; i < 20; i++)
                    if (hashPassBytes[i + 16] != newKey[i])
                        equal = 0;
                if (equal == 1)

                    return true;
            }
            return false;

        }
    }
}