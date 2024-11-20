  
-- [UserAudit].[GetPrimaryPropertyAudit] 764  
  
  
CREATE PROCEDURE [UserAudit].[GetPrimaryPropertyAudit]  (
 @RequestId bigint   
 )
AS  
BEGIN  
 SET NOCOUNT ON;  
  
  
  
          drop table if exists #TempUPFMProperties  
                select * into #TempUPFMProperties from [UserAudit].[PrimaryPropertyAudit] where requestId = @RequestId and productId = 3  
                  
                  
                drop table if exists #ProductsTranslatedProperties  
                select UPFM.ProductId as UPFMProductId, UPFM.ProductName as UPFMProductName,UPFM.PropertyName as UPFMPropertyName,  
                UPFM.PropertyGUID as UPFMGUID, PPA.ProductId, PPA.ProductName,PPA.PropertyName, PPA.PropertyGUID as SelectedProductGUID, PPA.ProductInstanceId  
                into #ProductsTranslatedProperties from #TempUPFMProperties UPFM  
                inner join [UserAudit].[PrimaryPropertyAudit] PPA on UPFM.PropertyGUID = PPA.PropertyGUID where PPA.ProductId <> 3 and PPA.RequestId = @RequestId and UPFM.RequestId = @RequestId  
                order by PPA.PropertyGUID  
                  
                  
                update #ProductsTranslatedProperties set  ProductName = ltrim(rtrim(ProductName))  
                  
                drop table if exists #DistinctColumns  
                (SELECT DISTINCT productName as [ProductName],ProductId,IDENTITY(INT,1,1) AS ID INTO #DistinctColumns  
                FROM [UserAudit].[PrimaryPropertyAudit] where ProductId <> 3 and requestId = @RequestId)  
                  
                drop table if exists  #TempFinal  
                create table #TempFinal (ID int identity(1,1) , UPFMProductGUID nvarchar(250) , PrimaryProperties nvarchar(250),[OneSite ID] nvarchar(100),[OneSite] nvarchar(250))  
                  
                declare @colName nvarchar(100)  
                declare @cnt int = 1  
                declare @TotalRows int= (select count(*) from #DistinctColumns)  
                declare @sql nvarchar(max) = ''  
                while (@cnt <= @TotalRows)  
                begin  
                set @colName  = (select productName from #DistinctColumns where ID = @cnt)  
                if(@colName <> '[OneSite]' and @colName <> 'OneSite' )  
                begin  
                  
                set @sql = @sql + ' alter table #TempFinal add ' + @colName + ' nvarchar(256) ; '  
                end  
                set @cnt = @cnt +1;  
                end  
                exec (@sql)  
                  
                insert into #TempFinal (UPFMProductGUID,PrimaryProperties)  
                select PropertyGUID,PropertyName from #TempUPFMProperties  
                  
                declare @rcnt int = 1  
                declare @FTotalRows int = (select count(1) from #TempFinal)  
                declare @sqlstmt nvarchar(max) = ''  
                declare @GUIDToCompare nvarchar(100) = ''  
                  
                declare @colcnt int = 1  
                declare @colTotalRows int = (select count(1) from #DistinctColumns)  
                declare @selectedcolName nvarchar(50) = ''  
                  
                while (@rcnt <= @FTotalRows)  
                begin  
                set @GUIDToCompare = (select UPFMProductGUID from #TempFinal where ID = @rcnt)  
                set @sqlstmt = ' '  
                set @colcnt = 1  
                set  @colTotalRows = (select count(1) from #DistinctColumns)  
                while(@colcnt  <= @colTotalRows)  
                begin  
                set @selectedcolName = (select ProductName from #DistinctColumns where ID = @colcnt)  
                set @sqlstmt = @sqlstmt + ' update #TempFinal set '+ @selectedcolName +' = PropertyName from #ProductsTranslatedProperties  
                where UPFMProductGUID =  ''' + @GUIDToCompare + '''  and UPFMGUID = ''' + @GUIDToCompare + ''' and ProductName ='''+ @selectedcolName +'''; '  
                set @colcnt = @colcnt + 1  
                end;  
                exec (@sqlstmt)  
                set @rcnt = @rcnt + 1 ;  
                WAITFOR DELAY '00:00:00.002'  
                end  
                  
                if exists(select top 1 1 from #DistinctColumns where productId = 1)  
                begin  
                update #TempFinal set [OneSite ID] = PPA.[ProductInstanceId] from #TempUPFMProperties PTP  
                inner join [UserAudit].[PrimaryPropertyAudit] PPA on PTP.PropertyGUID = PPA.PropertyGUID  
                where PPA.ProductName = 'OneSite' and PPA.requestId = @RequestId and PTP.requestId = @RequestId                  
                end  
                     
                WAITFOR DELAY '00:00:00.005'       
                declare @insertProdId int = 0
                declare @insertsqlstmt nvarchar(max) = ''  
                set @rcnt = 1  
                while (@rcnt <= @colTotalRows)  
                            begin   
                insert into #TempFinal(UPFMProductGUID)  
                select NULL  
              
                set @selectedcolName = (select ProductName from #DistinctColumns where ID = @rcnt)  
                         set @insertsqlstmt = ' insert into #TempFinal('+ @selectedcolName + ') '  
                      set @insertsqlstmt = @insertsqlstmt + ' select PPA.PropertyName from  [UserAudit].[PrimaryPropertyAudit] PPA 
                                                  left join #TempUPFMProperties TUPFM on PPA.PropertyGUID = TUPFM.PropertyGUID 
                                                  where  PPA.ProductId <> 3 and  PPA.ProductId = '+ convert(nvarchar(20), @insertProdId) +'  and PPA.RequestId = '+ convert(nvarchar(255), @RequestId) +' and TUPFM.PropertyGUID is null '  
                   exec (@insertsqlstmt)      
                   set @rcnt = @rcnt + 1 ;  
                WAITFOR DELAY '00:00:00.003'  
                end  
  
                declare @tmpSelectedcolumns varchar(max) = ''  
                declare @outputcolumns nvarchar(max) = 'PrimaryProperties,[OneSite ID],[OneSite] '  
                delete from #DistinctColumns where productId = 1  
                select @tmpSelectedcolumns = @tmpSelectedcolumns + ProductName + ', ' from #DistinctColumns  
                set @outputcolumns = @outputcolumns + ',' + @tmpSelectedcolumns  
                set @tmpSelectedcolumns = SUBSTRING(@outputcolumns, 0, LEN(@outputcolumns))  
                set @tmpSelectedcolumns = REPLACE(@tmpSelectedcolumns,',,',',')  
                declare @sqlfinaloutputcolumns nvarchar(max)  = 'select ' + @tmpSelectedcolumns + ' from #TempFinal order by ID'  
                exec (@sqlfinaloutputcolumns)  
  
  
END