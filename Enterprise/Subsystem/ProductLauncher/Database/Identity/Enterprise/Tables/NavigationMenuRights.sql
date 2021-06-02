CREATE TABLE [Enterprise].[NavigationMenuRights]
(
	NavigationMenuId int not null,
	RightId int not null, 
    CONSTRAINT [PK_NavigationMenuRights] PRIMARY KEY ([RightId], [NavigationMenuId]), 
    CONSTRAINT [FK_NavigationMenuRights_Right] FOREIGN KEY (RightId) REFERENCES [Security].[Right](RightId)
)
