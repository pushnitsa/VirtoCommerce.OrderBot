namespace VirtoCommerce.OrderBot.Bots.Models
{
    public class ProductSearchCriteria
    {
        public string SearchPhrase { get; set; }

        public int Take { get; set; } = 20;
    }
}
