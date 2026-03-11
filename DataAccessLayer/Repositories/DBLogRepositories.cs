using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Models.DB;
using BusinessLogicLayer.Utility;
using DataAccessLayer.Repositories.BREB;
using DisputeAutomation.DAL.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer.Repositories
{
    public class DBLogRepositories : IDBLogManager
    {
        private readonly Enc_Dec _dec;
        private readonly string _ascendConnectionString;
        private readonly string _primaryConnectionString;
        private readonly DBConfiguration _context;

        public DBLogRepositories(IConfiguration configuration, DBConfiguration context)
        {
            _context = context;
            _ascendConnectionString = configuration.GetConnectionString("MTBAscendv2DB");
            _dec = new Enc_Dec();
        }
          
        public async Task<bool> SaveErrorLog(SaveErrorLog _saveErrorLog)
        {
            try
            {
                using var con = new SqlConnection(_dec.DecryptString(_context.Database.GetConnectionString()));
                using var cmd = new SqlCommand("sp_InsertBillCollectionAutomationErrorLog", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@ControllerName", (object?)_saveErrorLog.ControllerName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MethodName", (object?)_saveErrorLog.MethodName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ErrorMsg", (object?)_saveErrorLog.ErrorMsg ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StackTrace", (object?)_saveErrorLog.StackTrace ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ServiceId", (object?)_saveErrorLog.ServiceId ?? DBNull.Value);

                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return true;
            }
            catch
            {
                // Swallow — error logging must never crash the main flow
                return false;
            }
        }

        public async Task<bool> SaveReqRespLog(MwApiReqRespLog reqRespLog)
        {
            try
            {
                using var con = new SqlConnection(_dec.DecryptString(_context.Database.GetConnectionString()));
                using var cmd = new SqlCommand("sp_InsertBillCollectionAutomationReqResLog", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@ServiceId", (object?)reqRespLog.ServiceId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RefNo", (object?)reqRespLog.RefNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@JsonData", (object?)reqRespLog.JsonData ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TranType", (object?)reqRespLog.TranType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DataType", (object?)reqRespLog.DataType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EndPoint", (object?)reqRespLog.EndPoint ?? DBNull.Value);

                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return true;
            }
            catch
            {
                // Swallow — logging must never crash the main flow
                return false;
            }
        }
    }
}
