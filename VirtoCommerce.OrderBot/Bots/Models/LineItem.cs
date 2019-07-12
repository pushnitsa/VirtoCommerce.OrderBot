namespace VirtoCommerce.OrderBot.Bots.Models
{
    public class LineItem
    {
        public string Code { get; set; }

        public string ProductId { get; set; }

        public string Name { get; set; }

        public decimal ListPrice { get; set; }

        public string CatalogId { get; set; }

        public string CategoryId { get; set; }
    }
}
