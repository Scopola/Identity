using Prism.Windows.Mvvm;
using Windows.UI.Xaml.Media.Imaging;

namespace PrismForcedLoginApp.ViewModels
{
    public class UserViewModel : ViewModelBase
    {
        private string _name;
        private string _userPrincipalName;
        private BitmapImage _photo;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string UserPrincipalName
        {
            get { return _userPrincipalName; }
            set { SetProperty(ref _userPrincipalName, value); }
        }

        public BitmapImage Photo
        {
            get { return _photo; }
            set { SetProperty(ref _photo, value); }
        }

        public UserViewModel()
        {
        }
    }
}
