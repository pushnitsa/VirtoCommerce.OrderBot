using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace VirtoCommerce.OrderBot.Bots
{
    public interface IConversationStateAccessor
    {
        Task<T> GetAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken)) where T : new();
    }
}
