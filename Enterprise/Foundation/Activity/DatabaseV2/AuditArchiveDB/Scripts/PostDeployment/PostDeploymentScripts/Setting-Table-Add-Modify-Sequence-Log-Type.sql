
IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Modified Sequence')
BEGIN
	INSERT INTO Logging.LogType VALUES(175, 8, 'Modified Sequence', 'Modified Sequence')
END