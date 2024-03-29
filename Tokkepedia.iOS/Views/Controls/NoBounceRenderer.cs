﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using Tokkepedia.iOS.Views.Controls;
using UIKit;

    using Xamarin.Forms;
    using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CollectionView), typeof(NoBounceRenderer))]
namespace Tokkepedia.iOS.Views.Controls
{
    public class NoBounceRenderer : CollectionViewRenderer
    {
        public NoBounceRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<GroupableItemsView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
                Controller.CollectionView.Bounces = false;
        }
    }
}
