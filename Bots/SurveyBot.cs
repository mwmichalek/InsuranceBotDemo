// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace CDPHP.Bot.Survey {
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class SurveyBot<T> : ActivityHandler where T : Dialog {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;

        public SurveyBot(ConversationState conversationState, UserState userState, T dialog, ILogger<SurveyBot<T>> logger) {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken)) {
            Logger.LogInformation($"OnTurnAsync: {turnContext.Activity}");
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken) {
            Logger.LogInformation($"OnMessageActivityAsync: {turnContext.Activity}");

            // Run the Dialog with the new message Activity.
            await Dialog.Run(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        protected override async Task OnEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken) {
            Logger.LogInformation($"OnEventActivityAsync: {turnContext.Activity}");
            await base.OnEventAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken) {
            Logger.LogInformation($"OnMembersAddedAsync: {turnContext.Activity}");


            await turnContext.SendActivityAsync("Hello!", null, null, cancellationToken);


            await Dialog.Run(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

            await base.OnMembersAddedAsync(membersAdded, turnContext, cancellationToken);
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken) {
            Logger.LogInformation($"OnEventActivityAsync: {turnContext.Activity}");


            var eventActivity = turnContext.Activity.AsEventActivity();

            if (eventActivity != null && ((string)eventActivity.Value) == "welcome") {
                await turnContext.SendActivityAsync("Hello!", null, null, cancellationToken);


                await Dialog.Run(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }

            await base.OnEventActivityAsync(turnContext, cancellationToken);

            
        }

        

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken) {
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);

            //await SendWelcomeMessageAsync(turnContext, cancellationToken);
        }

        private async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken) {
            foreach (var member in turnContext.Activity.MembersAdded) {
                if (member.Id != turnContext.Activity.Recipient.Id) {
                    var reply = turnContext.Activity.CreateReply();
                    reply.Text = "Hello!";
                    await turnContext.SendActivityAsync(reply, cancellationToken);

                    await Dialog.Run(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                }
            }
        }
    }
}
