using Mailier.Core.Entities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Dapper;

namespace Mailier.Core.Providers
{
    public class HouseSndProvider
    {
        public IList<HouseSndModel> GetData()
        {
            #region
            const string sql = @"
                SELECT REGIONNAME,CYCLENAME,GARDENID,GARDENNAME,ASNAMEONE,ASNAMETWO,ASNAMETHREE,
                GARDENX,GARDENY, 
                '@'||REPLACE(SUBWAYLIST,'|','|@') as SUBWAYLIST,
                INTHOUSENO,HOUSEAREA,PRICEALL,PRICE,HOUSEFORWARD,SLEEPROOMNUM,
                ISFULLFIVE,ISONLY,ISFULLTWO,ISSUBWAYHOUSE,ISSCHOOLHOUSE,ANYTIMELOOK,
                ISFASTSALES,EXCLUSIVES,(CASE
                         WHEN EXCLUSIVES <> '0' OR LOCKEDR_AGENT_ID <> '0' THEN 1 ELSE  0
                       END)AS ISEXCLUSIVES,ISTERRACE,ISBAYWINDOW,ISGIVEPARKINGLOT,CASE WHEN PRICEALL>=1000 THEN 1 ELSE 0 END AS HOUSINGTYPE,
                BROKERID,RANKVALUE,STATUS,to_char(CREATETIME,'yyyy-MM-dd') as CREATETIME,ISTOP, (case when TOPTIME <> '0' OR TOPTIME is not null then TOPTIME else '0' end ) as TOPTIME,
                to_number(to_char(sysdate,'yyyy')-tonumeric(BUILTYEAR)) as HOUSEAGE,(CASE
                         WHEN NVL(EXCLUSIVES,'0') <> '0' OR NVL(LOCKEDR_AGENT_ID,'0') <> '0' OR NVL(AUTHOR_AGENT_ID,'0')<>'0' THEN  '1'ELSE '0'
                       END) as ISREALSOURCE,
                    Nvl(INTHOUSEBUILTYEAR, '-1') as INTHOUSEBUILTYEAR, inthousefloor as INTHOUSEFLOOR,
                case when  ceil(FLOORHEIGHT / 3.0 * 2) < INTHOUSEFLOOR then '3'/*高*/
                  when  ceil(FLOORHEIGHT / 3.0 * 2) >= INTHOUSEFLOOR and  ceil(FLOORHEIGHT / 3.0) < INTHOUSEFLOOR then '2'/*中*/
                  else '1'/*低*/  end
                as HOUSEFLOORLEVEL
                FROM VW_HOUSESECOND
                WHERE STATUS='1' AND BUSSINESS_STS = '1' AND REAL_HOUSE_FLAG='1' AND PRIVATE_HOUSE_FLAG='0' ";
            //and inthouseno in ('FY00538850','FY00569609','FY00556907','FY00570390','FY00571784','FY00565535','FY00566367','FY00560570','FY00570351','FY00562399)'

            #endregion
            using (var conn = new OracleConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                conn.Open();
                return conn.Query<HouseSndModel>(sql).ToList();
            }
        }
    }
}