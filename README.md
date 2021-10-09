# SendPlaysToDiscord
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

How to install:
1. Install Beat Saber ModAssistant. If this mod is on ModAssistant, install from there and skip all these steps.
2. Use ModAssistant to install BSIPA, BeatSaberMarkupLanguage, BeatSaverSharp, BS Utils, and Data Puller.
3. Download SendScoresToDiscord.dll from release and put it in Beat Saber's Plugins folder.
4. Download Libs.zip from release, extract, and copy its files (discord-webhook-client.dll, notofique-me.dll, and Polly.dll) into Beat Saber's Libs folder.

Current Issues:
- Test button is in an aesthetically unpleasing position.
- Webhook URL setting goes way off the left side of the settings window.
- Fails to send scores for 90 Degree/360 Degree maps.

How to build .dll file from source code:
1. Download and extract source code from release.
2. Create project file `SendPlaysToDiscord/SendPlaysToDiscord.csproj.user` with the text below, but replace BeatSaberDir and ReferencePath with your own Beat Saber directories.
```
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <BeatSaberDir>C:\Program Files (x86)\Steam\steamapps\common\Beat Saber</BeatSaberDir>
    <ReferencePath>C:\Program Files (x86)\Steam\steamapps\common\Beat Saber\Beat Saber_Data\Managed;C:\Program Files (x86)\Steam\steamapps\common\Beat Saber\Libs;C:\Program Files (x86)\Steam\steamapps\common\Beat Saber\Plugins</ReferencePath>
  </PropertyGroup>
</Project>
```
3. Open SendPlaysToDiscord.sln in Visual Studio and build solution.
