CREATE TABLE [Enterprise].[NavigationMenu]
(
	Id int primary key identity(1, 1),
	Title nvarchar(30),
	PageId nvarchar(30),
	Icon nvarchar(30),
	[URL] nvarchar(255),
	OrderIndex int not null,
	ParentId int
)