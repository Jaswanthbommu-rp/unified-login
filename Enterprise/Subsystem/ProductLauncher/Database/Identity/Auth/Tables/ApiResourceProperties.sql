CREATE TABLE [Auth].[ApiResourceProperties](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ApiResourceId] [int] NOT NULL,
	[Key] [nvarchar](250) NOT NULL,
	[Value] [nvarchar](2000) NOT NULL,
 CONSTRAINT [PK_ApiResourceProperties] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Auth].[ApiResourceProperties]  WITH CHECK ADD  CONSTRAINT [FK_ApiResourceProperties_ApiResources_ApiResourceId] FOREIGN KEY ([ApiResourceId])
REFERENCES [Auth].[ApiResources] ([Id])
ON DELETE CASCADE
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ApiResourceProperties_ApiResourceId_Key]
    ON [Auth].[ApiResourceProperties]([ApiResourceId] ASC, [Key] ASC);
GO