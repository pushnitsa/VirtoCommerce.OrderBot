using VirtoCommerce.OrderBot.Infrastructure.Cart.Model;

namespace VirtoCommerce.OrderBot.Infrastructure.Cart
{
    public interface ICartBuilder
    {
        void AddItemToCart(Product product);

        void Save();
    }
}
