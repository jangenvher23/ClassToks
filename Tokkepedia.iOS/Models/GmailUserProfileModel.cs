using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Tokkepedia.iOS.Models
{

    public class GmailUserProfileModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public Uri Picture { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}