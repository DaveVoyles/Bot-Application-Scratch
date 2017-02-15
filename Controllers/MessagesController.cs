using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Activity = Microsoft.Bot.Connector.Activity;

namespace Bot_Application
{
    [BotAuthentication] // used to validate your Bot Connector credentials over HTTPS
    public class MessagesController : ApiController
    {
        /// <summary>
        /// Handles activity of all types except for messages
        /// TODO: Note sure why / how this function ever gets called 
        /// </summary>
        /// <param name="activity"></param>
        private async void HandleSystemMessage(Activity activity)
        {
            if (activity == null) return;

            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    await PostMessage(activity);
                    break;
                case ActivityTypes.DeleteUserData:
                    // Implement user deletion here
                    // If we handle user deletion, return a real activity
                    break;
                case ActivityTypes.ConversationUpdate:
                    // Handle conversation state changes, like members being added and removed
                    // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                    // Not available in all channels
                    await WelcomeNewUser(activity);
                    break;
                case ActivityTypes.ContactRelationUpdate:
                    // Handle add/remove from contact lists
                    // Activity.From + Activity.Action represent what happened
                    break;
                case ActivityTypes.Typing:
                    // Handle knowing tha the user is typing
                    await ResponseTyping(activity);
                    break;
                case ActivityTypes.Ping:
                    await ResponsePing(activity);
                    break;
                default:
                    Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                    break;
            }
        }


        /// <summary>
        /// POST: api/Messages
        /// Receive a message activity from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> PostMessage([FromBody]Activity activity)
        {
            await Conversation.SendAsync(activity, () => new EchoDialog());
            
            // Request has been accepted for further processing
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }


        // Alternative to using the Echo Dialoge. Place this in PostMessage([FromBody]Activity activity):
        //  await Conversation.SendAsync(activity, MakeRootDialog);
        internal static IDialog<OptionsOrder> MakeRootDialog()
        {
            return Chain.From(() => FormDialog.FromForm(OptionsOrder.BuildForm));
        }



        private static async Task ReturnDialogAsync(Activity activity)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            Activity reply = activity.CreateReply($"RETURN DIALOGUE: You sent {activity.Text} which was {length} characters");
            await connector.Conversations.ReplyToActivityAsync(reply);
        }

    

        /* HANDLE SYSTEM RESPONSES
         ****************************************************************/

        private static async Task ResponsePing(Activity message)
        {
            Activity reply            = message.CreateReply($"You sent a PING!");
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync(reply);
        }


        private static async Task ResponseTyping(Activity message)
        {
            Activity reply            = message.CreateReply($"User is typing...");
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync(reply);
        }


        /// <summary>
        /// Creates a response to welcoem new users
        /// </summary>
        /// <param name="message">Incoming text</param>
        /// <returns>A response to the new user, with their name</returns>
        private static async Task WelcomeNewUser(Activity message)
        {
            var client = new ConnectorClient(new Uri(message.ServiceUrl));
            IConversationUpdateActivity update = message;
            if (update.MembersAdded.Any())
            {
                var createReply   = message.CreateReply();
                var newMembers    = update.MembersAdded?.Where(t => t.Id != message.Recipient.Id);

                // Welcome each new member
                foreach (var newMember in newMembers)
                {
                    createReply.Text = "Welcome";
                    if (!string.IsNullOrEmpty(newMember.Name))
                    {
                        createReply.Text += $" {newMember.Name}";
                    }
                    createReply.Text += "!";
                    await client.Conversations.ReplyToActivityAsync(createReply);
                }
            }
        }
    }
}