CREATE TABLE [CustomField].[FieldType] (
    [FieldTypeId] TINYINT        IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (100) NOT NULL,
    [Description] NVARCHAR (500) NULL,
    [CreatedDate] DATETIME       NOT NULL,
    [CreatedBy]   BIGINT         NOT NULL,
    CONSTRAINT [PK_CustomFieldType_CustomFieldTypeId] PRIMARY KEY CLUSTERED ([FieldTypeId] ASC) WITH (FILLFACTOR = 80)
);


GO


