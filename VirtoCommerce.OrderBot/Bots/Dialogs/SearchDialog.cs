using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrderBot.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.OrderBot.AutoRestClients.CatalogModuleApi.Models;
using VirtoCommerce.OrderBot.Bots.Dialogs.DialogInjector;
using VirtoCommerce.OrderBot.Infrastructure;

namespace VirtoCommerce.OrderBot.Bots.Dialogs
{
    public class SearchDialog : InterceptorExtendedBaseDialog
    {
        private readonly ICatalogModuleSearch _catalogModuleSearch;

        public SearchDialog(IMessageInterceptor messageInterceptor, ICatalogModuleSearch catalogModuleSearch) 
            : base(nameof(SearchDialog), messageInterceptor)
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
                    var cards = products
                        .Items
                        .Select(
                            item => new HeroCard
                            {
                                Images = new[] { new CardImage(item.ImgSrc) },
                                Text = item.Name,
                                Buttons = new[]
                                {
                                    new CardAction
                                    {
                                        Title = "Add to cart",
                                        Type = ActionTypes.ImBack,
                                        Value = $"{BotCommands.AddToCart}{item.Code}"
                                    }
                                }
                            })
                        .Select(
                            card => card.ToAttachment()
                            )
                        .ToList();

                    await stepContext.Context.SendActivityAsync(MessageFactory.Carousel(cards), cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Nothing found"), cancellationToken);
                }
                
            }

            return await stepContext.ReplaceDialogAsync(nameof(SearchDialog), cancellationToken: cancellationToken);
        }
    }
}
