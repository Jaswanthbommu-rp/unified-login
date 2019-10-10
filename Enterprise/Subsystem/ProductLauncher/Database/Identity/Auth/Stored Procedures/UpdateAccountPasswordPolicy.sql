CREATE PROCEDURE [Auth].[UpdateAccountPasswordPolicy] (
	@AllowUsersToChangePassword BIT = 0,
	@HardExpiry BIT = 0,
	@MaxPasswordAge INT = 0,
	@MinimumPasswordLength INT = 6,
	@PasswordReusePrevention INT = 0,
	@RequireLowercaseCharacters BIT = 0,
	@RequireNumbers BIT = 0,
	@RequireSymbols BIT = 0,
	@RequireUppercaseCharacters BIT = 0
)
AS
BEGIN

/*
Updates the password policy for the Account

NOTE: This action does not support partial updates. 
No Parameters are required, but if you do not specify a parameter, that parameter's value reverts to its default value.
See the Request Parameters section

Request Parameters:

AllowUsersToChangePassword
	Allows all RealPage users in your account to use the RealPage Management Console to change their own passwords.

	Default Value: False

	Type: Boolean

	Required: No

HardExpiry
	Prevents RealPage users from setting a new password after their password has expired.

	Default value: false

	Type: Boolean

	Required: No

MaxPasswordAge
	The number of days that a RealPage user password is valid. The default value of 0 means RealPage user passwords
	never expire.

	Default value: 0

	Type: Integer

	Valid Range: Minimum value of 1. Maximum value of 1095.

	Required: No

MinimumPasswordLength
	The minimum number of characters allowed in a RealPage user password.

	Default Value: 6

	Type: Integer

	Valid Range: Minimum value of 6. Maximum value of 128

	Required: No

PasswordReusePrevention
	Specifies the number of previous passwords that RealPage users are prevented from reusing. The default value of 0 means 
	RealPage users are not prevented from reusing previous passwords.

	Default value: false

	Type: Boolean

	Required: No

RequireLowercaseCharacters
	Specifies whether RealPage user passwords must contain at least one lowercase character from 
	the ISO basic Latin alphabet (a to z).

	Default value: false

	Type: Boolean

	Required: No

RequireNumbers

	Specifies whether RealPage user passwords must contain at least one numeric character (0 to 9).

	Default value: false

	Type: Boolean

	Required: No

RequireSymbols
	Specifies whether RealPage user passwords must contain at least one of the following non-alphanumeric characters:

	!@#$%^&*()_+-=[]{}|'

	Default value: false

	Type: Boolean

	Required: No

RequireUppercaseCharacters
	Specifies whether RealPage user passwords must contain at least one uppercase character from 
	the ISO basic Latin alphabet (A to Z).

	Default value: false

	Type: Boolean

	Required: No

*/

	SELECT 'Not implemented.'
END
GO
