CREATE TABLE [Ident].[SMSAuthenticationCode](
	[UserId] [bigint] NOT NULL,
	[AuthenticationCode] [nvarchar](10) NOT NULL,
	[ExpirationTime] [datetime2](7) NULL
) ON [PRIMARY]
GO

ALTER TABLE [Ident].[SMSAuthenticationCode]  WITH CHECK ADD  CONSTRAINT [FK_SMSAuthCode_UserId] FOREIGN KEY([UserId])
REFERENCES [Ident].[UserLogin] ([UserId])
GO

ALTER TABLE [Ident].[SMSAuthenticationCode] CHECK CONSTRAINT [FK_SMSAuthCode_UserId]
GO
