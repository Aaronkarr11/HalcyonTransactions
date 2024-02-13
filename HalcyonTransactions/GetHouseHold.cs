using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using HalcyonCore.SharedEntities;
using Azure.Data.Tables;
using Azure;

namespace HalcyonTransactions
{
    public class GetHouseHold
    {
        private readonly IConfiguration _configuration;

        public GetHouseHold(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("GetHouseHold")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                HouseHoldMember RequestObject = JsonConvert.DeserializeObject<HouseHoldMember>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}HouseHold");
                await client.CreateIfNotExistsAsync();

                Pageable<HouseHoldMemberTableEntity> entities = client.Query<HouseHoldMemberTableEntity>();

                List<HouseHoldMember> HouseholdList = new List<HouseHoldMember>();

                foreach (var household in entities)
                {
                    HouseHoldMember householdModel = new HouseHoldMember();

                    householdModel.Name = household.Name;
                    householdModel.Email = household.Email;
                    householdModel.PartitionKey = household.PartitionKey;
                    householdModel.RowKey = household.RowKey;
                    householdModel.PhoneNumber = household.PhoneNumber;
                    householdModel.DeviceName = household.DeviceName;
                    HouseholdList.Add(householdModel);
                }
                return new OkObjectResult(JsonConvert.SerializeObject(HouseholdList));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
