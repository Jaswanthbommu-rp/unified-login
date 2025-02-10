CREATE PROCEDURE [UserAudit].[SaveUserNotifications]
 @UserNotifications [UserAudit].[UserNotificationInfo] READONLY	
AS
BEGIN
	Insert into [UserAudit].[UserNotification](AuditUserId, PersonaId, ProductName, CategoryName, Feed, Banner,
				Email, EmailSummary, SMS)
	SELECT AuditUserId, PersonaId, ProductName, CategoryName, Feed, Banner, Email, EmailSummary, SMS FROM @UserNotifications
END
