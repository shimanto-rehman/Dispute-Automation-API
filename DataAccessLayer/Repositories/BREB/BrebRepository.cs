using System;
using System.Data;
using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Manager.BREB;
using BusinessLogicLayer.Models.DB;
using BusinessLogicLayer.Utility;
using DisputeAutomation.DAL.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer.Repositories.BREB
{
    public class BrebRepository : IBrebManager
    {
        private readonly DBConfiguration _context;
        private readonly Enc_Dec _dec;
        private readonly string _ascendConnectionString;
        private readonly IDBLogManager _dbLog;

        public BrebRepository(DBConfiguration context, IConfiguration configuration, IDBLogManager dbLog)
        {
            _context = context;
            _dec = new Enc_Dec();
            _dbLog = dbLog;
            _ascendConnectionString = configuration.GetConnectionString("MTBAscendv2DB");
        }

        public async Task<BrebBillCollectionModel> getBrebBillCollectionByCollectionId(long collectionId, int clientId)
        {
            BrebBillCollectionModel? result = null;
            try
            {
                using (SqlConnection con = new SqlConnection(_dec.DecryptString(_context.Database.GetConnectionString())))
                using (SqlCommand cmd = new SqlCommand("breb_GetBrebBillCollection", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CollectionId", collectionId);

                    await con.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result = new BrebBillCollectionModel
                            {
                                AutoId = reader["AutoId"] == DBNull.Value ? 0 : Convert.ToInt64(reader["AutoId"]),
                                CollectionId = reader["CollectionId"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CollectionId"]),

                                RefNo = reader["RefNo"] == DBNull.Value ? null : reader["RefNo"].ToString(),
                                TransactionId = reader["TransactionId"] == DBNull.Value ? null : reader["TransactionId"].ToString(),
                                SMSAccountNo = reader["SMSAccountNo"] == DBNull.Value ? null : reader["SMSAccountNo"].ToString(),
                                BranchRoutingNo = reader["BranchRoutingNo"] == DBNull.Value ? null : reader["BranchRoutingNo"].ToString(),
                                DueType = reader["DueType"] == DBNull.Value ? null : reader["DueType"].ToString(),
                                BillMonth = reader["BillMonth"] == DBNull.Value ? null : reader["BillMonth"].ToString(),
                                MobileNo = reader["MobileNo"] == DBNull.Value ? null : reader["MobileNo"].ToString(),

                                BillAmount = reader["BillAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["BillAmount"]),
                                VatAmount = reader["VatAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["VatAmount"]),
                                LPC = reader["LPC"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["LPC"]),
                                TotalAmount = reader["TotalAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["TotalAmount"]),

                                CollectionDate = reader["CollectionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CollectionDate"]),
                                PaymentDateTime = reader["PaymentDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PaymentDateTime"]),

                                Remarks = reader["Remarks"] == DBNull.Value ? null : reader["Remarks"].ToString()
                            };

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await _dbLog.SaveErrorLog(new SaveErrorLog
                {
                    ControllerName = nameof(BrebRepository),
                    MethodName = nameof(getBrebBillCollectionByCollectionId),
                    ServiceId = clientId.ToString(),
                    ErrorMsg = ex.Message,
                    StackTrace = ex.StackTrace
                });
                return null;
            }
            return result;
        }

        public async Task<BrebPaymentStatusLogModel?> getPaymentStatusRefNoAsync(string transactionId, int clientId)
        {
            BrebPaymentStatusLogModel? result = null;

            try
            {
                using (SqlConnection con = new SqlConnection(_dec.DecryptString(_ascendConnectionString)))
                using (SqlCommand cmd = new SqlCommand("breb_GetPaymentStatusRefNo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ClientReq", transactionId);

                    await con.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result = new BrebPaymentStatusLogModel
                            {
                                ReqId = reader["req_id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["req_id"]),
                                RefNo = reader["ref_id"] == DBNull.Value ? null : reader["ref_id"].ToString(),
                                ReqTime = reader["req_time"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["req_time"]),
                                ClientReq = reader["client_req"] == DBNull.Value ? null : reader["client_req"].ToString(),
                                Method = reader["method"] == DBNull.Value ? null : reader["method"].ToString(),
                                RespData = reader["resp_data"] == DBNull.Value ? null : reader["resp_data"].ToString(),
                                RespTime = reader["resp_time"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["resp_time"])
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _dbLog.SaveErrorLog(new SaveErrorLog
                {
                    ControllerName = nameof(BrebRepository),
                    MethodName = nameof(getPaymentStatusRefNoAsync),
                    ServiceId = clientId.ToString(),
                    ErrorMsg = ex.Message,
                    StackTrace = ex.StackTrace
                });
                return null;
            }

            return result;
        }

        public async Task<bool> updateCollectionTable(CollectionModel up_collection)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_dec.DecryptString(_context.Database.GetConnectionString())))
                using (SqlCommand cmd = new SqlCommand("UpdateCollectionByAutomation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@BranchCode", up_collection.BranchCode);
                    cmd.Parameters.AddWithValue("@ClientId", up_collection.ClientId);
                    cmd.Parameters.AddWithValue("@CollectionId", up_collection.CollectionId);
                    cmd.Parameters.AddWithValue("@ApproveBy", up_collection.ApproveBy);
                    cmd.Parameters.AddWithValue("@Remarks", up_collection.Remarks);

                    await con.OpenAsync();

                    var result = await cmd.ExecuteScalarAsync();

                    int affectedRows = result != null ? Convert.ToInt32(result) : 0;

                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                await _dbLog.SaveErrorLog(new SaveErrorLog
                {
                    ControllerName = nameof(BrebRepository),
                    MethodName = nameof(updateCollectionTable),
                    ServiceId = up_collection.ClientId.ToString(),
                    ErrorMsg = ex.Message,
                    StackTrace = ex.StackTrace
                });
                return false;
            }

        }
    }
}