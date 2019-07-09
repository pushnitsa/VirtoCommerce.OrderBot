using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace VirtoCommerce.OrderBot.Bots
{
    public class BotAccessors
    {
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; internal set; }
    }
}
