using System;
using System.Data.SqlClient;
using System.Reflection;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.Tests
{
    public class ManageProductOneSite_DuplicateKey_Tests
    {
        private static bool InvokeIsDuplicateKeyException(Exception ex)
        {
            // Look up the type via reflection so the test does not require a hard project reference
            // to all the dependencies of ManageProductOneSite (the helper is internal/static).
            var asm = Assembly.Load("RP.Enterprise.Subsystem.ProductLauncher.Component");
            var type = asm.GetType(
                "RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.ManageProductOneSite",
                throwOnError: true);
            var method = type.GetMethod(
                "IsDuplicateKeyException",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(method);
            return (bool)method.Invoke(null, new object[] { ex });
        }

        [Fact]
        public void IsDuplicateKeyException_Recognizes_UniqueIndexViolation_FromMessage()
        {
            // Simulates the production failure: AK_UsrPr_UsrLogin unique index 2601 violation
            // surfaced through a SOAP/remote call as a generic Exception with the SQL text in the message.
            var ex = new Exception(
                "Cannot insert duplicate key row in object 'dbo.USER_PROFILE' with unique index 'AK_UsrPr_UsrLogin'. The duplicate key value is (MAppelgrenHemke). Error 2601");

            var result = InvokeIsDuplicateKeyException(ex);

            Assert.True(result, "Expected duplicate-key safeguard to recognize SQL error 2601 / AK_UsrPr_UsrLogin");
        }

        [Fact]
        public void IsDuplicateKeyException_ReturnsFalse_ForUnrelatedException()
        {
            var ex = new Exception("Some other unrelated error");
            var result = InvokeIsDuplicateKeyException(ex);
            Assert.False(result);
        }
    }
}
