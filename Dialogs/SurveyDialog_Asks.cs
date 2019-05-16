// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace CDPHP.Bot.Survey {
    public partial class SurveyDialog : ComponentDialog {

        //Ready
        private async Task<DialogTurnResult> AskReadyToStartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            return await stepContext.PromptAsync("AskReadyToStart", ReadyToStartPromptOptions(), cancellationToken);
        }

        //Ready -> Insurance Request Type
        private async Task<DialogTurnResult> HandleReadyToStart_AskInsuranceRequesteeTypeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            if (ProcessReadyToStartResult(stepContext)) {
                return await stepContext.PromptAsync("AskInsuranceRequesteeType",
                                                     InsuranceRequesteeTypePromptOptions(),
                                                     cancellationToken);
            } else {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Call us at 1-900-MIX-ALOT when you are ready."), cancellationToken);

                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        //Insurance Request Type -> Zipcode
        private async Task<DialogTurnResult> HandleInsuranceRequesteeType_AskZipCodeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var insuranceQuestionaire = await ProcessInsuranceRequesteeTypeResultAsync(stepContext, cancellationToken);
            return await stepContext.PromptAsync("AskZipCode", ZipCodePromptOptions(), cancellationToken);
        }

        //Zipcode -> Household Size
        private async Task<DialogTurnResult> HandleZipCode_AskHouseholdCountAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var insuranceQuestionaire = await ProcessZipCodeResultAsync(stepContext, cancellationToken);

            //TODO: Connect to API with ZipCode and determine availability

            //OVERRIDE
            insuranceQuestionaire.ZipCode = 12205;
            var town = "Albany";

            if (insuranceQuestionaire.ZipCode == 99999) {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Sorry, we don't service your area.  :("), cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            } else {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Looking good, {town}!"), cancellationToken);
            }

            return await stepContext.PromptAsync("AskHouseholdCount", HouseholdCountPromptOptions(), cancellationToken);
        }

        //Household -> Kid Count / IncomeWF
        private async Task<DialogTurnResult> HandleHouseholdCount_AskKidCountAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var insuranceQuestionaire = await ProcessHouseholdCountResultAsync(stepContext, cancellationToken);

            //OVERRIDE
            insuranceQuestionaire.FamilyMemberCount = 4;


            if (insuranceQuestionaire.InsuranceRequesteeType.Contains("Kid"))
                return await stepContext.PromptAsync("AskKidCount", KidCountPromptOptions(), cancellationToken);

            // No need to ask about friggin' kids, they aren't covered.
            await stepContext.EndDialogAsync(null, cancellationToken);
            return await stepContext.BeginDialogAsync("IncomeBudgetWF", null, cancellationToken);
        }

        //Kid Count -> IncomeWF
        private async Task<DialogTurnResult> HandleKidCountAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var insuranceQuestionaire = await ProcessKidCountResultAsync(stepContext, cancellationToken);

            await stepContext.EndDialogAsync(null, cancellationToken);
            return await stepContext.BeginDialogAsync("IncomeBudgetWF", null, cancellationToken);
        }

        // --> Income
        private async Task<DialogTurnResult> AskIncomeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            return await stepContext.PromptAsync("AskHouseIncome", IncomePromptOptions(), cancellationToken);
        }

        //Income -> Budget Preference

        private async Task<DialogTurnResult> HandleIncome_AskBudgetPreferenceAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var insuranceQuestionaire = await ProcessHouseIncomeResultAsync(stepContext, cancellationToken);

            //OVERRIDE
            insuranceQuestionaire.HouseIncome = 50000;

            if (insuranceQuestionaire.HouseIncome <= 50000) {
                var foundChoice = new FoundChoice {
                    Index = 0,
                    Score = 1,
                    Value = "I prefer a lower monthly payment even if it means I have to pay more when I'm receiving care."
                };

                return await stepContext.NextAsync(foundChoice, cancellationToken);
            }

            return await stepContext.PromptAsync("AskBudgetPreference", BudgetPreferencePromptOptions(), cancellationToken);
        }

        //Budget Preference -> Summary
        private async Task<DialogTurnResult> HandleBudgetPreference_AskToDiscussAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var insuranceQuestionaire = await ProcessBudgetPreferenceResultAsync(stepContext, cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Great! It looks like you and your spouse are eligible for the \"Essential Plan 1\" - $20 / month for each of you. "), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"And, your 2 children are eligible for subsidized Child Health Plus: $0/month with no additional costs."), cancellationToken);


            return await stepContext.PromptAsync("AskToDiscussAsync", ToDiscussPromptOptions(), cancellationToken);

            //TODO: Include choice for:

            //Want to discuss further with one of our experts?

            //[Yes, Schedule a Call][Yes, Call Me Now][Not Right Now]

            //return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }
}
