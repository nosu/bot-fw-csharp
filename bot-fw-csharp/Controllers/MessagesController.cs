using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

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
                await Conversation.SendAsync(activity, () => new SampleLuisDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
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

        [LuisModel("0dbfcffa-372e-4f8a-8eef-097e8e35eaa5", "c06e2e79ee13450399babf09b94dfa8e")]
        [Serializable]
        public class SampleLuisDialog : LuisDialog<object>
        {
            public const string DefaultAlarmWhat = "default";
            public const string Entity_Area = "area";
            public const string Entity_Gourmet_Category = "gourmetCategory";
            protected string _area;
            protected string _gourmetCategory;

            [LuisIntent("")]
            public async Task None(IDialogContext context, LuisResult result)
            {
                string message = $"わかりません: " + string.Join(", ", result.Intents.Select(i => i.Intent));
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }

            //[LuisIntent("builtin.intent.getRestaurant")]
            [LuisIntent("getRestaurant")]
            public async Task GetRestaurant(IDialogContext context, LuisResult result)
            {
                EntityRecommendation area, gourmetCategory;
                bool isAreaExist = result.TryFindEntity(Entity_Area, out area);
                bool isCategoryExist = result.TryFindEntity(Entity_Gourmet_Category, out gourmetCategory);
                if (!isAreaExist && !isCategoryExist)
                {
                    string message = $"ごめんなさい、情報が足りません";
                    await context.PostAsync(message);
                    context.Wait(MessageReceived);
                }
                else if (!isAreaExist)
                {
                    _gourmetCategory = gourmetCategory.Entity;
                    PromptDialog.Text(context, ReplyRestaurantAsync, "探したい都道府県を教えてください", "都道府県の名前で教えてください", 3);
                }
                else if (!isCategoryExist)
                {
                    _area = area.Entity;
                    PromptDialog.Text(context, ReplyRestaurantAsync, "探したいレストランのジャンルを教えてください", "「イタリアン」などの名前で教えてください", 3);
                }
                else
                {
                    _area = area.Entity;
                    _gourmetCategory = gourmetCategory.Entity;
                    await ReplyRestaurantAsync(context);
                }
            }

            public async Task ReplyRestaurantAsync(IDialogContext context)
            {
                string message = $"{_area}の{_gourmetCategory}を探しますね";
                await context.PostAsync(message);
                _area = null;
                _gourmetCategory = null;
                context.Wait(MessageReceived);
            }

            public async Task ReplyRestaurantAsync(IDialogContext context, IAwaitable<string> argument)
            {
                var area = _area ?? await argument;
                var category = _gourmetCategory ?? await argument;
                string message = $"{area}の{category}を探しますね";
                await context.PostAsync(message);
                _area = null;
                _gourmetCategory = null;
                context.Wait(MessageReceived);
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