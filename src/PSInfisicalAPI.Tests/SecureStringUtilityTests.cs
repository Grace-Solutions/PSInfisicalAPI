using System;
using System.Security;
using PSInfisicalAPI.Security;
using Xunit;

namespace PSInfisicalAPI.Tests
{
    public class SecureStringUtilityTests
    {
        [Fact]
        public void ToReadOnlySecureString_Returns_ReadOnly_Instance()
        {
            SecureString secure = SecureStringUtility.ToReadOnlySecureString("hello");
            Assert.True(secure.IsReadOnly());
            Assert.Equal(5, secure.Length);
        }

        [Fact]
        public void ToReadOnlySecureString_Handles_Null_And_Empty()
        {
            SecureString fromNull = SecureStringUtility.ToReadOnlySecureString(null);
            SecureString fromEmpty = SecureStringUtility.ToReadOnlySecureString(string.Empty);
            Assert.True(fromNull.IsReadOnly());
            Assert.True(fromEmpty.IsReadOnly());
            Assert.Equal(0, fromNull.Length);
            Assert.Equal(0, fromEmpty.Length);
        }

        [Fact]
        public void UsePlainText_Roundtrips_Value()
        {
            SecureString secure = SecureStringUtility.ToReadOnlySecureString("RoundTripValue");
            string captured = SecureStringUtility.UsePlainText(secure, plainText => plainText);
            Assert.Equal("RoundTripValue", captured);
        }

        [Fact]
        public void UsePlainText_Throws_For_Null_Action()
        {
            SecureString secure = SecureStringUtility.ToReadOnlySecureString("x");
            Assert.Throws<ArgumentNullException>(() => SecureStringUtility.UsePlainText<string>(secure, null));
        }
    }
}
