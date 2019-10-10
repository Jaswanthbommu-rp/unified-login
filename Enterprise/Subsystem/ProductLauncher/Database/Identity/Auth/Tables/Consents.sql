CREATE TABLE [Auth].[Consents] (
    [SubjectCode]  NVARCHAR (200)  NOT NULL,
    [ClientCode] NVARCHAR (200)  NOT NULL,
    [Scopes]   NVARCHAR (2000) NOT NULL,
    CONSTRAINT [PK_Consents] PRIMARY KEY CLUSTERED ([SubjectCode] ASC, [ClientCode] ASC)
);

