using System;
using System.Windows.Input;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.Services;

namespace ForcedLogInApp.ViewModels
{
    public class MainViewModel : Observable
    {
        private ICommand _showToastCommand;

        public ICommand ShowToastCommand => _showToastCommand ?? (_showToastCommand = new RelayCommand(OnShowToast));

        public MainViewModel()
        {
        }

        private void OnShowToast()
        {
            Singleton<ToastNotificationsService>.Instance.ShowToastNotificationSample();
        }
    }
}
