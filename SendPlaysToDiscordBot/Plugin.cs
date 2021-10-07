using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPALogger = IPA.Logging.Logger;
using BS_Utils.Utilities;
using BS_Utils.Gameplay;
using WebSocketSharp;
using BeatSaverSharp;
using BeatSaverSharp.Models;
using System.Reflection;
using System.Net;

namespace SendPlaysToDiscordBot
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        private UserInfo userInfo;
        private WebSocket socket;
        private BeatSaver beatSaver = new BeatSaver("SendPlaysToDiscordBot", Assembly.GetExecutingAssembly().GetName().Version);
        private string currentLevelKey;
        private BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic currentCharacteristic;
        private BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty currentDifficulty;

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger) {
            Instance = this;
            Log = logger;
            Log.Info("SendPlaysToDiscordBot initialized.");
        }

        #region BSIPA Config
        [Init]
        public void InitWithConfig(IPA.Config.Config conf) {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        #endregion

        [OnStart]
        public void OnApplicationStart() {
            Log.Debug("OnApplicationStart");
            BSEvents.levelCleared += OnLevelClear;
            socket = new WebSocket("ws://" + getIP() + ":2946/BSDataPuller/MapData");
            socket.Connect();
            socket.OnMessage += OnSocketMessage;
        }

        [OnExit]
        public void OnApplicationQuit() {
            Log.Debug("OnApplicationQuit");
        }

        private string getIP() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) 
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            return "No IP address found.";
        }

        //When DataPuller sends data of the selected level, update variables used for finding BeatSaver data.
        public void OnSocketMessage(object sender, MessageEventArgs message) {
            string data = message.Data;
            int index = data.IndexOf("BSRKey") + 9;
            int length = data.IndexOf("\"", index) - index;
            currentLevelKey = data.Substring(index, length);
            index = data.IndexOf("MapType") + 10;
            length = data.IndexOf("\"", index) - index;
            SetCurrentCharacteristic(data.Substring(index, length));
            index = data.IndexOf("Difficulty") + 13;
            length = data.IndexOf("\"", index) - index;
            SetCurrentDifficulty(data.Substring(index, length));
        }

        private void SetCurrentCharacteristic(string c) {
            BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic result;
            switch (c) {
                case "Standard": result = BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic.Standard; break;
                case "OneSaber": result = BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic.OneSaber; break;
                case "NoArrows": result = BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic.NoArrows; break;
                case "Degree90": result = BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic._90Degree; break;
                case "Degree360": result = BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic._360Degree; break;
                case "Lawless": result = BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic.Lawless; break;
                default: result = BeatSaverSharp.Models.BeatmapDifficulty.BeatmapCharacteristic.Lightshow; break;
            }
            currentCharacteristic = result;
        }

        private void SetCurrentDifficulty(string d) {
            BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty result;
            switch (d) {
                case "ExpertPlus": result = BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty.ExpertPlus; break;
                case "Expert": result = BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty.Expert; break;
                case "Hard": result = BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty.Hard; break;
                case "Normal": result = BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty.Normal; break;
                default: result = BeatSaverSharp.Models.BeatmapDifficulty.BeatSaverBeatmapDifficulty.Easy; break;
            }
            currentDifficulty = result;
        }

        //Data sent: Steam User ID, BeatSaver Level ID, Star Ranking, Passed Level, Score Accuracy, Max Combo, and (Modifiers).
        public async void OnLevelClear(StandardLevelScenesTransitionSetupDataSO a, LevelCompletionResults results) {
            Log.Info("Creating data to send:");
            string data = "";

            userInfo = await GetUserInfo.GetUserAsync();
            data += "UserID: " + userInfo.platformUserId;
            data += ", LevelID: " + currentLevelKey;
            BeatmapVersion level = (await beatSaver.Beatmap(currentLevelKey)).LatestVersion;
            BeatSaverSharp.Models.BeatmapDifficulty map = GetCurrentDifficulty(level);
            data += ", DifficultyStars: " + map.Stars;
            int maxScore = MaxScore(map.Notes);
            data += ", PassedLevel: " + (results.modifiedScore > maxScore / 2);
            data += ", Accuracy: " + ((float)results.rawScore / maxScore);
            data += ", MaxCombo: " + results.maxCombo;
            data += ", Modifiers: (" + StringOfModifiers(results.gameplayModifiers) + ")";

            Log.Info(data);
        }

        private BeatSaverSharp.Models.BeatmapDifficulty GetCurrentDifficulty(BeatmapVersion level) {
            IReadOnlyCollection<BeatSaverSharp.Models.BeatmapDifficulty> difficulties = level.Difficulties;
            foreach (BeatSaverSharp.Models.BeatmapDifficulty map in difficulties)
                if (map.Characteristic.Equals(currentCharacteristic) && map.Difficulty.Equals(currentDifficulty))
                    return map;
            return null;
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

        //Creates a string of modifiers.
        private string StringOfModifiers(GameplayModifiers modifiers) {
            string result = "";
            result += "Speed: " + modifiers.songSpeedMul;
            result += ", GhostNotes: " + modifiers.ghostNotes;
            result += ", GhostArrows: " + modifiers.disappearingArrows;
            result += ", ProNotes: " + modifiers.proMode;
            result += ", SmallNotes: " + modifiers.smallCubes;
            result += ", StrictAngles: " + modifiers.strictAngles;
            result += ", NoWalls: " + modifiers.enabledObstacleType.Equals(GameplayModifiers.EnabledObstacleType.NoObstacles);
            result += ", NoBombs: " + modifiers.noBombs;
            return result;
        }
    }
}
