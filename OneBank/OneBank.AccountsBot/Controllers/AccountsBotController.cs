﻿namespace OneBank.AccountsBot.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using OneBank.AccountsBot.Dialogs;

    [RoutePrefix("api/messages")]
    public class AccountsBotController : ApiController
    {
        /// <summary>
        /// Receive a message from a user and send replies
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>
        /// Return replies
        /// </returns>
        [HttpPost]
        [Route("")]
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new AccountsEchoDialog());
            }
            else
            {
                this.HandleSystemMessage(activity);
            }

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// The HandleSystemMessage
        /// </summary>
        /// <param name="message">The <see cref="Activity"/>Activity</param>
        /// <returns>The <see cref="Activity"/>Activity reply</returns>
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
