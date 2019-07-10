using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrderBot.Bots.Models;
using VirtoCommerce.OrderBot.Security;

namespace VirtoCommerce.OrderBot.Bots.Dialogs
{
    public class AuthDialog : ComponentDialog
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IConversationStateAccessor _conversationStateAccessor;

        public AuthDialog(
            IAuthorizationService authService, 
            IConversationStateAccessor conversationStateAccessor, 
            MainDialog mainDialog
            ) 
            : base(nameof(AuthDialog))
        {
            _authorizationService = authService;
            _conversationStateAccessor = conversationStateAccessor;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(mainDialog);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AuthPlease,
                TryToAuth
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AuthPlease(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var profile = await _conversationStateAccessor.GetAsync<UserProfile>(stepContext.Context, cancellationToken);

            if (!await _authorizationService.IsAuthorizedAsync(profile.UserId))
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = stepContext.Context.Activity.CreateReply($"Your's identifier is: {profile.UserId}.{Environment.NewLine}Please send it to VC administrator."),
                    Choices = new[] { new Choice("Try auth") }
                };

                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
            }
            else
            {
                await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> TryToAuth(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = (stepContext.Result as FoundChoice)?.Value;

            return await stepContext.ReplaceDialogAsync(nameof(AuthDialog), cancellationToken: cancellationToken);
            
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = new CancellationToken())
        {

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }
    }
}
