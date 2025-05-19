CREATE PROCEDURE Enterprise.BulkProductRemove
    @EditorUserPersonaId BIGINT,
    @SubjectUserPersonaIds [Enterprise].[SyncPersonaList] READONLY,
    @ProductIds [Enterprise].[ProductIdType] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert bulk product update records
    INSERT INTO Enterprise.BulkProductUpdate (EditorUserPersonaId, SubjectUserPersonaId, ProductId, CreatedDate)
    SELECT 
        @EditorUserPersonaId, 
        su.PersonaId AS SubjectUserPersonaId, 
        p.ProductId AS ProductId, 
        GETDATE()
    FROM 
        @SubjectUserPersonaIds su
    CROSS JOIN 
        @ProductIds p;
END;
