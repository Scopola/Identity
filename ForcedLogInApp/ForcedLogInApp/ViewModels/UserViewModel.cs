using System;
using System.Threading.Tasks;
using ForcedLogInApp.Core.Models;
using ForcedLogInApp.Helpers;
using Windows.UI.Xaml.Media.Imaging;

namespace ForcedLogInApp.ViewModels
{
    public class UserViewModel : Observable
    {
        private string _name;
        private string _mail;
        private BitmapImage _photo;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        public string Mail
        {
            get { return _mail; }
            set { Set(ref _mail, value); }
        }

        public BitmapImage Photo
        {
            get { return _photo; }
            set { Set(ref _photo, value); }
        }

        public UserViewModel()
        {
        }

        public UserViewModel(User userData, BitmapImage userPhoto)
        {
            Name = userData.DisplayName;
            Mail = userData.Mail;
            Photo = userPhoto;
        }        

        public void Update(UserViewModel freshData)
        {
            Name = freshData?.Name;
            Mail = freshData?.Mail;
            Photo = freshData?.Photo;
        }
    }
}
