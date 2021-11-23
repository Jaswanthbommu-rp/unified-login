CREATE TABLE [Security].[ADGroup](
	[ADGroupId] [int] IDENTITY(1,1) NOT NULL,
	[DisplayName] [nvarchar](255) NULL,
	[ActiveDirectoryId] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[CreatedBy] [nvarchar](25) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL DEFAULT(1)
 CONSTRAINT [PK_ADGroup] PRIMARY KEY CLUSTERED 
(
	[ADGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[ADGroup] ADD  CONSTRAINT [DF_ADGroup_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
