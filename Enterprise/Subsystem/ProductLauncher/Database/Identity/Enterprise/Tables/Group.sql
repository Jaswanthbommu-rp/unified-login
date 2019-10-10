CREATE TABLE [Enterprise].[Group](
	[GroupId] [int] IDENTITY(1,1) NOT NULL,
	[OrganizationPartyId] [bigint] NULL,
	[Value] [nvarchar](50) NULL,
	[Description] [nvarchar](200) NULL,
	[FromDate] [datetime] NULL,
	[ThruDate] [datetime] NULL,
 CONSTRAINT [FK_Group_Organization] FOREIGN KEY([OrganizationPartyId]) REFERENCES [Enterprise].[Organization] ([PartyId]) ON UPDATE CASCADE ON DELETE CASCADE, 
 CONSTRAINT [PK_Group] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
