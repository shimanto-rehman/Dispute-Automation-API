using Oracle.ManagedDataAccess.Client;
using RemitAnnex.Models.DB;
using System.Data;

namespace RemitAnnex.DBManager
{
    public class RemitAnexDBMgt
    {
        private string ConnectionString = string.Empty;        

        public int modifyTxnList(string txnDetails,string PurposeOfUpdate,string EmployeeId)
        {
			int status = 0;
			try
			{

			}
			catch (Exception ex)
			{
                RemitAnexDBMgt remitAnexDBMgt = new RemitAnexDBMgt();
                remitAnexDBMgt.saveErrorLog(new SaveErrorLog()
                {
                    serviceId = 0,
                    logId = 0,
                    searchKey = string.Empty,
                    errorCode = "ERR000-1004 : RemitAnexDBMgt-modifyTxnList",
                    errorMsg = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }

			return status;
        }

        public void saveErrorLog(SaveErrorLog errLog)
        {
            using (OracleConnection _dbConn = new OracleConnection(ConnectionString))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = _dbConn;
                    _dbConn.Open();

                    cmd.CommandText = "PKG_REMIT_SERVICE.SP_INSERT_ERROR_LOG";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("P_SERVICE_ID", OracleDbType.Int32).Value = errLog.serviceId;
                    cmd.Parameters.Add("P_LOG_ID", OracleDbType.Int32).Value = errLog.logId;
                    cmd.Parameters.Add("P_SEARCH_KEY", OracleDbType.Varchar2).Value = errLog.searchKey;
                    cmd.Parameters.Add("P_ERROR_CODE", OracleDbType.Varchar2).Value = errLog.errorCode;
                    cmd.Parameters.Add("P_ERROR_MSG", OracleDbType.Varchar2).Value = errLog.errorMsg;
                    cmd.Parameters.Add("P_STACK_TRACE", OracleDbType.Varchar2).Value = errLog.stackTrace;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    _dbConn.Close();
                }
            }
        }
    }
}
