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

        private bool ProcessReadyToStartResult(WaterfallStepContext stepContext) {
            var doitornot = stepContext.Result as FoundChoice;
            return (doitornot != null && doitornot.Index == 0);
        }

        private async Task<InsuranceQuestionaire> ProcessInsuranceRequesteeTypeResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var insuranceRequesteeTypeChoice = (FoundChoice)stepContext.Result;
            var insuranceQuestionaire = await _insuranceQuestionaireAccessor.GetAsync(stepContext.Context, () => new InsuranceQuestionaire(), cancellationToken);
            if (insuranceRequesteeTypeChoice != null) 
                insuranceQuestionaire.InsuranceRequesteeType = insuranceRequesteeTypeChoice.Value;

            return insuranceQuestionaire;
        }

        private async Task<InsuranceQuestionaire> ProcessHouseholdCountResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var insuranceQuestionaire = await _insuranceQuestionaireAccessor.GetAsync(stepContext.Context, () => new InsuranceQuestionaire(), cancellationToken);
            var householdCount = (int)stepContext.Result;
            insuranceQuestionaire.FamilyMemberCount = householdCount;
            return insuranceQuestionaire;
        }

        private async Task<InsuranceQuestionaire> ProcessZipCodeResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var zipCode = (int)stepContext.Result;
            var insuranceQuestionaire = await _insuranceQuestionaireAccessor.GetAsync(stepContext.Context, () => new InsuranceQuestionaire(), cancellationToken);
            insuranceQuestionaire.ZipCode = zipCode;
            return insuranceQuestionaire;
        }

        //private async Task<InsuranceQuestionaire> ProcessFamilyMemberCountResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        //    var familyMemberCount = (int)stepContext.Result;
        //    var insuranceQuestionaire = await _accessors.InsuranceQuestionaire.GetAsync(stepContext.Context, () => new InsuranceQuestionaire(), cancellationToken);
        //    insuranceQuestionaire.FamilyMemberCount = familyMemberCount;
        //    return insuranceQuestionaire;
        //}

        private async Task<InsuranceQuestionaire> ProcessKidCountResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var kidCount = (int)stepContext.Result;
            var insuranceQuestionaire = await _insuranceQuestionaireAccessor.GetAsync(stepContext.Context, () => new InsuranceQuestionaire(), cancellationToken);
            insuranceQuestionaire.ChildCount = kidCount;
            return insuranceQuestionaire;
        }

        private async Task<InsuranceQuestionaire> ProcessHouseIncomeResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var houseIncome = (int)stepContext.Result;
            var insuranceQuestionaire = await _insuranceQuestionaireAccessor.GetAsync(stepContext.Context, () => new InsuranceQuestionaire(), cancellationToken);
            insuranceQuestionaire.HouseIncome = houseIncome;
            return insuranceQuestionaire;
        }

        private async Task<InsuranceQuestionaire> ProcessBudgetPreferenceResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var budgetPreferenceChoice = (FoundChoice)stepContext.Result;
            var insuranceQuestionaire = await _insuranceQuestionaireAccessor.GetAsync(stepContext.Context, () => new InsuranceQuestionaire(), cancellationToken);
            if (budgetPreferenceChoice != null)
                insuranceQuestionaire.BudgetPreference = budgetPreferenceChoice.Value;

            return insuranceQuestionaire;
        }
        
    }
}
