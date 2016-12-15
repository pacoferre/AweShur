using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public enum SortDirection
    {
        //
        // Summary:
        //     Sort from smallest to largest. For example, from A to Z.
        Ascending = 0,
        //
        // Summary:
        //     Sort from largest to smallest. For example, from Z to A.
        Descending = 1
    }

    public enum ElementType
    {
        View,
        Collection
    }

    public enum HorizontalAlign
    {
        //
        // Summary:
        //     The horizontal alignment is not set.
        NotSet = 0,
        //
        // Summary:
        //     The contents of a container are left justified.
        Left = 1,
        //
        // Summary:
        //     The contents of a container are centered.
        Center = 2,
        //
        // Summary:
        //     The contents of a container are right justified.
        Right = 3,
        //
        // Summary:
        //     The contents of a container are uniformly spread out and aligned with both the
        //     left and right margins.
        Justify = 4
    }

    public static class Lib
    {
        public static string NoNullString(this object myObject)
        {
            return myObject?.ToString() ?? String.Empty;
        }

        public static DateTime NoNullDatetime(this object myObject)
        {
            DateTime res = DateTime.MinValue;

            if (myObject != null)
            {
                if (myObject is DateTime)
                {
                    res = (DateTime)myObject;
                }
                else
                {
                    try
                    {
                        res = System.DateTime.Parse(myObject.ToString());
                    }
                    catch
                    {
                    }
                }
            }

            return res;
        }

        public static DateTime FirstMonthDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0);
        }

        public static DateTime FirstYearDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 1, 1, 0, 0, 0);
        }

        public static int NoNullInt(this object number)
        {
            int res = 0;

            if (number is string && (number.ToString() == "" || number.ToString() == "0"))
            {
                return 0;
            }

            if (number != null)
            {
                if (number is int)
                {
                    res = (int)number;
                }
                else
                {
                    try
                    {
                        res = System.Int32.Parse(number.ToString());
                    }
                    catch
                    {
                    }
                }
            }

            return res;
        }

        public static long NoNullLong(this object number)
        {
            long res = 0;

            if (number != null)
            {
                if (number is long)
                {
                    res = (long)number;
                }
                else
                {
                    long.TryParse(number.ToString(), out res);
                }
            }

            return res;
        }

        public static Int16 NoNullInt16(this object number)
        {
            Int16 res = 0;

            if (number != null)
            {
                if (number is Int16)
                {
                    res = (Int16)number;
                }
                else
                {
                    System.Int16.TryParse(number.ToString(), out res);
                }
            }

            return res;
        }

        public static bool NoNullBool(this object obj)
        {
            bool res = false;

            if (obj != null)
            {
                if (obj is bool)
                {
                    res = (bool)obj;
                }
                else if (obj is int)
                {
                    res = (int)obj != 0;
                }
                else
                {
                    res = obj.NoNullInt() != 0;
                }
            }

            return res;
        }

        private static int numerizeMethod = 0;

        public static string Numerize(string number)
        {
            string res = number;

            Numerize(ref res);

            return res;
        }

        public static void Numerize(ref string number)
        {
            if (numerizeMethod == 0)
            {
                if (5.2 == double.Parse("5,2"))
                {
                    numerizeMethod = 1;
                }
                else
                {
                    numerizeMethod = 2;
                }
            }

            if (numerizeMethod == 1)
            {
                if (number.IndexOf(",") == -1 && number.IndexOf(".") >= 0)
                {
                    number = number.Replace(".", ",");
                }

                number = number.Replace(".", "");
            }
            else
            {
                if (number.IndexOf(",") == -1 && number.IndexOf(".") >= 0)
                {
                    number = number.Replace(",", ".");
                }

                number = number.Replace(",", "");
            }

            number = number.Replace("€", "").Replace("%", "").Trim();
        }

        public static double NoNullDouble(this object number)
        {
            double res = 0;

            if (number != null)
            {
                if (number is double)
                {
                    res = (double)number;
                }
                else if (number is Single)
                {
                    res = Convert.ToSingle((Single)number);
                }
                else
                {
                    Double.TryParse(Numerize(number.ToString()), out res);
                }
            }

            return res;
        }

        public static Single NoNullSingle(this object number)
        {
            Single res = 0;

            if (number != null)
            {
                if (number is Single)
                {
                    res = (Single)number;
                }
                else
                {
                    Single.TryParse(Numerize(number.ToString()), out res);
                }
            }

            return res;
        }


        public static decimal NoNullDecimal(this object number)
        {
            decimal res = 0;

            if (number != null)
            {
                if (number is decimal)
                {
                    res = (decimal)number;
                }
                else
                {
                    Decimal.TryParse(Numerize(number.ToString()), out res);
                }
            }

            return res;
        }

        public static bool AreEmails(string emailList)
        {
            if (emailList.IndexOf(";") > 0)
            {
                string[] emails = emailList.Split(new char[] { ';' });

                foreach (string email in emails)
                {
                    if (!IsEmail(email))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return IsEmail(emailList);
            }
        }

        public static bool IsEmail(string email)
        {
            if (email.NoNullString() != "")
            {
                string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                    @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                Regex re = new Regex(strRegex);

                return re.IsMatch(email.Trim());
            }

            return false;
        }

        public static string Months(int mes)
        {
            return System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames[mes];
        }

        public static string MonthsFirstUpper(int mes)
        {
            string m = System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames[mes];

            m = m.Substring(0, 1).ToUpper() + m.Substring(1);

            return m;
        }

        private static System.TimeSpan span = new System.TimeSpan((new DateTime(1970, 1, 1)).Ticks);

        public static long GetJavascriptTimestamp(System.DateTime input)
        {
            return (long)(input.Subtract(span).Ticks / 10000);
        }

        private static string UNT_BYTE = "Byte";
        private static string UNT_BYTES = "Bytes";
        private static string UNT_KILOBYTES = "KB";
        private static string UNT_MEGABYTES = "MB";
        private static string UNT_GIGABYTES = "GB";
        private static string UNT_TERRABYTES = "TB";

        public static void ByteSize(long SourceSize, ref double TargetSize, ref string UnitName)
        {
            if ((SourceSize / 1024) > 1)
            {
                if ((SourceSize / 1048576) > 1)
                {
                    if ((SourceSize / 1073741824) > 1)
                    {
                        if ((SourceSize / 1099511627776) > 1)
                        {
                            UnitName = UNT_TERRABYTES;
                            TargetSize = SourceSize / 1099511627776;
                        }
                        else
                        {
                            UnitName = UNT_GIGABYTES;
                            TargetSize = (double)SourceSize / 1073741824.0;
                        }
                    }
                    else
                    {
                        UnitName = UNT_MEGABYTES;
                        TargetSize = (double)SourceSize / 1048576.0;
                    }
                }
                else
                {
                    UnitName = UNT_KILOBYTES;
                    TargetSize = (double)SourceSize / 1024.0;
                }
            }
            else
            {
                TargetSize = SourceSize;
                if (TargetSize == 1)
                {
                    UnitName = UNT_BYTE;
                }
                else
                {
                    UnitName = UNT_BYTES;
                }
            }
        }

        //public static string Compress(string strInput)
        //{
        //    byte[] bytData = System.Text.Encoding.UTF8.GetBytes(strInput);
        //    MemoryStream ms = new MemoryStream();
        //    Stream s = new DeflaterOutputStream(ms);
        //    s.Write(bytData, 0, bytData.Length);
        //    s.Close();
        //    byte[] compressedData = (byte[])ms.ToArray();

        //    return Convert.ToBase64String(compressedData);
        //}

        //public static string DeCompress(string strInput)
        //{
        //    byte[] bytInput = Convert.FromBase64String(strInput);
        //    string strResult = "";
        //    int totalLength = 0;
        //    byte[] writeData = new byte[4096];
        //    Stream s2 = new InflaterInputStream(new MemoryStream(bytInput));

        //    while (true)
        //    {
        //        int size = s2.Read(writeData, 0, writeData.Length);
        //        if (size > 0)
        //        {
        //            totalLength += size;
        //            strResult += System.Text.Encoding.ASCII.GetString(writeData, 0,
        //                size);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    s2.Close();

        //    return strResult;
        //}

        // Spanish section :)

        private const string LetrasNIF = "TRWAGMYFPDXBNJZSQVHLCKET";

        /// <summary> Genera la letra correspondiente a un DNI. </summary>
        /// <param name="dni"> DNI a procesar. </param>
        /// <returns> Letra correspondiente al DNI. </returns>
        public static string LetraNIF(ref string dni)
        {
            int n;

            if (dni == null || dni.Length != 8 || !int.TryParse(dni, out n))
            {
                throw new ArgumentException("Error al calcular la letra del DNI " + dni + ".");
            }

            return LetrasNIF[n % 23].ToString();
        }

        public static bool ValidarNIF(ref string nif, ref string Message)
        {
            bool correcto = true;
            string letraAnterior = "";
            string letra = "";

            if (nif.Length <= 2)
            {
                correcto = false;
            }

            if (correcto)
            {
                letraAnterior = nif[nif.Length - 1].ToString();

                nif = nif.Substring(0, nif.Length - 1);
                if (nif != null && nif.Length < 8)
                {
                    nif = new string('0', 8 - nif.Length) + nif;
                }

                try
                {
                    letra = LetraNIF(ref nif);
                }
                catch (Exception exp)
                {
                    correcto = false;
                    Message = exp.Message;
                }
            }

            if (correcto)
            {
                if (letra != letraAnterior)
                {
                    correcto = false;
                    Message = "Letra " + letraAnterior + " incorrecta.";
                }
            }

            nif = nif + letraAnterior;

            return correcto;
        }

        public static bool ValidarCIF(string cif, ref string Message)
        {
            bool correcto = true;
            int[] v1 = new int[] { 0, 2, 4, 6, 8, 1, 3, 5, 7, 9 };
            string letras = "JABCDEFGHIJ";
            int temp = 0;
            string value = cif.ToUpper();
            string Tipo = "", Provincia = "", Digito = "";

            //http://club.telepolis.com/jagar1/Economia/Ccif.htm

            // A81840274
            if (cif.Length != 9)
            {
                correcto = false;
                Message = "El CIF debe constar de una letra y 9 dígitos.";
            }

            if (correcto)
            {
                int v = 0;

                if (!int.TryParse(cif.Substring(1, 6), out v))
                {
                    correcto = false;
                    Message = "El CIF debe constar de una letra y 9 dígitos.";
                }

                if (correcto)
                {
                    if (v.ToString() != cif.Substring(1, 6))
                    {
                        correcto = false;
                        Message = "El CIF debe constar de una letra y 9 dígitos.";
                    }
                }
            }

            if (correcto)
            {
                for (int i = 2; i <= 6; i += 2)
                {
                    temp = temp + v1[Int32.Parse(value.Substring(i - 1, 1))];
                    temp = temp + Int32.Parse(value.Substring(i, 1));
                }

                temp = temp + v1[Int32.Parse(value.Substring(7, 1))];
                temp = (10 - (temp % 10));

                switch (value.Substring(0, 1))
                {
                    case "A":
                        Tipo = "Sociedad An&oacute;nima.";
                        break;
                    case "B":
                        Tipo = "Sociedad de responsabilidad limitada.";
                        break;
                    case "C":
                        Tipo = "Sociedad colectiva.";
                        break;
                    case "D":
                        Tipo = "Sociedad comanditaria.";
                        break;
                    case "E":
                        Tipo = "Comunidad de bienes y herencias yacentes.";
                        break;
                    case "F":
                        Tipo = "Sociedad cooperativa.";
                        break;
                    case "G":
                        Tipo = "Asociaciones.";
                        break;
                    case "H":
                        Tipo = "Comunidad de propietarios en r&eacute;gimen de propiedad horizontal.";
                        break;
                    case "J":
                        Tipo = "Sociedades Civiles, con o sin personalidad jur&iacute;dica.";
                        break;
                    case "K":
                        Tipo = "Espa&ntilde;oles menores de 14 a&ntilde;os.";
                        break;
                    case "L":
                        Tipo = "Espa&ntilde;oles residentes en el extranjero sin DNI.";
                        break;
                    case "M":
                        Tipo = "Extranjeros que no tienen NIE.";
                        break;
                    case "N":
                        Tipo = "Entidades extranjeras.";
                        break;
                    case "P":
                        Tipo = "Corporaci&oacute;n local.";
                        break;
                    case "Q":
                        Tipo = "Organismo p&uacute;blicos.";
                        break;
                    case "R":
                        Tipo = "Congregaciones e Instituciones Religiosas.";
                        break;
                    case "S":
                        Tipo = "Organos de la Administraci&oacute;n del Estado y Comunidades Aut&oacute;nomas.";
                        break;
                    case "U":
                        Tipo = "Uniones temporales de Empresas.";
                        break;
                    case "V":
                        Tipo = "Otros tipos no definidos en el resto de claves.";
                        break;
                    case "W":
                        Tipo = "Establecimientos permanentes de entidades no residentes en Espa&ntilde;a.";
                        break;
                    default:
                        Tipo = "No existente";
                        break;
                }

                switch (value.Substring(1, 2))
                {
                    case "01":
                        Provincia = "Alava";
                        break;
                    case "02":
                        Provincia = "Albacete";
                        break;
                    case "03":
                    case "53":
                    case "54":
                        Provincia = "Alicante";
                        break;
                    case "04":
                        Provincia = "Almería";
                        break;
                    case "05":
                        Provincia = "Ávila";
                        break;
                    case "06":
                        Provincia = "Badajoz";
                        break;
                    case "07":
                    case "57":
                        Provincia = "Islas Baleares";
                        break;
                    case "08":
                    case "58":
                    case "59":
                    case "60":
                    case "61":
                    case "62":
                    case "63":
                    case "64":
                        Provincia = "Barcelona";
                        break;
                    case "09":
                        Provincia = "Burgos";
                        break;
                    case "10":
                        Provincia = "Cáceres";
                        break;
                    case "11":
                    case "72":
                        Provincia = "Cádiz";
                        break;
                    case "12":
                        Provincia = "Castellón";
                        break;
                    case "13":
                        Provincia = "Ciudad Real";
                        break;
                    case "14":
                    case "56":
                        Provincia = "Córdoba";
                        break;
                    case "15":
                    case "70":
                        Provincia = "A Coruña";
                        break;
                    case "16":
                        Provincia = "Cuenca";
                        break;
                    case "17":
                    case "55":
                        Provincia = "Girona";
                        break;
                    case "18":
                        Provincia = "Granada";
                        break;
                    case "19":
                        Provincia = "Guadalajara";
                        break;
                    case "20":
                    case "71":
                        Provincia = "Guipúzcoa";
                        break;
                    case "21":
                        Provincia = "Huelva";
                        break;
                    case "22":
                        Provincia = "Huesca";
                        break;
                    case "23":
                        Provincia = "Jaén";
                        break;
                    case "24":
                        Provincia = "León";
                        break;
                    case "25":
                        Provincia = "Lleida";
                        break;
                    case "26":
                        Provincia = "La Rioja";
                        break;
                    case "27":
                        Provincia = "Lugo";
                        break;
                    case "28":
                    case "78":
                    case "79":
                    case "80":
                    case "81":
                    case "82":
                    case "83":
                    case "84":
                        Provincia = "Madrid";
                        break;
                    case "29":
                    case "92":
                    case "93":
                        Provincia = "Málaga";
                        break;
                    case "30":
                    case "73":
                        Provincia = "Murcia";
                        break;
                    case "31":
                        Provincia = "Navarra";
                        break;
                    case "32":
                        Provincia = "Ourense";
                        break;
                    case "33":
                    case "74":
                        Provincia = "Oviedo";
                        break;
                    case "34":
                        Provincia = "Palencia";
                        break;
                    case "35":
                    case "76":
                        Provincia = "Las Palmas";
                        break;
                    case "36":
                    case "94":
                        Provincia = "Pontevedra";
                        break;
                    case "37":
                        Provincia = "Salamanca";
                        break;
                    case "38":
                    case "75":
                        Provincia = "Santa Cruz de Tenerife";
                        break;
                    case "39":
                        Provincia = "Cantabria";
                        break;
                    case "40":
                        Provincia = "Segovia";
                        break;
                    case "41":
                    case "91":
                        Provincia = "Sevilla";
                        break;
                    case "42":
                        Provincia = "Soria";
                        break;
                    case "43":
                    case "77":
                        Provincia = "Tarragona";
                        break;
                    case "44":
                        Provincia = "Teruel";
                        break;
                    case "45":
                        Provincia = "Toledo";
                        break;
                    case "46":
                    case "96":
                    case "97":
                    case "98":
                        Provincia = "Valencia";
                        break;
                    case "47":
                        Provincia = "Valladolid";
                        break;
                    case "48":
                    case "95":
                        Provincia = "Vizcaya";
                        break;
                    case "49":
                        Provincia = "Zamora";
                        break;
                    case "50":
                    case "99":
                        Provincia = "Zaragoza";
                        break;
                    case "51":
                        Provincia = "Ceuta";
                        break;
                    case "52":
                        Provincia = "Melilla ";
                        break;
                    default:
                        Provincia = "Provincia Desconocida";
                        break;
                }

                switch (value.Substring(0, 1))
                {
                    case "C":
                    case "K":
                    case "L":
                    case "M":
                    case "N":
                    case "P":
                    case "Q":
                    case "R":
                    case "S":
                    case "W":
                        Digito = letras[temp].ToString();
                        break;
                    case "A":
                    case "B":
                    case "D":
                    case "E":
                    case "F":
                    case "G":
                    case "H":
                    case "J":
                    case "U":
                    case "V":
                        Digito = (temp % 10).ToString();
                        break;
                    default:
                        correcto = false;
                        Message = "Letra incorrecta, debe ser una de estas (A,B,C,D,E,F,G,H,J,K,L,M,N,P,Q,R,S,U,V,W).";
                        break;
                }
            }

            if (correcto)
            {
                if (cif.Substring(8, 1) != Digito)
                {
                    correcto = false;
                    Message = "Dígito de control incorrecto.";
                }
            }

            if (correcto)
            {
                Message = "CIF : <b>" + cif.Substring(0, 8) + "-" + Digito + "</b><br>Tipo: <b> " + Tipo + "</b><br>Con sede (si es anterior a 2009) en: <b>" + Provincia + "</b><br>N&ordm; Secuencial: <b>" + value.Substring(3, 5) + "</b><br>D&iacute;gito de Control: <b>" + Digito + "</b>";
            }

            return correcto;
        }
    }
}
