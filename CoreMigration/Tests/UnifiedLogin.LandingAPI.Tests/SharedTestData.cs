namespace UnifiedLogin.LandingAPI.Tests
{
    /// <summary>
    /// Shared test data and helper methods for test classes.
    /// This class provides common test data that can be reused across multiple test classes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SharedTestData
    {
        /// <summary>
        /// Gets a sample collection of test users for testing purposes.
        /// </summary>
        /// <returns>A dictionary of test user data keyed by user identifier.</returns>
        public static Dictionary<string, TestUser> GetTestUsers()
        {
            return new Dictionary<string, TestUser>
            {
                {
                    "user1@test.com",
                    new TestUser
                    {
                        Email = "user1@test.com",
                        FirstName = "Test",
                        LastName = "User",
                        UserId = 1,
                        IsActive = true
                    }
                },
                {
                    "user2@test.com",
                    new TestUser
                    {
                        Email = "user2@test.com",
                        FirstName = "Sample",
                        LastName = "Person",
                        UserId = 2,
                        IsActive = true
                    }
                }
            };
        }

        /// <summary>
        /// Gets sample test configuration data.
        /// </summary>
        /// <returns>A dictionary of configuration key-value pairs.</returns>
        public static Dictionary<string, string> GetTestConfiguration()
        {
            return new Dictionary<string, string>
            {
                { "TestKey1", "TestValue1" },
                { "TestKey2", "TestValue2" },
                { "ApiBaseUrl", "https://test.api.com" }
            };
        }
    }

    /// <summary>
    /// Sample test user data model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TestUser
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserId { get; set; }
        public bool IsActive { get; set; }
    }
}
