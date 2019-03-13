using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using PrismForcedLoginApp.Services;

namespace PrismForcedLoginApp.ViewModels
{
    public class GraphSampleViewModel : ViewModelBase
    {
        private UserDataService _userDataService;

        public ObservableCollection<UserViewModel> People { get; } = new ObservableCollection<UserViewModel>();

        public GraphSampleViewModel(UserDataService userDataService)
        {
            _userDataService = userDataService;
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            var apiData = await _userDataService.GetPeopleFromGraphApiAsync();
            if (apiData != null)
            {
                foreach (var user in apiData)
                {
                    People.Add(user);
                }
            }
        }
    }
}
