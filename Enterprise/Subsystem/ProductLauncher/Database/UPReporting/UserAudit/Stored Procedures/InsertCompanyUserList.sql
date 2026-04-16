CREATE PROCEDURE [UserAudit].[InsertCompanyUserList]
    @RequestId    BIGINT,
    @JsonVariable NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [UserAudit].[User]
        (RequestId, PersonaId, OrganizationPartyId, FirstName, LastName,
         UserName, UserType, UserRelationship, LastLoginDate, [Status],
         OrganizationRealPageId)
    SELECT
        @RequestId,
        PersonaId,
        CompanyPartyId,
        FirstName,
        LastName,
        LoginName,
        UserType,
        UserRelationship,
        LastLoginDate,
        [Status],
        OrganizationRealPageId
    FROM OPENJSON(@JsonVariable, N'$')
    WITH (
        PersonaId              BIGINT        N'$.PersonaId',
        CompanyPartyId         BIGINT        N'$.CompanyPartyId',
        FirstName              VARCHAR(100)  N'$.FirstName',
        LastName               VARCHAR(100)  N'$.LastName',
        LoginName              VARCHAR(100)  N'$.LoginName',
        UserType               VARCHAR(50)   N'$.UserType',
        UserRelationship       VARCHAR(50)   N'$.UserRelationship',
        LastLoginDate          DATETIME2     N'$.LastLoginDate',
        [Status]               VARCHAR(50)   N'$.Status',
        OrganizationRealPageId VARCHAR(50)   N'$.OrganizationRealPageId'
    );
END