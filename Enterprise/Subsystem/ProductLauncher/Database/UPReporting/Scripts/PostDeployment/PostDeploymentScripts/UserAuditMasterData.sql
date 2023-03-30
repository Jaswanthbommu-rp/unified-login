IF NOT EXISTS (Select 1 From [UserAudit].[ReportParameter] Where ReportParameterName = 'ProductName')
BEGIN
	INSERT [UserAudit].[ReportParameter] ([ReportParameterName]) 
	VALUES (N'ProductName')
END

IF NOT EXISTS (Select 1 From [UserAudit].[ReportParameter] Where ReportParameterName = 'UserType')
BEGIN
	INSERT [UserAudit].[ReportParameter] ([ReportParameterName]) 
	VALUES (N'UserType')
END

IF NOT EXISTS (Select 1 From [UserAudit].[ReportParameter] Where ReportParameterName = 'UserStatus')
BEGIN
	INSERT [UserAudit].[ReportParameter] ([ReportParameterName]) 
	VALUES (N'UserStatus')
END

IF NOT EXISTS (Select 1 From [UserAudit].[ReportParameter] Where ReportParameterName = 'ReportFormat')
BEGIN
	INSERT [UserAudit].[ReportParameter] ([ReportParameterName]) 
	VALUES (N'ReportFormat')
END