using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrderBot.Bots.Dialogs.DialogInjector;
using VirtoCommerce.OrderBot.Bots.Models.Converters;
using VirtoCommerce.OrderBot.Fetcher;
using dto = VirtoCommerce.OrderBot.Bots.Models;

namespace VirtoCommerce.OrderBot.Bots.Dialogs
{
    public class CatalogDialog : InterceptorExtendedBaseDialog
    {
        private readonly IProductFetcher _productFetcher;

        public CatalogDialog(
            IMessageInterceptor messageInterceptor,
            IProductFetcher productFetcher,
            SearchDialog searchDialog
            ) 
            : base(nameof(CatalogDialog), messageInterceptor)
        {
            _productFetcher = productFetcher;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GreetingsMessageAsync
            }));
            AddDialog(searchDialog);

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GreetingsMessageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var criteria = new dto.ProductSearchCriteria
            {
                Take = 20
            };

            var products = await _productFetcher.GetProductsAsync(criteria);

            var cards = products.GetCards();

            await stepContext.Context.SendActivityAsync(MessageFactory.Carousel(cards.Select(c => c.ToAttachment())), cancellationToken);
            
            return await stepContext.BeginDialogAsync(nameof(SearchDialog), cancellationToken: cancellationToken);
        }
    }
}
