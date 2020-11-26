using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClassToksV2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WaitingView : ContentView
    {
        public WaitingView()
        {
            InitializeComponent();
        }

        //public static readonly BindableProperty TextProperty =
        //BindableProperty.Create(
        //    nameof(Text),
        //    typeof(string),
        //    typeof(WaitingView),
        //    "Please wait...",
        //    BindingMode.TwoWay,
        //    null,
        //    null);

        //public string Text
        //{
        //    get { return this.GetValue(TextProperty).ToString(); }
        //    set { this.SetValue(TextProperty, value); }
        //}

        //public static readonly BindableProperty TextColorProperty =
        //BindableProperty.Create(
        //    nameof(TextColor),
        //    typeof(Color),
        //    typeof(WaitingView),
        //    Color.White,
        //    BindingMode.TwoWay,
        //    null,
        //    null);

        //public Color TextColor
        //{
        //    get { return (Color)this.GetValue(TextColorProperty); }
        //    set { this.SetValue(TextColorProperty, value); }
        //}
    }
}