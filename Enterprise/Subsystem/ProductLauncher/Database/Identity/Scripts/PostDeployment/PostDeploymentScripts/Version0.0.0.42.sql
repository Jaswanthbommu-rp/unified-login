
--LPC
SET @PartyId = NULL;
SELECT @PartyId = PartyId
FROM Enterprise.organization
WHERE Name = 'LPC RESIDENT SERVICES LLC';
INSERT INTO Enterprise.PartyRole
(PartyId,
 RoleTypeId
)
       SELECT Organization.PartyId,
              401
       FROM Enterprise.Organization
            LEFT OUTER JOIN Enterprise.PartyRole Source ON Source.PartyId = Organization.PartyId
                                                           AND Source.RoleTypeId = 401
       WHERE Organization.PartyId = @PartyId
             AND PartyRoleId IS NULL
       UNION
       SELECT Organization.PartyId,
              402
       FROM Enterprise.Organization
            LEFT OUTER JOIN Enterprise.PartyRole Source ON Source.PartyId = Organization.PartyId
                                                           AND Source.RoleTypeId = 402
       WHERE Organization.PartyId = @PartyId
             AND PartyRoleId IS NULL;


--RealPage
SET @PartyId = NULL;
SELECT @PartyId = PartyId
FROM Enterprise.organization
WHERE Name = 'RealPage Employee';
INSERT INTO Enterprise.PartyRole
(PartyId,
 RoleTypeId
)
       SELECT Organization.PartyId,
              403
       FROM Enterprise.Organization
            LEFT OUTER JOIN Enterprise.PartyRole Source ON Source.PartyId = Organization.PartyId
                                                           AND Source.RoleTypeId = 403
       WHERE Organization.PartyId = @PartyId
             AND PartyRoleId IS NULL;

--Everyone Else
INSERT INTO Enterprise.PartyRole
(PartyId,
 RoleTypeId
)
       SELECT Organization.PartyId,
              401
       FROM Enterprise.Organization
            LEFT OUTER JOIN Enterprise.PartyRole Source ON Source.PartyId = Organization.PartyId
                                                           AND Source.RoleTypeId = 401
       WHERE PartyRoleId IS NULL
             AND Organization.Name NOT IN('RealPage Employee', 'LPC RESIDENT SERVICES LLC')
       UNION
       SELECT Organization.PartyId,
              402
       FROM Enterprise.Organization
            LEFT OUTER JOIN Enterprise.PartyRole Source ON Source.PartyId = Organization.PartyId
                                                           AND Source.RoleTypeId = 402
       WHERE PartyRoleId IS NULL
             AND Organization.Name NOT IN('RealPage Employee', 'LPC RESIDENT SERVICES LLC')
       UNION
       SELECT Organization.PartyId
              , 404
       FROM Enterprise.Organization
            LEFT OUTER JOIN Enterprise.PartyRole Source ON Source.PartyId = Organization.PartyId
                                                           AND Source.RoleTypeId = 404
       WHERE PartyRoleId IS NULL
             AND Organization.Name NOT IN('RealPage Employee', 'LPC RESIDENT SERVICES LLC');

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='43'
