using System;
using System.Windows;

namespace TOTP.Helper
{
    public class Common
    {
        public static bool PackUriExists(string packUri)
        {
            try
            {
                var streamInfo = Application.GetResourceStream(new Uri(packUri));
                return streamInfo != null;
            }
            catch
            {
                return false;
            }
        }

    }
}
