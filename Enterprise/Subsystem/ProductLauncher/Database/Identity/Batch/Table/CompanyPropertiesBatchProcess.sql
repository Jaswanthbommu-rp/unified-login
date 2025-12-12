CREATE TABLE [Batch].[CompanyPropertiesBatchProcess](
    [CompanyBatchJobId] [bigint] IDENTITY(1,1) NOT NULL,
    [CompanyInstanceSourceId] [uniqueidentifier],
    [CreateUserPersonaId] [bigint],
    [BatchProcessTypeId] [int],
    [IsActive] [bit],
    [StatusTypeId] [int],
    [CreatedDateTime] [datetime] NULL,
    [LastRunDateTime] [datetime] NULL,
    [CreatedBy] [bigint] NULL,
    [ErrorMessage] varchar(MAX) NULL,
    CONSTRAINT [PK_CompanyBatchJobId] PRIMARY KEY CLUSTERED ([CompanyBatchJobId] ASC),
    CONSTRAINT [FK_CompanyPropertiesBatchProcess_CreateUserPersonaId] FOREIGN KEY ([CreateUserPersonaId]) REFERENCES [Person].[Persona] ([PersonaId])
) ON [PRIMARY]
GO