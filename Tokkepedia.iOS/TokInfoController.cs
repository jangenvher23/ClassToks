using Foundation;
using ObjCRuntime;
using System;
using Tokkepedia.Shared.Models;
using UIKit;

namespace Tokkepedia.iOS
{
    public partial class TokInfoController : UIViewController
    {
        public string PreviousTitle { get; set; }
        public TokModel Tok { get; set; }


        public TokInfoController (IntPtr handle) : base (handle)
        {
            
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (this.NavigationController?.NavigationBar?.TopItem != null)
            {
                this.NavigationController.NavigationBar.TopItem.Title = Tok?.PrimaryFieldText;
            }
        }

        public override void DismissViewController(bool animated, Action completionHandler)
        {
            this.NavigationController.Title = PreviousTitle;
            base.DismissViewController(animated, completionHandler);
        }
    }
}