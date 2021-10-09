# SendPlaysToDiscord
Beat Saber mod that sends score data to a Discord Webhook when a level is finished.

Origin Story:
1. The university I go to has a rhythm game server.
2. They were holding an asynchronous osu! tournament using Discord bots that automatically check the leaderboards for new scores by specific users.
3. The bots calculated a custom "score" value separate from PP that's based on stars, accuracy, and combo.
4. I notice lots of Beat Saber players joining the server, want to do a Beat Saber tournament similar to the osu! tournament.
5. ScoreSaberSharp doesn't let you see user scores, or the highest combo of a play.
6. I make this mod.

How to receive scores:
1. Create text channel on your Discord server.
2. Go to text channel settings -> Integrations -> Create Webhook.
3. Click "Copy Webhook URL" and send the URL to everyone you want to receive scores from.

What the scores look like:
![image](https://user-images.githubusercontent.com/33010927/136639144-7a848094-ec6d-456d-872a-7495f8812b0f.png)

How to send scores:
1. Install this mod (see below).
2. Start the game to generate a .json config file in Beat Saber's UserData folder.
3. Paste the Discord Webhool URL into the "webhookURL" field and save the file (restart the game if you want to see your URL appear in-game). Alternatively, type it manually into the in-game settings tab.
4. The in-game settings tab is in the mod settings to the left of the level browser when you are selecting a level.
5. Click the "Test This URL" button to send a test message and the status text will tell you if it succeeds.
6. If the URL works, play a level to the end and this mod will automatically send score information to the URL.
7. Optional: If you are playing someone else's copy of Beat Saber, use the "Override User ID" setting to send your own User ID.

How to install manually:


How to build .dll file:
