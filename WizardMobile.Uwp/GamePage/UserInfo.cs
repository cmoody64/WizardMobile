using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WizardMobile.Uwp.GamePage
{
    class UserInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AccountName { get; set; }
        public ImageSource AvatarSource { get; set; }
        public string Initials => FirstName.Length > 0 && LastName.Length > 0
            ? $"{FirstName.ToUpper()[0]}{LastName.ToUpper()[0]}"
            : "P1";
    }
}
