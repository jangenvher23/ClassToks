using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tokkepedia.iOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddEditGroupPage : ContentPage
    {
        int numDetails = 3;

        public AddEditGroupPage()
        {
            InitializeComponent();
        }

        private void btnAddEditGroup_Clicked(object sender, EventArgs e)
        {

        }
    }

    
}