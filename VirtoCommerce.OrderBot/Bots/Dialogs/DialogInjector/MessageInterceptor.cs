using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace VirtoCommerce.OrderBot.Bots.Dialogs.DialogInjector
{
    public class MessageInterceptor : IMessageInterceptor
    {
        private readonly IMessageHandlerReciever _messageHandlerReciever;

        public MessageInterceptor(IMessageHandlerReciever messageHandlerReciever)
        {
            _messageHandlerReciever = messageHandlerReciever;
        }

        public async Task<DialogTurnResult> InterceptAsync(string message, DialogContext dialogContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var handler = _messageHandlerReciever.GetHandler(message);

            if (handler != null)
            {
                return await handler.HandleAsync(message, dialogContext, cancellationToken);
            }

            return null;
        }
    }
}
