﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.OrderBot.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.OrderBot.AutoRestClients.CatalogModuleApi.Models;
using VirtoCommerce.OrderBot.AutoRestClients.PricingModuleApi;
using VirtoCommerce.OrderBot.Bots.Models;
using VirtoCommerce.OrderBot.Builder;

namespace VirtoCommerce.OrderBot.Bots.Dialogs
{
    public class AddToCartDialog : ComponentDialog
    {
        private readonly ConversationState _conversationState;
        private readonly ICartBuilderFactory _cartBuilderFactory;
        private readonly ICatalogModuleSearch _catalogModuleSearch;
        private readonly IPricingModule _pricingModule;

        public AddToCartDialog(
            ICartBuilderFactory cartBuilderFactory, 
            ICatalogModuleSearch catalogModuleSearch,
            IPricingModule pricingModule,
            ConversationState conversationState) 
            : base(nameof(AddToCartDialog))
        {
            _conversationState = conversationState;
            _cartBuilderFactory = cartBuilderFactory;
            _catalogModuleSearch = catalogModuleSearch;
            _pricingModule = pricingModule;

            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                RequestQuantityAsync,
                ConfirmAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> RequestQuantityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var productCode = (string) stepContext.Options;
            stepContext.Values["code"] = productCode;

            var options = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter quantity"),
                RetryPrompt = MessageFactory.Text("Please enter a correct integer number")
            };

            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), options, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var code = (string) stepContext.Values["code"];
            var quantity = (int) stepContext.Result;

            var userProfileAccessor = _conversationState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            var productSearchCriteria = new ProductSearchCriteria
            {
                SearchPhrase = code
            };

            var searchResult = await _catalogModuleSearch.SearchProductsAsync(productSearchCriteria, cancellationToken);
            var product = searchResult.Items.FirstOrDefault();

            if (product != null)
            {
                var priceSearchResult = await _pricingModule.SearchProductPricesAsync(criteriaproductId: product.Id, cancellationToken: cancellationToken);
                var productPrice = priceSearchResult.Results.FirstOrDefault();

                using (var cartBuilder = _cartBuilderFactory.Create(userProfile.Customer))
                {
                    var lineItem = new LineItem
                    {
                        CatalogId = product.CatalogId,
                        CategoryId = product.CategoryId,
                        Code = product.Code,
                        Name = product.Name,
                        ProductId = product.Id,
                        ListPrice = Convert.ToDecimal(productPrice?.Prices?.FirstOrDefault()?.List)
                    };

                    await cartBuilder.AddCartItemAsync(lineItem, quantity);
                    await cartBuilder.SaveCartAsync();

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Product added to cart"), cancellationToken);
                }
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
