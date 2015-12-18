use quartz;

CREATE OR REPLACE VIEW `catalognames` AS 
select c.Id as CatalogId, concat(cn.Name, ' ', cf.Form) as CatalogName, c.Pharmacie as IsPharmacie, a.ProducerId
from catalogs.catalog c
inner join catalogs.assortment a on a.CatalogId = c.Id
left join catalogs.catalognames cn on cn.id = c.NameId
left join catalogs.catalogforms cf on cf.Id = c.FormId 
where c.Hidden = 0;

CREATE OR REPLACE VIEW `producernames` AS 
select p.Id as ProducerId, p.Name as ProducerName
from catalogs.Producers p;

CREATE OR REPLACE VIEW `regionnames` AS 
select r.RegionCode, r.Region as RegionName
from farm.regions r 
where r.DrugsSearchRegion = 0 
and r.RegionCode not in (524288);

CREATE OR REPLACE VIEW `suppliernames` AS 
select s.Id as SupplierId, concat(s.Name, ' - ', rg.Region) as SupplierName, s.HomeRegion
from Customers.suppliers s
left join farm.regions rg on rg.RegionCode = s.HomeRegion
where s.Name not like(CONVERT('%ассортимент%' USING CP1251)) 
and s.Disabled = 0
and s.Id in
(select FirmCode from usersettings.PricesData pd
where pd.Enabled = 1 and pd.IsLocal = 0);

CREATE OR REPLACE VIEW `ratingreportorderitems` AS
select `p`.`CatalogId` AS `CatalogId`,
`oh`.`RegionCode` AS `RegionCode`,
`pd`.`FirmCode` AS `SupplierId`,
`ol`.`CodeFirmCr` AS `ProducerId`,
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

DROP VIEW IF EXISTS orderitems;
