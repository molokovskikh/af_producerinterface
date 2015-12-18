use quartz;

CREATE OR REPLACE VIEW PharmacyNames AS 
select cl.Id as PharmacyId, concat(cl.FullName, ' - ', rg.Region) as PharmacyName
from Customers.Clients cl
left join farm.regions rg on rg.RegionCode = cl.RegionCode;

CREATE OR REPLACE VIEW RatingReportOrderItems AS 
select `p`.`CatalogId` AS `CatalogId`,
`oh`.`RegionCode` AS `RegionCode`,
`pd`.`FirmCode` AS `SupplierId`,
`ol`.`CodeFirmCr` AS `ProducerId`,
`oh`.`ClientCode` AS `PharmacyId`,
`ol`.`Cost`,
`ol`.`Quantity`,
`oh`.`RowID` AS `OrderId`,
`oh`.`AddressId`,
`pd`.`IsLocal` AS `IsLocal`,
`oh`.`WriteTime` AS `WriteTime` 
from (((`ordersold`.`ordershead` `oh` 
join `ordersold`.`orderslist` `ol` on((`ol`.`OrderID` = `oh`.`RowID`))) 
join `catalogs`.`products` `p` on((`p`.`Id` = `ol`.`ProductId`))) 
join `usersettings`.`pricesdata` `pd` on((`pd`.`PriceCode` = `oh`.`PriceCode`)));

CREATE OR REPLACE VIEW catalognames AS 
select c.Id as CatalogId, concat(cn.Name, ' ', cf.Form) as CatalogName, c.Pharmacie as IsPharmacie
from catalogs.catalog c
left join catalogs.catalognames cn on cn.id = c.NameId
left join catalogs.catalogforms cf on cf.Id = c.FormId 
where c.Hidden = 0;

CREATE OR REPLACE VIEW assortment AS 
select c.Id as CatalogId, concat(cn.Name, ' ', cf.Form) as CatalogName, c.Pharmacie as IsPharmacie, a.ProducerId
from catalogs.catalog c
inner join catalogs.assortment a on a.CatalogId = c.Id
left join catalogs.catalognames cn on cn.id = c.NameId
left join catalogs.catalogforms cf on cf.Id = c.FormId 
where c.Hidden = 0;

DROP PROCEDURE IF EXISTS RatingReport;

DROP VIEW IF EXISTS orderitems;


