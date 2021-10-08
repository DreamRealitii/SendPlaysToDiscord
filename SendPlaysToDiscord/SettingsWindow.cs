using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.GameplaySetup;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using SendPlaysToDiscord.Configuration;
using System.Threading.Tasks;

namespace SendPlaysToDiscord
{
    public class SettingsWindow : BSMLResourceViewController
    {
        // For this method of setting the ResourceName, this class must be the first class in the file.
        public override string ResourceName => "SendPlaysToDiscord.Views.SettingsWindow.bsml";

        [UIComponent("EnableButton")]
        private ToggleSetting enableButton;
        [UIValue("Enabled")]
        private bool isEnabled;
        [UIComponent("WebhookURLBox")]
        private StringSetting webhookURLBox;
        [UIValue("WebhookURL")]
        private string webhookURL;
        [UIComponent("StatusTextGUI")]
        private TextMeshProUGUI connectionTestStatus;
        [UIComponent("OverrideIDButton")]
        private ToggleSetting overrideIDButton;
        [UIValue("OverrideID")]
        private bool overrideID;
        [UIComponent("CustomIDBox")]
        private StringSetting customIDBox;
        [UIValue("CustomID")]
        private string customID;

        public SettingsWindow(PluginConfig config) {
            GameplaySetup.instance.AddTab("Send Plays>Discord", ResourceName, this);
            isEnabled = config.enabled;
            webhookURL = config.webhookURL;
            overrideID = config.overrideUserID;
            customID = config.customUserID;
        }

        [UIAction("OnSetting1")]
        public void OnEnableUpdate(bool value) { PluginConfig.Instance.enabled = value; }
        [UIAction("OnSetting2")]
        public void OnWebhookUpdate(object value) { PluginConfig.Instance.webhookURL = (string)value; }
        [UIAction("OnSetting3")]
        public void OnOverrideIDUpdate(bool value) { PluginConfig.Instance.overrideUserID = value; }
        [UIAction("OnSetting4")]
        public void OnCustomIDUpdate(object value) { PluginConfig.Instance.customUserID = (string)value; }

        [UIAction("TestURL")]
        public async void TestWebhook() {
            bool success = await DiscordMessenging.TestMessage();
            if (success)
                connectionTestStatus.text = "Successfully sent test message to this URL!";
            else
                connectionTestStatus.text = "Could not send test message to this URL.";
        }
    }
}
