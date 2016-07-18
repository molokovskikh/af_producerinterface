use ProducerInterface;
alter table MediaFiles
change column ImageType ImageType varchar(255) not null;
