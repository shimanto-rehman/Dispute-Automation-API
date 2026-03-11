using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Models.DB;
using BusinessLogicLayer.Models.Request;
using BusinessLogicLayer.Models.Response;
using BusinessLogicLayer.Utility;
using DisputeAutomation.DAL.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class ClientRepo : IClientManager
    {
        private readonly DBConfiguration _context;
        private readonly Enc_Dec _dec;
        public ClientRepo(DBConfiguration _context)
        {
            this._context = _context;
            _dec = new Enc_Dec();

        }
        public bool CanProcess(string clientType)
        {
            throw new NotImplementedException();
        }

        public async Task<CollectionModel?> ProcessCollectionAsync(ProcessCollectionRequest collectionRequest)
        {
            CollectionModel? result = null;
            try
            {
                using (SqlConnection con = new SqlConnection(_dec.DecryptString(_context.Database.GetConnectionString())))
                using (SqlCommand cmd = new SqlCommand("breb_GetCollectionByCollFrom", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CollFrom", collectionRequest.BillerId);

                    await con.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result = new CollectionModel
                            {
                                CollectionId = reader["CollectionId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CollectionId"]),
                                ApproveBy = reader["ApproveBy"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ApproveBy"]),
                                ClientId = reader["ClientId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ClientId"]),
                                CollStatusId = reader["CollStatusId"] == DBNull.Value ? null : (byte?)Convert.ToByte(reader["CollStatusId"]),
                                BranchCode = reader["BranchCode"] == DBNull.Value ? null : reader["BranchCode"].ToString(),
                                Remarks = reader["Remarks"] == DBNull.Value ? null : reader["Remarks"].ToString(),
                            };

                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex){ 
            
            }
            return result;
        }
    }
}
