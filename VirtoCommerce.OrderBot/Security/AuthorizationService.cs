using System.Threading.Tasks;
using VirtoCommerce.OrderBot.AutoRestClients.CustomerModuleApi;
using VirtoCommerce.OrderBot.AutoRestClients.CustomerModuleApi.Models;

namespace VirtoCommerce.OrderBot.Security
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ICustomerModule _customerModuleApi;

        public AuthorizationService(ICustomerModule customerModule)
        {
            _customerModuleApi = customerModule;
        }

        public async Task<bool> IsAuthorizedAsync(string identifier)
        {
            var criteria = new MembersSearchCriteria
            {
                SearchPhrase = $"botusername:{identifier}"
            };

            var result = await _customerModuleApi.SearchContactsAsync(criteria);
            
            return result.Results.Count != 0;
        }
    }
}
