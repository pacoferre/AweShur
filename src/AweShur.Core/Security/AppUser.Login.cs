using Microsoft.AspNetCore.Http;
using Microsoft.Framework.Runtime.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace AweShur.Core.Security
{
    public partial class AppUser
    {
        private static string CurrentUserIDSessionKey = "USER_ID";
        public static byte[] SALT;

        public static bool Login(string email, string password, HttpContext context)
        {
            AppUser theUser = (AppUser)BusinessBaseProvider.Instance.CreateObject("AppUser");

            return theUser.LoginInternal(email, password, context);
        }

        protected bool LoginInternal(string email, string password, HttpContext context)
        {
            dynamic userData = DB.Instance.QueryFirstOrDefault("Select idAppUser, email, password From AppUser Where email = @Email", new { Email = email });
            AppUser usu = null;
            bool valid = false;

            if (userData != null)
            {
                string enc = PasswordDerivedString(userData.email, password);

                if (enc == userData.password)
                {
                    usu = (AppUser)BusinessBaseProvider.Instance.CreateObject("AppUser");

                    usu.ReadFromDB((int)userData.idAppUser);
                }

                // Hack to 
                if (usu == null)
                {
                    if (userData.password == password)
                    {
                        usu = (AppUser)BusinessBaseProvider.Instance.CreateObject("AppUser");

                        usu.ReadFromDB((int)userData.idAppUser);

                        //usu["password"] = password;

                        //usu.StoreToDB();
                    }
                    else
                    {
                        usu = null;
                    }
                }

                if (usu != null && usu["deactivated"].NoNullBool())
                {
                    usu = null;
                }
            }
            else
            {
                //// Si no existen usuarios, creamos el admin.
                //if (Data.Instancia().DameExpresionInt("Select COUNT(*) From UserApp WHERE SU = 1") == 0)
                //{
                //    usu = (UserApp)BusinessProviderBase.Provider.NewObject("UserApp");

                //    usu.NuevoEnBlanco();
                //    usu["name"] = "UserApp";
                //    usu["surname"] = "Administrador";
                //    usu["su"] = true;
                //    usu["email"] = "admin@local";
                //    usu["password"] = "admin";
                //    usu.creacionAutomatica = true;
                //    usu.Guardar(false);
                //    usu.creacionAutomatica = false;
                //}
            }

            if (usu != null)
            {
                SetAppUser(usu, context);
                valid = true;
            }

            //LogUserEntry(ref user, login);

            return valid;
        }

        public static string PasswordDerivedString(string email, string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: email + "_fsk53g5djfskj_" + password,
                salt: SALT,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 200,
                numBytesRequested: 256 / 8));
        }

        public static bool UserIsAuthenticated(HttpContext req)
        {
            return IDAppUser(req) != null;
        }

        public static int? IDAppUser(HttpContext req)
        {
            return req.Session.GetInt32(CurrentUserIDSessionKey);
        }

        private static void SetAppUser(AppUser user, HttpContext context)
        {
            // New user logon.
            context.Session.Clear();

            context.Session.SetInt32(CurrentUserIDSessionKey, (int)user[0]);

            BusinessBaseProvider.StoreObject(user, "AppUser", context);
        }

        public static AppUser GetAppUserWithoutHttpContext()
        {
            return GetAppUser(BusinessBaseProvider.HttpContext);
        }

        public static AppUser GetAppUser(HttpContext context)
        {
            int idAppUser = context.Session.GetInt32(CurrentUserIDSessionKey).NoNullInt();

            if (idAppUser > 0)
            {
                return (AppUser)BusinessBaseProvider.RetreiveObject("AppUser", idAppUser.ToString(), context);
            }

            return null;
        }
    }
}
