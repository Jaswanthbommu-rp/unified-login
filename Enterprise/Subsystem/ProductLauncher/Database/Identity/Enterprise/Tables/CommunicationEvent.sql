CREATE TABLE [Enterprise].[CommunicationEvent](
	[CommunicationEventID] [bigint] IDENTITY(1,1) NOT NULL,
	[StatusTypeID] [int] NOT NULL,
	[PartyContactMechanismIdFrom] [bigint] NOT NULL,
	[PartyContactMechanismIdTo] [bigint] NOT NULL,
	[Started] [datetime] NOT NULL,
	[Ended] [datetime] NULL,
	[Note] [nvarchar](1000) NULL,
    CONSTRAINT [PK_CommunicationEvent] PRIMARY KEY CLUSTERED ([CommunicationEventID] ASC),
    CONSTRAINT [FK_CommunicationEvent_PartyContactMechanism] FOREIGN KEY([PartyContactMechanismIdFrom])REFERENCES [Enterprise].[PartyContactMechanism] ([PartyContactMechanismId]),
    CONSTRAINT [FK_CommunicationEvent_PartyContactMechanism1]  FOREIGN KEY([PartyContactMechanismIdTo])REFERENCES [Enterprise].[PartyContactMechanism] ([PartyContactMechanismId]),
    CONSTRAINT [FK_CommunicationEvent_StatusType] FOREIGN KEY([StatusTypeID]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId]) ON UPDATE  CASCADE 
);
GO
CREATE INDEX [IX_CommunicationEvent_PartyContactMechanismIdFrom_PartyContactMechanismIdTo] ON [Enterprise].[CommunicationEvent] ([PartyContactMechanismIdFrom], [PartyContactMechanismIdTo])
GO

CREATE INDEX [IX_CommunicationEvent_PartyContactMechanismIdTo]
ON [Enterprise].[CommunicationEvent]
( [PartyContactMechanismIdTo]
) 
INCLUDE( [CommunicationEventID] );

