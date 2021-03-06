using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MultiplayerExtensions.OverrideClasses;
using Polyglot;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.UI
{
    class HostLobbySetupPanel : BSMLResourceViewController
    {
        public override string ResourceName => "MultiplayerExtensions.UI.HostLobbySetupPanel.bsml";
        private IMultiplayerSessionManager sessionManager;

        CurvedTextMeshPro? modifierText;

        [Inject]
        internal void Inject(IMultiplayerSessionManager sessionManager, HostLobbySetupViewController hostViewController, MultiplayerLevelLoader levelLoader)
        {
            this.sessionManager = sessionManager;
            base.DidActivate(true, false, true);

            hostViewController.didActivateEvent += OnActivate;
        }

        #region UIComponents
        [UIComponent("CustomSongsToggle")]
        public ToggleSetting customSongsToggle = null!;

        [UIComponent("FreeModToggle")]
        public ToggleSetting freeModToggle = null!;

        [UIComponent("HostPickToggle")]
        public ToggleSetting hostPickToggle = null!;

        [UIComponent("VerticalHUDToggle")]
        public ToggleSetting verticalHUDToggle = null!;

        [UIComponent("DefaultHUDToggle")]
        public ToggleSetting defaultHUDToggle = null!;

        [UIComponent("HologramToggle")]
        public ToggleSetting hologramToggle = null!;

        [UIComponent("LagReducerToggle")]
        public ToggleSetting lagReducerToggle = null!;

        [UIComponent("MissLightingToggle")]
        public ToggleSetting missLightingToggle = null!;

        [UIComponent("DownloadProgressText")]
        public FormattableText downloadProgressText = null!;
        #endregion

        #region UIValues
        [UIValue("CustomSongs")]
        public bool CustomSongs
        {
            get => Plugin.Config.CustomSongs;
            set { 
                Plugin.Config.CustomSongs = value;
                if (MPState.CustomSongsEnabled != value)
                {
                    MPState.CustomSongsEnabled = value;
                    MPEvents.RaiseCustomSongsChanged(this, value);
                }
            }
        }

        [UIValue("FreeMod")]
        public bool FreeMod
        {
            get => Plugin.Config.FreeMod;
            set { 
                Plugin.Config.FreeMod = value;
                if (MPState.FreeModEnabled != value)
                {
                    MPState.FreeModEnabled = value;
                    MPEvents.RaiseFreeModChanged(this, value);
                }
            }
        }

        [UIValue("HostPick")]
        public bool HostPick
        {
            get => Plugin.Config.HostPick;
            set
            {
                Plugin.Config.HostPick = value;
                if (MPState.HostPickEnabled != value)
                {
                    MPState.HostPickEnabled = value;
                    MPEvents.RaiseHostPickChanged(this, value);
                }
            }
        }

        [UIValue("VerticalHUD")]
        public bool VerticalHUD
        {
            get => Plugin.Config.VerticalHUD;
            set { Plugin.Config.VerticalHUD = value; }
        }

        [UIValue("DefaultHUD")]
        public bool DefaultHUD
        {
            get => Plugin.Config.SingleplayerHUD;
            set { Plugin.Config.SingleplayerHUD = value; }
        }

        [UIValue("Hologram")]
        public bool Hologram
        {
            get => Plugin.Config.Hologram;
            set { Plugin.Config.Hologram = value; }
        }

        [UIValue("LagReducer")]
        public bool LagReducer
        {
            get => Plugin.Config.LagReducer;
            set { Plugin.Config.LagReducer = value; }
        }

        [UIValue("MissLighting")]
        public bool MissLighting
        {
            get => Plugin.Config.MissLighting;
            set { Plugin.Config.MissLighting = value; }
        }

        [UIValue("DownloadProgress")]
        public string DownloadProgress
        {
            get => downloadProgressText.text;
            set { downloadProgressText.text = value; }
        }
        #endregion

        #region UIActions
        [UIAction("SetCustomSongs")]
        public void SetCustomSongs(bool value)
        {
            CustomSongs = value;
            customSongsToggle.Value = value;

            UpdateStates();
        }

        [UIAction("SetFreeMod")]
        public void SetFreeMod(bool value)
        {
            FreeMod = value;
            freeModToggle.Value = value;

            UpdateStates();
            SetModifierText();
        }

        [UIAction("SetHostPick")]
        public void SetHostPick(bool value)
        {
            HostPick = value;
            hostPickToggle.Value = value;

            UpdateStates();
        }

        [UIAction("SetVerticalHUD")]
        public void SetVerticalHUD(bool value)
        {
            VerticalHUD = value;
            verticalHUDToggle.Value = value;

            DefaultHUD = !(!DefaultHUD || !value);
            defaultHUDToggle.Value = !(!DefaultHUD || !value);
        }

        [UIAction("SetDefaultHUD")]
        public void SetDefaultHUD(bool value)
        {
            DefaultHUD = value;
            defaultHUDToggle.Value = value;

            VerticalHUD = VerticalHUD || value;
            verticalHUDToggle.Value = VerticalHUD || value;
        }

        [UIAction("SetHologram")]
        public void SetHologram(bool value)
        {
            Hologram = value;
            hologramToggle.Value = value;
        }

        [UIAction("SetLagReducer")]
        public void SetLagReducer(bool value)
        {
            LagReducer = value;
            lagReducerToggle.Value = value;
        }

        [UIAction("SetMissLighting")]
        public void SetMissLighting(bool value)
        {
            MissLighting = value;
            missLightingToggle.Value = value;
        }
        #endregion

        private void OnActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                Transform spectatorText = transform.Find("Wrapper").Find("SpectatorModeWarningText");
                spectatorText.position = new Vector3(spectatorText.position.x, 0.25f, spectatorText.position.z);
            }
        }

        private void UpdateStates()
        {
            sessionManager?.SetLocalPlayerState("customsongs", CustomSongs);
            sessionManager?.SetLocalPlayerState("freemod", FreeMod);
            sessionManager?.SetLocalPlayerState("hostpick", HostPick);
        }

        private void SetModifierText()
        {
            if (modifierText == null)
            {
                modifierText = Resources.FindObjectsOfTypeAll<CurvedTextMeshPro>().ToList().Find(text => text.gameObject.name == "SuggestedModifiers");
                Destroy(modifierText.gameObject.GetComponent<LocalizedTextMeshPro>());
            }

            if (modifierText != null)
                modifierText.text = MPState.FreeModEnabled ? "Selected Modifiers" : Localization.Get("SUGGESTED_MODIFIERS");
        }
    }
}
