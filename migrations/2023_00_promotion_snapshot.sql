use ProducerInterface;
create table PromotionSnapshots
(
	Id int not null auto_increment,
	AuthorId int unsigned not null,
	CreatedOn DateTime not null,
	AuthorName varchar(255),
	PromotionId int unsigned not null,
	Name varchar(255),
	SnapshotName varchar(255),
	Annotation varchar(255),
	Status varchar(255),
	Begin DateTime not null,
	End DateTime not null,
	FileId int,
	ProductsJson text,
	RegionsJson text,
	SuppliersJson text,
	primary key(Id), 
	constraint FK_PromotionSnapshots_AuthorId foreign key(AuthorId) references Account(Id) on delete cascade,
	constraint FK_PromotionSnapshots_PromotionId foreign key(PromotionId) references Promotions(Id) on delete cascade,
	constraint FK_PromotionSnapshots_FileId foreign key(FileId) references MediaFiles(Id) on delete set null
)
