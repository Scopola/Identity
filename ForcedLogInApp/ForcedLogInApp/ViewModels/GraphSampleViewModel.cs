using ForcedLogInApp.Core.Helpers;
using ForcedLogInApp.Core.Models;
using ForcedLogInApp.Helpers;
using ForcedLogInApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ForcedLogInApp.ViewModels
{
    public class GraphSampleViewModel : Observable
    {
        private UserDataService _userDataService => Singleton<UserDataService>.Instance;

        public ObservableCollection<UserViewModel> People { get; } = new ObservableCollection<UserViewModel>();

        public GraphSampleViewModel()
        {
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
