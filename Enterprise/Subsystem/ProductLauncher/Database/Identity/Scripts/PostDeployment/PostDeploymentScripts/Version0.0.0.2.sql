	PRINT 'Purge Old Data'
	DELETE [Enterprise].[CommunicationEventEmail]
	DELETE [Enterprise].[CESCommunicationEvent]
	DELETE [Enterprise].[CommunicationEvent]
	DELETE [Enterprise].[CommunicationEmailTemplate];
	DELETE [Enterprise].[CommunicationEventPurposeType];
	DELETE [Enterprise].[CommunicationEventAudienceType];

	PRINT 'Create Purpose Type'
	SET IDENTITY_INSERT Enterprise.CommunicationEventPurposeType ON
	INSERT INTO Enterprise.CommunicationEventPurposeType (CommunicationEventPurposeTypeId, [Description]) VALUES(1,N'New User Setup')
	INSERT INTO Enterprise.CommunicationEventPurposeType (CommunicationEventPurposeTypeId, [Description]) VALUES(2,N'Password Reset')
	INSERT INTO Enterprise.CommunicationEventPurposeType (CommunicationEventPurposeTypeId, [Description]) VALUES(3,N'Unlock Account')
	INSERT INTO Enterprise.CommunicationEventPurposeType (CommunicationEventPurposeTypeId, [Description]) VALUES(4,N'Account Recovery')
	SET IDENTITY_INSERT Enterprise.CommunicationEventPurposeType OFF
                           
	PRINT 'Create Audience Types'
	SET IDENTITY_INSERT [Enterprise].[CommunicationEventAudienceType] ON                          
	INSERT INTO Enterprise.CommunicationEventAudienceType ([CommunicationEventAudienceTypeId],[Description]) VALUES(1, 'Super User')
	INSERT INTO Enterprise.CommunicationEventAudienceType([CommunicationEventAudienceTypeId],[Description]) VALUES(2, 'Regular User')
	SET IDENTITY_INSERT [Enterprise].[CommunicationEventAudienceType] OFF

--Insert Email Templates
SET IDENTITY_INSERT [Enterprise].[CommunicationEmailTemplate] ON                          
INSERT INTO [Enterprise].[CommunicationEmailTemplate] ([CommunicationEmailTemplateID], [CommunicationEventAudienceTypeId], [CommunicationEventPurposeTypeId], [Subject], [Body]) 
	VALUES(1,1,1,
	'Welcome to RealPage!',
	'<!DOCTYPE html>
		<html dir="ltr" lang="en">
			<body>
				<table border="0" cellspacing="0" cellpadding="0" width="100%" style="table-layout:fixed; font-size:14px; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
					<tbody>
						<tr>
							<td>
								<center>
									<table border="0" cellspacing="0" cellpadding="0" width="600" style="margin:0 auto; max-width:535px; width:inherit;">
										<tbody>	
											<tr>
												<td align="left">
													<table border="0" cellspacing="0" cellpadding="0" width="100%">
														<tbody>
															<tr>
																<td width="100%">
																	<table border="0" cellspacing="0" cellpadding="0" width="100%">
																		<tbody>
																			<tr>
																				<td style="padding:18px 0 0 0;">
																					<tbody>
																						<tr>
																							<td style="padding:0 10px" align="center">
																								<a href="https://www.realpage.com" style="text-decoration:none;">
																									<img src="{IMAGES}/settings/email/email-notification/images/RealPage-Logo.png" alt="RealPage" width="270" height="80" style="margin: 0; border: 0; padding: 0; display: block;" />
																								</a>	
																							</td>
																						</tr>
																					</tbody>
																				</td>
																			</tr>
																		</tbody>
																	</table>
																	<table border="0" cellspacing="0" cellpadding="0" width="100%">
																		<tbody>
																			<tr>
																				<td width="100%" style="padding:24px 24px 32px 24px; border-style:none;">
																					<table border="0" cellspacing="0" cellpadding="0" width="100%">
																						<tbody>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; color:#757575;">
																													<span>Hello {FIRST NAME},</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px; color:#757575;">
																													<span>You have been provided access by {COMPANY NAME} to one or more RealPage products. RealPage provides a variety of tools to support all aspects of Property Management, Leasing, Marketing, and Operations for the multifamily industry.  Welcome aboard your RealPage journey.</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px; color:#757575;">
																													<span>We have designed the My RealPage user interface with you, the user in mind. Click the Complete Your Profile button below to set your password and complete your RealPage User Profile:</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td align="center" style="padding:18px 0 0 0">
																									<table border="0" cellpadding="0" cellspacing="0" align="center">
																										<tbody>
																								  			<tr>
																												<td>
																									      			<table width="100%" border="0" cellspacing="0" cellpadding="0">
																										  				<tr>
																									          				<td align="center" style="-webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px;" bgcolor="#42a5f6">
																															  <a href="{LINK}" style="font-size: 16px; font-family: Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; -webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px; padding: 9px 28px; border: 1px solid #42a5f6; display: inline-block;">Complete Your Profile</a>
																														  </td>
																										  				</tr>
																													</table>
																												</td>
																								  			</tr>
																						  				</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px; color:#757575;">
																													<span>Please click this button within 72 hours to complete your profile.  After that, your administrator will need to send you a new email.</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px; color:#757575;">
																													<span>After completing your User Profile, click the following link for next steps:</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px;">
																													<a href="{LANDING}" style="color:#42A5F5;">Getting Started With My RealPage: New End User</a>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px;">
																													<span style="color:#757575;">We thank you for your business and look forward to working with you. If you have trouble accessing your profile, please consult our</span>
																													<a href="https://www.realpage.com/company/office-locations" style="color:#42A5F5;">Support<a>
																													<span style="color:#757575;">page for information on getting assistance.</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																						</tbody>
																					</table>
																				</td>
																			</tr>
																		</tbody>
																	</table>
																</td>
															</tr>
														</tbody>
													</table>
												</td>
											</tr>
											<tr>
												<td align="left">
													<table border="0" cellspacing="0" cellpadding="0" width="100%" style="padding:0 24px;">
														<tbody>
															<tr>
																<td align="center" width="100%">
																	<table border="0" cellspacing="0" cellpadding="0" width="100%">
																		<tbody>
																			<tr>
																				<td align="center" width="100%" style="border-top:1px solid #757575; padding:16px 0;">
																					<a href="https://www.realpage.com/privacy-policy" style="color:#757575;text-decoration:none;"><span>Privacy Policy</span></a>
																					<span style="font-size:18px;color:#757575;">|</span>
																					<a href="https://www.realpage.com/company/office-locations" style="color:#757575;text-decoration:none;"><span>Contact Us</span></a>
																					<span style="font-size:18px;color:#757575;">|</span>
																					<span style="color:#757575;">&copy; 2017 RealPage, Inc.</span>
																				</td>
																			</tr>
																			<tr>
																				<td style="text-align:center;">
																					<a href="https://facebook.com/realpage" style="text-decoration:none;">
																						<img style="height:35px;" src="{IMAGES}/settings/email/email-notification/images/Facebook-Logo.png" alt="Facebook">
																					</a>
																					<a href="https://twitter.com/RealPage" style="text-decoration:none;">
																						<img style="height:35px;" src="{IMAGES}/settings/email/email-notification/images/Twitter-Logo.png" alt="Twitter">
																					</a>
																					<a href="https://www.linkedin.com/company/realpage" style="text-decoration:none;">
																						<img style="height:35px;" src="{IMAGES}/settings/email/email-notification/images/LinkedIn-Logo.png" alt="LinkedIn">
																					</a>
																				</td>
																			</tr>
																		</tbody>
																	</table>
																</td>
															</tr>
														</tbody>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</center>
							</td>
						</tr>
					</tbody>
				</table>
			</body>
		</html>')
INSERT INTO [Enterprise].[CommunicationEmailTemplate] ([CommunicationEmailTemplateID], [CommunicationEventAudienceTypeId], [CommunicationEventPurposeTypeId], [Subject], [Body]) 
	VALUES(2, 2, 1,
	'Welcome to RealPage!',
	'<!DOCTYPE html>
		<html dir="ltr" lang="en">
			<body>
				<table border="0" cellspacing="0" cellpadding="0" width="100%" style="table-layout:fixed; font-size:14px; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
					<tbody>
						<tr>
							<td>
								<center>
									<table border="0" cellspacing="0" cellpadding="0" width="600" style="margin:0 auto; max-width:535px; width:inherit;">
										<tbody>	
											<tr>
												<td align="left">
													<table border="0" cellspacing="0" cellpadding="0" width="100%">
														<tbody>
															<tr>
																<td width="100%">
																	<table border="0" cellspacing="0" cellpadding="0" width="100%">
																		<tbody>
																			<tr>
																				<td style="padding:18px 0 0 0;">
																					<tbody>
																						<tr>
																							<td style="padding:0 10px" align="center">
																								<a href="https://www.realpage.com" style="text-decoration:none;">
																									<img src="{IMAGES}/settings/email/email-notification/images/RealPage-Logo.png" alt="RealPage" width="270" height="80" style="margin: 0; border: 0; padding: 0; display: block;" />
																								</a>	
																							</td>
																						</tr>
																					</tbody>
																				</td>
																			</tr>
																		</tbody>
																	</table>
																	<table border="0" cellspacing="0" cellpadding="0" width="100%">
																		<tbody>
																			<tr>
																				<td width="100%" style="padding:24px 24px 32px 24px; border-style:none;">
																					<table border="0" cellspacing="0" cellpadding="0" width="100%">
																						<tbody>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; color:#757575;">
																													<span>Hello {FIRST NAME},</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px; color:#757575;">
																													<span>You have been provided access by {COMPANY NAME} to one or more RealPage products. RealPage provides a variety of tools to support all aspects of Property Management, Leasing, Marketing, and Operations for the multifamily industry.  Welcome aboard your RealPage journey.</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px; color:#757575;">
																													<span>We have designed the My RealPage user interface with you, the user in mind. Click the Complete Your Profile button below to set your password and complete your RealPage User Profile:</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td align="center" style="padding:18px 0 0 0">
																									<table border="0" cellpadding="0" cellspacing="0" align="center">
																										<tbody>
																								  			<tr>
																												<td>
																									      			<table width="100%" border="0" cellspacing="0" cellpadding="0">
																										  				<tr>
																									          				<td align="center" style="-webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px;" bgcolor="#42a5f6">
																															  <a href="{LINK}" style="font-size: 16px; font-family: Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; -webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px; padding: 9px 28px; border: 1px solid #42a5f6; display: inline-block;">Complete Your Profile</a>
																														  </td>
																										  				</tr>
																													</table>
																												</td>
																								  			</tr>
																						  				</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px; color:#757575;">
																													<span>Please click this button within 72 hours to complete your profile.  After that, your administrator will need to send you a new email.</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px; color:#757575;">
																													<span>After completing your User Profile, click the following link for next steps:</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px;">
																													<a href="{LANDING}" style="color:#42A5F5;">Getting Started With My RealPage: New End User</a>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																							<tr>
																								<td width="100%" style="padding:18px 0 0 0">
																									<table border="0" cellspacing="0" cellpadding="0" width="100%">
																										<tbody>
																											<tr>
																												<td style="padding:0 10px; line-height:27px;">
																													<span style="color:#757575;">We thank you for your business and look forward to working with you. If you have trouble accessing your profile, please consult our</span>
																													<a href="https://www.realpage.com/company/office-locations" style="color:#42A5F5;">Support<a>
																													<span style="color:#757575;">page for information on getting assistance.</span>
																												</td>
																											</tr>
																										</tbody>
																									</table>
																								</td>
																							</tr>
																						</tbody>
																					</table>
																				</td>
																			</tr>
																		</tbody>
																	</table>
																</td>
															</tr>
														</tbody>
													</table>
												</td>
											</tr>
											<tr>
												<td align="left">
													<table border="0" cellspacing="0" cellpadding="0" width="100%" style="padding:0 24px;">
														<tbody>
															<tr>
																<td align="center" width="100%">
																	<table border="0" cellspacing="0" cellpadding="0" width="100%">
																		<tbody>
																			<tr>
																				<td align="center" width="100%" style="border-top:1px solid #757575; padding:16px 0;">
																					<a href="https://www.realpage.com/privacy-policy" style="color:#757575;text-decoration:none;"><span>Privacy Policy</span></a>
																					<span style="font-size:18px;color:#757575;">|</span>
																					<a href="https://www.realpage.com/company/office-locations" style="color:#757575;text-decoration:none;"><span>Contact Us</span></a>
																					<span style="font-size:18px;color:#757575;">|</span>
																					<span style="color:#757575;">&copy; 2017 RealPage, Inc.</span>
																				</td>
																			</tr>
																			<tr>
																				<td style="text-align:center;">
																					<a href="https://facebook.com/realpage" style="text-decoration:none;">
																						<img style="height:35px;" src="{IMAGES}/settings/email/email-notification/images/Facebook-Logo.png" alt="Facebook">
																					</a>
																					<a href="https://twitter.com/RealPage" style="text-decoration:none;">
																						<img style="height:35px;" src="{IMAGES}/settings/email/email-notification/images/Twitter-Logo.png" alt="Twitter">
																					</a>
																					<a href="https://www.linkedin.com/company/realpage" style="text-decoration:none;">
																						<img style="height:35px;" src="{IMAGES}/settings/email/email-notification/images/LinkedIn-Logo.png" alt="LinkedIn">
																					</a>
																				</td>
																			</tr>
																		</tbody>
																	</table>
																</td>
															</tr>
														</tbody>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</center>
							</td>
						</tr>
					</tbody>
				</table>
			</body>
		</html>')
SET IDENTITY_INSERT [Enterprise].[CommunicationEmailTemplate] OFF

	--Insert Reserved Values of System
	DECLARE @PartyId INT
	DECLARE @FromDate DateTime = GETUTCDATE()
	IF NOT EXISTS(SELECT 1 FROM Enterprise.Organization WHERE Name = 'System')
	BEGIN
       INSERT  INTO [Enterprise].Party  
       (	RealPageId,  
            CreateDate  
       )  
       VALUES  
       (	'C34616E3-D52E-4420-824C-595D1585D7C0',  
			@FromDate
       );  
       SET @PartyId = SCOPE_IDENTITY();  
       INSERT INTO [Enterprise].Organization  
       (  
			PartyId,  
			Name  
       )  
       VALUES  
       (  
			 @PartyId,  
			'System'  
       )  
  
       --Create new contact mechanism
       DECLARE @ContactMechanism INT
       EXEC Person.CreateContactMechanism @ContactMechanism OUTPUT
       SELECT @ContactMechanism

       --Add an email address under the contact mechanism
	   EXEC [Person].[CreateElectronicAddress] @ContactMechanism,'no-reply@realpage.com'
       
       --Link the contact mechanism to the party
       DECLARE @PartyContactMechanismid INT
	   DECLARE @PartyContact TABLE
	   ( 
			Id INT,
			RealPageId UNIQUEIDENTIFIER,
			ErrorMessage NVARCHAR(200)
		); 

		INSERT @PartyContact
		EXEC [Person].[LinkContactMechanismToParty] 'C34616E3-D52E-4420-824C-595D1585D7C0',@ContactMechanism,@FromDate,null,null 
		SELECT @PartyContactMechanismid=Id FROM @PartyContact

		--GET THE ID from Link, then link to Usage type = 301 (Email Notifications)
		EXEC [Person].[LinkUsageTypeToPartyContactMechanism] @PartyContactMechanismid,301

	END
	
	EXEC sys.sp_updateextendedproperty @name=N'Build', @value='3'
