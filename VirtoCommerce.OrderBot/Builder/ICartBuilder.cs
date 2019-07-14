using System;
using System.Threading.Tasks;
using dto = VirtoCommerce.OrderBot.Bots.Models;

namespace VirtoCommerce.OrderBot.Builder
{
    public interface ICartBuilder : IDisposable
    {
        Task AddCartItemAsync(dto.LineItem lineItem, int quantity);

        Task<dto.LineItem[]> GetLineItemsFromCartAsync();

        Task SaveCartAsync();

        Task RemoveCartAsync();

        Task<dto.ShoppingCart> GetCartAsync();
    }
}
