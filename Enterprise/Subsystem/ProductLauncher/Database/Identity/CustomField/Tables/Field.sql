CREATE TABLE [CustomField].[Field] (
    [FieldId]          BIGINT         IDENTITY (1, 1) NOT NULL,
    [OrganizationId]   BIGINT         NOT NULL,
    [DependentFieldId] BIGINT         NULL,
    [Enabled]          BIT            NOT NULL,
    [Name]             NVARCHAR (100) NOT NULL,
    [Description]      NVARCHAR (500) NULL,
    [FieldTypeId]      TINYINT        NOT NULL,
    [Required]         BIT            NULL,
    [ReadOnly]         BIT            NULL,
    [DefaultValue]     NVARCHAR (MAX) NULL,
    [SyncField]        NVARCHAR (150) NULL,
    [Sequence]         SMALLINT       NOT NULL,
    [HelpText]         VARCHAR (MAX)  NULL,
    [MinCharLength]    INT            NULL,
    [MaxCharLength]    INT            NULL,
    [CreatedDate]      DATETIME       NOT NULL,
    [CreatedBy]        BIGINT         NOT NULL,
    CONSTRAINT [PK_CustomField_CustomFieldId] PRIMARY KEY CLUSTERED ([FieldId] ASC) WITH (FILLFACTOR = 80),
    CONSTRAINT [FK_Field_FieldType] FOREIGN KEY ([FieldTypeId]) REFERENCES [CustomField].[FieldType] ([FieldTypeId])
);


GO


