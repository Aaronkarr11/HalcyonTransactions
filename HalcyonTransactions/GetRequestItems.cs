using Azure;
using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.Azure.Functions.Worker;

namespace HalcyonTransactions
{
    public class GetRequestItems
    {
        private readonly IConfiguration _configuration;

        public GetRequestItems(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("GetRequestItems")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                RequestItemsTableTemplate RequestObject = JsonConvert.DeserializeObject<RequestItemsTableTemplate>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}RequestItems");
                await client.CreateIfNotExistsAsync();

                Pageable<RequestItemsTableEntity> entities = client.Query<RequestItemsTableEntity>();

                List<RequestItemsModel> requestItemList = new List<RequestItemsModel>();

                foreach (var item in entities.ToList().Where(o => o.IsFulfilled == 0))
                {
                    RequestItemsModel model = new RequestItemsModel();

            
                    model.PartitionKey = item.PartitionKey;
                    model.RowKey = item.RowKey;
                    model.Title = item.Title;
                    model.IsFulfilled = item.IsFulfilled;
                    model.DesiredDate = item.DesiredDate;
                    model.DesiredDateDisplay = Convert.ToDateTime(model.DesiredDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    model.ReasonDescription = item.ReasonDescription;
                    model.DeviceName = item.DeviceName;
                    requestItemList.Add(model);
                }
                return new OkObjectResult(JsonConvert.SerializeObject(requestItemList));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
