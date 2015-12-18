DROP PROCEDURE quartz.RatingReport;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE quartz.`RatingReport`(IN CatalogId VARCHAR(255), IN RegionCode VARCHAR(255), IN SupplierId VARCHAR(255), IN DateFrom datetime, IN DateTo datetime)
BEGIN

  SET @sql = CONCAT('select c.CatalogName,
if(c.IsPharmacie = 1, p.ProducerName, \'Нелекарственный ассортимент\') as ProducerName,
r.RegionName, 
s.SupplierName,
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder, 
T.MinCost, T.AvgCost, T.MaxCost, T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId, RegionCode, SupplierId, ProducerId,
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
	and SupplierId not in (', SupplierId, ')
	and WriteTime > \'', DateFrom, '\'
	and WriteTime < \'', DateTo, '\'
	group by CatalogId,ProducerId,RegionCode,SupplierId
	order by Summ desc) as T
left join quartz.CatalogNames c on c.CatalogId = T.CatalogId
left join quartz.ProducerNames p on p.ProducerId = T.ProducerId
left join quartz.RegionNames r on r.RegionCode = T.RegionCode
left join quartz.SupplierNames s on s.SupplierId = T.SupplierId');
  
  PREPARE stmt FROM @sql;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;
  
  #select @sql;

END;
