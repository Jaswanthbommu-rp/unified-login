CREATE TABLE [Enterprise].[GroupMember](
	[GroupMemberId] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[PersonaId] [bigint] NOT NULL,
	[FromDate] [datetime] NULL,
	[ThruDate] [datetime] NULL,
 CONSTRAINT [FK_GroupMember_Group] FOREIGN KEY([GroupId]) REFERENCES [Enterprise].[Group] ([GroupId]) ON UPDATE CASCADE ON DELETE CASCADE, 
 CONSTRAINT [FK_GroupMember_Persona] FOREIGN KEY([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]), 
 CONSTRAINT [PK_GroupMember] PRIMARY KEY CLUSTERED 
(
	[GroupMemberId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

