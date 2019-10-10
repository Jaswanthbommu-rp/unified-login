CREATE TABLE [Auth].[SecurityQuestion]
(
[SecurityQuestionId] [int] NOT NULL,
[Question] [nvarchar] (500) NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF_SecurityQuestion_IsActive] DEFAULT ((1))
)
GO
ALTER TABLE [Auth].[SecurityQuestion] ADD CONSTRAINT [PK_SecurityQuestion] PRIMARY KEY CLUSTERED  ([SecurityQuestionId])
GO
