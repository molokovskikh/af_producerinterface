DROP PROCEDURE IF EXISTS quartz.PharmacyRatingReport;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE quartz.`PharmacyRatingReport`(IN `CatalogId` VARCHAR(255), IN `RegionCode` VARCHAR(255), IN `DateFrom` datetime, IN `DateTo` datetime)
BEGIN

  SET @sql = CONCAT('select c.CatalogName,
ph.PharmacyName,
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder, 
T.MinCost, T.AvgCost, T.MaxCost, T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId, PharmacyId,
	Sum(Cost*Quantity) as Summ,
	Sum(Quantity) as PosOrder,
	Min(Cost) as MinCost,
	Avg(Cost) as AvgCost,
	Max(Cost) as MaxCost,
	Count(distinct OrderId) as DistinctOrderId,
	Count(distinct AddressId) as DistinctAddressId
	from quartz.RatingReportOrderItems
	where IsLocal = 0
	and CatalogId in (', CatalogId, ')
	and RegionCode in (', RegionCode, ')
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by CatalogId, PharmacyId
	order by Summ desc) as T
left join quartz.CatalogNames c on c.CatalogId = T.CatalogId
left join quartz.PharmacyNames ph on ph.PharmacyId = T.PharmacyId');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END;
