CREATE TABLE [Security].[Route](
	[RouteId] INT IDENTITY(1,1) NOT NULL,
	[RouteValue] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL,
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Route] PRIMARY KEY CLUSTERED 
(
	[RouteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[Route] ADD  CONSTRAINT [DF_Route_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
