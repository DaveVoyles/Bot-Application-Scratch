using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot_Application
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int iMsgCount = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }


        /// <summary>
        /// Parses the text from a message and adjsuts behavior based on the text
        /// </summary>
        /// <param name="context">Bot who sent the message</param>
        /// <param name="messageActivity">Message</param>
        /// <returns></returns>
        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> messageActivity)
        {
            var message = await messageActivity;

            // Should we reset the msg count?
            if (message.Text == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
            }

            // General response
            else
            {
                await context.PostAsync($"{this.iMsgCount++}: You said {message.Text}");
                context.Wait(MessageReceivedAsync);
            }
        }


        /// <summary>
        /// Should we reset the current activity count?
        /// </summary>
        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.iMsgCount = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }
    }
}