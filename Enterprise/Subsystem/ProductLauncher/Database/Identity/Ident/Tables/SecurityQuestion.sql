CREATE TABLE [Ident].[SecurityQuestion] (
    [SecurityQuestionId] INT            NOT NULL,
    [Question]           NVARCHAR (500) NOT NULL,
    [IsActive]           BIT            CONSTRAINT [DF_SecurityQuestion_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_SecurityQuestion] PRIMARY KEY CLUSTERED ([SecurityQuestionId] ASC)
);

