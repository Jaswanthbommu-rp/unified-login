

MERGE INTO [Logging].[LogType]  AS Target 
USING (VALUES 
			(1,1,N'Login Success',N'Login Success'),
			(2,1,N'Login failure',N'Login failure'),
			(3,1,N'Change password success',N'Change password success'),
			(4,1,N'Change password failure',N'Change password failure'),
			(5,1,N'User locked',N'User locked'),
			(6,1,N'User unlocked',N'User unlocked'),
			(7,2,N'Create user',N'Create user'),
			(8,2,N'Update user',N'Update user'),
			(9,2,N'Clone user',N'Clone user'),
			(10,2,N'Login enabled',N'Login enabled'),
			(11,2,N'Login disabled',N'Login disabled'),
			(12,2,N'User locked',N'User locked'),
			(13,2,N'User unlocked',N'User unlocked'),
			(14,4,N'Email sent',N'Email sent'),
			(15,4,N'Email resent',N'Email resent'),
			(16,1,N'Signout',N'Signout'),
			(17,3,N'Product access',N'Product access')
	)
AS Source (LogTypeId, LogCategoryTypeId, Name, Description) 
ON Target.LogTypeId = Source.LogTypeId 

--update matched rows 
WHEN MATCHED THEN 
	UPDATE SET LogCategoryTypeId=Source.LogCategoryTypeId, Name  = Source.Name, Description=Source.Description

--insert new rows 
WHEN NOT MATCHED BY TARGET THEN 
	INSERT (LogTypeId, LogCategoryTypeId, Name, Description)
	VALUES (LogTypeId, LogCategoryTypeId, Name, Description)
 
 --delete rows that are in the target but not the source 
WHEN NOT MATCHED BY SOURCE THEN
DELETE;
