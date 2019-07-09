using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrderBot.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.OrderBot.AutoRestClients.CatalogModuleApi.Models;

namespace VirtoCommerce.OrderBot.Bots.Dialogs
{
    public class CatalogDialog : ComponentDialog
    {
        private readonly ICatalogModuleSearch _catalogModuleSearch;

        public CatalogDialog(
            ICatalogModuleSearch catalogModuleSearch, 
            SearchDialog searchDialog
            ) 
            : base(nameof(CatalogDialog))
        {
            _catalogModuleSearch = catalogModuleSearch;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GreetingsMessageAsync
            }));
            AddDialog(searchDialog);

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GreetingsMessageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var criteria = new ProductSearchCriteria
            {
                Take = 20
            };

            var productSearchResult = await _catalogModuleSearch.SearchProductsAsync(criteria, cancellationToken);

            var cards = new List<Attachment>();
            foreach (var item in productSearchResult.Items)
            {
                var card = new HeroCard
                {
                    Images = new[] { new CardImage(item.ImgSrc) },
                    Title = item.Name
                };

                cards.Add(card.ToAttachment());
            }

            var attachments = MessageFactory.Carousel(cards);

            await stepContext.Context.SendActivityAsync(attachments, cancellationToken);
            
            return await stepContext.BeginDialogAsync(nameof(SearchDialog), cancellationToken: cancellationToken);
        }
    }
}
