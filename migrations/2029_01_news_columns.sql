use ProducerInterface;

alter table Newses
change column Name Subject varchar(255),
change column Description Body text;

drop table NewsSnapshots;

alter table Newses
change column Id Id int not null auto_increment;

create table NewsSnapshots
(
	Id int not null auto_increment,
	AuthorId int unsigned not null,
	CreatedOn DateTime not null,
	AuthorName varchar(255),
	NewsId int not null,
	SnapshotName varchar(255),
	Subject varchar(255),
	Body text,
	primary key(Id), 
	constraint FK_NewsSnapshots_AuthorId foreign key(AuthorId) references Account(Id) on delete cascade,
	constraint FK_NewsSnapshots_NewId foreign key(NewsId) references Newses(Id) on delete cascade
)

