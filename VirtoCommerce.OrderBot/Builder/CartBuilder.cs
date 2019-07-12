using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrderBot.AutoRestClients.CartModuleApi;
using VirtoCommerce.OrderBot.AutoRestClients.CartModuleApi.Models;
using VirtoCommerce.OrderBot.Bots.Models;
using api = VirtoCommerce.OrderBot.AutoRestClients.CartModuleApi.Models;
using dto = VirtoCommerce.OrderBot.Bots.Models;

namespace VirtoCommerce.OrderBot.Builder
{
    public class CartBuilder : ICartBuilder
    {
        private readonly ICartModule _cartModule;
 
        private readonly string _cartName = "Bot cart";

        private Customer _customer;
        private ShoppingCart _cart;
        private bool _isDisposed;

        private Customer Customer
        {
            get => _isDisposed ? throw new ObjectDisposedException(GetType().Name) : _customer;
            set => _customer = value ?? throw new ArgumentNullException(nameof(Customer));
        }

        private ShoppingCart Cart
        {
            get => _isDisposed ? throw new ObjectDisposedException(GetType().Name) : _cart;
            set => _cart = value;
        }

        public CartBuilder(ICartModule cartModule, Customer customer)
        {
            _cartModule = cartModule;
            Customer = customer;
        }

        public async Task AddCartItemAsync(dto.LineItem lineItem, int quantity)
        {
            await LoadOrCreateNewTransientCart();

            var existingLineItem = Cart.Items.FirstOrDefault(l => l.ProductId == lineItem.ProductId);

            if (existingLineItem != null)
            {
                existingLineItem.Quantity += Math.Max(1, quantity);
            }
            else
            {
                Cart.Items.Add(new api.LineItem
                {
                    CatalogId = lineItem.CatalogId,
                    CategoryId = lineItem.CategoryId,
                    Currency = Customer.Currency,
                    ListPrice = Convert.ToDouble(lineItem.ListPrice),
                    Name = lineItem.Name,
                    ProductId = lineItem.ProductId,
                    Quantity = Math.Max(1, quantity),
                    Sku = lineItem.Code
                });
            }
        }

        public async Task SaveCartAsync()
        {
            await LoadOrCreateNewTransientCart();

            if (string.IsNullOrEmpty(Cart.Id))
            {
                await _cartModule.CreateAsync(Cart);
            }
            else
            {
                await _cartModule.UpdateAsync(Cart);
            }
        }

        private async Task<ShoppingCart> SearchCartAsync()
        {
            var criteria = new ShoppingCartSearchCriteria
            {
                Currency = Customer.Currency,
                CustomerId = Customer.Id,
                StoreId = Customer.StoreId,
                Name = _cartName
            };

            var searchResult = await _cartModule.SearchAsync(criteria);

            return searchResult.Results.FirstOrDefault();
        }

        private ShoppingCart CreateCart()
        {
            return new ShoppingCart
            {
                CustomerId = Customer.Id,
                Name = _cartName,
                StoreId = Customer.StoreId,
                IsAnonymous = false,
                CustomerName = Customer.Name,
                Currency = Customer.Currency,
                Items = new List<api.LineItem>()
            };
        }

        private async Task LoadOrCreateNewTransientCart()
        {
            if (Cart == null)
            {
                Cart = await SearchCartAsync() ?? CreateCart();
            }   
        }

        public void Dispose()
        {
            _customer = null;
            _cart = null;
            _isDisposed = true;
        }
    }
}
