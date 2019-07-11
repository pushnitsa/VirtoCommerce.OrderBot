using VirtoCommerce.OrderBot.Bots.Dialogs.DialogInjector.Handlers;

namespace VirtoCommerce.OrderBot.Bots.Dialogs.DialogInjector
{
    public interface IMessageHandlerReciever
    {
        IMessageHandler GetHandler(string message);
    }
}
