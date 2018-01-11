--rpt.atlas

set transaction isolation level read uncommitted;

declare @point geography = geography::Point(30.08, -94.15, 4326);

select
 c.CountyId
from geo.County as c
where c.[Zone].STIntersects(@point) = 1;

select
 z.ZipCode
from geo.ZipCode as z
where z.[Zone].STIntersects(@point) = 1;  