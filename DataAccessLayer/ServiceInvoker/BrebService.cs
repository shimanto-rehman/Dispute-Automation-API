using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Models.DB;
using BusinessLogicLayer.Models.Request.BREB;
using BusinessLogicLayer.Models.Response.BREB;
using BusinessLogicLayer.ServiceInvoker;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer.ServiceInvoker
{
    public class BrebService : IBrebService
    {
        private readonly IConfiguration configuration;
        private readonly HttpClient _httpClient;
        private readonly IDBLogManager _dbLog;
        public BrebService(HttpClient httpClient, IConfiguration configuration, IDBLogManager dbLog)
        {
            _httpClient = httpClient;
            this.configuration = configuration;
            this._dbLog = dbLog;

        }
        public async Task<BrebPaymentStatusResponse?> GetPaymentStatusAsync(BrebPaymentStatusRequest request, int clientId,string billerId)
        {
            try
            {
                var url = configuration["BrebApi:BaseUrl"] + configuration["BrebApi:PaymentStatusUrl"];
                var username = configuration["BrebApi:Username"];
                var password = configuration["BrebApi:Password"];

                // Create Basic Auth header
                var authToken = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{username}:{password}")
                );

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", authToken);


                var json = JsonSerializer.Serialize(request);

                var content = new StringContent(json, Encoding.UTF8, "application/json");


                await _dbLog.SaveReqRespLog(new MwApiReqRespLog
                {
                    ServiceId = clientId,
                    RefNo = billerId,
                    JsonData = JsonSerializer.Serialize(request),
                    TranType = "BREB",
                    DataType = "Request",
                    EndPoint = configuration["BrebApi:PaymentStatusUrl"]
                });

                var response = await _httpClient.PostAsync(url, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                await _dbLog.SaveReqRespLog(new MwApiReqRespLog
                {
                    ServiceId = clientId,
                    RefNo = billerId,
                    JsonData = responseContent,
                    TranType = "BREB",
                    DataType = "Response",
                    EndPoint = configuration["BrebApi:PaymentStatusUrl"]
                });

                if (!response.IsSuccessStatusCode)
                {
                    // Optional: handle non-200 HTTP status
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                var responseObj = new BrebPaymentStatusResponse
                {
                    ResponseCode = root.GetProperty("ResponseCode").GetString(),
                    Message = root.GetProperty("Message").GetString(),
                    Status = root.GetProperty("Status").GetInt32()
                };

                var resultElement = root.GetProperty("Result");

                if (resultElement.ValueKind == JsonValueKind.Object)
                {
                    responseObj.Result =
                        JsonSerializer.Deserialize<BrebPaymentStatusResult>(
                            resultElement.GetRawText(), options);
                }
                else if (resultElement.ValueKind == JsonValueKind.Array)
                {
                    responseObj.Errors =
                        JsonSerializer.Deserialize<List<BrebErrorResult>>(
                            resultElement.GetRawText(), options);
                }
                return responseObj;
            }
            catch (Exception ex)
            {
                _dbLog.SaveErrorLog(
                new SaveErrorLog
                {
                    ControllerName = nameof(BrebService),
                    MethodName = nameof(GetPaymentStatusAsync),
                    ServiceId = clientId.ToString(),
                    ErrorMsg = ex.Message,
                    StackTrace = ex.StackTrace.ToString()
                }
                );
                return null;
            }

        }

        public async Task<BrebDisputeResponse?> UpdateDisputeAsync(BrebDisputeRequest request, int clientId, string BillerId)
        {
            try
            {
                BrebDisputeResponse brebDisputeResponse = new BrebDisputeResponse();
                var url = configuration["BrebApi:BaseUrl"] + configuration["BrebApi:DisputeUrl"];
                var username = configuration["BrebApi:Username"];
                var password = configuration["BrebApi:Password"];
                var authToken = Convert.ToBase64String(
                  Encoding.ASCII.GetBytes($"{username}:{password}")
              );

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", authToken);


                var json = JsonSerializer.Serialize(request);

                var content = new StringContent(json, Encoding.UTF8, "application/json");


                await _dbLog.SaveReqRespLog(new MwApiReqRespLog
                {
                    ServiceId = clientId,
                    RefNo = BillerId,
                    JsonData = JsonSerializer.Serialize(request),
                    TranType = "BREB",
                    DataType = "Request",
                    EndPoint = configuration["BrebApi:DisputeUrl"]
                });

                var response = await _httpClient.PostAsync(url, content);

                await _dbLog.SaveReqRespLog(new MwApiReqRespLog
                {
                    ServiceId = clientId,
                    RefNo = BillerId,
                    JsonData = JsonSerializer.Serialize(request),
                    TranType = "BREB",
                    DataType = "Response",
                    EndPoint = configuration["BrebApi:DisputeUrl"]
                });

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Optional: handle non-200 HTTP status
                    return null;
                }
                brebDisputeResponse = JsonSerializer.Deserialize<BrebDisputeResponse>(responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return brebDisputeResponse;
            }
            catch (Exception ex)
            {
                _dbLog.SaveErrorLog(
                    new SaveErrorLog
                    {
                        ControllerName = nameof(BrebService),
                        MethodName ="UpdateDisputeAsync",
                        ServiceId = clientId.ToString(),
                        ErrorMsg = ex.Message,
                        StackTrace = ex.StackTrace.ToString()
                    }
                );
                return null;
            }
            
        }
    }
}
