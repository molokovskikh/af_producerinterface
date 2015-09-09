use ProducerInterface;
alter table producerInterface.DrugDescriptionRemark 
	add index (ModificatorId), 
	add constraint DrugDescriptionRemark_ModificatorId 
	foreign key (ModificatorId) 
	references accessright.regionaladmins (RowID);

alter table producerInterface.DrugDescriptionRemark 
	add index (ProducerUserId), 
	add constraint DrugDescriptionRemark_ProducerUserId 
	foreign key (ProducerUserId) 
	references ProducerInterface.Users (Id);

alter table producerInterface.DrugDescriptionRemark 
	add index (DrugFamilyId), 
	add constraint DrugDescriptionRemark_DrugFamilyId 
	foreign key (DrugFamilyId) 
	references catalogs.catalogNames (Id);

alter table producerInterface.DrugDescriptionRemark 
	add index (MNNId), 
	add constraint DrugDescriptionRemark_MNNId 
	foreign key (MNNId) 
	references catalogs.mnn (Id);