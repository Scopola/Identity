using System;
using ForcedLogInApp.Helpers;
using Windows.UI.Xaml.Media.Imaging;

namespace ForcedLogInApp.ViewModels
{
    public class UserViewModel : Observable
    {
        private string _name;
        private string _userPrincipalName;
        private BitmapImage _photo;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        public string UserPrincipalName
        {
            get { return _userPrincipalName; }
            set { Set(ref _userPrincipalName, value); }
        }

        public BitmapImage Photo
        {
            get { return _photo; }
            set { Set(ref _photo, value); }
        }

        public UserViewModel()
        {
        }
    }
}
