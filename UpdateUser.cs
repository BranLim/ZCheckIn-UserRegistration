using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using ZCheckIn.Backend.DTOs;

namespace ZCheckIn.Backend
{
    public static class UpdateUser
    {
        [FunctionName("UpdateUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("users", FileAccess.Write, Connection = "Azurite_Storage")] CloudBlobContainer outputContainer,
            ILogger log)
        {
            log.LogInformation("Registering User.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Invalid request body");
            }

            UserDTO data = JsonConvert.DeserializeObject<UserDTO>(requestBody);

            string uuid = data?.UUID;

            if (string.IsNullOrEmpty(uuid))
            {
                return new BadRequestObjectResult("missing UUID");
            }
            CloudBlockBlob blob = outputContainer.GetBlockBlobReference("user/" + uuid);
            if (blob == null)
            {
                return new NotFoundObjectResult(string.Format("no user {0} found", uuid));
            }

            await blob.UploadTextAsync(requestBody);
            log.LogInformation("User updated");


            return new OkObjectResult(string.Format("User {0} updated.", data?.Name));
        }
    }
}
