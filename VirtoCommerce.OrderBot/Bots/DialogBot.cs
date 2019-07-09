using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrderBot.Bots.Dialogs.Extensions;
using VirtoCommerce.OrderBot.Bots.Models;

namespace VirtoCommerce.OrderBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        private readonly Dialog _dialog;
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly ILogger _logger;
        private readonly IConversationStateAccessor _conversationStateAccessor;

        public DialogBot(
            ConversationState conversationState, 
            UserState userState, 
            T dialog, 
            ILogger<DialogBot<T>> logger,
            IConversationStateAccessor conversationStateAccessor
            )
        {
            _conversationState = conversationState;
            _userState = userState;
            _dialog = dialog;
            _logger = logger;
            _conversationStateAccessor = conversationStateAccessor;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }


        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var profile = await _conversationStateAccessor.GetAsync<UserProfile>(turnContext, cancellationToken);

                    profile.UserId = member.Id;

                    await turnContext.SendActivityAsync(MessageFactory.Text("Greetings! Type something to begin."), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await _dialog.Run(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
    }
}
