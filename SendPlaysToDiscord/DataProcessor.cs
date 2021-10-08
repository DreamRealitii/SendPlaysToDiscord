using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using BeatSaverSharp;
using BeatSaverSharp.Models;
using System.Reflection;
using System.Net;
using BS_Utils.Utilities;
using BS_Utils.Gameplay;
using SendPlaysToDiscord.Configuration;

namespace SendPlaysToDiscord {
    class DataProcessor {
        
        public static DataProcessor instance = new DataProcessor();
        private UserInfo userInfo;
        private WebSocket socket;
        private BeatSaver beatSaver = new BeatSaver("SendPlaysToDiscordBot", Assembly.GetExecutingAssembly().GetName().Version);
        private string currentLevelKey, currentCharacteristic, currentDifficulty;

        public void Init() {
            instance = this;
            BSEvents.levelCleared += OnLevelClear;
            socket = new WebSocket("ws://" + getIP() + ":2946/BSDataPuller/MapData");
            socket.Connect();
            socket.OnMessage += OnSocketMessage;
        }

        private string getIP() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            Plugin.Log.Info("No IP address found.");
            return null;
        }

        //When DataPuller sends map data, update current level information
        private void OnSocketMessage(object sender, MessageEventArgs message) {
            string data = message.Data;
            int index = data.IndexOf("BSRKey") + 9;
            int length = data.IndexOf("\"", index) - index;
            currentLevelKey = data.Substring(index, length);
            index = data.IndexOf("MapType") + 10;
            length = data.IndexOf("\"", index) - index;
            currentCharacteristic = data.Substring(index, length);
            index = data.IndexOf("Difficulty") + 13;
            length = data.IndexOf("\"", index) - index;
            currentDifficulty = data.Substring(index, length);
        }

        //Sends data to Discord Messenger when level is completed.
        public async void OnLevelClear(StandardLevelScenesTransitionSetupDataSO a, LevelCompletionResults results) {
            //Skip if level does not exist on BeatSaver, or mod is disabled.
            if (currentLevelKey.Equals("ull")) {
                Plugin.Log.Info("User did not play a map from BeatSaver. No score will be sent.");
                return;
            }
            if (!Configuration.PluginConfig.Instance.enabled)
                return;

            //Create data strings.
            userInfo = await GetUserInfo.GetUserAsync();
            BeatmapVersion level = (await beatSaver.Beatmap(currentLevelKey)).LatestVersion;
            BeatSaverSharp.Models.BeatmapDifficulty map = GetCurrentDifficulty(level, currentCharacteristic, currentDifficulty);
            string ids = "User ID: " + (PluginConfig.Instance.overrideUserID ?  PluginConfig.Instance.customUserID : userInfo.platformUserId) +
                "\nUTC Time: " + DateTime.UtcNow.ToString("u") +
                "\nBeatSaver Level ID: " + currentLevelKey + 
                "\nCharacteristic: " + currentCharacteristic +
                "\nDifficulty: " + currentDifficulty +
                "\nStar Ranking: " + map.Stars;
            string score = "Raw Score: " + results.rawScore + "/" + MaxScore(map.Notes) +
                "\nCombo: " + results.maxCombo + "/" + map.Notes + (results.fullCombo ? ", Full Combo!" : "") + 
                "\nAccuracy: " + ((float)results.rawScore / MaxScore(map.Notes)).ToString("P2");
            string modifiers = StringOfModifiers(results.gameplayModifiers, (float)results.modifiedScore / Math.Max(results.rawScore, 1));

            //Send data.
            DiscordMessenging.SendScore(ids, score, modifiers);
        }

        //Gets the specific BeatSaver difficulty the player played.
        private BeatSaverSharp.Models.BeatmapDifficulty GetCurrentDifficulty(BeatmapVersion level, string characteristic, string difficulty) {
            BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic bsCharacteristic = BeatSaverCharacteristic(characteristic);
            BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty bsDifficulty = BeatSaverDifficulty(difficulty);
            IReadOnlyCollection<BeatSaverSharp.Models.BeatmapDifficulty> difficulties = level.Difficulties;
            foreach (BeatSaverSharp.Models.BeatmapDifficulty map in difficulties)
                if (map.Characteristic.Equals(bsCharacteristic) && map.Difficulty.Equals(bsDifficulty))
                    return map;
            return null;
        }

        //Convert string to characteristic.
        private BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic BeatSaverCharacteristic(string c) {
            switch (c) {
                case "Standard": return BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic.Standard;
                case "OneSaber": return BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic.OneSaber;
                case "NoArrows": return BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic.NoArrows;
                case "Degree90": return BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic._90Degree;
                case "Degree360": return BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic._360Degree;
                case "Lawless": return BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic.Lawless;
                default: return BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic.Lightshow;
            }
        }

        //Convert string to difficulty.
        private BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty BeatSaverDifficulty(string d) {
            switch (d) {
                case "ExpertPlus": return BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty.ExpertPlus;
                case "Expert": return BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty.Expert;
                case "Hard": return BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty.Hard;
                case "Normal": return BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty.Normal;
                default: return BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty.Easy;
            }
        }

        //Copied from ScorePercentage mod.
        private int MaxScore(int numberOfNotes) {
            if (numberOfNotes < 14) {
                if (numberOfNotes == 1)
                    return 115;
                if (numberOfNotes < 5)
                    return (numberOfNotes - 1) * 230 + 115;
                return (numberOfNotes - 5) * 460 + 1035;
            }
            return (numberOfNotes - 13) * 920 + 4715;
        }

        //Creates a string of modifiers. Also checks if player failed a level, but had No Fail turned on.
        private string StringOfModifiers(GameplayModifiers modifiers, float modifiedRatio) {
            float modifierAmount = 1f;
            string result = "";
            if (modifiers.songSpeedMul == 0.85f) {
                result += "Slower Song, ";
                modifierAmount -= 0.30f;
            }
            else if (modifiers.songSpeedMul == 1.20f) {
                result += "Faster Song, ";
                modifierAmount += 0.08f;
            }
            else if (modifiers.songSpeedMul == 1.50f) {
                result += "Super Fast Song, ";
                modifierAmount += 0.10f;
            }
            if (modifiers.disappearingArrows) {
                result += "Disappearing Arrows, ";
                modifierAmount += 0.07f;
            }
            else if (modifiers.ghostNotes) {
                result += "Ghost Notes, ";
                modifierAmount += 0.11f;
            }
            if (modifiers.noArrows) {
                result += "No Arrows, ";
                modifierAmount -= 0.30f;
            }
            if (modifiers.noBombs) {
                result += "No Bombs, ";
                modifierAmount -= 0.10f;
            }
            if (modifiers.enabledObstacleType.Equals(GameplayModifiers.EnabledObstacleType.NoObstacles)) {
                result += "No Walls, ";
                modifierAmount -= 0.05f;
            }
            if (modifiedRatio - (modifierAmount - 0.5f) < 0.01f) {
                result = "No Fail (Failed), " + result;
                modifierAmount -= 0.50f;
            }
            if (modifiers.proMode)
                result += "Pro Mode, ";
            if (modifiers.smallCubes)
                result += "Small Notes, ";
            if (modifiers.strictAngles)
                result += "Strict Angles, ";
            if (modifierAmount != 1.0f)
                result += "." + modifierAmount.ToString("P0") + ", ";
            return result.Substring(0, result.Length - 2);
        }
    }
}
