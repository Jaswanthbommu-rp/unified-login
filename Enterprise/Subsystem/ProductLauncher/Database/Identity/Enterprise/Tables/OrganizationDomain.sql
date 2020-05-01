CREATE TABLE [Enterprise].[OrganizationDomain](
	[OrganizationDomainId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](20) NULL,
	[CreateDate] [datetime] NULL DEFAULT GETUTCDATE(),
 CONSTRAINT [PK_OrganizationDomainId] PRIMARY KEY CLUSTERED 
(
	[OrganizationDomainId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]