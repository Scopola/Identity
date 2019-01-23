﻿using System;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.Services;

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace ForcedLogInApp
{
    public sealed partial class App : Application
    {
        private IdentityService _identityService => Singleton<IdentityService>.Instance;
        private Lazy<ActivationService> _activationService;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            InitializeComponent();

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
            // Start #AddWithdIdentity
            _identityService.LoggedOut += OnLoggedOut;
            // End
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(Views.MainPage), new Lazy<UIElement>(CreateShell));
        }

        // Start #AddWithdIdentity
        private void OnLoggedOut(object sender, EventArgs e)
        {
            ActivationService.SetShell(new Lazy<UIElement>(CreateShell));
        }
        // End

        private UIElement CreateShell()
        {
            return new Views.ShellPage();
        }
    }
}