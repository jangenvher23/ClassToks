#define _CLASSTOKS
using ClassToksV2.Models;
using ClassToksV2.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;
using Xamarin.Forms.Xaml;

namespace ClassToksV2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddEditTokPage : ContentPage
    {
        int numDetails = 3;

        #region Tok Section Views
        //Contains all Section views stacks
        //[0]: Section1, [1] Section2, etc
        ObservableCollection<StackLayout> lstSectionsViews = new ObservableCollection<StackLayout>();

        //The items in each section stack:
        List<Entry> lstSectionTitlesEntry = new List<Entry>();
        List<Label> lstSectionTitlesCounter = new List<Label>();

        List<Editor> lstSectionContentEditor = new List<Editor>();
        List<Label> lstSectionContentCounter = new List<Label>();
        #endregion
        

        AddEditTokViewModel ViewModel { get; set; }
        public AddEditTokPage()
        {
            InitializeComponent();
            ViewModel = new AddEditTokViewModel() { Loader = waitingView }; //  

            //Edit
            if (App.TokParameter != null)
            {
                App.TokParameter.IsAddMode = false; //Edit
                ViewModel.Item = App.TokParameter;

                if (ViewModel.Item.TokGroup == "Mega")
                {
                    ViewModel.Item.IsMegaTok = true;
                    ViewModel.Item.IsMega = true;
                }

                ViewModel.Sections = new TokSectionsViewModel() { TokId = ViewModel.Item.Id };
            }
            //else
            //    ViewModel.Item = new ClassTokXF() { TokGroup = "Basic", IsAddMode = true }; //Add

            ViewModel.ToolbarText = ViewModel.Item.IsAddMode ? "Add Class Tok" : "Update";

            #region Get from Tok Info
            int filledDetails = 0;
            if (!ViewModel.Item.IsAddMode && ViewModel.Item.IsDetailBased) //Edit
            {
                for (int i = 0; i < (ViewModel.Item.Details?.Length ??0); ++i)
                {
                    filledDetails += string.IsNullOrEmpty(ViewModel.Item.Details?[i]) ? 0 : 1;
                }
            }

            var lst = new List<TokDetailXF>();
            for (int i = 0; i < (ViewModel.Item.IsAddMode ? 3 : filledDetails); ++i)
            {
                lst.Add(new TokDetailXF()
                {
                    DetailLabel = $"Detail {i + 1}",
                    Detail = ViewModel.Item.IsAddMode ? "" : ViewModel.Item.Details[i]
                });
            }

            if (ViewModel.Item.IsAddMode)
            {
                pckTokGroup.SelectedIndex = 0;
            }
            else
            {
                pckTokGroup.SelectedIndex = 0;
                if (ViewModel.Item.IsDetailBased)
                    pckTokGroup.SelectedIndex = 1;
                if (ViewModel.Item.TokGroup == "Mega")
                    pckTokGroup.SelectedIndex = 2;

                //pckDetails.SelectedIndex = (filledDetails - 3);
                ViewModel.DetailsCount = filledDetails;

                //Hide non filled
                if (filledDetails >= 4)
                {
                    for (int i = 4; i <= filledDetails; ++i) //If 4 filled, then 4 last num
                    {
                        SetDetailVisibility(i, false);
                    }
                }
            }

            ViewModel.Item.DetailsXF = lst;
            ViewModel.DetailsCount = lst.Count;
            #endregion

            BindingContext = ViewModel;

            // A Picker can be initialized to display a specific item by setting the SelectedIndex or SelectedItem properties. However, these properties must be set after initializing the ItemsSource collection. 
            // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/picker/populating-itemssource#responding-to-item-selection
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            string format = ViewModel.Item.TokGroup; //Basic
            if (!ViewModel.Item.Details?.IsAllEmpty() ?? false) //isn't all empty
                format = "Detailed";
            else if (!ViewModel.Item.SectionTitles?.IsAllEmpty() ?? false)
                format = "Mega";
            ToggleFields(format);

            //Show/Hide English
            stkEnglish.IsVisible = ViewModel.Item.IsEnglish ? false : true;

            //Picker
            ViewModel.TokFormatNum = 0;
            pckTokGroup.SelectedIndex = 0;

            if (!ViewModel.Item.IsAddMode)
            {
                if (ViewModel.Item.IsDetailBased || (!ViewModel.Item.Details?.IsAllEmpty() ?? false))
                {
                    pckTokGroup.SelectedIndex = 1;
                    ViewModel.TokFormatNum = 1;
                }
                else if (ViewModel.Item.IsMega || (!ViewModel.Item.SectionTitles?.IsAllEmpty() ?? false))
                {
                    pckTokGroup.SelectedIndex = 2;
                    ViewModel.TokFormatNum = 2;
                }
                pckTokGroup_SelectedIndexChanged(pckTokGroup, new EventArgs());
            }

            //Section initialize
            ViewModel.Item.SectionCount = 1;
            AddSection(1, ViewModel.Item.IsAddMode);

            //If Mega and Edit, load sections
            if (ViewModel?.Item.IsMegaTok == true && !ViewModel.Item.IsAddMode)
            {
                // some long running task
                stkSectionsScroll.IsEnabled = false;
                ViewModel.IsLoaderVisible = true;

                Task.Run(() => {
                    //Details

                        //Get everything
                        ViewModel.Sections.ExecuteLoadItemsCommand().ContinueWith(s => {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (ViewModel.Sections.Items.Count > 0)
                            {
                                //Parallel.For(0, ViewModel.Sections.Items.Count, )

                                for (int i = 0; i < ViewModel.Sections.Items.Count; ++i)
                                {
                                    if (ViewModel.Sections.Items.Count > 0)
                                        InsertSectionTitle(i + 1, ViewModel.Sections.Items[i].Title);
                                }

                                ViewModel.Item.SectionCount = ViewModel.Sections.Items.Count;
                            }

                            ViewModel.IsLoaderVisible = false;
                            stkSectionsScroll.IsEnabled = true;
                        });
                    });

                });
            }

            //English
            if (!string.IsNullOrEmpty(ViewModel.Item.EnglishPrimaryFieldText) && !(ViewModel.Item.IsMega || !(ViewModel.Item.SectionTitles?.IsAllEmpty() ?? true))) //Not mega = all empty
            {
                chkEnglish.IsChecked = false;
                stkEnglish.IsVisible = true;
            }
        }

        public async Task LoadSectionsAsync()
        {
            await Task.Run(() => {
                // some long running task
                stkSectionsScroll.IsEnabled = false;
                ViewModel.IsLoaderVisible = true;

                //Get everything
                while (string.IsNullOrEmpty(ViewModel.Sections.ContinuationToken)) {
                    ViewModel.Sections.ExecuteLoadItemsCommand().ContinueWith(s => {
                        MainThread.BeginInvokeOnMainThread( () =>
                        {
                            if (ViewModel.Sections.Items.Count > 0)
                            {

                                //Parallel.For(0, ViewModel.Sections.Items.Count, )

                                for (int i = 0; i < ViewModel.Sections.Items.Count; ++i)
                                {
                                    if (ViewModel.Sections.Items.Count > 0)
                                        InsertSectionTitle(i + 1, ViewModel.Sections.Items[i].Title);
                                }

                                ViewModel.Item.SectionCount = ViewModel.Sections.Items.Count;
                            }

                            ViewModel.IsLoaderVisible = false;
                            stkSectionsScroll.IsEnabled = true;
                        });
                    });
                }
                
            });
        }

        #region Tok Format
        private void pckTokGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (sender as Picker).SelectedItem.ToString();
            ViewModel.Item.TokGroup = item;
            ToggleFields(ViewModel.Item.TokGroup);
        }

        private void ToggleFields(string format)
        {
            if (format == "Detailed")
            {
                ViewModel.Item.IsMegaTok = false;
                ViewModel.Item.IsDetailBased = true;
                ViewModel.Item.IsBasic = false;

                stkSections.IsVisible = false;

                lblSecondary.IsVisible = false;
                txtSecondary.IsVisible = false;
                lblSecondaryCounter.IsVisible = false;

                lblEnglishSecondary.IsVisible = false;
                txtEnglishSecondary.IsVisible = false;
                lblEnglishSecondaryCounter.IsVisible = false;

                lblDetailCount.IsVisible = true;
                stkDetailsToggle.IsVisible = true;
                stkDetails.IsVisible = true;
                stkEnglishDetails.IsVisible = true;

                //Sections
                stkSections.IsVisible = false;
                lblSections.IsVisible = false;
                stkSectionsScroll.IsVisible = false;
                stkSectionsToggle.IsVisible = false;

                //Notes
                lblNotes.IsVisible = false;
                txtNotes.IsVisible = false;
                lblNotesCounter.IsVisible = false;

                //Language
                stkEnglishCheck.IsVisible = true;
                stkEnglish.IsVisible = chkEnglish.IsChecked ? false : true;
            }
            else if (format == "Basic")
            {
                ViewModel.Item.IsMegaTok = false;
                ViewModel.Item.IsDetailBased = false;
                ViewModel.Item.IsBasic = true;
                stkSections.IsVisible = false;

                lblSecondary.IsVisible = true;
                txtSecondary.IsVisible = true;
                lblSecondaryCounter.IsVisible = true;

                lblEnglishSecondary.IsVisible = true;
                txtEnglishSecondary.IsVisible = true;
                lblEnglishSecondaryCounter.IsVisible = true;

                lblDetailCount.IsVisible = false;
                stkDetailsToggle.IsVisible = false;
                stkDetails.IsVisible = false;
                stkEnglishDetails.IsVisible = false;

                //Sections
                stkSections.IsVisible = false;
                lblSections.IsVisible = false;
                stkSectionsScroll.IsVisible = false;
                stkSectionsToggle.IsVisible = false;

                //Notes
                lblNotes.IsVisible = true;
                txtNotes.IsVisible = true;
                lblNotesCounter.IsVisible = true;

                //Language
                stkEnglishCheck.IsVisible = true;
                stkEnglish.IsVisible = chkEnglish.IsChecked ? false : true;
            }
            else //Mega
            {
                ViewModel.Item.IsMegaTok = true;
                ViewModel.Item.IsDetailBased = false;
                ViewModel.Item.IsBasic = false;
                stkSections.IsVisible = true;

                lblSecondary.IsVisible = false;
                txtSecondary.IsVisible = false;
                lblSecondaryCounter.IsVisible = false;

                lblEnglishSecondary.IsVisible = false;
                txtEnglishSecondary.IsVisible = false;
                lblEnglishSecondaryCounter.IsVisible = false;

                lblDetailCount.IsVisible = false;
                stkDetailsToggle.IsVisible = false;
                stkDetails.IsVisible = false;
                stkEnglishDetails.IsVisible = false;

                //Sections
                stkSections.IsVisible = true;
                lblSections.IsVisible = true;
                stkSectionsScroll.IsVisible = true;
                stkSectionsToggle.IsVisible = true;
                
                if (ViewModel.Item.SectionCount == null)
                {
                    ViewModel.Item.SectionCount = 1;
                    btnRemoveSection.IsVisible = false;
                }

                //Notes
                lblNotes.IsVisible = false;
                txtNotes.IsVisible = false;

                //Language
                stkEnglishCheck.IsVisible = false;
                stkEnglish.IsVisible = false;
            }
        }
        #endregion

        #region Details
        private async void ToggleDetail_Clicked(object sender, EventArgs e)
        {
            var button = (sender as Button);
            if (button.ClassId == "AddDetail")
            {
                await SetDetailVisibility(numDetails + 1, true);
                numDetails++;
                ++ViewModel.DetailsCount;
            }
            else if (button.ClassId == "RemoveDetail")
            {
                await SetDetailVisibility(numDetails, false);
                numDetails--;
                --ViewModel.DetailsCount;
            }

            btnAddDetail.IsVisible = (numDetails >= 10) ? false : true;
            btnRemoveDetail.IsVisible = (numDetails <= 3) ? false : true;
        }

        private async void btnAddDetail_Clicked(object sender, EventArgs e)
        {
            await SetDetailVisibility(numDetails + 1, true);

            numDetails++;
            //Maximum of 3 details
            //btnRemoveDetail.IsVisible = true;
            //if (numDetails >= 10)
                //btnAddDetail.IsVisible = false;
        }

        private async void btnRemoveDetail_Clicked(object sender, EventArgs e)
        {
            await SetDetailVisibility(numDetails, false);

            numDetails--;
            //Minimum of 3 details
            //btnAddDetail.IsVisible = true;
            //if (numDetails <= 3)
            //    btnRemoveDetail.IsVisible = false;
        }

        private async Task SetDetailVisibility(int detailNum, bool isVisible)
        {
            switch (detailNum)
            {
                case 4:
                    lblDetail4.IsVisible = isVisible;
                    txtDetail4.IsVisible = isVisible;
                    lblDetail4Counter.IsVisible = isVisible;
                    lblEnglishDetail4.IsVisible = isVisible;
                    txtEnglishDetail4.IsVisible = isVisible;
                    lblEnglishDetail4Counter.IsVisible = isVisible;
                    if (isVisible) await stkDetails.ScrollToAsync(lblDetail4Counter, ScrollToPosition.End, false); else await stkDetails.ScrollToAsync(lblDetail3Counter, ScrollToPosition.End, false);
                    break;
                case 5:
                    lblDetail5.IsVisible = isVisible;
                    txtDetail5.IsVisible = isVisible;
                    lblDetail5Counter.IsVisible = isVisible;
                    lblEnglishDetail5.IsVisible = isVisible;
                    txtEnglishDetail5.IsVisible = isVisible;
                    lblEnglishDetail5Counter.IsVisible = isVisible;
                    if (isVisible) await stkDetails.ScrollToAsync(lblDetail5Counter, ScrollToPosition.End, false); else await stkDetails.ScrollToAsync(lblDetail4Counter, ScrollToPosition.End, false);
                    break;
                case 6:
                    lblDetail6.IsVisible = isVisible;
                    txtDetail6.IsVisible = isVisible;
                    lblDetail6Counter.IsVisible = isVisible;
                    lblEnglishDetail6.IsVisible = isVisible;
                    txtEnglishDetail6.IsVisible = isVisible;
                    lblEnglishDetail6Counter.IsVisible = isVisible;
                    if (isVisible) await stkDetails.ScrollToAsync(lblDetail6Counter, ScrollToPosition.End, false); else await stkDetails.ScrollToAsync(lblDetail5Counter, ScrollToPosition.End, false);
                    break;
                case 7:
                    lblDetail7.IsVisible = isVisible;
                    txtDetail7.IsVisible = isVisible;
                    lblDetail7Counter.IsVisible = isVisible;
                    lblEnglishDetail7.IsVisible = isVisible;
                    txtEnglishDetail7.IsVisible = isVisible;
                    lblEnglishDetail7Counter.IsVisible = isVisible;
                    if (isVisible) await stkDetails.ScrollToAsync(lblDetail7Counter, ScrollToPosition.End, false); else await stkDetails.ScrollToAsync(lblDetail6Counter, ScrollToPosition.End, false);
                    break;
                case 8:
                    lblDetail8.IsVisible = isVisible;
                    txtDetail8.IsVisible = isVisible;
                    lblDetail8Counter.IsVisible = isVisible;
                    lblEnglishDetail8.IsVisible = isVisible;
                    txtEnglishDetail8.IsVisible = isVisible;
                    lblEnglishDetail8Counter.IsVisible = isVisible;
                    if (isVisible) await stkDetails.ScrollToAsync(lblDetail8Counter, ScrollToPosition.End, false); else await stkDetails.ScrollToAsync(lblDetail7Counter, ScrollToPosition.End, false);
                    break;
                case 9:
                    lblDetail9.IsVisible = isVisible;
                    txtDetail9.IsVisible = isVisible;
                    lblDetail9Counter.IsVisible = isVisible;
                    lblEnglishDetail9.IsVisible = isVisible;
                    txtEnglishDetail9.IsVisible = isVisible;
                    lblEnglishDetail9Counter.IsVisible = isVisible;
                    if (isVisible) await stkDetails.ScrollToAsync(lblDetail9Counter, ScrollToPosition.End, false); else await stkDetails.ScrollToAsync(lblDetail8Counter, ScrollToPosition.End, false);
                    break;
                case 10:
                    lblDetail10.IsVisible = isVisible;
                    txtDetail10.IsVisible = isVisible;
                    lblDetail10Counter.IsVisible = isVisible;
                    lblEnglishDetail10.IsVisible = isVisible;
                    txtEnglishDetail10.IsVisible = isVisible;
                    lblEnglishDetail10Counter.IsVisible = isVisible;
                    if (isVisible) await stkDetails.ScrollToAsync(lblDetail10Counter, ScrollToPosition.End, false); else await stkDetails.ScrollToAsync(lblDetail9Counter, ScrollToPosition.End, false);
                    break;
            }
        }
        #endregion

        #region Sections
        private void ToggleSection_Clicked(object sender, EventArgs e)
        {
            var button = (sender as Button);
            if (button.ClassId == "AddSection")
            {
                //Add NEXT
                ++ViewModel.Item.SectionCount;
                //Add
                AddSection((long)ViewModel.Item.SectionCount, ViewModel.Item.IsAddMode);
            }
            else if (button.ClassId == "RemoveSection")
            {
                //Remove CURRENT
                RemoveSection((long)ViewModel.Item.SectionCount);
                --ViewModel.Item.SectionCount;
            }

            btnAddSection.IsVisible = true;
            btnRemoveSection.IsVisible = (ViewModel.Item.SectionCount == 1) ? false : true;
        }

        private void AddSection(long sectionNum, bool addMode = true)
        {
            if (stkSections.Children.FirstOrDefault(x => x.ClassId == sectionNum.ToString()) != null)
            {
                stkSections.Children.FirstOrDefault(x => x.ClassId == sectionNum.ToString()).IsVisible = true;
                return;
            }

            if (addMode)
            {
                //Add
                Label lblSection = new Label()
                {
                    ClassId = $"lblSection{sectionNum}",
                    FontSize = 20,
                    HorizontalTextAlignment = TextAlignment.Start,
                    Text = $"Section {sectionNum}"
                };
                //lstSectionsLabel.Add(lblSection);

                #region Title
                Entry txtSectionTitleEntry = new Entry()
                {
                    ClassId = $"txtSectionTitle{sectionNum}",
                    MaxLength = 200,
                    Placeholder = $"Section {sectionNum} Title"
                };
                txtSectionTitleEntry.TextChanged += Text_TextChanged;
                lstSectionTitlesEntry.Add(txtSectionTitleEntry);

                Label lblSectionTitleCounter = new Label()
                {
                    ClassId = $"lblSectionContentCounter{sectionNum}",
                    FontSize = 10,
                    HorizontalTextAlignment = TextAlignment.End,
                    Text = $"0 / 200",
                    TextColor = Xamarin.Forms.Color.LightGray
                };
                lstSectionTitlesCounter.Add(lblSectionTitleCounter);
                #endregion

                #region Content
                Editor txtSectionContent = new Editor()
                {
                    ClassId = $"txtSectionContent{sectionNum}",
                    MaxLength = 150000,
                    Placeholder = $"Enter section {sectionNum} content here...",
                    HeightRequest = 200
                };
                txtSectionContent.TextChanged += Text_TextChanged;
                lstSectionContentEditor.Add(txtSectionContent);

                Label lblSectionContentCounter = new Label()
                {
                    ClassId = $"lblSectionContentCounter{sectionNum}",
                    FontSize = 10,
                    HorizontalTextAlignment = TextAlignment.End,
                    Text = $"0 / 150000",
                    TextColor = Xamarin.Forms.Color.LightGray
                };
                lstSectionContentCounter.Add(lblSectionContentCounter);
                #endregion

                //stkSections.Children.Add(lblSection);
                //stkSections.Children.Add(lblSectionEntry);
                //stkSections.Children.Add(lblSectionContent);
                //stkSections.Children.Add(lblSectionCounter);

                StackLayout sec = new StackLayout()
                {
                    ClassId = $"Section{sectionNum}",
                    Children = { lblSection, txtSectionTitleEntry, lblSectionTitleCounter, txtSectionContent, lblSectionContentCounter }
                };
                lstSectionsViews.Add(sec);
                stkSections.Children.Add(lstSectionsViews.ElementAt((int)sectionNum - 1));
            }
            else
            {
                InsertSectionTitle(sectionNum);
            }
        }

        private void RemoveSection(long sectionNum)
        {
            if (sectionNum > 1)
            {
                stkSections.Children.FirstOrDefault(x => x.ClassId == sectionNum.ToString()).IsVisible = false;
            }
        }

        private void InsertSectionTitle(long sectionNum, string sectionTitle = null)
        {
            //Clear existing if 1
            if (sectionNum == 1)
            {
                lstSectionsViews.Clear();
                stkSections.Children.Clear();
            }

            //Grid
            //<Label Text="{Binding Title}" FontSize="Small" FontAttributes="Bold" VerticalTextAlignment="Center" />
            //<Button Text="View" BackgroundColor="Transparent" BorderColor="Transparent" TextColor="Black" TextTransform="None" VerticalOptions="Center"/>
            //<Button Text="●●●" FontSize="7" BackgroundColor="Transparent" BorderColor="Transparent" TextColor="Black" TextTransform="None" VerticalOptions="Center" Clicked="Button_Clicked"/>


            //Edit
            Grid circleNumber = new Grid()
            {
                RowDefinitions = new RowDefinitionCollection() { new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } },
                ColumnDefinitions = new ColumnDefinitionCollection() { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } }
            };
            Ellipse circle = new Ellipse()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 24,
                WidthRequest = 24,
                Stroke = Brush.Black,
                StrokeThickness = 1,
                Fill = Brush.Black
            };
            //< Label Grid.Row = "0" Grid.Column = "0" FontSize = "10" Text = "{Binding SectionNumber}" VerticalOptions = "Center" HorizontalOptions = "Center" TextColor = "White" FontAttributes = "Bold" HorizontalTextAlignment = "Center" VerticalTextAlignment = "Center" />
            Label lblNumber = new Label()
            {
                ClassId = $"Section{sectionNum}Number",
                StyleId = $"Section{sectionNum}Number",
                FontAttributes = FontAttributes.Bold,
                FontSize = 10,
                TextColor = Xamarin.Forms.Color.White,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                Text = sectionNum.ToString()
            };
            circleNumber.Children.Add(circle, 0, 0);
            circleNumber.Children.Add(lblNumber, 0, 0);

            //<Label Text="{Binding Title}" FontSize="Small" FontAttributes="Bold" VerticalTextAlignment="Center" />
            Label lblTitle = new Label()
            {
                ClassId = $"Section{sectionNum}Title",
                StyleId = $"Section{sectionNum}Title",
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                VerticalTextAlignment = TextAlignment.Center,
                Text = string.IsNullOrEmpty(sectionTitle) ? $"Section {sectionNum} Title" : sectionTitle
            };


            //<Button Text="View" BackgroundColor="Transparent" BorderColor="Transparent" TextColor="Black" TextTransform="None" VerticalOptions="Center"/>
            Button btnView = new Button()
            {
                ClassId = $"Section{sectionNum}View",
                StyleId = $"Section{sectionNum}View",
                Text = "View",
                BackgroundColor = Xamarin.Forms.Color.Transparent,
                BorderColor = Xamarin.Forms.Color.Transparent,
                TextColor = Xamarin.Forms.Color.Black,
                TextTransform = TextTransform.None,
                VerticalOptions = LayoutOptions.Center
            };
            btnView.Clicked += BtnView_Clicked;

            //<Button Text="●●●" FontSize="7" BackgroundColor="Transparent" BorderColor="Transparent" TextColor="Black" TextTransform="None" VerticalOptions="Center" Clicked="Button_Clicked"/>
            Button btnMore = new Button()
            {
                ClassId = $"Section{sectionNum}More",
                StyleId = $"Section{sectionNum}More",
                Text = "●●●",
                FontSize = 7,
                BackgroundColor = Xamarin.Forms.Color.Transparent,
                BorderColor = Xamarin.Forms.Color.Transparent,
                TextColor = Xamarin.Forms.Color.Black,
                TextTransform = TextTransform.None,
                VerticalOptions = LayoutOptions.Center
            };
            btnMore.Clicked += BtnMore_Clicked;

            //stkSections.Children.Add(lblSection);
            //stkSections.Children.Add(lblSectionEntry);
            //stkSections.Children.Add(lblSectionContent);
            //stkSections.Children.Add(lblSectionCounter);

            StackLayout sec = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,

                ClassId = $"{sectionNum}",
                Children =
                    {
                        circleNumber, lblTitle, btnView, btnMore
                    }
            };
            lstSectionsViews.Add(sec);
            stkSections.Children.Add(lstSectionsViews.ElementAt((int)sectionNum - 1));
        }

        private async void BtnView_Clicked(object sender, EventArgs e)
        {
            await Application.Current.MainPage.DisplayActionSheet("Tok Section", "Cancel", null, "Edit", "Delete");
        }

        private async void BtnMore_Clicked(object sender, EventArgs e)
        {
            await Application.Current.MainPage.DisplayActionSheet("Tok Section", "Cancel", null, "Edit", "Delete");
        }
        #endregion

        #region Bilingual
        private void chkEnglish_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            ViewModel.Item.IsEnglish = ViewModel.Item.IsEnglish ? false : true;
            stkEnglish.IsVisible = !ViewModel.Item.IsEnglish;
        }
        #endregion

        #region Add / Edit Class Tok
        private async void btnAddEditTok_Clicked(object sender, EventArgs e)
        {
            //Loader
            waitingView.IsVisible = true;

            //Required
            var reqFields = SetRequiredFields();
            if (!reqFields.Item1)
            {
                await Application.Current.MainPage.DisplayAlert("Required", reqFields.Item2, "OK");
                waitingView.IsVisible = false;
                return;
            }
                

            //Copy
            SetPrivacy();

            //Details
            if ((ViewModel.Item?.IsDetailBased ?? false) && !(ViewModel.Item?.IsMegaTok ?? false))
                SetDetails();
            else
            {
                ViewModel.Item.Details = null;
                ViewModel.Item.EnglishDetails = null;
            }

            //Sections
            SetSections();

            ViewModel.OnAddEditTok();
        }

        (bool, string) SetRequiredFields()
        {
            if (string.IsNullOrEmpty(txtTokType.Text))
                return (false, "Please enter a class name.");
            if (string.IsNullOrEmpty(txtCategory.Text))
                return (false, "Please enter a category.");
            if (string.IsNullOrEmpty(txtPrimary.Text))
                return (false, "Please enter a title.");
            if (string.IsNullOrEmpty(txtPrimary.Text) && !chkEnglish.IsChecked)
                return (false, "Please enter an English translation for the title.");

            //Tok Format and English based
            if (pckTokGroup.SelectedIndex == 0)
            {
                if (string.IsNullOrEmpty(txtSecondary.Text))
                    return (false, "Please enter a title.");
                if (string.IsNullOrEmpty(txtSecondary.Text) && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for the secondary field.");
            }
            else if (pckTokGroup.SelectedIndex == 1)
            {
                if (string.IsNullOrEmpty(txtDetail1.Text))
                    return (false, "Please enter text for detail 1.");
                if (string.IsNullOrEmpty(txtDetail1.Text) && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for Detail 1.");

                if (string.IsNullOrEmpty(txtDetail2.Text))
                    return (false, "Please enter text for detail 2.");
                if (string.IsNullOrEmpty(txtDetail2.Text) && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for Detail 2.");

                if (string.IsNullOrEmpty(txtDetail3.Text))
                    return (false, "Please enter text for detail 3.");
                if (string.IsNullOrEmpty(txtDetail3.Text) && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for Detail 3.");

                //Must be visible
                if (string.IsNullOrEmpty(txtDetail4.Text) && txtDetail4.IsVisible)
                    return (false, "Please enter text for detail 4.");
                if (string.IsNullOrEmpty(txtDetail4.Text) && txtDetail4.IsVisible && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for Detail 4.");

                if (string.IsNullOrEmpty(txtDetail5.Text) && txtDetail5.IsVisible)
                    return (false, "Please enter text for detail 5.");
                if (string.IsNullOrEmpty(txtDetail5.Text) && txtDetail5.IsVisible && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for Detail 5.");

                if (string.IsNullOrEmpty(txtDetail6.Text) && txtDetail6.IsVisible)
                    return (false, "Please enter text for detail 6.");
                if (string.IsNullOrEmpty(txtDetail6.Text) && txtDetail6.IsVisible && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for Detail 6.");

                if (string.IsNullOrEmpty(txtDetail7.Text) && txtDetail7.IsVisible)
                    return (false, "Please enter text for detail 7.");
                if (string.IsNullOrEmpty(txtDetail7.Text) && txtDetail7.IsVisible && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for Detail 7.");

                if (string.IsNullOrEmpty(txtDetail8.Text) && txtDetail8.IsVisible)
                    return (false, "Please enter text for detail 8.");
                if (string.IsNullOrEmpty(txtDetail8.Text) && txtDetail8.IsVisible && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for Detail 8.");

                if (string.IsNullOrEmpty(txtDetail9.Text) && txtDetail9.IsVisible)
                    return (false, "Please enter text for detail 9.");
                if (string.IsNullOrEmpty(txtDetail9.Text) && txtDetail9.IsVisible && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for Detail 9.");

                if (string.IsNullOrEmpty(txtDetail10.Text) && txtDetail10.IsVisible)
                    return (false, "Please enter text for detail 10.");
                if (string.IsNullOrEmpty(txtDetail10.Text) && txtDetail10.IsVisible && !chkEnglish.IsChecked)
                    return (false, "Please enter an English translation for Detail 10.");
            }
            else if (pckTokGroup.SelectedIndex == 2)
            {
                for (int i = 0; i < lstSectionTitlesEntry.Count; ++i)
                {
                    if (string.IsNullOrEmpty(lstSectionTitlesEntry[i].Text) && lstSectionsViews[i].IsVisible)
                        return (false, $"Please enter text for Section Title {i+1}.");
                    if (string.IsNullOrEmpty(lstSectionContentEditor[i].Text) && lstSectionsViews[i].IsVisible)
                        return (false, $"Please enter text for Section Content {i + 1}.");
                }
            }

            return (true, "All required fields set.");
        }

        void SetPrivacy()
        {
            ViewModel.Item.IsPrivate = chkPrivate.IsChecked ? true : false;
            ViewModel.Item.IsGroup = chkGroup.IsChecked ? true : false;
            ViewModel.Item.IsPublic = chkPublic.IsChecked ? true : false;
        }

        void SetDetails()
        {
            //Will grab in order and skip over empty
            List<string> lstDetails = new List<string>();

            if (!string.IsNullOrEmpty(txtDetail1.Text))
                lstDetails.Add(txtDetail1.Text);
            if (!string.IsNullOrEmpty(txtDetail2.Text))
                lstDetails.Add(txtDetail2.Text);
            if (!string.IsNullOrEmpty(txtDetail3.Text))
                lstDetails.Add(txtDetail3.Text);
            if (!string.IsNullOrEmpty(txtDetail4.Text))
                lstDetails.Add(txtDetail4.Text);
            if (!string.IsNullOrEmpty(txtDetail5.Text))
                lstDetails.Add(txtDetail5.Text);
            if (!string.IsNullOrEmpty(txtDetail6.Text))
                lstDetails.Add(txtDetail6.Text);
            if (!string.IsNullOrEmpty(txtDetail7.Text))
                lstDetails.Add(txtDetail7.Text);
            if (!string.IsNullOrEmpty(txtDetail8.Text))
                lstDetails.Add(txtDetail8.Text);
            if (!string.IsNullOrEmpty(txtDetail9.Text))
                lstDetails.Add(txtDetail9.Text);
            if (!string.IsNullOrEmpty(txtDetail10.Text))
                lstDetails.Add(txtDetail10.Text);

            ViewModel.Item.Details = lstDetails.Take(ViewModel.DetailsCount).ToArray();

            #region English Translation
            if (!ViewModel.Item.IsEnglish)
            {
                List<string> lstEnglishDetails = new List<string>();
                if (!string.IsNullOrEmpty(txtEnglishDetail1.Text))
                    lstEnglishDetails.Add(txtEnglishDetail1.Text);
                if (!string.IsNullOrEmpty(txtEnglishDetail2.Text))
                    lstEnglishDetails.Add(txtEnglishDetail2.Text);
                if (!string.IsNullOrEmpty(txtEnglishDetail3.Text))
                    lstEnglishDetails.Add(txtEnglishDetail3.Text);
                if (!string.IsNullOrEmpty(txtEnglishDetail4.Text))
                    lstEnglishDetails.Add(txtEnglishDetail4.Text);
                if (!string.IsNullOrEmpty(txtEnglishDetail5.Text))
                    lstEnglishDetails.Add(txtEnglishDetail5.Text);
                if (!string.IsNullOrEmpty(txtEnglishDetail6.Text))
                    lstEnglishDetails.Add(txtEnglishDetail6.Text);
                if (!string.IsNullOrEmpty(txtEnglishDetail7.Text))
                    lstEnglishDetails.Add(txtEnglishDetail7.Text);
                if (!string.IsNullOrEmpty(txtEnglishDetail8.Text))
                    lstEnglishDetails.Add(txtEnglishDetail8.Text);
                if (!string.IsNullOrEmpty(txtEnglishDetail9.Text))
                    lstEnglishDetails.Add(txtEnglishDetail9.Text);
                if (!string.IsNullOrEmpty(txtEnglishDetail10.Text))
                    lstEnglishDetails.Add(txtEnglishDetail10.Text);

                ViewModel.Item.EnglishDetails = lstEnglishDetails.Take(ViewModel.DetailsCount).ToArray();
            }
            #endregion
        }
        
        void SetSections()
        {
            if (pckTokGroup.SelectedIndex == 2)
            {
                //Will grab in order and skip over empty
                List<TokSection> lstSections = new List<TokSection>();
                for (int i = 0; i < lstSectionTitlesEntry.Count; ++i)
                {
                    if (!string.IsNullOrEmpty(lstSectionTitlesEntry[i].Text) && !string.IsNullOrEmpty(lstSectionContentEditor[i].Text) && lstSectionsViews[i].IsVisible)
                    {
                        lstSections.Add(new TokSection()
                        {
                            Title = lstSectionTitlesEntry[i].Text,
                            Content = lstSectionContentEditor[i].Text,
                            SectionNumber = lstSections.Count + 1,
                            SectionLength = lstSectionContentEditor[i].Text.Length
                        });
                    }

                    ViewModel.Item.Sections = lstSections.ToArray();
                    ViewModel.Item.SectionTitles = lstSections.Take(lstSections.Count > 5 ? 5 : lstSections.Count).Select(x => x.Title).ToArray();
                }
            }
        }
        #endregion

        #region Character Counters
        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry) 
            {
                var entry = (sender as Entry);
                var counter = GetCounter(entry.ClassId);

                if (counter != null)
                    counter.Text = $"{entry.Text.Length} / {entry.MaxLength}";
            }
            else if (sender is Editor)
            {
                var editor = (sender as Editor);
                if (editor.ClassId.Contains("txtSectionContent"))
                {
                    int num = Convert.ToInt32(editor.ClassId.Replace("txtSectionContent", ""));
                    var counter = lstSectionContentCounter.ElementAt(num - 1);
                    counter.Text = $"{editor.Text.Length} / {editor.MaxLength}";
                }
                else
                {
                    lblNotesCounter.Text = $"{editor.Text.Length} / {editor.MaxLength}";
                }
            }
        }

        private Label GetCounter(string classId)
        {
            if (classId.Contains("txtSectionTitle"))
            {
                int num = Convert.ToInt32(classId.Replace("txtSectionTitle", ""));
                return lstSectionTitlesCounter.ElementAt(num - 1);
            }

            if (classId == "txtTokType")
                return lblTokTypeCounter;
            else if (classId == "txtCategory")
                return lblCategoryCounter;
            else if (classId == "txtPrimary")
                return lblPrimaryCounter;
            else if (classId == "txtSecondary")
                return lblSecondaryCounter;
            else if (classId == "txtDetail1")
                return lblDetail1Counter;
            else if (classId == "txtDetail2")
                return lblDetail2Counter;
            else if (classId == "txtDetail3")
                return lblDetail3Counter;
            else if (classId == "txtDetail4")
                return lblDetail4Counter;
            else if (classId == "txtDetail5")
                return lblDetail5Counter;
            else if (classId == "txtDetail6")
                return lblDetail6Counter;
            else if (classId == "txtDetail7")
                return lblDetail7Counter;
            else if (classId == "txtDetail8")
                return lblDetail8Counter;
            else if (classId == "txtDetail9")
                return lblDetail9Counter;
            else if (classId == "txtDetail10")
                return lblDetail10Counter;
            else if (classId == "txtEnglishPrimary")
                return lblEnglishPrimaryCounter;
            else if (classId == "txtEnglishSecondary")
                return lblEnglishSecondaryCounter;
            else if (classId == "txtEnglishDetail1")
                return lblEnglishDetail1Counter;
            else if (classId == "txtEnglishDetail2")
                return lblEnglishDetail2Counter;
            else if (classId == "txtEnglishDetail3")
                return lblEnglishDetail3Counter;
            else if (classId == "txtEnglishDetail4")
                return lblEnglishDetail4Counter;
            else if (classId == "txtEnglishDetail5")
                return lblEnglishDetail5Counter;
            else if (classId == "txtEnglishDetail6")
                return lblEnglishDetail6Counter;
            else if (classId == "txtEnglishDetail7")
                return lblEnglishDetail7Counter;
            else if (classId == "txtEnglishDetail8")
                return lblEnglishDetail8Counter;
            else if (classId == "txtEnglishDetail9")
                return lblEnglishDetail9Counter;
            else if (classId == "txtEnglishDetail10")
                return lblEnglishDetail10Counter;
            return null;
        }
        #endregion
    
        private async void ScrollView_OnScrolled(object sender, ScrolledEventArgs e)
        {
            if (!(sender is ScrollView scrollView))
                return;

            var scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;

            if (scrollingSpace > e.ScrollY)
                return;

            // load more content.
            //DisplayAlert("Alert", "End of scroll view detected", "OK");
            ViewModel.IsLoaderVisible = true;
            await ViewModel.Sections.OnThresholdReached();
            ViewModel.IsLoaderVisible = false;
        }
    }
}

//private void LblSectionEntry_TextChanged(object sender, TextChangedEventArgs e)
//{
//    var editor = (sender as Editor);
//    var id = editor.ClassId;
//    var sectionNum = Convert.ToInt32(id.Replace("Section", "").Replace("Editor", ""));

//    InitializeSections(sectionNum);
//    ViewModel.Item.Sections[sectionNum - 1].Title = editor.Text;
//}

//private void LblSectionContent_TextChanged(object sender, TextChangedEventArgs e)
//{
//    var editor = (sender as Editor);
//    var id = editor.ClassId;
//    var sectionNum = Convert.ToInt32(id.Replace("Section", "").Replace("Editor", ""));

//    InitializeSections(sectionNum);
//    ViewModel.Item.Sections[sectionNum - 1].Content = editor.Text;
//}

//void InitializeSections(int sectionNum)
//{
//    if (ViewModel.Item.Sections == null)
//    {
//        ViewModel.Item.Sections = new TokSection[sectionNum];
//        for (int i = 0; i < sectionNum; ++i)
//        {
//            ViewModel.Item.Sections[i] = new TokSection()
//            {
//                Title = "",
//                Content = "",
//                SectionNumber = i + 1
//            };
//        }
//    }
//    else if (ViewModel.Item.Sections.Length < sectionNum)
//    {
//        var newList = ViewModel.Item.Sections.ToList();
//        for (int i = 0; i < sectionNum - ViewModel.Item.Sections.Length; ++i)
//        {
//            newList.Add(new TokSection()
//            {
//                Title = "",
//                Content = "",
//                SectionNumber = i + 1
//            });
//        }
//        ViewModel.Item.Sections = newList.ToArray();
//    }
//}

//private void pckDetails_SelectedIndexChanged(object sender, EventArgs e)
//{
//    //ViewModel.DetailsCount = (int)(sender as Picker).SelectedItem;
//    //ViewModel.Item.DetailsXF = ViewModel.Item.DetailsXF.Take(ViewModel.DetailsCount).ToList();

//    ////var arr = ViewModel.Item?.Details ?? new string[ViewModel.DetailsCount];
//    ////if (arr != null)
//    ////    Array.Resize(ref arr, ViewModel.DetailsCount);
//    ////for (int i = 0; i < arr.Length; ++i) arr[i] = $"Detail {i + 1}";
//    ////ViewModel.Item.DetailsXF = lst;

//    //stkDetailsStack.Children.Clear();
//    //BindableLayout.SetItemsSource(stkDetailsStack, ViewModel.Item.DetailsXF);

//    //pckDetails.SelectedIndex = ViewModel.DetailsCount - 3;
//}

//if ((ViewModel.Item.DetailsXF?.Count ?? 0) < 3)
//{
//    ViewModel.Item.DetailsXF = new List<TokDetailXF>();
//    for (int i = 0; i < 3; ++i) ViewModel.Item.DetailsXF[i] = new TokDetailXF() { DetailLabel = $"Detail {i + 1}" };
//}

////Add details
//if (ViewModel.Item.DetailsXF.Count < ViewModel.DetailsCount)
//{
//    for (int i = ViewModel.Item.DetailsXF.Count; i <= ViewModel.DetailsCount; ++i)
//    {
//        ViewModel.Item.DetailsXF.Add(new TokDetailXF() { DetailLabel = $"Detail {i + 1}" });
//    }
//}
//else
//    ViewModel.Item.DetailsXF = ViewModel.Item.DetailsXF.Take(ViewModel.DetailsCount).ToList();

//< Label FontSize = "20" HorizontalTextAlignment = "Start" Text = "{Binding DetailLabel}" />
//                             < Entry Text = "{Binding Detail}" Placeholder = "{Binding DetailLabel}" HorizontalOptions = "Fill" HeightRequest = "50" WidthRequest = "400" />
//                                      < Label FontSize = "10" HorizontalTextAlignment = "End" Text = "0 / 200" TextColor = "LightGray" />

//stkDetailsStack.Children.Clear();

//for (int i = 0; i <= ViewModel.Item.DetailsXF.Count; ++i)
//{
//    stkDetailsStack.Children.Add(new Label()
//    {
//        FontSize = 20,
//        HorizontalTextAlignment = TextAlignment.Start,
//        Text = ViewModel.Item.DetailsXF[i].DetailLabel
//    });

//    stkDetailsStack.Children.Add(new Entry()
//    {
//        Text = ViewModel.Item.DetailsXF[i].Detail,
//        Placeholder = ViewModel.Item.DetailsXF[i].DetailLabel,
//        HorizontalOptions = LayoutOptions.Fill,
//        HeightRequest = 50,
//        WidthRequest = 400
//    });

//    stkDetailsStack.Children.Add(new Label()
//    {
//        FontSize = 10,
//        HorizontalTextAlignment = TextAlignment.End,
//        Text = "0 / 200",
//        TextColor = Color.LightGray
//    });
//}

//stkDetailsStack.Children.Clear();
//BindableLayout.SetItemsSource(stkDetailsStack, ViewModel.Item.DetailsXF.Select(x=>x.DetailLabel));

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