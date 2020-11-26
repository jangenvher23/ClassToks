using Foundation;
using System;
using Tokkepedia.iOS.ViewModels;
using UIKit;

namespace Tokkepedia.iOS
{
    public partial class PickerViewController : UIViewController
    {
        public PickerViewModel pickerViewModel { get; set; }
        public PickerViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            txtPicker.Model = pickerViewModel;
            btnSelect.TouchUpInside += async (object sender, EventArgs e) =>
            {
                RegistrationController reg = Storyboard.InstantiateViewController("RegistrationController") as RegistrationController;
                reg.State = pickerViewModel.SelectedItem;
                DismissModalViewController(true);
            };
        }
    }
}