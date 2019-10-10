CREATE TABLE [CustomField].[FieldValue] (
    [FieldValueId]       BIGINT         IDENTITY (1, 1) NOT NULL,
    [UserLoginPersonaId] BIGINT         NOT NULL,
    [FieldId]            BIGINT         NOT NULL,
    [Value]              NVARCHAR (MAX) NULL,
    [CreatedDate]        DATETIME       NOT NULL,
    [CreatedBy]          BIGINT         NOT NULL,
    CONSTRAINT [PK_CustomFieldValue] PRIMARY KEY CLUSTERED ([FieldValueId] ASC) WITH (FILLFACTOR = 80),
    CONSTRAINT [FK_FieldValue_Field] FOREIGN KEY ([FieldId]) REFERENCES [CustomField].[Field] ([FieldId]),
    --CONSTRAINT [FK_FieldValue_UserLoginPersona] FOREIGN KEY ([UserLoginPersonaId]) REFERENCES [Ident].[UserLoginPersona] ([UserLoginPersonaId])
);


GO

CREATE NONCLUSTERED INDEX [IX_CustomField_FieldValue_FieldId_UserLoginId] ON [CustomField].[FieldValue]
(
	[FieldId] ASC,
	[UserLoginPersonaId] ASC
)
INCLUDE([Value]) ON [PRIMARY]
GO


