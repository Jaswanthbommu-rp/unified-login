CREATE TABLE [Enterprise].[ProductRuleType](
	[ProductRuleTypeId]		INT IDENTITY(1,1) NOT NULL,
	[ProductRuleType]		[nvarchar](100) NOT NULL,
	[Description]			[nvarchar](512) NULL,	
 CONSTRAINT [PK_EnterpriseProductRuleTypeId] PRIMARY KEY CLUSTERED 
(
	[ProductRuleTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
