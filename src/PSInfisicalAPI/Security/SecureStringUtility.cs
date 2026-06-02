using System;
using System.Runtime.InteropServices;
using System.Security;

namespace PSInfisicalAPI.Security
{
    public static class SecureStringUtility
    {
        public static SecureString ToReadOnlySecureString(string value)
        {
            SecureString secureString = new SecureString();

            if (!string.IsNullOrEmpty(value))
            {
                foreach (char character in value)
                {
                    secureString.AppendChar(character);
                }
            }

            secureString.MakeReadOnly();

            return secureString;
        }

        public static SecureString EmptyReadOnly()
        {
            SecureString secureString = new SecureString();
            secureString.MakeReadOnly();
            return secureString;
        }

        public static T UsePlainText<T>(SecureString secureString, Func<string, T> action)
        {
            if (secureString == null)
            {
                throw new ArgumentNullException(nameof(secureString));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            IntPtr pointer = IntPtr.Zero;

            try
            {
                pointer = Marshal.SecureStringToBSTR(secureString);
                string plainText = Marshal.PtrToStringBSTR(pointer);
                return action(plainText);
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(pointer);
                }
            }
        }

        public static void UsePlainText(SecureString secureString, Action<string> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            UsePlainText<bool>(secureString, plainText =>
            {
                action(plainText);
                return true;
            });
        }
    }
}
