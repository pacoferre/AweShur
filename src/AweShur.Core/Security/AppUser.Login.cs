﻿using Microsoft.AspNetCore.Http;
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
        private static byte[] SALT = null;

        public static AppUser Login(string login, string password, ISession session)
        {
            AppUser theUser = (AppUser)BusinessBaseProvider.Instance.CreateObject("AppUser");

            return theUser.LoginInternal(login, password, session);
        }

        protected virtual AppUser LoginInternal(string login, string password, ISession session)
        {
            dynamic userData = DB.Instance.QuerySingle("Select IDUserApp, Login, Password From UserApp Where Login = @Login", new { Login = login });
            AppUser usu = null;

            if (userData != null)
            {
                string enc = PasswordDerivedString(userData.Login + "akj121hkj" + password);

                if (enc == userData.Password)
                {
                    usu = (AppUser)BusinessBaseProvider.Instance.CreateObject("AppUser");

                    usu.ReadFromDB((int)userData.IDUserApp);
                }

                // Hack to 
                if (usu == null)
                {
                    if (userData.Password == password)
                    {
                        usu = (AppUser)BusinessBaseProvider.Instance.CreateObject("AppUser");

                        usu.ReadFromDB((int)userData.IDUserApp);

                        usu["Password"] = enc;

                        //usu.StoreToDB();
                    }
                    else
                    {
                        usu = null;
                    }
                }

                if (usu != null && usu["Deactivated"].NoNullBool())
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
                //    usu["Name"] = "UserApp";
                //    usu["SurName"] = "Administrador";
                //    usu["SU"] = true;
                //    usu["Login"] = "admin";
                //    usu["Password"] = "admin";
                //    usu.creacionAutomatica = true;
                //    usu.Guardar(false);
                //    usu.creacionAutomatica = false;
                //}
            }

            if (usu != null)
            {
                SetAppUser(usu, session);
            }

            //LogUserEntry(ref user, login);

            return usu;
        }

        public static string PasswordDerivedString(string password)
        {
            if (SALT == null)
            {
                lock (SALT)
                {
                    if (SALT == null)
                    {
                        SALT = Encoding.Unicode.GetBytes(DB.Configuration.GetSection("Security")["SALT"]).Take(16).ToArray();
                    }
                }
            }

            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
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

        private static void SetAppUser(AppUser user, ISession session)
        {
            // New user logon.
            session.Clear();

            session.SetInt32(CurrentUserIDSessionKey, (int)user[0]);

            BusinessBaseProvider.StoreToSession(user, "AppUser", session);
        }

        public static AppUser GetAppUserWithoutHttpContext()
        {
            IHttpContextAccess‌​or accesor = (IHttpContextAccess‌​or)CallContextServiceLocator.Locator.ServiceProvider.GetService(typeof(IHttpContextAccess‌​or));
            HttpContext context = accesor.HttpContext;
            int idAppUser = context.Session.GetInt32(CurrentUserIDSessionKey).NoNullInt();

            if (idAppUser > 0)
            {
                return (AppUser)BusinessBaseProvider.RetreiveFromSession("AppUser", idAppUser.ToString(), context.Session);
            }

            return null;
        }
    }
}
