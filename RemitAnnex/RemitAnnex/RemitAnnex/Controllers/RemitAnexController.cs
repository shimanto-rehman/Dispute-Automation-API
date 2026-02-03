using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RemitAnnex.BusinessLayer;
using RemitAnnex.DBManager;
using RemitAnnex.Manager;
using RemitAnnex.Models.DB;
using RemitAnnex.Models.Request;
using RemitAnnex.Models.Validation;

namespace RemitAnnex.Controllers
{
    [ApiController]
    [Route("api/remitannex")]
    public class RemitAnexController : ControllerBase
    {
        [HttpPost("update-data")]
        public IActionResult UpdateData([FromBody] RemitAnexUpdateRequest request)
        {
            try
            {
                var clientId = Request.Headers["RA-Api-Key"].FirstOrDefault();              
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                ValidatorManager validatorMgt = new ValidatorManager();
                CommonValidationResponse validator = validatorMgt.RequestValidation(clientId, request);              
                if (!validator.IsValid)
                {
                    return BadRequest(new { RespDesc = validator.Msg, RespCode = "999" });
                }
                else
                {
                    RemitAnexBL remitAnexBL = new RemitAnexBL();
                    int status = remitAnexBL.modifyTxnList(request);
                    if (status == 1)
                    {
                        return Ok(new { RespDesc = "Success", RespCode = "000" });
                    }
                    else
                    {
                        return BadRequest(new { RespDesc = "Failed", RespCode = "999" });
                    }
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
                    errorCode = "ERR000-1001 : remitannex-update-data",
                    errorMsg = ex.Message,
                    stackTrace = ex.StackTrace
                });
                return BadRequest(new { RespDesc = "Something went wrong. Please try again later.", RespCode = "999" });
            }
        }

    }
}
