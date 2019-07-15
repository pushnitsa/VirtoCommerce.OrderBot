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
    public class SearchDialog : InterceptorExtendedBaseDialog
    {
        private readonly IProductFetcher _productFetcher;

        public SearchDialog(IMessageInterceptor messageInterceptor, IProductFetcher productFetcher) 
            : base(nameof(SearchDialog), messageInterceptor)
        {
            _productFetcher = productFetcher;

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
                var criteria = new dto.ProductSearchCriteria
                {
                    SearchPhrase = result
                };
                var products = await _productFetcher.GetProductsAsync(criteria);

                if (products.Length != 0)
                {
                    var cards = products.GetCards();

                    await stepContext.Context.SendActivityAsync(MessageFactory.Carousel(cards.Select(c => c.ToAttachment())), cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Nothing found"),
                        cancellationToken);
                }
            }

            return await stepContext.ReplaceDialogAsync(nameof(SearchDialog), cancellationToken: cancellationToken);
        }
    }
}
