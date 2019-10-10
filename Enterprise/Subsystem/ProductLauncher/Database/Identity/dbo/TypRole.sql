CREATE TYPE [dbo].[TypRole] AS TABLE
([RoleId]    [INT] NULL,
 [RightID]   [INT] NULL,
 [IsDeleted] [BIT] NULL
                   DEFAULT((0))
);  