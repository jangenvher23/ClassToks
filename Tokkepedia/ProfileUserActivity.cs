using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.Core.Widget;
using AndroidX.Preference;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using DE.Hdodenhof.CircleImageViewLib;
using Google.Android.Material.AppBar;
using ImageViews.Photo;
using Newtonsoft.Json;
using Supercharge;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tokkepedia.Adapters;
using Tokkepedia.Fragments;
using Tokkepedia.Helpers;
using Tokkepedia.Shared.Extensions;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokkepedia.ViewModels;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using Xamarin.Essentials;
using Color = Android.Graphics.Color;
using ServiceAccount = Tokkepedia.Shared.Services;

namespace Tokkepedia
{
    [Activity(Label = " ", Theme = "@style/AppTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class ProfileUserActivity : BaseActivity, View.IOnTouchListener
    {
        // Allows us to know if we should use MotionEvent.ACTION_MOVE
        private bool tracking = false, isCointapped = true;
        // The Position where our touch event started
        private float startY;
        float imgScale;
        internal static ProfileUserActivity Instance { get; private set; }
        List<TokModel> tokResult, TokDataList;
        List<Tokmoji> ListTokmojiModel; public List<ClassTokModel> ClassTokList;
        TokDataAdapter tokDataAdapter; TokCardDataAdapter tokcardDataAdapter; string UserId;
        TokModel tokModel; ClassTokModel classTokModel;
        ClassTokDataAdapter classtokDataAdapter; ClassTokCardDataAdapter classtokcardDataAdapter; 
        GridLayoutManager mLayoutManager;
        public ProfilePageViewModel ProfileVm => App.Locator.ProfilePageVM;
        ISharedPreferences prefs;
        ISharedPreferencesEditor editor;
        CancellationTokenSource source;
        private int EDIT_PROFILE_REQUEST_CODE = 1004;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.profile_page);

            ProfileToolBar.Visibility = ViewStates.Visible;

            SetSupportActionBar(ProfileToolBar);

            if (SupportActionBar != null)
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            editor = prefs.Edit();

            UserId = Intent.GetStringExtra("userid");
            ProfileVm.UserId = UserId;
            Instance = this;
            Settings.ActivityInt = (int)ActivityType.ProfileActivity;

            ProfileVm.activity = this;
            ProfileVm.ProfileCoverPhoto = ProfileCoverPhoto;
            ProfileVm.ProfileUserPhoto = ProfileUserPhoto;
            this.RunOnUiThread(async () => await ProfileVm.Initialize());

            ProfileCoverPhoto.LayoutChange += delegate
            {
                //set color status bar
                Window.SetStatusBarColor(ManipulateColor.manipulateColor(ProfileVm.GListenerCover.mColorPalette, 0.32f));

                if (Window.StatusBarColor == 0)
                {
                    Window.SetStatusBarColor(Color.Black);
                }
                
            };

            if (UserId != Settings.GetUserModel().UserId)
            {
                btnEditProfile.Visibility = ViewStates.Gone;
            }
            UrlEditIcon.Visibility = ViewStates.Gone;

            btnEditProfile.Click += delegate
            {
                Intent nextActivity = new Intent(this, typeof(EditProfileActivity));
                nextActivity.PutExtra("bio", ProfileVm.tokketUser.Bio);
                nextActivity.PutExtra("web", ProfileVm.tokketUser.Website);
                nextActivity.PutExtra("displayname", ProfileVm.tokketUser.DisplayName);
                this.StartActivityForResult(nextActivity, EDIT_PROFILE_REQUEST_CODE);
            };

            ProfileUsername.Text = ProfileVm.UserDisplayName;

            if (!string.IsNullOrEmpty(ProfileVm.tokketUser.TitleId))
            {
                UserTitle.Visibility = ViewStates.Visible;
                UserTitle.Text = ProfileVm.tokketUser.TitleId;
            }

            UserDescription.Text = ProfileVm.tokketUser.Bio;
            LinkProfileUrl.Text = ProfileVm.tokketUser.Website;

            if (UserId != Settings.GetUserModel().UserId)
            {
                if (string.IsNullOrEmpty(ProfileVm.tokketUser.Bio))
                {
                    UserDescription.Visibility = ViewStates.Gone;
                }
            }

            if (UserId != Settings.GetUserModel().UserId)
            {
                if (string.IsNullOrEmpty(ProfileVm.tokketUser.Website))
                {
                    LinkProfileUrl.Visibility = ViewStates.Gone;
                    UrlEditIcon.Visibility = ViewStates.Gone;
                }
            }

            if (UserId == Settings.GetUserModel().UserId)
            {
                loadNameBioWebsite(ProfileVm.UserDisplayName, ProfileVm.tokketUser.Bio, ProfileVm.tokketUser.Website);

                CoverPhotoIcon.Visibility = ViewStates.Visible;
                UserPhotoIcon.Visibility = ViewStates.Visible;
                AvatarsButton.Visibility = ViewStates.Visible;

                if (!string.IsNullOrEmpty(Settings.GetTokketUser().AccountType))
                {
                    if (Settings.GetTokketUser().AccountType == "group")
                    {
                        BtnSubAccnts.Visibility = ViewStates.Visible;
                        BtnSubAccnts.Text = Settings.GetTokketUser().GroupAccountType.Substring(0, 1).ToUpper() + Settings.GetTokketUser().GroupAccountType.Substring(1) + " Subaccounts";
                    }
                }
            }
            else
            {
                BtnSubAccnts.Visibility = ViewStates.Gone;
                CoverPhotoIcon.Visibility = ViewStates.Gone;
                UserPhotoIcon.Visibility = ViewStates.Gone;
                AvatarsButton.Visibility = ViewStates.Gone;

                BetterLinkMovementMethod
                .Linkify(MatchOptions.WebUrls, new List<TextView> { LinkProfileUrl });

                UrlEditIcon.Visibility = ViewStates.Gone;
            }

            long longcoins = Settings.UserCoins;
            //string stringcoins = ProfileVm.Coins.ToString();
            //if (string.IsNullOrEmpty(stringcoins))
            //{
            //    longcoins = 0;
            //}
            //else
            //{
            //    longcoins = long.Parse(stringcoins);
            //}

            TextProfileCoins.Text = longcoins.ToKMB();

            ShowCurrentRank();

            //UserDescription.SetOnTouchListener(this);
            //UrlEditIcon.SetOnTouchListener(this);
            //ProfileUsername.SetOnTouchListener(this);
            GifCoinIcon.SetOnTouchListener(this);
            TextProfileCoins.SetOnTouchListener(this);
            ImgLevel.SetOnTouchListener(this);
            ParentImageViewer.SetOnTouchListener(this);

            int numcol = 1;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                numcol = (int)NumDisplay.tablet + 1;
            }
            tokModel = new TokModel();
            classTokModel = new ClassTokModel();
            mLayoutManager = new GridLayoutManager(Application.Context, numcol);
            RecyclerToksList.SetLayoutManager(mLayoutManager);

            this.RunOnUiThread(async () => await LoadToks());

            var scrollRange = -1;

            AppBarProfile.OffsetChanged += (sender, args) =>
            {
                //if (scrollRange == -1)
                scrollRange = AppBarProfile.TotalScrollRange;

                if (scrollRange + args.VerticalOffset == 0)
                {
                    //set color action bar
                    //SupportActionBar.SetBackgroundDrawable(new ColorDrawable(ManipulateColor.manipulateColor(ProfileVm.GListenerCover.mColorPalette, 0.62f)));

                    if (!isCointapped || NestedScroll.ScrollY == 0)
                    {
                        if (ProfileUserPhoto.Visibility == ViewStates.Visible)
                        {
                            Animation myAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.fab_scaledown);
                            ProfileUserPhoto.StartAnimation(myAnim);
                            ProfileUserPhoto.Visibility = ViewStates.Invisible;
                        }
                    }

                    UserPhotoIcon.Visibility = ViewStates.Gone;
                    CollapsingToolbar.Title = ProfileVm.UserDisplayName;
                    SwipeRefreshProfile.Enabled = false;
                }

                if (args.VerticalOffset == 0) //Expanded
                {
                    SwipeRefreshProfile.Enabled = true;

                    CollapsingToolbar.Title = " ";

                    ProfileUserPhoto.Visibility = ViewStates.Visible;

                    if (UserId == Settings.GetUserModel().UserId)
                    {
                        UserPhotoIcon.Visibility = ViewStates.Visible;
                    }

                    if (!isCointapped)
                    {
                        Animation myAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.view_scaleup);
                        ProfileUserPhoto.StartAnimation(myAnim);
                    }
                }
            };

            //LoadMore
            if (RecyclerToksList != null)
            {
                RecyclerToksList.HasFixedSize = true;
                RecyclerToksList.NestedScrollingEnabled = false;

                NestedScroll.ScrollChange += async (object sender, NestedScrollView.ScrollChangeEventArgs e) =>
                {
                    View view = (View)NestedScroll.GetChildAt(NestedScroll.ChildCount - 1);

                    int diff = (view.Bottom - (NestedScroll.Height + NestedScroll.ScrollY));

                    if (diff == 0)
                    {
                        if (!string.IsNullOrEmpty(Settings.ContinuationToken))
                        {
                            await LoadMoreToks();
                        }
                    }

                    if (NestedScroll.ScrollY == 0)
                    {
                        //SwipeRefreshProfile.Enabled = true;
                        isCointapped = false;
                    }
                    else
                    {
                        SwipeRefreshProfile.Enabled = false;
                        isCointapped = true;
                    }
                };
            }

            Glide.With(this)
                .Load(Resource.Drawable.gold)
                .Into(GifCoinIcon);

            /*Stream input = Resources.OpenRawResource(Resource.Drawable.gold);
            byte[] bytes = ConvertByteArray(input);
            GifCoinIcon.SetBytes(bytes);
            GifCoinIcon.StartAnimation();*/

            Stream sr = null;
            AssetManager assetManager = Application.Context.Assets;
            if (!string.IsNullOrEmpty(ProfileVm.tokketUser.Country))
            {
                try
                {
                    sr = assetManager.Open("Flags/" + ProfileVm.tokketUser.Country + ".jpg");
                }
                catch (Exception ex)
                {
                   
                }
            }


            if (ProfileVm.tokketUser.Country == "us")
            {
                TxtProfileWordCountry.Text = "State";
                TxtProfileCountryOrState.Visibility = ViewStates.Visible;
                //If Country ="us" but null/empty State, show the "us" Country flag
                if (String.IsNullOrEmpty(ProfileVm.tokketUser.State))
                {
                    sr = assetManager.Open("Flags/us.jpg");
                    Bitmap bitmapFlag = BitmapFactory.DecodeStream(sr);
                    ImgFlag.SetImageBitmap(bitmapFlag);
                    ImgFlag.Visibility = ViewStates.Visible;
                    TxtProfileCountryOrState.Text = "None";
                }
                else
                {
                    var statelink = CountryTool.GetCountryFlagJPG1x1(ProfileVm.tokketUser.State);
                    Glide.With(this).Load(statelink).Into(ImgFlag);
                    TxtProfileCountryOrState.Text = ProfileVm.tokketUser.State;
                }
            }
            else
            {
                //If null/empty Country, show an empty space the size of the flag
                TxtProfileCountryOrState.Visibility = ViewStates.Gone;
                Bitmap bitmapFlag = BitmapFactory.DecodeStream(sr);
                ImgFlag.SetImageBitmap(bitmapFlag);
                ImgFlag.Visibility = ViewStates.Visible;
            }


            //if (ProfileVm.tokketUser.Country == "us")
            //{
            //    TxtProfileWordCountry.Text = "State";
            //    TxtProfileCountryOrState.Text = "None";
            //    TxtProfileCountryOrState.Visibility = ViewStates.Visible;

            //    ImgFlag.Visibility = ViewStates.Gone;
            //}
            //else
            //{
            //    Bitmap bitmapFlag = BitmapFactory.DecodeStream(sr);
            //    ImgFlag.SetImageBitmap(bitmapFlag);
            //    ImgFlag.Visibility = ViewStates.Visible;
            //    TxtProfileCountryOrState.Visibility = ViewStates.Gone;
            //}

            //AvatarsButton.Click += delegate
            //{
            //    Intent nextActivity = new Intent(this, typeof(AvatarsActivity));
            //    this.StartActivityForResult(nextActivity, 30011);
            //};

            //BtnBadges.Click += delegate
            //{
            //    Intent nextActivity = new Intent(this, typeof(BadgesActivity));
            //    this.StartActivityForResult(nextActivity, 40011);
            //};

            BtnSubAccnts.Click += delegate
            {
                Intent nextActivity = new Intent(this, typeof(SubAccountActivity));
                this.StartActivity(nextActivity);
            };

#if (_TOKKEPEDIA)
            SwipeRefreshProfile.SetColorSchemeColors(Color.DarkViolet, Color.Violet, Color.Lavender, Color.LavenderBlush);
#endif

#if (_CLASSTOKS)
            SwipeRefreshProfile.SetColorSchemeColors(Color.ParseColor("#3498db"), Color.ParseColor("#4ea4de"), Color.ParseColor("#68aede"), Color.ParseColor("#88bee3"));
#endif
            SwipeRefreshProfile.Refresh += RefreshLayout_Refresh;
        }
        private void loadNameBioWebsite(string displayname, string userbio, string userwebsite)
        {
            /*string DisplayIcon = "";
            SpannableString spannableString;
            Typeface typeface;
            //Display Name
            DisplayIcon = displayname + " edit";
            spannableString = new SpannableString(DisplayIcon);
            typeface = Typeface.CreateFromAsset(Application.Assets, "fa_solid_900.otf");
            spannableString.SetSpan(new RelativeSizeSpan(1.0f), DisplayIcon.Length - 4, DisplayIcon.Length, SpanTypes.ExclusiveExclusive);
            spannableString.SetSpan(new TypefaceSpan(typeface), DisplayIcon.Length - 4, DisplayIcon.Length, SpanTypes.ExclusiveExclusive);
            ProfileUsername.SetText(spannableString, TextView.BufferType.Spannable);*/
            ProfileUsername.Text = displayname;

            //Bio
            /*DisplayIcon = userbio + " edit";
            spannableString = new SpannableString(DisplayIcon);
            typeface = Typeface.CreateFromAsset(Application.Assets, "fa_solid_900.otf");
            spannableString.SetSpan(new RelativeSizeSpan(1.2f), DisplayIcon.Length - 4, DisplayIcon.Length, SpanTypes.ExclusiveExclusive);
            spannableString.SetSpan(new TypefaceSpan(typeface), DisplayIcon.Length - 4, DisplayIcon.Length, SpanTypes.ExclusiveExclusive);
            UserDescription.SetText(spannableString, TextView.BufferType.Spannable);*/
            UserDescription.Text = userbio;

            if (UserId != Settings.GetUserModel().UserId)
            {
                if (string.IsNullOrEmpty(UserDescription.Text))
                {
                    UserDescription.Visibility = ViewStates.Gone;
                }
            }

            /*var spannableEdit = new SpannableString("edit");
            typeface = Typeface.CreateFromAsset(Application.Assets, "fa_solid_900.otf");
            spannableEdit.SetSpan(new RelativeSizeSpan(1.2f), 0, 4, SpanTypes.ExclusiveExclusive);
            spannableEdit.SetSpan(new TypefaceSpan(typeface), 0, 4, SpanTypes.ExclusiveExclusive);
            UrlEditIcon.SetText(spannableEdit, TextView.BufferType.Spannable);*/
            UrlEditIcon.Visibility = ViewStates.Gone;

            LinkProfileUrl.Text = userwebsite;
            BetterLinkMovementMethod
                .Linkify(MatchOptions.WebUrls, new List<TextView> { LinkProfileUrl });
            //.SetOnLinkClickListener(LinkProfileUrl.Click);


            if (UserId != Settings.GetUserModel().UserId)
            {
                if (string.IsNullOrEmpty(LinkProfileUrl.Text))
                {
                    LinkProfileUrl.Visibility = ViewStates.Gone;
                    UrlEditIcon.Visibility = ViewStates.Gone;
                }
            }
        }
        
        public async Task LoadToks()
        {
            TokDataList = new List<TokModel>();
            RecyclerToksList.SetAdapter(null);
            ShimmerToksList.StartShimmerAnimation();
            ShimmerToksList.Visibility = Android.Views.ViewStates.Visible;

            var tokmojiResult = await TokMojiService.Instance.GetTokmojisAsync(null);
            ListTokmojiModel = tokmojiResult.Results.ToList();

#if (_TOKKEPEDIA)
            var result = await GetToksData();
            TokDataList = result;

            if (Settings.FilterToksProfile == (int) FilterToks.Toks)
            {
                SetRecyclerAdapter(result);
            }
            else if (Settings.FilterToksProfile == (int)FilterToks.Cards)
            {
                SetCardsRecyclerAdapter(result);
            }
#endif

#if (_CLASSTOKS)
            var result = await GetClassToksData();
            ClassTokList = new List<ClassTokModel>();
            foreach (var item in result)
            {
                if (item.IsMegaTok == true || item.TokGroup.ToLower() == "mega")
                {
                    if (Settings.FilterToksProfile == (int)FilterToks.Cards)
                    {
                        var getTokSectionsResult = await TokService.Instance.GetTokSectionsAsync(item.Id);
                        var getTokSections = getTokSectionsResult.Results;
                        item.Sections = getTokSections.ToArray();
                    }
                }
                ClassTokList.Add(item);
            }

            if (Settings.FilterToksProfile == (int)FilterToks.Toks)
            {
                SetClassTokRecyclerAdapter();
            }
            else if (Settings.FilterToksProfile == (int)FilterToks.Cards)
            {
                SetClassCardsRecyclerAdapter();
            }
#endif

            ShimmerToksList.Visibility = Android.Views.ViewStates.Gone;
        }
        public async Task<List<ClassTokModel>> GetClassToksData(string filter = "")
        {
            bool isPublicFeed = false;
            if (Settings.FilterTag == (int)FilterType.All)
            {
                isPublicFeed = true;
            }

            List<ClassTokModel> classTokModelsResult = new List<ClassTokModel>();
            ResultData<ClassTokModel> tokResult = new ResultData<ClassTokModel>();
            tokResult.Results = new List<ClassTokModel>();

            source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            source.CancelAfter(TimeSpan.FromSeconds(30));
            tokResult = await ClassService.Instance.GetClassToksAsync(new ClassTokQueryValues()
            {
                partitionkeybase = $"{Settings.GetUserModel().UserId}-classtoks",
                groupid = Settings.GetUserModel().UserId,
                userid = Settings.GetUserModel().UserId,
                text = filter,
                startswith = false,
                publicfeed = isPublicFeed,
                FilterBy = FilterBy.None,
                FilterItems = new List<string>()
            }, source.Token);

            token.Register(() =>
            {
                Console.WriteLine("Task is canceled");
            });

            classTokModelsResult = tokResult.Results.ToList();
            RecyclerToksList.ContentDescription = tokResult.ContinuationToken;
            return classTokModelsResult;
        }

        public async Task LoadMoreToks()
        {
            TokQueryValues tokQueryModel = new TokQueryValues() { userid = UserId, streamtoken = null };
            tokQueryModel.token = Settings.ContinuationToken;
            tokQueryModel.loadmore = "yes";
            Settings.ContinuationToken = null;
            var result = await TokService.Instance.GetAllToks(tokQueryModel);
            if (result != null)
            {
                foreach (var item in result)
                {
                    if (item.IsMegaTok == true || item.TokGroup.ToLower() == "mega")
                    {
                        if (Settings.FilterToksProfile == (int)FilterToks.Cards)
                        {
                            var getTokSectionsResult = await TokService.Instance.GetTokSectionsAsync(item.Id);
                            var getTokSections = getTokSectionsResult.Results;
                            item.Sections = getTokSections.ToArray();
                        }
                    }
                }
                tokDataAdapter.UpdateItems(result);
                TokDataList.AddRange(result);
            }
        }
        private void SetRecyclerAdapter(List<TokModel> tokModelRes)
        {
            tokDataAdapter = new TokDataAdapter(tokModelRes, ListTokmojiModel);
            tokDataAdapter.ItemClick += OnGridBackgroundClick;
            RecyclerToksList.SetAdapter(tokDataAdapter);
        }
        private void SetCardsRecyclerAdapter(List<TokModel> tokModelRes)
        {
            tokcardDataAdapter = new TokCardDataAdapter(tokModelRes, ListTokmojiModel);
            tokcardDataAdapter.ItemClick += OnGridBackgroundClick;
            RecyclerToksList.SetAdapter(tokcardDataAdapter);
        }
        private void SetClassTokRecyclerAdapter()
        {
            classtokDataAdapter = new ClassTokDataAdapter(ClassTokList, ListTokmojiModel);
            classtokDataAdapter.ItemClick += OnGridBackgroundClick;
            RecyclerToksList.SetAdapter(classtokDataAdapter);
        }
        private void SetClassCardsRecyclerAdapter()
        {
            classtokcardDataAdapter = new ClassTokCardDataAdapter(ClassTokList, ListTokmojiModel);
            classtokcardDataAdapter.ItemClick += OnGridBackgroundClick;
            RecyclerToksList.SetAdapter(classtokcardDataAdapter);
        }

        public void ShowCurrentRank()
        {
            string oldcolor = ProfileVm.tokketUser.PointsSymbolColor;
            //ProfileVm.tokketUser = Settings.GetTokketUser();

            foreach (var item in PointsSymbolsHelper.PointsSymbols)
            {
                if (!string.IsNullOrEmpty(oldcolor))
                {
                    item.Image = item.Image.Replace(oldcolor, Settings.GetTokketUser().PointsSymbolColor);
                }
            }

            PointsSymbolModel pointResult = PointsSymbolsHelper.GetPatchExactResult(ProfileVm.userpoints);
            TextLevelRank.Text = pointResult.Level;
            Glide.With(this).Load(pointResult.Image).Into(ImgLevel);
            TextLevelPoints.Text = ProfileVm.userpoints.ToString() + " points";
        }
        public async Task<List<Shared.Models.TokModel>> GetToksData()
        {
            FilterType type = (FilterType)Settings.FilterTag;
            tokResult = new List<Shared.Models.TokModel>();
            string strtoken = ProfileVm.tokketUser.StreamToken;
            tokResult = await TokService.Instance.GetAllToks(new TokQueryValues() { userid = UserId, streamtoken = strtoken });

            var toksWithSticker = new List<TokModel>();
            tokResult = tokResult.OrderByDescending(x => x.DateCreated.Value).ToList() ?? new List<TokModel>();
            var cnt = 0;
            foreach (var tok in tokResult)
            {
                var sticker = StickersTool.Stickers.FirstOrDefault(x => x.Id == (string.IsNullOrEmpty(tok.Sticker) ? tok.Sticker : tok.Sticker.Split("-")[0]));
                tok.StickerImage = sticker?.Image ?? string.Empty;
                tok.IndexCounter = cnt;
                toksWithSticker.Add(tok);
                cnt += 1;
            }
            toksWithSticker = toksWithSticker.ToList();
            return tokResult;
        }
        public void OnGridBackgroundClick(object sender, int position)
        {
            int photoNum = position + 1;
#if (_TOKKEPEDIA)
            Intent nextActivity = new Intent(this, typeof(TokInfoActivity));
            tokModel = TokDataList[position];
            var modelConvert = JsonConvert.SerializeObject(tokModel);
            nextActivity.PutExtra("tokModel", modelConvert);
            this.StartActivityForResult(nextActivity, 20001);
#endif

#if (_CLASSTOKS)
            Intent nextActivity = new Intent(this, typeof(TokInfoActivity));
            classTokModel = ClassTokList[position];
            var modelConvert = JsonConvert.SerializeObject(classTokModel);
            nextActivity.PutExtra("classtokModel", modelConvert);
            this.StartActivityForResult(nextActivity, (int)ActivityType.HomePage);
#endif

        }
        public bool OnTouch(View v, MotionEvent e)
        {
            int ndx = 0;
            if (v.ContentDescription != "patches")
            {
                try
                {
                    try { ndx = (int)v.Tag; } catch { ndx = int.Parse((string)v.Tag); }
                }
                catch { }
            }


            if (e.Action == MotionEventActions.Up)
            {
                if (v.ContentDescription == "coins")
                {
                    LinearCoinsToast.Visibility = ViewStates.Gone;
                }
                else if (v.ContentDescription == "patches")
                {
                    Intent nextActivity = new Intent(this, typeof(PatchesActivity));
                    this.StartActivity(nextActivity);
                }
                else
                {
                    if (v.Tag != null)
                    {
                        if (UserId == Settings.GetUserModel().UserId)
                        {
                            //When Image of User Photo is clicked.
                            int position = Convert.ToInt32(v.ContentDescription);
                            Intent nextActivity = new Intent(this, typeof(ProfileInputBox));

                            if (ndx == 1002)
                            {
                                nextActivity.PutExtra("inputbox", ProfileVm.tokketUser.Website); //used a substring to remove the "edit" word at the end of every text
                                nextActivity.PutExtra("inputtype", ndx);
                            }
                            else
                            {
                                nextActivity.PutExtra("inputbox", (v as TextView).Text.Substring(0, (v as TextView).Text.Length - 4)); //used a substring to remove the "edit" word at the end of every text
                                nextActivity.PutExtra("inputtype", ndx);
                            }

                            this.StartActivityForResult(nextActivity, ndx);
                        }
                    }
                }
            }
            else if (e.Action == MotionEventActions.Down)
            {
                if (v.ContentDescription == "coins")
                {
                    isCointapped = true;
                    LinearCoinsToast.Visibility = ViewStates.Visible;
                    TextCoinsToast.Text = string.Format("{0:#,##0.##}", Settings.UserCoins);
                }
                else if (v.ContentDescription == "ImgProfileImageView")
                {
                    Rect hitRect = new Rect();
                    ParentImageViewer.GetHitRect(hitRect);

                    if (hitRect.Contains((int)e.GetX(), (int)e.GetY()))
                    {
                        tracking = true;
                    }
                    startY = e.GetY();
                }
            }
            else if (e.Action == MotionEventActions.Cancel)
            {
                if (v.ContentDescription == "ImgProfileImageView")
                {
                    tracking = false;
                    //animateSwipeView(customView.Height);
                    return true;
                }
                else if (v.ContentDescription == "coins")
                {
                    LinearCoinsToast.Visibility = ViewStates.Gone;
                }
            }
            else if (e.Action == MotionEventActions.Move)
            {
                if (v.ContentDescription == "ImgProfileImageView")
                {
                    if (e.GetY() - startY > 1)
                    {
                        if (tracking)
                        {
                            ParentImageViewer.TranslationY = e.GetY() - startY;
                        }
                        animateSwipeView(ParentImageViewer.Height);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private int mSlop;
        private float mDownX;
        private float mDownY;
        private bool mSwiping;
        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            if (ParentImageViewer.Visibility == ViewStates.Visible)
            {
                SwipeRefreshProfile.Enabled = false;
            }

            switch (ev.Action)
            {
                case MotionEventActions.Cancel:
                    tracking = false;
                    break;
                case MotionEventActions.Down:
                    mDownX = ev.GetX();
                    mDownY = ev.GetY();
                    mSwiping = false;

                    Rect hitRect = new Rect();
                    ParentImageViewer.GetHitRect(hitRect);

                    if (hitRect.Contains((int)ev.GetX(), (int)ev.GetY()))
                    {
                        tracking = true;
                    }
                    startY = ev.GetY();
                    break;
                case MotionEventActions.Move:
                    if (ImgUserImageView.Scale == imgScale)
                    {
                        float x = ev.GetX();
                        float y = ev.GetY();
                        float xDelta = Math.Abs(x - mDownX);
                        float yDelta = Math.Abs(y - mDownY);

                        if (yDelta > mSlop && yDelta / 2 > xDelta)
                        {
                            mSwiping = true;
                            //return true;
                        }


                        if (ev.GetY() - startY > 1)
                        {
                            if (tracking)
                            {
                                ParentImageViewer.TranslationY = ev.GetY() - startY;
                            }
                            animateSwipeView(ParentImageViewer.Height);
                        }
                        else if (startY - ev.GetY() > 1)
                        {
                            if (tracking)
                            {
                                ParentImageViewer.TranslationY = ev.GetY() - startY;
                            }
                            animateSwipeView(ParentImageViewer.Height);
                        }
                    }
                    break;
                case MotionEventActions.Up:

                    if (mSwiping)
                    {
                        bool isReturntonormal = false;
                        if (ParentImageViewer.Visibility == ViewStates.Visible)
                        {
                            int quarterHeight = ParentImageViewer.Height / 4;
                            float currentPosition = ParentImageViewer.TranslationY;
                            if (currentPosition < -quarterHeight)
                            {
                                isReturntonormal = true;
                            }
                            else if (currentPosition > quarterHeight)
                            {
                                isReturntonormal = true;
                            }
                        }

                        if (isReturntonormal)
                        {
                            ParentImageViewer.Visibility = ViewStates.Gone;
                            this.SupportActionBar.Show();
                            SwipeRefreshProfile.Enabled = true;
                            isCointapped = true;
                        }
                    }
                    break;
                default:
                    break;
            }

            return base.DispatchTouchEvent(ev);
        }
        private void animateSwipeView(int parentHeight)
        {
            int quarterHeight = parentHeight / 4;
            float currentPosition = ParentImageViewer.TranslationY;
            float animateTo = 0.0f;
            if (currentPosition < -quarterHeight)
            {
                animateTo = -parentHeight;
            }
            else if (currentPosition > quarterHeight)
            {
                animateTo = parentHeight;
            }
            ObjectAnimator.OfFloat(ParentImageViewer, "translationY", currentPosition, animateTo)
                    .SetDuration(200)
                    .Start();
        }
        [Java.Interop.Export("OnClickProfileImage")]
        public void OnClickProfileImage(View v)
        {
            if (UserId == Settings.GetUserModel().UserId)
            {
                Settings.BrowsedImgTag = Convert.ToInt32(v.ContentDescription);
                bottomsheet_userphoto_fragment bottomsheet = new bottomsheet_userphoto_fragment(this,ProfileUserPhoto);
                bottomsheet.Show(this.SupportFragmentManager, "tag");
            }
        }

        [Java.Interop.Export("OnClickCoverPhoto")]
        public void OnClickCoverPhoto(View v)
        {
            if (UserId == Settings.GetUserModel().UserId)
            {
                Settings.BrowsedImgTag = Convert.ToInt32(v.ContentDescription);
                bottomsheet_userphoto_fragment bottomsheet = new bottomsheet_userphoto_fragment(this, ProfileCoverPhoto);
                bottomsheet.Show(this.SupportFragmentManager, "tag");

                /*Intent = new Intent();
                Intent.SetType("image/*");
                Intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), (int)ActivityType.ProfileActivity);*/
            }
        }

        [Java.Interop.Export("OnClickProfileUserImage")]
        public void OnClickProfileUserImage(View v)
        {
            //float currentPosition = ParentImageViewer.TranslationY;
            //if (currentPosition != ViewDummyForTouch.TranslationY)
            //{
            ParentImageViewer.TranslationY = ViewDummyForTouch.TranslationY;
            //}

            if (v.ContentDescription == "0")
            {
                //ImgUserImageView.SetBackgroundColor(ManipulateColor.manipulateColor(ProfileVm.GListenerUserPhoto.mColorPalette, 0.62f));
                ImgUserImageView.SetBackgroundColor(GetDominantColor.GetDominantColorImg(((BitmapDrawable)ProfileUserPhoto.Drawable).Bitmap, 0.62f));
                ImgUserImageView.SetImageDrawable(ProfileUserPhoto.Drawable);
            }
            else if (v.ContentDescription == "1")
            {
                //ImgUserImageView.SetBackgroundColor(ManipulateColor.manipulateColor(ProfileVm.GListenerCover.mColorPalette, 0.62f));
                ImgUserImageView.SetBackgroundColor(GetDominantColor.GetDominantColorImg(((BitmapDrawable)ProfileCoverPhoto.Drawable).Bitmap, 0.62f));
                ImgUserImageView.SetImageDrawable(ProfileCoverPhoto.Drawable);
            }
            imgScale = ImgUserImageView.Scale;

            ParentImageViewer.Visibility = ViewStates.Visible;
            Animation myAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.fab_scaleup);
            ParentImageViewer.StartAnimation(myAnim);

            SwipeRefreshProfile.Enabled = false;
            RequestedOrientation = ScreenOrientation.Unspecified;
        }
        [Java.Interop.Export("OnClickCloseImgView")]
        public void OnClickCloseImgView(View v)
        {
            ParentImageViewer.Visibility = ViewStates.Gone;
            SwipeRefreshProfile.Enabled = true;
            RequestedOrientation = ScreenOrientation.Portrait;
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == (int)ActivityType.ProfileActivity) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                Intent nextActivity = new Intent(this, typeof(CropImageActivity));
                Settings.ImageBrowseCrop = (string)uri;
                this.StartActivityForResult(nextActivity, requestCode);
            }
            else if ((requestCode == 1001) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                ProfileVm.tokketUser.Bio = data.GetStringExtra("inputbox");
                UserDescription.Text = ProfileVm.tokketUser.Bio;
                loadNameBioWebsite(ProfileVm.tokketUser.DisplayName, ProfileVm.tokketUser.Bio, ProfileVm.tokketUser.Website);
            }
            else if ((requestCode == 1002) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                ProfileVm.tokketUser.Website = data.GetStringExtra("inputbox");
                LinkProfileUrl.Text = ProfileVm.tokketUser.Website;
                loadNameBioWebsite(ProfileVm.tokketUser.DisplayName, ProfileVm.tokketUser.Bio, ProfileVm.tokketUser.Website);
            }
            else if ((requestCode == 1003) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                ProfileUsername.Text = data.GetStringExtra("inputbox");
                ProfileVm.tokketUser.DisplayName = ProfileUsername.Text;
                loadNameBioWebsite(ProfileVm.tokketUser.DisplayName, ProfileVm.tokketUser.Bio, ProfileVm.tokketUser.Website);
            }
            else if ((requestCode == EDIT_PROFILE_REQUEST_CODE) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                ProfileVm.tokketUser.Bio = data.GetStringExtra("bio");
                ProfileVm.tokketUser.Website = data.GetStringExtra("web");
                ProfileVm.tokketUser.DisplayName = data.GetStringExtra("displayname");

                UserDescription.Text = ProfileVm.tokketUser.Bio;
                LinkProfileUrl.Text = ProfileVm.tokketUser.Website;
                ProfileUsername.Text = ProfileVm.tokketUser.DisplayName;
                loadNameBioWebsite(ProfileVm.tokketUser.DisplayName, ProfileVm.tokketUser.Bio, ProfileVm.tokketUser.Website);
            }
            else if ((requestCode == 20001) && (resultCode == Android.App.Result.Ok))
            {
                var isDeleted = data.GetBooleanExtra("isDeleted", false);
                if (isDeleted)
                {
                    TokDataList.Remove(tokModel);
                    SetRecyclerAdapter(TokDataList);
                }
            }
            else if ((requestCode == (int)ActivityType.AvatarsActivity) && (resultCode == Android.App.Result.Ok)) //Avatar
            {
                var avatarString = data.GetStringExtra("Avatar");
                var avatarModel = JsonConvert.DeserializeObject<Avatar>(avatarString);
                Glide.With(this).Load(avatarModel.Image).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(ProfileUserPhoto);
            }
            else if ((requestCode == 40011) && (resultCode == Android.App.Result.Ok)) //Badge
            {
                var badgeString = data.GetStringExtra("Badge");
                var badgeModel = JsonConvert.DeserializeObject<BadgeOwned>(badgeString);
                Glide.With(this).Load(badgeModel.Image).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(ProfileUserPhoto);
            }
        }
        public void displayImageBrowse()
        {
            byte[] imageMainBytes = Convert.FromBase64String(Settings.ImageBrowseCrop);
            if (Settings.BrowsedImgTag == 0) //UserPhoto
            {
                ProfileUserPhoto.SetImageBitmap((BitmapFactory.DecodeByteArray(imageMainBytes, 0, imageMainBytes.Length)));
            }
            else if (Settings.BrowsedImgTag == 1) //Cover Photo
            {
                ProfileCoverPhoto.SetImageBitmap((BitmapFactory.DecodeByteArray(imageMainBytes, 0, imageMainBytes.Length)));
            }

            this.RunOnUiThread(async () => await SaveUserCoverPhoto(Settings.ImageBrowseCrop));
            Settings.ImageBrowseCrop = null;
        }
        private async Task SaveUserCoverPhoto(string base64img)
        {
            base64img = "data:image/jpeg;base64," + base64img;
            if (Settings.BrowsedImgTag == 0) //UserPhoto
            {
                await ServiceAccount.AccountService.Instance.UploadProfilePictureAsync(base64img);
            }
            else if (Settings.BrowsedImgTag == 1) //Cover Photo
            {
                await ServiceAccount.AccountService.Instance.UploadProfileCoverAsync(base64img);
            }
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.profile_menu, menu);

            //IMenuItem Android.Resource.Id
            var itemReport = menu.FindItem(Resource.Id.item_Report);
            var itemTitle = menu.FindItem(Resource.Id.item_titles);
            var item_avatar = menu.FindItem(Resource.Id.item_avatar);

            if (UserId == Settings.GetUserModel().UserId)
            {
                itemReport.SetVisible(false);
                itemTitle.SetVisible(true);
                item_avatar.SetVisible(true);
            }
            else
            {
                itemReport.SetVisible(false);
                itemTitle.SetVisible(false);
                item_avatar.SetVisible(false);
            }

            return true;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent nextActivity;
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Settings.ActivityInt = (int)ActivityType.HomePage;
                    Finish();
                    break;
                case Resource.Id.item_filter:
                    nextActivity = new Intent(this, typeof(FilterActivity));
                    nextActivity.PutExtra("activitycaller", "Profile");
                    nextActivity.PutExtra("SubTitle", "User: " + ProfileVm.UserDisplayName);
                    //nextActivity.PutExtra("TokList",JsonConvert.SerializeObject(TokDataList));

                    editor.PutString("TokModelList", JsonConvert.SerializeObject(TokDataList));
                    editor.Apply();

                    StartActivity(nextActivity);
                    break;
                case Resource.Id.item_titles:
                    nextActivity = new Intent(this, typeof(ProfileTitleActivity));
                    StartActivity(nextActivity);
                    break;
                case Resource.Id.item_avatar:
                    nextActivity = new Intent(this, typeof(AvatarsActivity));
                    this.StartActivityForResult(nextActivity, (int)ActivityType.AvatarsActivity);
                    break;
                case Resource.Id.item_badges:
                    nextActivity = new Intent(this, typeof(BadgesActivity));
                    this.StartActivityForResult(nextActivity, 40011);
                    break;
                case Resource.Id.item_sets:
                    nextActivity = new Intent(this, typeof(MySetsActivity));
                    nextActivity.PutExtra("isAddToSet", false);
                    nextActivity.PutExtra("TokTypeId", "");
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    StartActivity(nextActivity);
                    break;
                case Resource.Id.item_Share:
                    RunOnUiThread(async () => await Share.RequestAsync(new ShareTextRequest
                    {
                        Uri = Shared.Config.Configurations.Url + "user/" + ProfileVm.UserId,
                        Title = ProfileVm.UserDisplayName
                    }));

                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        public override void OnBackPressed()
        {
            Settings.ActivityInt = (int)ActivityType.HomePage;
            base.OnBackPressed();
        }
        /*private byte[] ConvertByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);
                return ms.ToArray();
            }
        }*/
        private void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                TextNothingFound.SetTextColor(Color.White);
                TextNothingFound.SetBackgroundColor(Color.Transparent);
                TextNothingFound.Visibility = ViewStates.Gone;

                //Data Refresh Place  
                BackgroundWorker work = new BackgroundWorker();
                work.DoWork += Work_DoWork;
                work.RunWorkerCompleted += Work_RunWorkerCompleted;
                work.RunWorkerAsync();
            }
            else
            {
                TextNothingFound.Text = "No Internet Connection!";
                TextNothingFound.SetTextColor(Color.White);
                TextNothingFound.SetBackgroundColor(Color.Black);
                TextNothingFound.Visibility = ViewStates.Visible;
                SwipeRefreshProfile.Refreshing = false;
            }
        }
        private void Work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SwipeRefreshProfile.Refreshing = false;
        }
        private void Work_DoWork(object sender, DoWorkEventArgs e)
        {
            this.RunOnUiThread(async () => await LoadToks());
            Thread.Sleep(3000);
        }

        public Button btnEditProfile => FindViewById<Button>(Resource.Id.btnEditProfile);
        public ImageView GifCoinIcon => FindViewById<ImageView>(Resource.Id.gif_profileCoins);
        public CircleImageView ProfileUserPhoto => FindViewById<CircleImageView>(Resource.Id.img_profileUserPhoto);
        public ImageView ImgFlag => FindViewById<ImageView>(Resource.Id.imageProfileFlag);
        public ImageView ProfileCoverPhoto => FindViewById<ImageView>(Resource.Id.img_profileCoverPhoto);
        public TextView LinkProfileUrl => FindViewById<TextView>(Resource.Id.lblProfileUrl);
        public TextView ProfileUsername => FindViewById<TextView>(Resource.Id.lblProfileUserName);
        public TextView UserTitle => FindViewById<TextView>(Resource.Id.lblUserTitle);
        public TextView UserDescription => FindViewById<TextView>(Resource.Id.lblProfileUserDescription);
        public ImageView CoverPhotoIcon => FindViewById<ImageView>(Resource.Id.ProfileCoverCameraIcon);
        public CircleImageView UserPhotoIcon => FindViewById<CircleImageView>(Resource.Id.ProfileUserCameraIcon);
        public RecyclerView RecyclerToksList => FindViewById<RecyclerView>(Resource.Id.RecyclerProfilePageToks);
        public ShimmerLayout ShimmerToksList => FindViewById<ShimmerLayout>(Resource.Id.ShimmerProfilePageToks);
        public NestedScrollView NestedScroll => FindViewById<NestedScrollView>(Resource.Id.NestedProfilePage);
        public TextView TxtProfileWordCountry => FindViewById<TextView>(Resource.Id.TxtProfileWordCountry);
        public TextView TxtProfileCountryOrState => FindViewById<TextView>(Resource.Id.TxtProfileCountrystate);
        public Button AvatarsButton => FindViewById<Button>(Resource.Id.btnProfileAvatars);
        public TextView TextLevelRank => FindViewById<TextView>(Resource.Id.TextLevelRank);
        public TextView TextLevelPoints => FindViewById<TextView>(Resource.Id.TextLevelPoints);
        public ImageView ImgLevel => FindViewById<ImageView>(Resource.Id.ImgLevel);
        public TextView TextProfileCoins => FindViewById<TextView>(Resource.Id.TextProfileCoins);
        public PhotoView ImgUserImageView => FindViewById<PhotoView>(Resource.Id.ImgProfileImageView);
        public Button BtnSubAccnts => FindViewById<Button>(Resource.Id.btnProfileSubAccnts);
        public AppBarLayout AppBarProfile => FindViewById<AppBarLayout>(Resource.Id.AppBarProfile);
        public AndroidX.AppCompat.Widget.Toolbar ProfileToolBar => FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.profile_toolbar);
        public CollapsingToolbarLayout CollapsingToolbar => FindViewById<CollapsingToolbarLayout>(Resource.Id.CollapsingToolbar);
        public SwipeRefreshLayout SwipeRefreshProfile => FindViewById<SwipeRefreshLayout>(Resource.Id.SwipeRefreshProfile);
        public TextView UrlEditIcon => FindViewById<TextView>(Resource.Id.lblProfileUrlEditIcon);
        public LinearLayout LinearCoinsToast => FindViewById<LinearLayout>(Resource.Id.LinearCoinsToast);
        public TextView TextCoinsToast => FindViewById<TextView>(Resource.Id.TextCoinsToast); 
        public View ViewDummyForTouch => FindViewById<View>(Resource.Id.ViewDummyForTouch);
        public RelativeLayout ParentImageViewer => FindViewById<RelativeLayout>(Resource.Id.ParentImageViewer);
        public TextView TextNothingFound => FindViewById<TextView>(Resource.Id.TextNothingFound);
    }
}