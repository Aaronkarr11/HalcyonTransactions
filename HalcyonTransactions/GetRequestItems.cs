using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HalcyonCore.SharedEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Azure.Data.Tables;
using Azure;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace HalcyonTransactions
{
    public class GetRequestItems
    {
        private readonly IConfiguration _configuration;

        public GetRequestItems(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("GetRequestItems")]
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
