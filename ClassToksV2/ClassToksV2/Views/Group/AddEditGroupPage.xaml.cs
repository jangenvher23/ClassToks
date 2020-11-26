#define _CLASSTOKS
using ClassToksV2.Models;
using ClassToksV2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClassToksV2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddEditGroupPage : ContentPage
    {
        int numDetails = 3;

        AddEditGroupViewModel ViewModel { get; set; } = new AddEditGroupViewModel();

        public AddEditGroupPage()
        {
            InitializeComponent();
            ViewModel = new AddEditGroupViewModel() { Loader = waitingView };

            if (App.GroupParameter != null)
                ViewModel.Item = App.GroupParameter;
            else
                ViewModel.Item = new ClassGroupXF() { IsAddMode = true };
            ViewModel.ToolbarText = ViewModel.Item.IsAddMode ? "Add Group" : "Save Edit";

            BindingContext = ViewModel;
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            //ClassTokXF newItem = new ClassTokXF()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    PrimaryFieldText = txtPrimary.Text,
            //    UserDisplayName = "Hello",
            //    ColorXF = Color.FromHex("#d32f2f")
            //};
            //App.Toks.Items.Insert(0, Item);
            //await DataStore.AddItemAsync(newItem);


            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }
    }
}

//private async void btnAddEditTok_Clicked(object sender, EventArgs e)
//{
//    ////Show loader
//    //waitingView.IsVisible = true;

//    //#region Get from text enties
//    //ViewModel.Item.UserId = "1tULByrYkkdXQzvRVR3EZqavKZh1"; //Settings.GetUserModel().UserId;
//    //ViewModel.Item.Label = "classtok";

//    //#region Secondary field only 
//    //if (!(ViewModel.Item?.IsDetailBased ?? false) && !(ViewModel.Item?.IsMegaTok ?? false)) //Basic (not detailed or mega classTok)
//    //{
//    //    ViewModel.Item.SecondaryFieldText = txtSecondary.Text;

//    //    ViewModel.Item.Details = null;
//    //    ViewModel.Item.IsMegaTok = null;
//    //}
//    //else if ((ViewModel.Item?.IsDetailBased ?? false) && !(ViewModel.Item?.IsMegaTok ?? false)) //Detailed (not basic or mega classTok)
//    //{
//    //    ViewModel.Item.SecondaryFieldText = null;
//    //    ViewModel.Item.IsMegaTok = null;
//    //}
//    //else if (ViewModel.Item?.IsMegaTok ?? false) //Mega
//    //{
//    //    ViewModel.Item.SecondaryFieldText = null;
//    //    ViewModel.Item.Details = null;
//    //}
//    //#endregion
//    //#endregion

//    ////API
//    //await ClassService.Instance.AddClassToksAsync(ViewModel.Item);

//    ////Hide loader
//    //waitingView.IsVisible = false;

//    //#if _CLASSTOKS
//    //            #region Get from text enties
//    //            Item.UserId = Settings.GetUserModel().UserId;
//    //            Item.Label = "classtok";
//    //            Item.Category = txtCategory.Text;
//    //            Item.PrimaryFieldText = txtPrimary.Text;
//    //            Item.Notes = txtNotes.Text;

//    //            #region Secondary field only 
//    //            if (!(Item?.IsDetailBased ?? false) && !(Item?.IsMegaTok ?? false)) //Basic (not detailed or mega classTok)
//    //            {
//    //                Item.SecondaryFieldText = txtSecondary.Text;

//    //                Item.Details = null;
//    //                Item.IsMegaTok = null;
//    //            }
//    //            else if ((Item?.IsDetailBased ?? false) && !(Item?.IsMegaTok ?? false)) //Detailed (not basic or mega classTok)
//    //            {
//    //                Item.SecondaryFieldText = null;
//    //                Item.IsMegaTok = null;
//    //            }
//    //            else if (Item?.IsMegaTok ?? false) //Mega
//    //            {
//    //                Item.SecondaryFieldText = null;
//    //                Item.Details = null;
//    //            }
//    //            Item.SecondaryFieldText = txtSecondary.Text;
//    //            #endregion
//    //            #endregion

//    //            //API
//    //            //await ClassService.Instance.AddClassToksAsync(Item);
//    //#else
//    //            #region Get from text enties
//    //            tok.Category = txtCategory.Text;
//    //            tok.PrimaryFieldText = txtPrimary.Text;
//    //            tok.Notes = txtNotes.Text;

//    //            #region Secondary field only 
//    //            if (!(tok?.IsDetailBased ?? false) && !(tok?.IsMegaTok ?? false)) //Basic (not detailed or mega tok)
//    //            {
//    //                tok.SecondaryFieldText = txtSecondary.Text;

//    //                tok.Details = null;
//    //                tok.IsMegaTok = null;
//    //            }
//    //            else if ((tok?.IsDetailBased ?? false) && !(tok?.IsMegaTok ?? false)) //Detailed (not basic or mega tok)
//    //            {
//    //                tok.SecondaryFieldText = null;
//    //                tok.IsMegaTok = null;
//    //            }
//    //            else if (tok?.IsMegaTok ?? false) //Mega
//    //            {
//    //                tok.SecondaryFieldText = null;
//    //                tok.Details = null;
//    //            }
//    //            tok.SecondaryFieldText = txtSecondary.Text;
//    //            #endregion
//    //            #endregion

//    //            //API
//    //            await Tokkepedia.Shared.Services.TokService.Instance.CreateTokAsync(tok);
//    //#endif
//}