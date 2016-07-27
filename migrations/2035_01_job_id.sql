use ProducerInterface;
alter table jobextend
add column Id int not null auto_increment,
add key `IdKey` (Id);
