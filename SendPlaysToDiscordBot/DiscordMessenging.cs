using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JNogueira.Discord.Webhook.Client;

namespace SendPlaysToDiscordBot
{
    class DiscordMessenging {
        private static string webhookURL = "https://discord.com/api/webhooks/895725675779080263/0hhSup2JkZU09HAMbLhHqhN9EuMvCMAaWiEttJ0TzGYu6GoboMynFtEgB0btd09U_1_b";
        private static string avatarIconURL = "https://cdn.akamai.steamstatic.com/steam/apps/620980/capsule_616x353.jpg?t=1622461922";
        private static string botName = "Beat Saber Score Delivery";
        private const int color = 5570815; //Purple (convert hex code to decimal number)

        //Sends data to Discord Webhook.
        public static async void SendMessage(string ids, string score, string modifiers) {
            DiscordWebhookClient client = new DiscordWebhookClient(webhookURL);
            List<DiscordMessageEmbedField> embedFields = new List<DiscordMessageEmbedField>();

            embedFields.Add(new DiscordMessageEmbedField("Score Info", score));

            string modifierTitle = modifiers.Length != 0 ? "Modifiers Used" : "No Modifiers Used";
            //If modifiers change the modified score value, show the percent change.
            if (modifiers.Contains(".")) {
                int index = modifiers.IndexOf(".") + 1, length = modifiers.IndexOf("%") - index + 1;
                modifierTitle += " (Score Multiplier: " + modifiers.Substring(index, length) + ")";
                modifiers = modifiers.Remove(modifiers.LastIndexOf(","));
            }
            embedFields.Add(new DiscordMessageEmbedField(modifierTitle, modifiers.Length != 0 ? modifiers : null));
            DiscordMessageEmbed[] embeds = new DiscordMessageEmbed[] { new DiscordMessageEmbed("Beat Saber Score Info", color, null, null, ids, embedFields) };
            
            DiscordMessage message = new DiscordMessage(null, botName, avatarIconURL, false, embeds);
            await client.SendToDiscord(message);
        }

        //Sends test message to Discord Webhook, returns true if successful.
        public static async Task<bool> SendTestMessage(string userId) {
            try{
                DiscordWebhookClient client = new DiscordWebhookClient(webhookURL);
                DiscordMessage message = new DiscordMessage("Test message from UserID " + userId + ".\nYou can delete this.", botName, avatarIconURL);
                await client.SendToDiscord(message);
                return true;
            }
            catch (Exception e) {
                Plugin.Log.Info("Test message failed to send. Error: " + e.Message);
                return false;
            }
        }
    }
}
