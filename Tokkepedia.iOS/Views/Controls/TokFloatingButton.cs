using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FloatingActionButton;
using Foundation;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace Tokkepedia.iOS.Views
{
    public class TokFloatingButton : FloatingButton
    {
        public TokFloatingButton(float size) : base(size)
        {
            
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // get the touch
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                //OpenTokChoice();
            }

            // reset our tracking flags
            //touchStartedInside = false;
        }

    }
}