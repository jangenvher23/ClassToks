using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClassToksV2.Views.Tok.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TokCardView : ContentView
    {                                                         
        public TokCardView()
        {                                   
            InitializeComponent();
        }

        private void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {
            //var item = (sender as Frame);
            var item = (sender as XFFlipView);
            item.IsFlipped = item.IsFlipped ? true : false;
        }

        //public static readonly BindableProperty IsFlippedProperty =
        //BindableProperty.Create(
        //    nameof(IsFlipped),
        //    typeof(bool),
        //    typeof(TokCardView),
        //    false,
        //    BindingMode.Default,
        //    null,
        //    IsFlippedPropertyChanged);

        ///// <summary>
        ///// Gets or Sets whether the view is already flipped
        ///// ex : 
        ///// </summary>
        //public bool IsFlipped
        //{
        //    get { return (bool)this.GetValue(IsFlippedProperty); }
        //    set { this.SetValue(IsFlippedProperty, value); }
        //}

        //private static void IsFlippedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        //{
        //    if ((bool)newValue)
        //    {
        //        ((TokCardView)bindable).FlipFromFrontToBack();
        //    }
        //    else
        //    {
        //        ((TokCardView)bindable).FlipFromBackToFront();
        //    }
        //}

        ///// <summary>
        ///// Performs the flip
        ///// </summary>
        //private async void FlipFromFrontToBack()
        //{
        //    await FrontToBackRotate();

        //    // Change the visible content
        //    this.FrontView.IsVisible = false;
        //    this.BackView.IsVisible = true;

        //    await BackToFrontRotate();
        //}

        ///// <summary>
        ///// Performs the flip
        ///// </summary>
        //private async void FlipFromBackToFront()
        //{
        //    await FrontToBackRotate();

        //    // Change the visible content
        //    this.BackView.IsVisible = false;
        //    this.FrontView.IsVisible = true;

        //    await BackToFrontRotate();
        //}

        //#region Animation Stuff

        //private async Task<bool> FrontToBackRotate()
        //{
        //    ViewExtensions.CancelAnimations(this);

        //    this.RotationY = 360;

        //    await this.RotateYTo(270, 500, Easing.Linear);

        //    return true;
        //}

        //private async Task<bool> BackToFrontRotate()
        //{
        //    ViewExtensions.CancelAnimations(this);

        //    this.RotationY = 90;

        //    await this.RotateYTo(0, 500, Easing.Linear);

        //    return true;
        //}

        //#endregion
    }

}