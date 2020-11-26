using System.ComponentModel;
using Xamarin.Forms;
using ClassToksV2.ViewModels;
using Tokkepedia.Shared.Helpers;
using System;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;

namespace ClassToksV2.Views
{
    public partial class TokInfoPage : ContentPage
    {
        TokInfoViewModel ViewModel { get; set; }
        public TokInfoPage()
        {
            InitializeComponent();
            ViewModel = new TokInfoViewModel(App.TokParameter) { Loader = waitingView };
            if (App.TokParameter != null)
            {
                ViewModel.Item = App.TokParameter;
                ViewModel.Sections = new TokSectionsViewModel() { TokId = ViewModel.Item.Id };
                ViewModel.Comments = new CommentsViewModel() { TokId = ViewModel.Item.Id };
            }

            BindingContext = ViewModel;
        }

        protected override async void OnAppearing()
        {
            ViewModel.Item = App.TokParameter;
            BindingContext = ViewModel;
            base.OnAppearing();

            if (ViewModel?.Item.IsMegaTok == true)
            {
                await ViewModel.Sections.OnAppearing();
            }
            await ViewModel.Comments.OnAppearing();

            if (ViewModel.Item?.UserId != Settings.GetTokketUser()?.Id)
            {
                ViewModel.options = new string[] { "+ Add to Class Set", "Report Bad Tok", "Share" };
                ViewModel.isOwner = false;
            }
        }

        private async void Button_Clicked(object sender, System.EventArgs e)
        {
            string[] options = new string[] { "Edit", "Delete" };

            if (ViewModel?.Item.UserId != Settings.GetTokketUser()?.Id)
            {
                options = new string[] { "Report" };
            }

            await Application.Current.MainPage.DisplayActionSheet("Tok Section", "Cancel", null, options);
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool hasText = !string.IsNullOrEmpty((sender as Entry).Text);
            btnPost.IsEnabled = hasText;
        }

        private async void btnPost_Clicked(object sender, System.EventArgs e)
        {
            ViewModel.Loader.IsVisible = true;

            try
            {
                ReactionModel tokkepediaReaction = new ReactionModel();
                tokkepediaReaction.Text = txtComment.Text; //Comment
                tokkepediaReaction.UserId = Settings.GetUserModel().UserId;
                tokkepediaReaction.UserDisplayName = Settings.GetUserModel().DisplayName;
                tokkepediaReaction.UserPhoto = Settings.GetUserModel().UserPhoto;
                tokkepediaReaction.Timestamp = DateTime.Now;
                tokkepediaReaction.IsComment = true;
                tokkepediaReaction.Kind = "comment";
                tokkepediaReaction.Label = "reaction";
                tokkepediaReaction.DetailNum = 0;
                tokkepediaReaction.CategoryId = ViewModel.Item.CategoryId;
                tokkepediaReaction.TokTypeId = ViewModel.Item.TokTypeId;
                tokkepediaReaction.OwnerId = ViewModel.Item.UserId;
                tokkepediaReaction.ItemId = ViewModel.Item.Id;

                var result = await ReactionService.Instance.AddReaction(tokkepediaReaction);

                txtComment.Text = "";
            }
            catch (Exception ex)
            {
                var accept = await Application.Current.MainPage.DisplayAlert("Uh-oh", "Could not post the comment.", "Delete", "Cancel");
            }

            ViewModel.Loader.IsVisible = false;
        }
    }
}