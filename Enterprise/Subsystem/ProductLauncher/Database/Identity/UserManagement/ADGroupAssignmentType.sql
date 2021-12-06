CREATE TYPE [Security].[ADGroupAssignmentType] AS TABLE(
	[AdGroupId] [int] NULL,
	[ProductId] [int] NULL,
	[AssignmentOrder] [int] NULL,
	[CreatedBy] [nvarchar](100) NULL
)

