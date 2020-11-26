using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using GalaSoft.MvvmLight;
using UIKit;

namespace Tokkepedia.iOS.ViewModels.Base
{
    public class BaseViewModel : ViewModelBase
    {
        public UINavigationController Navigation { get; set; } = null;
    }
}