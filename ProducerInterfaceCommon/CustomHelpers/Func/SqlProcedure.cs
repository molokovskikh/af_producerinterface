using MySql.Data.MySqlClient;
using ProducerInterfaceCommon.CustomHelpers.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.CustomHelpers.Func
{
    public class SqlProcedure<T>
    {
        private List<T> ret;
        private Heap.MyAutoMapper<T> AM;
        private ulong regionMask;

        public SqlProcedure(ulong RegionMask)
        {
            this.regionMask = RegionMask;
            ret = new List<T>();
            AM = new Heap.MyAutoMapper<T>();
        }
        
        public List<long> GetPromotionId()
        {
            var PromotionInRegion = new CustomHelpers.Models.PromotionsInRegionMask(regionMask);
            SqlLinq(PromotionInRegion as BaseModel);                        

            List<PromotionsInRegionMask> return_ = ret as List<PromotionsInRegionMask>;
            return return_.Select(x => x.Id).ToList();                      
        }
        

        public void SqlLinq(BaseModel BM)
        {
            var querySort = new List<T>();
            var connString = ConfigurationManager.ConnectionStrings["producerinterface"].ConnectionString;
            using (var conn = new MySqlConnection(connString))
            {
                using (var command = new MySqlCommand(BM.GetSpName(), conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 0;
                    foreach (var spparam in BM.GetSpParams())
                        command.Parameters.AddWithValue(spparam.Key, spparam.Value);
                    conn.Open();
                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {                      
                        querySort = AM.Map(reader);
                    }
                }
            }

            ret = querySort;

        }
    }
}
