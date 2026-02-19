--User Story 2648822: Lumina Copilot - Enable Voice AI through product-level configuration settings

IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'CopilotVoiceAIDefault')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('CopilotVoiceAIDefault', 'Default value for copilot VoiceAI and product', 0);
END

IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'CopilotAgenticAIDefault')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('CopilotAgenticAIDefault', 'Default value for Copilot AgenticAI Default and product', 0);
END
GO