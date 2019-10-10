IF OBJECT_ID('tempdb..#HoldOrgs') IS NULL
BEGIN
CREATE TABLE #HoldOrgs
(RowNumber           INT IDENTITY(1, 1),
 OrganizationPartyID INT,
 PStatus             BIT DEFAULT 0
)
END

IF OBJECT_ID('tempdb..#HoldPersona') IS NULL
BEGIN
CREATE TABLE #HoldPersona
(PersonaId           BIGINT,
 PartyRoleID INT,
 PStatus             BIT DEFAULT 0
)
END