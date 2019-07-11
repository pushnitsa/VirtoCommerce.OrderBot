using VirtoCommerce.OrderBot.Bots.Dialogs.DialogInjector.Handlers;

namespace VirtoCommerce.OrderBot.Bots.Dialogs.DialogInjector
{
    public interface IMessageHandlerStorage
    {
        void AddHandler(IMessageHandler messageHandler);
    }
}
