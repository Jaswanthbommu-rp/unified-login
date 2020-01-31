go

set nocount on
select LogcategoryTypeId,name,count(1) from logging.LogType group by name,LogcategoryTypeId having count(1) > 1

declare @badlogtype table (seq int identity(1,1), logcattypeid int, name nvarchar(100), foundtype int )

insert into @badlogtype
select LogcategoryTypeId,name,count(1) from logging.LogType group by name,LogcategoryTypeId having count(1) > 1

declare @minbadtype int, @maxbattype int, @currenttypeid int
declare @replacetype table (seq int identity(1,1), replaceid int )
declare @currenttype nvarchar(100)
declare @keeptypeid int

select @minbadtype = min(seq) from @badlogtype
select @maxbattype = max(seq) from @badlogtype
select @currenttypeid = 1

declare @currentbadtypeid int
declare @minbadtypeid int
declare @maxbadtypeid int
declare @currentreplaceid int

begin tran
begin try
	while @currenttypeid <= @maxbattype
	begin
		select @currenttype = name from @badlogtype where seq = @currenttypeid
		select @keeptypeid = min(logtypeid) from logging.LogType where name = @currenttype
		insert into @replacetype ( replaceid )
			select logtypeid from logging.LogType where name = @currenttype and logtypeid != @keeptypeid
	
		select @minbadtypeid = min(seq), @currentbadtypeid = min(seq) from @replacetype
		select @maxbadtypeid = max(seq) from @replacetype
		while @currentbadtypeid <= @maxbadtypeid
		begin
		
			select @currentreplaceid = replaceid from @replacetype where seq = @currentbadtypeid
		
				update logging.Activity set LogTypeId = @keeptypeid where LogTypeId = @currentreplaceid
				delete from logging.LogType where LogTypeId = @currentreplaceid			

			set @currentbadtypeid = @currentbadtypeid + 1
		end
			
		set @currenttypeid = @currenttypeid + 1
		delete from @replacetype
	
	end
	commit

	select LogcategoryTypeId,name,count(1) from logging.LogType group by name,LogcategoryTypeId having count(1) > 1

end try
begin catch
	rollback tran
end catch

GO

DECLARE @LogCategoryTypeId int;

SELECT @LogCategoryTypeId=  LogcategoryTypeId
FROM [Logging].[LogCategoryType]
WHERE Name = 'User';

IF NOT EXISTS
(
	SELECT 1
	FROM [Logging].[LogType]
	WHERE LogCategoryTypeId = @LogCategoryTypeId AND 
		  Name = 'Change Company'
)
BEGIN
	INSERT INTO [Logging].[LogType]( LogCategoryTypeId, Name )
	VALUES( @LogCategoryTypeId, 'Change Company' );
END;

GO
