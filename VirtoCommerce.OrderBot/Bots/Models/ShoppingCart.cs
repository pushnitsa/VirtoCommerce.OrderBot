namespace VirtoCommerce.OrderBot.Bots.Models
{
    public class ShoppingCart
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public LineItem[] Items { get; set; }
    }
}
