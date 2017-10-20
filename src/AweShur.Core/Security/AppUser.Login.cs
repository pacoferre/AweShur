using Microsoft.AspNetCore.Http;
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
        internal static bool UseAppUserNoDB = false;

        internal static string CurrentUserIDSessionKey = "USER_ID";
        public static byte[] SALT;

        public static bool LoginWindowsWithoutDomain(HttpContext context)
        {
            string login = GetLoginWithOutDomain(context);
            int idAppUser = DB.Instance.QueryFirstOrDefault<int>(@"SELECT idAppUser FROM AppUser
WHERE (email = @email) AND (deactivated = 0)", new { email = login });
            AppUser usu = (AppUser)BusinessBaseProvider.Instance.CreateObject("AppUser");
            bool valid = false;

            if (idAppUser > 0)
            {
                valid = true;

                usu.ReadFromDB(idAppUser);
                SetAppUser(usu, context);

                usu.PostLogin(login, usu, valid);
            }
            else
            {
                usu.PostLogin(login, null, valid);
            }

            return valid;
        }

        private static string GetLoginWithOutDomain(HttpContext context)
        {
            string login = context.User.Identity.Name;

            if (login.Contains("\\"))
            {
                login = login.Split('\\')[1];
            }
            if (login.Contains("@"))
            {
                login = login.Split('@')[0];
            }

            return login;
        }

        public static bool Login(string email, string password, HttpContext context)
        {
            AppUser theUser = (AppUser)BusinessBaseProvider.Instance.CreateObject("AppUser");

            return theUser.LoginInternal(email, password, context);
        }

        protected bool LoginInternal(string email, string password, HttpContext context)
        {
            dynamic userData = DB.Instance.QueryFirstOrDefault("Select idAppUser, password From AppUser Where email = @Email", new { Email = email });
            AppUser usu = null;
            bool valid = false;

            if (userData != null)
            {
                string enc = PasswordDerivedString(((int)userData.idAppUser).NoNullString(), password);

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

                        usu["password"] = password;

                        usu.StoreToDB();
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
                //    usu.StoreToDB(false);
                //    usu.creacionAutomatica = false;
                //}
            }

            if (usu != null)
            {
                SetAppUser(usu, context);
                valid = true;
            }

            PostLogin(email, usu, valid);

            return valid;
        }

        public virtual void PostLogin(string email, AppUser user, bool valid)
        {

        }

        public virtual bool ChangePassword(string actual, string newPassword)
        {
            ReadFromDB();

            string enc = PasswordDerivedString(this[0].NoNullString(), actual);

            if (enc == this["checkpassword"].NoNullString() && CheckStrength(newPassword))
            {
                this["password"] = newPassword;

                StoreToDB();
            }
            else
            {
                return false;
            }

            return true;
        }

        public virtual bool ChangePasswordBlind(string newPassword)
        {
            ReadFromDB();

            if (CheckStrength(newPassword))
            {
                this["password"] = newPassword;

                StoreToDB();
            }
            else
            {
                return false;
            }

            return true;
        }

        protected virtual bool CheckStrength(string password)
        {
            return true;
        }

        public static string PasswordDerivedString(string idAppUser, string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: idAppUser + "_fsk53g5djfskj_" + password,
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

        public static void SetAppUser(AppUser user, HttpContext context)
        {
            // New user logon.
            context.Session.Clear();

            context.Session.SetInt32(CurrentUserIDSessionKey, (int)user[0]);

            BusinessBaseProvider.StoreObject(user, UseAppUserNoDB ? "AppUserNoDB" : "AppUser");
        }

        public static AppUser GetAppUserWithoutHttpContext()
        {
            return GetAppUser(BusinessBaseProvider.HttpContext);
        }

        private static object currentUserLock = new object();

        public static AppUser GetAppUser(HttpContext context)
        {
            int idAppUser = context.Session.GetInt32(CurrentUserIDSessionKey).NoNullInt();

            if (idAppUser > 0)
            {
                lock (currentUserLock)
                {
                    return (AppUser)BusinessBaseProvider.RetreiveObject(context,
                        UseAppUserNoDB ? "AppUserNoDB" : "AppUser", idAppUser.ToString());
                }
            }

            return null;
        }

        public static void StoreAppUser(AppUser user)
        {
            BusinessBaseProvider.StoreObject(user,
                UseAppUserNoDB ? "AppUserNoDB" : "AppUser");
        }
    }
}
