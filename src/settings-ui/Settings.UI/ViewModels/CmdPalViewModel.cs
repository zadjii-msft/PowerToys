// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

using global::PowerToys.GPOWrapper;
using Microsoft.PowerToys.Settings.UI.Library;
using Microsoft.PowerToys.Settings.UI.Library.Helpers;
using Microsoft.PowerToys.Settings.UI.Library.Interfaces;
using Microsoft.PowerToys.Settings.UI.ViewModels.Commands;

namespace Microsoft.PowerToys.Settings.UI.ViewModels
{
    public class CmdPalViewModel : Observable
    {
        private GpoRuleConfigured _enabledGpoRuleConfiguration;
        private bool _isEnabled;

        public ButtonClickCommand InstallModuleEventHandler => new(InstallModule);

        public ButtonClickCommand UninstallModuleEventHandler => new(UninstallModule);

        private GeneralSettings GeneralSettingsConfig { get; set; }

        private Func<string, int> SendConfigMSG { get; }

        public CmdPalViewModel(ISettingsUtils settingsUtils, ISettingsRepository<GeneralSettings> settingsRepository, Func<string, int> ipcMSGCallBackFunc)
        {
            ArgumentNullException.ThrowIfNull(settingsUtils);

            // To obtain the general settings configurations of PowerToys Settings.
            ArgumentNullException.ThrowIfNull(settingsRepository);

            GeneralSettingsConfig = settingsRepository.SettingsConfig;

            InitializeEnabledValue();

            // set the callback functions value to handle outgoing IPC message.
            SendConfigMSG = ipcMSGCallBackFunc;
        }

        private void InitializeEnabledValue()
        {
            _enabledGpoRuleConfiguration = GPOWrapper.GetConfiguredCmdPalEnabledValue();
            if (_enabledGpoRuleConfiguration is GpoRuleConfigured.Disabled or GpoRuleConfigured.Enabled)
            {
                // Get the enabled state from GPO.
                IsEnabledGpoConfigured = true;
                _isEnabled = _enabledGpoRuleConfiguration == GpoRuleConfigured.Enabled;
            }
            else
            {
                _isEnabled = GeneralSettingsConfig.Enabled.CmdPal;
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;

            set
            {
                if (IsEnabledGpoConfigured)
                {
                    // If it's GPO configured, shouldn't be able to change this state.
                    return;
                }

                if (value != _isEnabled)
                {
                    _isEnabled = value;

                    // Set the status in the general settings configuration
                    GeneralSettingsConfig.Enabled.AlwaysOnTop = value;
                    OutGoingGeneralSettings snd = new(GeneralSettingsConfig);

                    SendConfigMSG(snd.ToString());
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public bool IsEnabledGpoConfigured { get; private set; }

        public void RefreshEnabledState()
        {
            InitializeEnabledValue();
            OnPropertyChanged(nameof(IsEnabled));
        }

        private bool _isCmdPalInstalled;

        public bool IsCmdPalInstalled
        {
            get => _isCmdPalInstalled;
            set
            {
                if (_isCmdPalInstalled != value)
                {
                    _isCmdPalInstalled = value;
                    OnPropertyChanged(nameof(IsCmdPalInstalled));
                }
            }
        }

        private void InstallModule()
        {
            throw new NotImplementedException();
        }

        private void UninstallModule()
        {
            throw new NotImplementedException();
        }
    }
}
