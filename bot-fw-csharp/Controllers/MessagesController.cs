using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace bot_fw_csharp
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new EchoDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);

            //if (activity.Type == ActivityTypes.Message)
            //{
            //    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            //    // calculate something for us to return
            //    int length = (activity.Text ?? string.Empty).Length;

            //    // return our reply to the user
            //    Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
            //    await connector.Conversations.ReplyToActivityAsync(reply);
            //}
            //else
            //{
            //    HandleSystemMessage(activity);
            //}
            //var response = Request.CreateResponse(HttpStatusCode.OK);
            //return response;
        }

        [Serializable]
        public class EchoDialog : IDialog<object>
        {
            protected int resetCount = 1;
            protected int otherCount = 1;

            public async Task StartAsync(IDialogContext context)
            {
                context.Wait(MessageReceivedAsync);
            }

            public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
            {
                var message = await argument;
                if (message.Text == "リセット")
                {
                    PromptDialog.Confirm(
                        context,
                        AfterResetAsync,
                        "本当にリセットしてよろしいですか?",
                        "「はい」または「いいえ」でお答えください",
                        promptStyle: PromptStyle.None);
                }
                else
                {
                    await context.PostAsync($"'{message.Text}'と{this.otherCount++}回言いました");
                    context.Wait(MessageReceivedAsync);
                }
            }

            public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
            {
                var confirm = await argument;
                if (confirm)
                {
                    this.resetCount = 1;
                    await context.PostAsync($"{this.resetCount}回目のリセットを実行しました");
                }
                else
                {
                    await context.PostAsync("リセットを中止しました");
                }
                context.Wait(MessageReceivedAsync);
            }
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}