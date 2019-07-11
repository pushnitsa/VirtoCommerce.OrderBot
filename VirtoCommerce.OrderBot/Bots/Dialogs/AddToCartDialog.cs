using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace VirtoCommerce.OrderBot.Bots.Dialogs
{
    public class AddToCartDialog : ComponentDialog
    {
        public AddToCartDialog() 
            : base(nameof(AddToCartDialog))
        {
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                RequestCountAsync,
                ConfirmAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> RequestCountAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter quantity"),
                RetryPrompt = MessageFactory.Text("Please enter a correct integer number")
            };

            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), options, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = (int) stepContext.Result;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(response.ToString()), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
