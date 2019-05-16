// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// *********************************************************************
//
// https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-primitive-prompts?view=azure-bot-service-4.0&tabs=csharp
//
// *********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CDPHP.Bot.Survey;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples {
    public partial class InsuranceTextBot : IBot {

        private PromptOptions ReadyToStartPromptOptions() {
            var choices = new List<string> {
                "Let's Go",
                "I Prefer Humans"
            };
            return new PromptOptions {
                Choices = ChoiceFactory.ToChoices(choices),
                Prompt = MessageFactory.Text("I've got just a few questions to help quote the best plan for your needs."),
                RetryPrompt = MessageFactory.Text("I'm sorry I didn't understand your responce."),


            };
        }


        private const string JustMe = "Just Me";
        private const string MeSpouse = "Me & Spouse";
        private const string MeSpouseKids = "Me, Spouse & Kid(s)";
        private const string MeKids = "Me & Kid(s)";
        private const string JustKids = "Just Kid(s)";
 
        private PromptOptions InsuranceRequesteeTypePromptOptions() {
            var choices = new List<string> {
                JustMe,
                MeSpouse,
                MeSpouseKids,
                MeKids,
                JustKids
            };
            return new PromptOptions {
                Choices = ChoiceFactory.ToChoices(choices),
                Prompt = MessageFactory.Text("Who do you need insurance for?"),
                RetryPrompt = MessageFactory.Text("I'm sorry I didn't understand your responce, try typing the number of one of the choices.")
            };
        }

        private async Task<bool> InsuranceRequesteeTypeValidatorAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken) {
            if (!promptContext.Recognized.Succeeded) {
                if (string.IsNullOrEmpty(promptContext.Context.Activity.Text)) {
                    var responseText = promptContext.Context.Activity.Text.ToLower();


                    bool me = responseText.Contains("me");
                    bool spouse = responseText.Contains("spouse");
                    bool kids = responseText.Contains("kid");
                    //TODO: Figure out how to manually set choice from this test
                }
 
                await promptContext.Context.SendActivityAsync(
                    "I'm sorry, you need to select from one of the choices.",
                    cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }

        private PromptOptions ZipCodePromptOptions() {
            return new PromptOptions {
                Prompt = MessageFactory.Text("What's your zip code?")
            };
        }

        private async Task<bool> ZipCodeValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken) {
            if (!promptContext.Recognized.Succeeded) {
                await promptContext.Context.SendActivityAsync(
                    "I'm sorry I didn't understand your responce.  Please enter a 5 digit number for the zipcode.",
                    cancellationToken: cancellationToken);
                return false;
            }

            // Check whether the party size is appropriate.
            var zipCodeStr = promptContext.Recognized.Value.ToString().Trim();

            if (zipCodeStr.Length != 5) {
                await promptContext.Context.SendActivityAsync(
                    "I'm sorry but the zipcode needs to be a 5 digit number.  Please try again.",
                    cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }

        private PromptOptions HouseholdCountPromptOptions() {
            return new PromptOptions {
                Prompt = MessageFactory.Text("How many people live in your household, including you?"),
                RetryPrompt = MessageFactory.Text("I'm sorry I didn't understand your responce, we are expecting a number.")
            };
        }

        private PromptOptions KidCountPromptOptions() {
            return new PromptOptions {
                Prompt = MessageFactory.Text("How many kids are under 19? Tip: While you can cover children under age 26 on a family plan, children under age 19 may be eligible for free or low cost programs."),
                RetryPrompt = MessageFactory.Text("I'm sorry I didn't understand your responce, we are expecting a number.")
            };
        }

        private async Task<bool> KidCountValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken) {
            if (!promptContext.Recognized.Succeeded) {
                await promptContext.Context.SendActivityAsync(
                    "I'm sorry I didn't understand your responce.  Please enter a 5 digit number for the zipcode.",
                    cancellationToken: cancellationToken);
                return false;
            }

            var insuranceQuestionaire = await _accessors.InsuranceQuestionaire.GetAsync(promptContext.Context, () => new InsuranceQuestionaire(), cancellationToken);
            var kidCount = promptContext.Recognized.Value;

            if (kidCount >= insuranceQuestionaire.FamilyMemberCount) {
                await promptContext.Context.SendActivityAsync(
                    "I'm sorry but you can't have more kids than the number of people in your house.",
                    cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }


        private PromptOptions IncomePromptOptions() {
            return new PromptOptions {
                Prompt = MessageFactory.Text("What will your total household income be this year? Tip: Just your best guess is fine - this helps us determine if you're eligible for government-funded discounts and options."),
                RetryPrompt = MessageFactory.Text("I'm sorry I didn't understand your responce, we are expecting a number.")
            };
        }

        private PromptOptions BudgetPreferencePromptOptions() {
            var choices = new List<string> {
                "I prefer a lower monthly payment even if it means I have to pay more when I'm receiving care.",
                "I prefer lower, more predictable costs when I receive care, even if it means a higher monthly payment."
            };
            return new PromptOptions {
                Choices = ChoiceFactory.ToChoices(choices),
                Prompt = MessageFactory.Text("When you think about your budget, which best describes you?"),
                //RetryPrompt = MessageFactory.Text("I'm sorry I didn't understand your responce, try typing 1 or 2."),
            };
        }

        private async Task<bool> BudgetPreferenceValidatorAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken) {
            if (!promptContext.Recognized.Succeeded) {

                await promptContext.Context.SendActivityAsync(
                    "I'm sorry, you need to select from one of the choices.",
                    cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }

        private PromptOptions ToDiscussPromptOptions() {
            var choices = new List<string> {
                "Yes, Schedule a Call",
                "Yes, Call Me Now",
                "Not Right Now"
            };
            return new PromptOptions {
                Choices = ChoiceFactory.ToChoices(choices),
                Prompt = MessageFactory.Text("Want to discuss further with one of our experts?"),
                //RetryPrompt = MessageFactory.Text("I'm sorry I didn't understand your responce, try typing 1 or 2."),
            };
        }

        private async Task<bool> ToDiscussValidatorAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken) {
            if (!promptContext.Recognized.Succeeded) {

                await promptContext.Context.SendActivityAsync(
                    "I'm sorry, you need to select from one of the choices.",
                    cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }



        //private PromptOptions CardPromptOptions(WaterfallStepContext stepContext, string templatePath) {
        //    var cardAttachment = CreateAdaptiveCardAttachment(templatePath);
        //    var reply = stepContext.Context.Activity.CreateReply();
        //    reply.Attachments = new List<Attachment>() { cardAttachment };

        //    return new PromptOptions {
        //        Prompt = (Activity)MessageFactory.Attachment(new Attachment {
        //            ContentType = AdaptiveCard.ContentType,
        //            Content = cardAttachment.Content
        //        }),
        //    };
        //}
    }
}
