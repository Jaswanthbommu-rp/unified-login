CREATE TABLE [UserAudit].[ReportParameter] (
    [ReportParameterId]   INT           IDENTITY (1, 1) NOT NULL,
    [ReportParameterName] VARCHAR (100) NULL,
    PRIMARY KEY CLUSTERED ([ReportParameterId] ASC)
);

