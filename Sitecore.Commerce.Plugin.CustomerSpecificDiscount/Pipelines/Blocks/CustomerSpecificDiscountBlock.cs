using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.EntityViews.Commands;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.CustomerSpecificDiscount.Pipelines.Blocks
{
    public class CustomerSpecificDiscountBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        private readonly GetEntityViewCommand _getEntityViewCommand;
        public CustomerSpecificDiscountBlock(GetEntityViewCommand getEntityViewCommand)
        {
            _getEntityViewCommand = getEntityViewCommand;
        }
        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            // if cart is null
            Condition.Requires<Cart>(arg).IsNotNull<Cart>($"{(object)(this.Name)}: Cart cannot be null.");

            // cartlines cannot be null
            Condition.Requires<IList<CartLineComponent>>(arg.Lines).IsNotNull<IList<CartLineComponent>>($"{(object)(this.Name)}: The cart's lines cannot be null");

            if (!arg.Lines.Any<CartLineComponent>()) { return arg; }

            // get contact from Cart. If contact is null return. If contact is not registered, return. else get customer View and check for the child view with discount field
            var contactComponent = arg.GetComponent<ContactComponent>();

            if (!contactComponent.IsRegistered)
            {
                return arg;
            }

            // Get the customer master entity view
            var entityView = await _getEntityViewCommand.Process(context.CommerceContext, contactComponent.ShopperId, new int?(arg.EntityVersion), context.GetPolicy<KnownCustomerViewsPolicy>().Master, "", "");

            if (entityView != null && entityView.ChildViews.Any())
            {
                // if no discount field return arg
                var customerDiscountView = entityView.ChildViews.FirstOrDefault(x => x.Name == "CustomerDiscount") as EntityView;
                if (customerDiscountView == null)
                {
                    return arg;
                }

                // if discount field is blank, return arg
                var properties = customerDiscountView.Properties;
                var discountProperty = properties.FirstOrDefault(x => x.DisplayName == "Discount");
                if (discountProperty == null)
                {
                    return arg;
                }


                var discount = GetFirstDecimalFromString(discountProperty.Value);
                if (discount <= 0 || discount >= 100)
                {
                    return arg;
                }

                // if discount field is not blank and has a value, try get its decimal value.  if successful, add adjustment to cart.
                var disountAmount = GetFirstDecimalFromString(arg.Totals.SubTotal.ToString()) *
                                    (100.00m - discount);

                var currencyCode = context.CommerceContext.CurrentCurrency();
                if (!arg.Adjustments.Any(a =>
                    a.Name.Equals("CustomerDiscount", StringComparison.OrdinalIgnoreCase)))
                {
                    arg.Adjustments.Add(new CartLevelAwardedAdjustment
                    {
                        Name = "CustomerDiscount",
                        DisplayName = "CustomerDiscount",
                        Adjustment = new Money(currencyCode, -1 * disountAmount),
                        AdjustmentType = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount,
                        AwardingBlock = this.Name,
                        IsTaxable = false
                    });
                }
            }

            return arg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private decimal GetFirstDecimalFromString(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0.00M;
            var decList = Regex.Split(str, @"[^0-9\.]+").Where(c => c != "." && c.Trim() != "").ToList();
            var decimalVal = decList.Any() ? decList.FirstOrDefault() : string.Empty;

            if (string.IsNullOrEmpty(decimalVal)) return 0.00M;
            decimal decimalResult = 0;
            decimal.TryParse(decimalVal, out decimalResult);
            return decimalResult;
        }
    }
}
