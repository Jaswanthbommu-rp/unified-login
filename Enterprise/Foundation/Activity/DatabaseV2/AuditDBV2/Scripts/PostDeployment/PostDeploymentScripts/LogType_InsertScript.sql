


--[Logging].[LogType] data
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Login Success' AND [LogcategoryTypeId] = '1') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '1','1','Login Success','Login Success' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Login failure' AND [LogcategoryTypeId] = '1') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '2','1','Login failure','Login failure' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Change password success' AND [LogcategoryTypeId] = '1') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '3','1','Change password success','Change password success' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Change password failure' AND [LogcategoryTypeId] = '1') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '4','1','Change password failure','Change password failure' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'User locked' AND [LogcategoryTypeId] = '1') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '5','1','User locked','User locked' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'User unlocked' AND [LogcategoryTypeId] = '1') 
BEGIN 
	INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '6','1','User unlocked','User unlocked' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Create user' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '7','2','Create user','Create user' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Update user' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '8','2','Update user','Update user' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Clone user' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '9','2','Clone user','Clone user' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Login enabled' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '10','2','Login enabled','Login enabled' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Login disabled' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '11','2','Login disabled','Login disabled' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'User locked' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '12','2','User locked','User locked' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'User unlocked' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '13','2','User unlocked','User unlocked' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Email sent' AND [LogcategoryTypeId] = '4') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '14','4','Email sent','Email sent' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Email resent' AND [LogcategoryTypeId] = '4') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '15','4','Email resent','Email resent' )
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Signout' AND [LogcategoryTypeId] = '1') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '16','1','Signout','Signout' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Product access' AND [LogcategoryTypeId] = '3') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '17','3','Product access','Product access' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Support Tool' AND [LogcategoryTypeId] = '2') 
BEGIN
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '18','2','Support Tool','' )
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Migration Tool' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '19','2','Migration Tool','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Refresh User Data' AND [LogcategoryTypeId] = '5') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '20','5','Refresh User Data','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Merge New User' AND [LogcategoryTypeId] = '5') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '21','5','Merge New User','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Merge Existing User' AND [LogcategoryTypeId] = '5') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '22','5','Merge Existing User','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Unmerge User' AND [LogcategoryTypeId] = '5') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '23','5','Unmerge User','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Edit Staged User' AND [LogcategoryTypeId] = '5') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '24','5','Edit Staged User','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Create Unified Login' AND [LogcategoryTypeId] = '5') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '25','5','Create Unified Login','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Update Product User' AND [LogcategoryTypeId] = '5') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '26','5','Update Product User','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Product User Added' AND [LogcategoryTypeId] = '5') 
BEGIN
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '27','5','Product User Added','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Updated Setting' AND [LogcategoryTypeId] = '6') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '28','6','Updated Setting','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Change security questions success' AND [LogcategoryTypeId] = '1') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '29','1','Change security questions success','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Change security questions failure' AND [LogcategoryTypeId] = '1') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '30','1','Change security questions failure','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Hide Product User' AND [LogcategoryTypeId] = '5') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '35','5','Hide Product User','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Unhide Product User' AND [LogcategoryTypeId] = '5') 
BEGIN
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '36','5','Unhide Product User','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Deactivate User' AND [LogcategoryTypeId] = '5') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '37','5','Deactivate User','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'User Expired' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '40','2','User Expired','User Expired' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Enabled setting' AND [LogcategoryTypeId] = '6') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '42','6','Enabled setting','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Disabled setting' AND [LogcategoryTypeId] = '6') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '43','6','Disabled setting','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Change Company' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '47','2','Change Company','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Answered Question(s)' AND [LogcategoryTypeId] = '7') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '48','7','Answered Question(s)','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Submitted Questionnaire' AND [LogcategoryTypeId] = '7') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '49','7','Submitted Questionnaire','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Created Template' AND [LogcategoryTypeId] = '7') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '50','7','Created Template','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Template' AND [LogcategoryTypeId] = '7') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '51','7','Modified Template','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Applied Template' AND [LogcategoryTypeId] = '7') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '52','7','Applied Template','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Removed Template' AND [LogcategoryTypeId] = '7') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '53','7','Removed Template','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Email sent' AND [LogcategoryTypeId] = '2') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '54','2','Email sent','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Setting' AND [LogcategoryTypeId] = '8') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '55','8','Modified Setting','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Selected Button' AND [LogcategoryTypeId] = '8') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '56','8','Selected Button','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Added Table Row' AND [LogcategoryTypeId] = '8') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '57','8','Added Table Row','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Deleted Table Row' AND [LogcategoryTypeId] = '8') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '58','8','Deleted Table Row','' )
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Table Row' AND [LogcategoryTypeId] = '8') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '59','8','Modified Table Row','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Selected Table Actions' AND [LogcategoryTypeId] = '8') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '60','8','Selected Table Actions','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Table' AND [LogcategoryTypeId] = '8') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '61','8','Modified Table','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Setting' AND [LogcategoryTypeId] = '9') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '62','9','Modified Setting','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Added Template' AND [LogcategoryTypeId] = '10') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '63','10','Added Template','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Applied Template' AND [LogcategoryTypeId] = '10') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '64','10','Applied Template','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Deleted Template' AND [LogcategoryTypeId] = '10') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '65','10','Deleted Template','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Template' AND [LogcategoryTypeId] = '10') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '66','10','Modified Template','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Table' AND [LogcategoryTypeId] = '10') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '67','10','Modified Table','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Added Table Row' AND [LogcategoryTypeId] = '10') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '68','10','Added Table Row','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Deleted Table Row' AND [LogcategoryTypeId] = '10') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '69','10','Deleted Table Row','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Table Row' AND [LogcategoryTypeId] = '10')
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '70','10','Modified Table Row','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Selected Table Actions' AND [LogcategoryTypeId] = '10') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '71','10','Selected Table Actions','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Added Column' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '72','11','Added Column','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Added Page' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '73','11','Added Page','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Added Section' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '74','11','Added Section','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Added Setting' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '75','11','Added Setting','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Added Tile' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '76','11','Added Tile','' )
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Deleted Column' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '77','11','Deleted Column','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Deleted Page' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '78','11','Deleted Page','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Deleted Section' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '79','11','Deleted Section','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Deleted Setting' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '80','11','Deleted Setting','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Deleted Tile' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '81','11','Deleted Tile','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Column' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '82','11','Modified Column','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Page' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '83','11','Modified Page','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Section' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '84','11','Modified Section','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Setting' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '85','11','Modified Setting','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Modified Tile' AND [LogcategoryTypeId] = '11') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '86','11','Modified Tile','' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Company Create' AND [LogcategoryTypeId] = '12') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '87','12','Company Create','Company Create' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Company Update' AND [LogcategoryTypeId] = '12') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '88','12','Company Update','Company Update' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Company Product Update' AND [LogcategoryTypeId] = '12')
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '89','12','Company Product Update','Company Product Update' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Property Create' AND [LogcategoryTypeId] = '12') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '90','12','Property Create','Property Create' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Property Update' AND [LogcategoryTypeId] = '12') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '91','12','Property Update','Property Update' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Property Delete' AND [LogcategoryTypeId] = '12') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '92','12','Property Delete','Property Delete' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Provisioning Company Create' AND [LogcategoryTypeId] = '12') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '93','12','Provisioning Company Create','Provisioning Company Create' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Provisioning Property Create' AND [LogcategoryTypeId] = '12') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '94','12','Provisioning Property Create','Provisioning Property Create' ) 
END
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Provisioning Company Product Update' AND [LogcategoryTypeId] = '12') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '95','12','Provisioning Company Product Update','Provisioning Company Product Update' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Product Enablement' AND [LogcategoryTypeId] = '12') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '96','12','Product Enablement','Product Enablement' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Product Disablement' AND [LogcategoryTypeId] = '12') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '97','12','Product Disablement','Product Disablement' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'Reset Password Email' AND [LogcategoryTypeId] = '4') 
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '98','4','Reset Password Email','' ) 
END 
GO
IF NOT EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE [Name] = 'User requested new activation link' AND [LogcategoryTypeId] = '4')
BEGIN 
INSERT [Logging].[LogType] (LogTypeId, LogCategoryTypeId, [Name], [Description]) VALUES ( '99','4','User requested new activation link','' ) 
END 
GO