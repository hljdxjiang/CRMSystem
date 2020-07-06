using System;
using System.IO;
using JHC.ToolKit.Helper;
namespace JHC.ToolKit.Config
{
    public class AppSercretConfig
    {
        private static string Audience_Secret = Appsettings.Instance.app(new string[] { "JwtSettings", "SecretKey" });
        private static string Audience_Secret_File = Appsettings.Instance.app(new string[] { "JwtSettings", "SecretFile" });

        private static string RefAudience_Secret = Appsettings.Instance.app(new string[] { "JwtSettings", "RefSecretKey" });
        private static string RefAudience_Secret_File = Appsettings.Instance.app(new string[] { "JwtSettings", "RefSecretFile" });


        public static string Audience_Ref_Secret_String = InitAudience_RefSecret();
        public static string Audience_Secret_String => InitAudience_Secret();


        private static string InitAudience_Secret()
        {
            var securityString = DifDBConnOfSecurity(Audience_Secret_File);
            if (!string.IsNullOrEmpty(Audience_Secret_File) && !string.IsNullOrEmpty(securityString))
            {
                return securityString;
            }
            else
            {
                return Audience_Secret;
            }

        }

        private static string InitAudience_RefSecret()
        {
            var securityString = DifDBConnOfSecurity(RefAudience_Secret_File);
            if (!string.IsNullOrEmpty(RefAudience_Secret_File) && !string.IsNullOrEmpty(securityString))
            {
                return securityString;
            }
            else
            {
                return Audience_Secret;
            }

        }

        private static string DifDBConnOfSecurity(params string[] conn)
        {
            //foreach (var item in conn)
            //{
            //    try
            //    {
            //        if (File.Exists(item))
            //        {
            //            return File.ReadAllText(item).Trim();
            //        }
            //    }
            //    catch (System.Exception) { }
            //}

            return "";
        }

    }
}
