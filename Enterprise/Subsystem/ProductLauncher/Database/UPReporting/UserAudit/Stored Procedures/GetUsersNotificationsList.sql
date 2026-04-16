CREATE PROCEDURE [UserAudit].[GetUsersNotificationsList]
    @RequestId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UA.FirstName,
        UA.LastName,
        UA.UserName,
        UA.[Status] AS UserStatus,
        CASE WHEN UA.UserType = 'User'           THEN 'Regular User'
             WHEN UA.UserType = 'SuperUser'       THEN 'RealPage System Administrator'
             WHEN UA.UserType = 'User (No Email)' THEN 'Regular User (No Email)'
             ELSE UA.UserType
        END [UserType],
        UA.UserRelationship,
        UN.ProductName,
        UN.CategoryName,
        UN.Feed,
        UN.Banner,
        UN.Email,
        UN.EmailSummary,
        UN.SMS
    FROM [UserAudit].[User] UA
    INNER JOIN [UserAudit].[UserNotification] UN ON UA.AuditUserId = UN.AuditUserId
    WHERE UA.RequestId = @RequestId
    ORDER BY UA.LastName, UA.FirstName, UN.ProductName, UN.CategoryName;
END