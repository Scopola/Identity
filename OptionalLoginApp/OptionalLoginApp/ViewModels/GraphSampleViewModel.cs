using System.Collections.ObjectModel;
using System.Threading.Tasks;
using OptionalLoginApp.Core.Helpers;
using OptionalLoginApp.Helpers;
using OptionalLoginApp.Services;

namespace OptionalLoginApp.ViewModels
{
    public class GraphSampleViewModel : Observable
    {
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        public ObservableCollection<UserViewModel> People { get; } = new ObservableCollection<UserViewModel>();

        public GraphSampleViewModel()
        {
        }

        public async Task LoadDataAsync()
        {
            var apiData = await UserDataService.GetPeopleFromGraphApiAsync();
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
