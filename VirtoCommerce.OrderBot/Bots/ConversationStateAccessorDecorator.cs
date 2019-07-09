using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace VirtoCommerce.OrderBot.Bots
{
    public class ConversationStateAccessorDecorator : IConversationStateAccessor
    {
        private readonly ConversationState _conversationState;

        public ConversationStateAccessorDecorator(ConversationState conversationState)
        {
            _conversationState = conversationState;
        }

        public async Task<T> GetAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken)) where  T : new()
        {
            var conversationStatePropertyAccessor = _conversationState.CreateProperty<T>(nameof(T));

            return await conversationStatePropertyAccessor.GetAsync(turnContext, () => new T(), cancellationToken);
        }
    }
}
