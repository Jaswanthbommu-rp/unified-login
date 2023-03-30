CREATE TABLE [UserAudit].[Request] (
    [RequestId]   BIGINT        IDENTITY (1, 1) NOT NULL,
    [CreatedDate] DATETIME2 (7) DEFAULT (getutcdate()) NULL,
    [ReportKey]   VARCHAR (50)  NULL,
    [OrgPartyId]  BIGINT        NULL,
    [Status] INT DEFAULT 1
    PRIMARY KEY CLUSTERED ([RequestId] ASC)
);



