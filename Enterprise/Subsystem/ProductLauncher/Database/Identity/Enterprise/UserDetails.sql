CREATE TYPE [Enterprise].[UserDetails] AS TABLE(
	[FirstName] [NVARCHAR](200) NULL,
	[MiddleName] [NVARCHAR](100) NULL,
	[LastName] [NVARCHAR](200) NULL,
	[GBUserType] [NVARCHAR](200) NULL,
	[ThirdPartyIDP] [NVARCHAR](10) NULL,
	[LoginName] [NVARCHAR](200) NULL,
	[NotificationEmail] [NVARCHAR](200) NULL,
	[TemporaryPassword] [NVARCHAR](MAX) NULL,
	[UserEffectiveDate] [DATETIME] NULL,
	[UserExprirationDate] [DATETIME] NULL,
	[Phone] [NVARCHAR](20) NULL,
	[PhoneType] [NVARCHAR](100) NULL,
	[PreferredContactMethod] [NVARCHAR](200) NULL,
	[Title] [NVARCHAR](200) NULL,
	[CompanyJobTitle] [NVARCHAR](200) NULL
)