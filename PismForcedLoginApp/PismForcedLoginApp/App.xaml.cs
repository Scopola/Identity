using System;
using System.Globalization;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;
using PismForcedLoginApp.Core.Services;
using PismForcedLoginApp.Services;
using PismForcedLoginApp.Views;

using Prism.Mvvm;
using Prism.Unity.Windows;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;

using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PismForcedLoginApp
{
    [Windows.UI.Xaml.Data.Bindable]
    public sealed partial class App : PrismUnityApplication
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void ConfigureContainer()
        {
            // register a singleton using Container.RegisterType<IInterface, Type>(new ContainerControlledLifetimeManager());
            base.ConfigureContainer();
            Container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));            
            Container.RegisterInstance<IIdentityService>(new IdentityService());
            Container.RegisterType<IMicrosoftGraphService, MicrosoftGraphService>();
            Container.RegisterType<IUserDataService, UserDataService>();
        }

        protected override async Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            await LaunchApplicationAsync(PageTokens.MainPage, null);
        }

        private async Task LaunchApplicationAsync(string page, object launchParam)
        {
            await ThemeSelectorService.SetRequestedThemeAsync();
            NavigationService.Navigate(page, launchParam);
            Window.Current.Activate();
        }

        protected override async Task OnActivateApplicationAsync(IActivatedEventArgs args)
        {
            await Task.CompletedTask;
        }

        protected override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            var identityService = Container.Resolve<IIdentityService>();
            var silentLoginSuccess = await identityService.LoginWithCommonAuthorityAsync();
            await ThemeSelectorService.InitializeAsync().ConfigureAwait(false);

            // We are remapping the default ViewNamePage and ViewNamePageViewModel naming to ViewNamePage and ViewNameViewModel to
            // gain better code reuse with other frameworks and pages within Windows Template Studio
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewModelTypeName = string.Format(CultureInfo.InvariantCulture, "PismForcedLoginApp.ViewModels.{0}ViewModel, PismForcedLoginApp", viewType.Name.Substring(0, viewType.Name.Length - 4));
                return Type.GetType(viewModelTypeName);
            });
            await base.OnInitializeAsync(args);
        }

        protected override IDeviceGestureService OnCreateDeviceGestureService()
        {
            var service = base.OnCreateDeviceGestureService();
            service.UseTitleBarBackButton = false;
            return service;
        }

        public void SetNavigationFrame(Frame frame)
        {
            var sessionStateService = Container.Resolve<ISessionStateService>();
            CreateNavigationService(new FrameFacadeAdapter(frame), sessionStateService);
        }

        protected override UIElement CreateShell(Frame rootFrame)
        {
            var shell = Container.Resolve<ShellPage>();
            shell.SetRootFrame(rootFrame);
            return shell;
        }
    }
}
