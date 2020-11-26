using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClassToksV2.Models;
using ClassToksV2.Views;
using Newtonsoft.Json;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using Xamarin.Forms;

namespace ClassToksV2.ViewModels
{
    public enum TokFormat { Basic=0, Detailed=1, Mega=2 }

    public class AddEditTokViewModel : BaseViewModel
    {
        //private ClassTokXF _item;
        //public ClassTokXF Item
        //{
        //    get { return _item; }
        //    set { SetProperty(ref _item, value); }
        //}


        //Section
        public TokSectionsViewModel Sections { get; set; }

        public ClassTokXF Item { get; set; } = new ClassTokXF() { IsAddMode = true };

        public List<string> TokFormats { get; set; } = new List<string>() { "Basic", "Detailed", "Mega" };

        private int _tokFormatNum = (int)TokFormat.Basic;
        public int TokFormatNum
        {
            get { return _tokFormatNum; }
            set {  SetProperty(ref _tokFormatNum, value); }
        }

        public WaitingView Loader { get; set; }
        private bool _isLoaderVisible = false;
        public bool IsLoaderVisible
        {
            get { return _isLoaderVisible; }
            set { SetProperty(ref _isLoaderVisible, value); }
        }


        public AddEditTokViewModel()
        {
            AddEditTokCommand = new Command(OnAddEditTok);
        }

        public Command AddEditTokCommand { get; }

        public async void OnAddEditTok()
        {
            //if (!App.CheckCredentials())
            //{
            //    await Application.Current.MainPage.DisplayAlert("Notice", "You must be logged in to add a tok.", "OK");
            //    return;
            //}

            if (!IsBusy)
            {
                IsBusy = true;
                //Loader.IsVisible = true;
                //IsLoaderVisible = true;

                try
                {
                    #region Get from text entries
                    Item.Label = "classtok";

                    #region Secondary field only 
                    if (!(Item?.IsDetailBased ?? false) && !(Item?.IsMegaTok ?? false)) //Basic (not detailed or mega classTok)
                    {
                        //Leave as is
                        //Item.SecondaryFieldText = txtSecondary.Text;

                        Item.Details = null;
                        Item.EnglishDetails = null;
                        Item.IsMegaTok = null;
                    }
                    else if ((Item?.IsDetailBased ?? false) && !(Item?.IsMegaTok ?? false)) //Detailed (not basic or mega classTok)
                    {
                        Item.SecondaryFieldText = null;
                        Item.EnglishSecondaryFieldText = null;
                        Item.IsMegaTok = null;


                        //Check each detail
                        //Item.Details = new string[DetailsCount];
                        //for (int i = 0; i < DetailsCount; ++i)
                        //{
                        //    Item.Details[i] = Item.DetailsXF[i].Detail;
                        //}

                        //if (!Item.IsEnglish)
                        //{
                        //    Item.EnglishDetails = new string[DetailsCount];
                        //    //Check each detail
                        //    for (int i = 0; i < DetailsCount; ++i)
                        //    {
                        //        Item.EnglishDetails[i] = Item.EnglishDetailsXF[i].Detail;
                        //    }
                        //}
                    }
                    else if (Item?.IsMegaTok ?? false) //Mega
                    {
                        Item.SecondaryFieldText = null;
                        Item.EnglishSecondaryFieldText = null;
                        Item.Details = null;
                    }
                    #endregion
                    #endregion

                    //Fields
                    Item.CategoryId = $"category-{Item.Category?.ToIdFormat()}";
                    Item.TokTypeId = $"toktype-{Item.TokGroup?.ToIdFormat()}-{Item.TokType?.ToIdFormat()}";

                    //User
                    //User Data
                    Item.UserDisplayName = Settings.GetTokketUser().DisplayName;
                    Item.UserId = Settings.GetTokketUser().Id;
                    Item.UserCountry = Settings.GetTokketUser().Country;
                    Item.UserState = Settings.GetTokketUser().State;
                    Item.UserPhoto = Settings.GetTokketUser().UserPhoto;

                    //API
                    if (Item.IsAddMode)
                    {
                        var result = await ClassService.Instance.AddClassToksAsync(Item);

                        //Mega Tok sections if necessary
                        await AddTokSections(result);

                        Item.ColorMainHex = "#FFFFFF";
                        Item.ColorXF = Color.FromHex("#000000"); //Black border before page refresh
                        Item = Item.ProcessItem();
                        App.Toks.Items.Insert(0, Item);
                    }
                    else
                    {
                        await ClassService.Instance.UpdateClassToksAsync(Item);
                        var curIndex = App.Toks.Items.IndexOf(App.TokParameter);

                        App.Toks.Items.RemoveAt(curIndex);
                        App.Toks.Items.Insert(curIndex, Item);
                        App.TokParameterUIList.SetBinding(CollectionView.ItemsSourceProperty, nameof(App.Toks.Items));
                    }

                    Loader.IsVisible = false;
                    App.TokParameter = Item;
                    await Shell.Current.GoToAsync("../..");
                }
                catch (Exception ex)
                {

                }

                //IsLoaderVisible = false;
                //Loader.IsVisible = false;
                IsBusy = false;
            }

        }

        private async Task AddTokSections(ResultModel result)
        {
            var tok = JsonConvert.DeserializeObject<ClassTokModel>(JsonConvert.SerializeObject(result.ResultObject));

            //Saving Sections
            if (result.ResultEnum == Result.Success)
            {
                if (Item.TokGroup.ToLower() == "mega") //If Mega
                {
                    if (Item.IsAddMode)
                    {
                        for (int i = 0; i < Item.Sections.Length; ++i)
                        {
                            Item.Sections[i].Id = $"{tok.Id}-section{i+1}";
                            Item.Sections[i].TokId = tok.Id;
                            Item.Sections[i].TokTypeId = Item.TokTypeId;
                            Item.Sections[i].UserId = Settings.GetUserModel().UserId;

                            var section = await TokService.Instance.CreateTokSectionAsync(Item.Sections[i], tok.Id, 0);
                        }
                    }
                    //TODO: Edit
                }
            }
        }

        private string _toolbarText = "Add Class Tok";
        public string ToolbarText
        {
            get { return _toolbarText; }
            set { SetProperty(ref _toolbarText, value); }
        }

        private int _detailsCount = 0;
        public int DetailsCount
        {
            get { return _detailsCount; }
            set {
                //var arr = Item?.Details;
                //if (arr != null)
                //    Array.Resize(ref arr, _detailsCount);
                //Item.Details = arr;

                SetProperty(ref _detailsCount, value); }
        }

        private List<int> _detailsCounts = new List<int>() { 3, 4, 5, 6, 7, 8, 9, 10 };
        public List<int> DetailsCounts
        {
            get { return _detailsCounts; }
            set {
                SetProperty(ref _detailsCounts, value); 
            }
        }
    }
}
