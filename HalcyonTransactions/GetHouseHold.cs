using Azure;
using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

namespace HalcyonTransactions
{
    public class GetHouseHold
    {
        private readonly IConfiguration _configuration;

        public GetHouseHold(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("GetHouseHold")]
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
