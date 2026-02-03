using RemitAnnex.DBManager;
using RemitAnnex.Models.DB;
using RemitAnnex.Models.Request;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;

namespace RemitAnnex.BusinessLayer
{
    public class RemitAnexBL
    {
        private string GenerateXMLPara(List<TranDetails> tranDetails)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("<statement>");
                foreach (var item in tranDetails)
                {
                    sb.Append("<rowrecord>");
                    sb.AppendFormat("<PartyId>{0}</PartyId>", item.PartyId);
                    sb.AppendFormat("<ReferenceNumber>{0}</ReferenceNumber>", item.ReferenceNumber?.Trim());
                    sb.AppendFormat("<EntityId>{0}</EntityId>", item.EntityId);
                    sb.AppendFormat("<TranChannel>{0}</TranChannel>", item.TranChannel?.Trim());
                    sb.AppendFormat("<IsIncentive>{0}</IsIncentive>", item.IsIncentive);
                    sb.AppendFormat("<IsReverse>{0}</IsReverse>", item.IsReverse);
                    sb.AppendFormat("<StatusCode>{0}</StatusCode>", item.StatusCode?.Trim());
                    sb.Append("</rowrecord>");
                }

                sb.Append("</statement>");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                RemitAnexDBMgt remitAnexDBMgt = new RemitAnexDBMgt();
                remitAnexDBMgt.saveErrorLog(new SaveErrorLog()
                {
                    serviceId = 0,
                    logId = 0,
                    searchKey = string.Empty,
                    errorCode = "ERR000-1002 : RemitAnexBL-GenerateXMLPara",
                    errorMsg = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
            return string.Empty;
        }


        public int modifyTxnList(RemitAnexUpdateRequest request)
        {
            int status = 0;
            try
            {
                string XmlValue = GenerateXMLPara(request.TranDetails);
                if (!string.IsNullOrEmpty(XmlValue))
                {
                    RemitAnexDBMgt remitAnexDBMgt = new RemitAnexDBMgt();
                    status = remitAnexDBMgt.modifyTxnList(XmlValue, request.MakerDetails.PurposeOfUpdate, request.MakerDetails.EmployeeId);
                }
            }
            catch (Exception ex)
            {
                RemitAnexDBMgt remitAnexDBMgt = new RemitAnexDBMgt();
                remitAnexDBMgt.saveErrorLog(new SaveErrorLog()
                {
                    serviceId = 0,
                    logId = 0,
                    searchKey = string.Empty,
                    errorCode = "ERR000-1003 : RemitAnexBL-modifyTxnList",
                    errorMsg = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
            return status;
        }
    }
}
