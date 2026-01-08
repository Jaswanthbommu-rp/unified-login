/*
DECLARE @UserIdList [Enterprise].[BigIntListType];

INSERT INTO @UserIdList VALUES (7090), (7089);

EXEC [Ident].[UpdateUsersIDP]
    @OrganizationPartyId = 350,
    @UserIds = @UserIdList,
    @IsEnabled = 0;
*/

CREATE PROCEDURE [Ident].[UpdateUsersIDP]      
(
    @OrganizationPartyId BIGINT,
    @UserIds [Enterprise].[BigIntListType] READONLY,
    @IsEnabled BIT
)      
AS      
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY          
        BEGIN TRAN;
        
        -- Variable declarations
        DECLARE @IDPValue INT;
        DECLARE @OldIDPValue INT;
        DECLARE @UpdatedIDPCount INT = 0;
        DECLARE @UpdatedStatusCount INT = 0;
        
        -- Status Type IDs (verified from StatusType table)
        DECLARE @ActiveStatusId INT = 1;        -- Active
        DECLARE @PendingStatusId INT = 2;       -- Pending
        DECLARE @LockedStatusId INT = 3;        -- Locked
        DECLARE @ExpiredStatusId INT = 23;      -- Expired
        DECLARE @DisabledStatusID INT = 24;     -- Disabled (typo fixed)
        
        -- Determine the target IDP value
        IF (@IsEnabled = 1)
        BEGIN
            -- Find Company IDP in Enterprise.Organization table
            SELECT @IDPValue = O.IdentityProviderTypeId
            FROM [Enterprise].[Organization] O
            WHERE O.PartyId = @OrganizationPartyId;
        END
        ELSE
        BEGIN
            -- Get current and alternative IDP
            SELECT @OldIDPValue = O.IdentityProviderTypeId
            FROM [Enterprise].[Organization] O
            WHERE O.PartyId = @OrganizationPartyId;
            
            SELECT @IDPValue = ipt.IdentityProviderTypeId 
            FROM [Enterprise].[PartyContactMechanism] pcm
            INNER JOIN [Ident].[IdentityProviderType] ipt 
                ON ipt.ContactMechanismId = pcm.ContactMechanismId
            WHERE pcm.PartyId = @OrganizationPartyId 
                AND ipt.IdentityProviderTypeId <> @OldIDPValue;
        END
        
        -- Validate that we found a valid IDP value
        IF @IDPValue IS NULL
        BEGIN
            RAISERROR('Unable to determine Identity Provider Type ID for Organization %d', 16, 1, @OrganizationPartyId);
            RETURN;
        END
        
        -- Create temp table for eligible users
        DROP TABLE IF EXISTS #FinalUsers;
        
        -- Define CTE for eligible users to avoid duplicate JOIN logic
        WITH EligibleUsers AS (
            SELECT DISTINCT UL.UserId
            FROM [Ident].[UserLogin] UL
            INNER JOIN [Ident].[UserLoginPersona] ULP 
                ON UL.UserId = ULP.UserLoginId
            INNER JOIN [Person].[Persona] P 
                ON P.UserLoginPersonaId = ULP.UserLoginPersonaId
            INNER JOIN [Enterprise].[PartyRelationship] PR 
                ON PR.PartyIdFrom = UL.PersonPartyId 
                AND PR.PartyIdTo = ULP.OrganizationPartyId
            INNER JOIN @UserIds UIDS 
                ON UIDS.Id = UL.UserId
            WHERE ULP.OrganizationPartyId = @OrganizationPartyId
                AND ULP.PrimaryOrganization = 1
                AND UL.IdentityProviderTypeId <> @IDPValue
                AND PR.RoleTypeIdFrom <> 405
                AND PR.RoleTypeIdTo = 205
                AND PR.ThruDate IS NULL
        )
        SELECT UserId 
        INTO #FinalUsers
        FROM EligibleUsers;
        
        -- Update IDP for eligible users
        UPDATE UL 
        SET UL.IdentityProviderTypeId = @IDPValue
        FROM [Ident].[UserLogin] UL
        INNER JOIN #FinalUsers FU ON UL.UserId = FU.UserId;
        
        SET @UpdatedIDPCount = @@ROWCOUNT;
        
        -- Update user status: Pending/Expired/Locked/Disabled -> Active
        -- When IDP toggle is changed (enabled or disabled),
        -- users with these statuses should become Active
        UPDATE ULP
        SET ULP.StatusTypeId = @ActiveStatusId,
            ULP.StatusThruDate = NULL
        FROM [Ident].[UserLoginPersona] ULP
        INNER JOIN [Ident].[UserLogin] UL 
            ON UL.UserId = ULP.UserLoginId
        INNER JOIN @UserIds UIDS 
            ON UIDS.Id = UL.UserId
        WHERE ULP.OrganizationPartyId = @OrganizationPartyId
            AND ULP.PrimaryOrganization = 1
            AND ULP.StatusTypeId IN (@PendingStatusId, @ExpiredStatusId, @LockedStatusId, @DisabledStatusID);
        
        SET @UpdatedStatusCount = @@ROWCOUNT;
        
        -- Return results with row counts
        SELECT 
            UserId
        FROM #FinalUsers
        
        -- Clean up temp table
        DROP TABLE IF EXISTS #FinalUsers;
        
        COMMIT;
        
    END TRY
    BEGIN CATCH
        -- Rollback on error
        IF @@TRANCOUNT > 0
            ROLLBACK;
        
        -- Log error
        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
        
        -- Re-throw error for caller
        THROW;
        
    END CATCH;
END
GO