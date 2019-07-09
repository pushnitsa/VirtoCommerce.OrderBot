using System.Threading.Tasks;

namespace VirtoCommerce.OrderBot.Security
{
    public interface IUserAuthState
    {
        Task<bool> IsAuthorizedAsync(string identifier);
    }
}
