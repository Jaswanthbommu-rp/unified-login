        
-- [UserAudit].[GetPrimaryPropertyAudit] 764        
  
        
CREATE PROCEDURE [UserAudit].[GetPrimaryPropertyAudit]  (      
 @RequestId bigint         
 )      
AS        
BEGIN        
 SET NOCOUNT ON;        
        
        
        
                declare @outputcolumns nvarchar(max) = 'PrimaryProperties '         
                drop table if exists #TempUPFMProperties                       
                select * into #TempUPFMProperties from [UserAudit].[PrimaryPropertyAudit] where 1 = 0      
                insert into #TempUPFMProperties (RequestId,ProductId,ProductName,PropertyGUID,PropertyName,ProductInstanceId,CreatedDate)      
                select RequestId,ProductId,ProductName,PropertyGUID,PropertyName,ProductInstanceId,CreatedDate      
                from [UserAudit].[PrimaryPropertyAudit] where requestId = @RequestId and productId = 3  order by PropertyName      
     
                drop table if exists #ProductsTranslatedProperties        
                select UPFM.ProductId as UPFMProductId, UPFM.ProductName as UPFMProductName,UPFM.PropertyName as UPFMPropertyName,        
                UPFM.PropertyGUID as UPFMGUID, PPA.ProductId, PPA.ProductName,PPA.PropertyName, PPA.PropertyGUID as SelectedProductGUID, PPA.ProductInstanceId        
                into #ProductsTranslatedProperties from #TempUPFMProperties UPFM        
                inner join [UserAudit].[PrimaryPropertyAudit] PPA on UPFM.PropertyGUID = PPA.PropertyGUID where PPA.ProductId <> 3 and PPA.RequestId = @RequestId and UPFM.RequestId = @RequestId        
                order by PPA.PropertyGUID        
                        
    
                 CREATE NONCLUSTERED  index IDX_Temp_#TempUPFMProperties       
                    on #TempUPFMProperties (PropertyGUID)      
                        
                update #ProductsTranslatedProperties set  ProductName = ltrim(rtrim(ProductName))        
                        
                drop table if exists #DistinctColumns        
                (SELECT DISTINCT productName as [ProductName],ProductId,IDENTITY(INT,1,1) AS ID INTO #DistinctColumns        
                FROM [UserAudit].[PrimaryPropertyAudit] where ProductId <> 3 and requestId = @RequestId)        
                        
                drop table if exists  #TempFinal        
                create table #TempFinal (ID int identity(1,1) primary key, UPFMProductGUID nvarchar(250) , PrimaryProperties nvarchar(250))        
                        
                declare @colName nvarchar(100)        
                declare @cnt int = 1        
                declare @TotalRows int= (select count(*) from #DistinctColumns)        
      
                if exists(select top 1 1 from #DistinctColumns where ProductId = 1)      
                begin      
                set @outputcolumns = @outputcolumns + '  ,[OneSite ID],[OneSite] '      
                alter table #TempFinal       
                add [OneSite ID] nvarchar(100)  
                
                end      
     
                declare @sql nvarchar(max) = ''        
                while (@cnt <= @TotalRows)        
                begin        
                set @colName  = (select productName from #DistinctColumns where ID = @cnt)                       
                      
                set @sql = @sql + ' alter table #TempFinal add ' + @colName + ' nvarchar(256) ; '        
                      
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
                        
    
                while(@colcnt  <= @colTotalRows)      
                begin      
                set @selectedcolName = (select ProductName from #DistinctColumns where ID = @colcnt)      
                set @sqlstmt = @sqlstmt + ' update TF set '+ @selectedcolName +' = PropertyName from #ProductsTranslatedProperties PTP   
                inner join #TempFinal TF on PTP.UPFMGUID = TF.UPFMProductGUID  
                where ProductName ='''+ @selectedcolName +'''; '      
                set @colcnt = @colcnt + 1      
                end  
                exec (@sqlstmt)   
  
    
                if exists(select top 1 1 from #DistinctColumns where productId = 1)        
                begin                        
                declare @onesiteupdatestmt nvarchar(2000) =  ' update TF set TF.[OneSite ID] = PPA.[ProductInstanceId] from #TempUPFMProperties PTP       
                inner join #TempFinal TF on TF.UPFMProductGUID  = PTP.PropertyGUID      
                inner join [UserAudit].[PrimaryPropertyAudit] PPA on PTP.PropertyGUID = PPA.PropertyGUID          
                where PPA.ProductId <> 3 and PPA.ProductId = 1 and PPA.requestId = '+ convert(nvarchar(255), @RequestId) +' and PTP.requestId = '+ convert(nvarchar(255), @RequestId) +''       
          
                exec (@onesiteupdatestmt)      
      
                 insert into #TempFinal(UPFMProductGUID)        
                 select NULL        
      
                      
                 declare @onesiteinsertstmt nvarchar(2000) = ' insert into #TempFinal ([OneSite ID],[OneSite])       
                 select PPA.ProductInstanceId,PPA.PropertyName from  [UserAudit].[PrimaryPropertyAudit] PPA       
                 left join #TempUPFMProperties TUPFM on PPA.PropertyGUID = TUPFM.PropertyGUID       
                 where  PPA.ProductId <> 3 and  PPA.ProductId = 1  and PPA.RequestId = '+ convert(nvarchar(255), @RequestId) +' and TUPFM.PropertyGUID is null order by PPA.PropertyName '      
                 exec (@onesiteinsertstmt)                     
      
                end        
                           
                WAITFOR DELAY '00:00:00.005'        
      
                drop index  IDX_Temp_#TempUPFMProperties on #TempUPFMProperties       
                declare @insertProdId int = 0      
                declare @insertsqlstmt nvarchar(max) = ''        
                set @rcnt = 1        
                while (@rcnt <= @colTotalRows)        
                begin         
                select @selectedcolName = ProductName ,@insertProdId = ProductId from #DistinctColumns where ID = @rcnt        
                if (@insertProdId <> 1)      
                begin      
                    insert into #TempFinal(UPFMProductGUID)        
                    select NULL        
                          
                        
                    set @insertsqlstmt = ' insert into #TempFinal('+ @selectedcolName + ') '        
                    set @insertsqlstmt = @insertsqlstmt + ' select PPA.PropertyName from  [UserAudit].[PrimaryPropertyAudit] PPA       
                                                      left join #TempUPFMProperties TUPFM on PPA.PropertyGUID = TUPFM.PropertyGUID       
                                                      where  PPA.ProductId <> 3 and  PPA.ProductId = '+ convert(nvarchar(20), @insertProdId) +'  and PPA.RequestId = '+ convert(nvarchar(255), @RequestId) +' and TUPFM.PropertyGUID is null order by PPA.PropertyName '        
                   exec (@insertsqlstmt)          
                     end      
                   set @rcnt = @rcnt + 1 ;        
                   WAITFOR DELAY '00:00:00.003'        
                end       
        
                declare @tmpSelectedcolumns varchar(max) = ''         
                delete from #DistinctColumns where productId = 1       
                select @tmpSelectedcolumns = @tmpSelectedcolumns + ProductName + ', ' from #DistinctColumns        
                set @outputcolumns = @outputcolumns + ',' + @tmpSelectedcolumns        
                set @tmpSelectedcolumns = SUBSTRING(@outputcolumns, 0, LEN(@outputcolumns))        
                set @tmpSelectedcolumns = REPLACE(@tmpSelectedcolumns,',,',',')        
                declare @sqlfinaloutputcolumns nvarchar(max)  = 'select ' + @tmpSelectedcolumns + ' from #TempFinal order by ID'        
                exec (@sqlfinaloutputcolumns)      
      
END