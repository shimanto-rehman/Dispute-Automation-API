using Microsoft.AspNetCore.DataProtection.KeyManagement;
using RemitAnnex.Models.Request;
using RemitAnnex.Models.Validation;
using RemitAnnex.Utility;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;

namespace RemitAnnex.Manager
{
    public class ValidatorManager
    {
        public CommonValidationResponse RequestValidation(string RAApiKey, RemitAnexUpdateRequest request)
        {
            var resp = new CommonValidationResponse();
            resp.IsValid = false;

            if (string.IsNullOrEmpty(RAApiKey))
            {                
                resp.Msg = "Missing required headers.";
                return resp;
            }
            else if (RAApiKey != Definition.RAApiKey)
            {
                resp.Msg = "RA-Api-Key does not match";
                return resp;
            }
            else if (request.TranDetails.Count==0)
            {
                resp.Msg = "tranDetails is required";
                return resp;
            }
            else if (string.IsNullOrEmpty(request.MakerDetails.PurposeOfUpdate))
            {
                resp.Msg = "PurposeOfUpdate is required";
                return resp;
            }
            else if (string.IsNullOrEmpty(request.MakerDetails.EmployeeId))
            {
                resp.Msg = "EmployeeId is required";
                return resp;
            }
            else
            {
                resp.IsValid = true;
                return resp;
            }

        }
    }
}
