use quartz;

CREATE OR REPLACE VIEW UserNames AS 
select u.Id as UserId, u.Name as UserName, u.Email, p.Id as ProducerId, p.Name as ProducerName 
from producerinterface.ProducerUser u
inner join catalogs.producers p on p.Id = u.ProducerId;

