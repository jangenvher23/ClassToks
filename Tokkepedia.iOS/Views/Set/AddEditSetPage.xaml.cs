using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tokkepedia.iOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddEditSetPage : ContentPage
    {
        int numDetails = 3;

#if _CLASSTOKS
        ClassSetModel model = new ClassSetModel();
#else
        Set model = new Set();
#endif

        public AddEditSetPage()
        {
            InitializeComponent();
        }

        private async void btnAddEditSet_Clicked(object sender, EventArgs e)
        {
#if _CLASSTOKS

            //model.UserId = UserId;
            model.TokGroup = pckTokGroup?.SelectedItem.ToString();
            model.TokType = txtTokType.Text;
            //model.Privacy = ;
            model.Name = txtName.Text;
            model.Description = txtDescription.Text;

            model.GroupId = pckTokGroup?.SelectedItem.ToString();
            model.TokTypeId = $"toktype-{model.TokGroup.ToIdFormat()}-{model.TokType.ToIdFormat()}";

            //API
            var result = await ClassService.Instance.AddClassSetAsync(model);
#else
            Set set = new Set();
#endif
        }

        private void pckTokGroup_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

    
}