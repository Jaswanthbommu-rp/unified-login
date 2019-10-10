CREATE TYPE [dbo].[SortOrder] AS TABLE (
    [Name]      NVARCHAR (200) NULL,
    [Sortorder] NVARCHAR (200) DEFAULT ('ASC') NULL);

