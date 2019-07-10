using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VirtoCommerce.OrderBot.Bots.Middlewares
{
    public class AddCartMiddleware : IMiddleware
    {
        private const string AddCartCommand = "add-cart:";

        public AddCartMiddleware()
        {

        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = new CancellationToken())
        {
            turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                var responses = await nextSend().ConfigureAwait(false);

                if (ctx.Activity.Type == ActivityTypes.Message)
                {
                    var source = ctx.Activity.Text;

                    if (!string.IsNullOrEmpty(source) && source.StartsWith(AddCartCommand, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var itemName = source.Substring(AddCartCommand.Length);
                    }
                }

                return responses;
            });

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
