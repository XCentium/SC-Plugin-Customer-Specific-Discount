
Sitecore Commerce Engine Customer Specific Discount plugin (Sitecore Commerce 9)
======================================

This plugin allows the user to allocate specific discount percentage to a customer via BizFT tool. 
- It is very easy to integrate or extend to fit your needs.
- You simply need to create a composer template and assign it to Customers entity


Sponsor
=======
This plugin was sponsored and created by Xcentium.

How to Install
==============

1. Copy it to your Sitecore Commerce Engine Solution and add it as a project 


2. Add it as a dependency to your Sitecore.Commerce.Engine project

3. To your ServiceCollectionExtensions.cs class, add the line below towards the end:

	              .Add<CustomerSpecificDiscountBlock>().Before<CalculateCartTotalsBlock>()

	So that the confic section looks somewhat like below:

```
        public static ISitecoreServicesConfiguration ConfigureCommercePipelines(this ISitecoreServicesConfiguration services)
        {
            services.Pipelines(config => config
                .ConfigurePipeline<IPopulateValidateCartPipeline>(builder => builder
                    .Add<ValidateCartCouponsBlock>().After<PopulateCartLineItemsBlock>())

                .ConfigurePipeline<ICalculateCartLinesPipeline>(builder => builder
                    .Add<CalculateCartLinesSubTotalsBlock>()
                    .Add<CalculateCartLinesFulfillmentBlock>()
                    .Add<UpdateCartLineFulfillmentChargeBlock>().After<CalculateCartLinesFulfillmentBlock>()
                    .Add<CalculateCartLinesPromotionsBlock>()
                    .Add<CalculateCartLinesTaxBlock>()
                    .Add<CalculateCartLinesTotalsBlock>())

               .ConfigurePipeline<ICalculateCartPipeline>(builder => builder
                    .Add<CalculateCartSubTotalsBlock>()
                    .Add<CalculateCartFulfillmentBlock>()
                    .Add<UpdateCartFulfillmentChargeBlock>().After<CalculateCartFulfillmentBlock>()
                    .Add<CalculateCartPromotionsBlock>()
                    .Add<CalculateCartTaxBlock>()
                    .Add<CalculateCartTotalsBlock>()
                    .Add<CustomerSpecificDiscountBlock>().Before<CalculateCartTotalsBlock>()
                    .Add<CalculateCartPaymentsBlock>()
                    .Add<WriteCartTotalsToContextBlock>())

                .ConfigurePipeline<IAddPaymentsPipeline>(builder =>
                    builder.Add<ValidateCartHasFulfillmentBlock>().After<ValidateCartAndPaymentsBlock>()));

            return services;
        }

```

4. It is now ready to use. Whatever % discount value on the customer account will always apply. 

Note:
=====

- If you have any questions, comment or need us to help install, extend or adapt to your needs, do not hesitate to reachout to us at XCentium.




