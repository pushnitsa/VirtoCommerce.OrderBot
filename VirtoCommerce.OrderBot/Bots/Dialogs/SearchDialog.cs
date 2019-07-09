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
    public class SearchDialog : ComponentDialog
    {
        private readonly ICatalogModuleSearch _catalogModuleSearch;

        public SearchDialog(ICatalogModuleSearch catalogModuleSearch) 
            : base(nameof(SearchDialog))
        {
            _catalogModuleSearch = catalogModuleSearch;

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SearchPromptAsync,
                SearchAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SearchPromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = new PromptOptions
            {
                Prompt = MessageFactory.Text("Type anything to search")
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> SearchAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (string) stepContext.Result;

            if (!string.IsNullOrEmpty(result))
            {
                var searchCriteria = new ProductSearchCriteria
                {
                    SearchPhrase = result
                };

                var products = await _catalogModuleSearch.SearchProductsAsync(searchCriteria, cancellationToken);

                if (products.Items != null)
                {
                    var cards = new List<Attachment>();
                    foreach (var item in products.Items)
                    {
                        var card = new HeroCard
                        {
                            Images = new[] { new CardImage(item.ImgSrc) },
                            Text = item.Name
                        };

                        cards.Add(card.ToAttachment());
                    }

                    await stepContext.Context.SendActivityAsync(MessageFactory.Carousel(cards), cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Nothing to find"), cancellationToken);
                }
                
            }

            return await stepContext.ReplaceDialogAsync(nameof(SearchDialog), cancellationToken: cancellationToken);
        }
    }
}
