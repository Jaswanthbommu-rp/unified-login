
INSERT INTO AuditDB.logging.Organization (Name, GBOrganizationId)
select a.name, a.partyid from [identity].enterprise.organization a
    left join auditdb.logging.organization  b
    on a.partyid = b.gborganizationid
    where b.name is null 