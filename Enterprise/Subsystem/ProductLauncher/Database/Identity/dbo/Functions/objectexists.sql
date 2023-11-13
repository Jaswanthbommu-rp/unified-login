CREATE FUNCTION [Maintenance].[ObjectExists] 
( 
   @p_object_name varchar(max),
   @p_object_type varchar(max)
)
RETURNS tinyint
AS 
BEGIN
	DECLARE
		@v_count INT,
		@v_debug tinyint = 0
		
	IF UPPER(@p_object_type) = 'TABLE PARTITION' 
		SELECT @v_count = COUNT(*) 
		  FROM sys.objects O
		 INNER JOIN sys.partitions p on P.object_id = O.object_id
		 INNER JOIN sys.indexes i on p.object_id = i.object_id and p.index_id = i.index_id
		 INNER JOIN sys.data_spaces ds on i.data_space_id = ds.data_space_id
		 INNER JOIN sys.partition_schemes ps on ds.data_space_id = ps.data_space_id
		 WHERE o.name = UPPER(@p_object_name)
	ELSE
		SELECT @v_count = COUNT(*) 
		  FROM INFORMATION_SCHEMA.TABLES
		 WHERE table_name = UPPER(@p_object_name)
	IF @v_count < 1 		
		RETURN 0
		
	RETURN 1
END