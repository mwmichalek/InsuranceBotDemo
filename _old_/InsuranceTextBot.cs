// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples {
    public partial class InsuranceTextBot : IBot {
        private const string WelcomeText = "Hi!";

        private readonly InsuranceTextBotAccessors _accessors;

        private DialogSet _dialogs;

        public InsuranceTextBot(InsuranceTextBotAccessors accessors) {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            _dialogs = new DialogSet(accessors.ConversationDialogState);

            //Ready
            //Insurance Request TypeType
            //Zipcode
            //Household Size
            //Kids Count
            //Income
            //Budget Preferences





            var readyAndInsuranceTypeWF = new WaterfallStep[] {
                AskReadyToStartAsync,
                HandleReadyToStart_AskInsuranceRequesteeTypeAsync,
                HandleInsuranceRequesteeType_AskZipCodeAsync,
                HandleZipCode_AskHouseholdCountAsync,
                HandleHouseholdCount_AskKidCountAsync,
                HandleKidCountAsyc
            };



            var incomeBudgetWF = new WaterfallStep[] {
                AskIncomeAsync,
                HandleIncome_AskBudgetPreferenceAsync,
                HandleBudgetPreference_AskToDiscussAsync
            };



            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            _dialogs.Add(new WaterfallDialog("ReadyAndInsuranceTypeWF", readyAndInsuranceTypeWF));
            _dialogs.Add(new WaterfallDialog("IncomeBudgetWF", incomeBudgetWF));



            _dialogs.Add(new ChoicePrompt("AskReadyToStart"));
            _dialogs.Add(new ChoicePrompt("AskInsuranceRequesteeType", InsuranceRequesteeTypeValidatorAsync));
            
            _dialogs.Add(new NumberPrompt<int>("AskZipCode", ZipCodeValidatorAsync));
            _dialogs.Add(new NumberPrompt<int>("AskHouseholdCount"));
            _dialogs.Add(new NumberPrompt<int>("AskKidCount", KidCountValidatorAsync));
            _dialogs.Add(new NumberPrompt<int>("AskHouseIncome"));
            _dialogs.Add(new ChoicePrompt("AskBudgetPreference", BudgetPreferenceValidatorAsync));
            _dialogs.Add(new ChoicePrompt("AskToDiscussAsync", ToDiscussValidatorAsync));
        }
        
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken)) {
            if (turnContext == null) throw new ArgumentNullException(nameof(turnContext));

            if (turnContext.Activity.Type == ActivityTypes.Message) {
                if (string.IsNullOrEmpty(turnContext.Activity.Text)) {
                    turnContext.Activity.Text = ((JObject)turnContext.Activity.Value).ToString();
                }

                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                //if (results.Status == DialogTurnStatus.Empty) {
                //    await dialogContext.BeginDialogAsync("readyAndInsuranceTypeWF", null, cancellationToken);
                //}

            } else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate) {
                //if (turnContext.Activity.MembersAdded != null) {
                //    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                //}
            } else if (turnContext.Activity.Type == ActivityTypes.Event) {
                var eventActivity = turnContext.Activity.AsEventActivity();
                if (eventActivity.Name == "setUserIdEvent") {
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            } else if (turnContext.Activity.Type == "connected") {
                //var reply = turnContext.Activity.CreateReply();
                //reply.Text = "I hear you, chilax";
                //await turnContext.SendActivityAsync(reply, cancellationToken);
                //await SendWelcomeMessageAsync(turnContext, cancellationToken);
            } else {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected", cancellationToken: cancellationToken);
            }

            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        private async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken) {
            foreach (var member in turnContext.Activity.MembersAdded) {
                if (member.Id != turnContext.Activity.Recipient.Id) {
                    var reply = turnContext.Activity.CreateReply();
                    reply.Text = WelcomeText;
                    await turnContext.SendActivityAsync(reply, cancellationToken);

                    var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                    var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                    if (results.Status == DialogTurnStatus.Empty) {
                        await dialogContext.BeginDialogAsync("ReadyAndInsuranceTypeWF", null, cancellationToken);
                        //await dialogContext.BeginDialogAsync(nameof(AskReadyToStartAsync), null, cancellationToken);

                        //await dialogContext.PromptAsync(nameof(AskReadyToStartAsync), ReadyToStartPromptOptions(), cancellationToken);

                    }
                }
            }
        }



        private static Attachment CreateAdaptiveCardAttachment(string filePath) {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment() {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}
