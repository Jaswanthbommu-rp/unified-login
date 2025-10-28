namespace UnifiedLogin.SharedObjects.Enum
{
	/// <summary>
	/// ActivityType
	/// </summary>
    public enum ActivityType
    {
		/// <summary>
		/// Login Activity
		/// </summary>
		Login = 1,

		/// <summary>
		/// Forgot Password
		/// </summary>
		ForgotPassword = 2,

		/// <summary>
		/// User has forced lock
		/// </summary>
		ForceLocked = 3,

		/// <summary>
		/// Brute Force Activity
		/// </summary>
		BruteForceAttack = 4,

		/// <summary>
		/// Question Attempts
		/// </summary>
		QuestionAttempts = 5,

		/// <summary>
		/// Verify Security Question Answers are correct
		/// </summary>
		VerifyAnswers = 6,

		/// <summary>
		/// Succesful log in activity
		/// </summary>
		LoginSuccess = 7,

		/// <summary>
		/// New User Registration, Expires in 7 days
		/// </summary>
		NewUserRegistration = 8,

		/// <summary>
		/// New User Registration Verification, Expires in 7 days
		/// </summary>
		NewUserRegistrationVerification = 9,

		/// <summary>
		/// Unlock User
		/// </summary>
		UnlockUser = 10
    }
}
