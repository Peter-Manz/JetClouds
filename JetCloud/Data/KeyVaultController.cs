using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;

namespace JetCloud.Data
{
    public class KeyVaultController
    {
        // start of adapted code https://www.c-sharpcorner.com/article/azure-key-vault-configuration-and-implementation-using-net-core-7-web-api/

        private readonly IConfiguration _configuration;
        private readonly SecretClient _secretClient;

        public KeyVaultController(IConfiguration configuration)
        {
            _configuration = configuration;
            var kvURL = configuration["KeyVaultURL"];
            _secretClient = new SecretClient(new Uri(kvURL), new DefaultAzureCredential());

        }

        [HttpGet]
        public async Task<String> GetSecretConnectionString()
        {

            var sqlConnString = await _secretClient.GetSecretAsync("AppDbContext");
            var connectionString = sqlConnString.Value.Value;
            return connectionString;
        }
        //end of adapted code
    }
}
