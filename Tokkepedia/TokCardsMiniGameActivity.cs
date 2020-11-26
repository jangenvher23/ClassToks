using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Tokkepedia.Shared.Extensions;
using Android.Content.Res;
using System.Timers;
using Android.Views.Animations;
using System.Threading;
using Tokkepedia.Fragments;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Tokkepedia.ViewModels;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.ViewModels;
using Android.Text;
using Tokkepedia.Helpers;
using AndroidX.Preference;
using Color = Android.Graphics.Color;

namespace Tokkepedia
{
    [Activity(Label = "Tok Cards", Theme = "@style/AppTheme")]
    public class TokCardsMiniGameActivity : BaseActivity
    {
        SpannableStringBuilder ssbName;
        ISpannable spannableResText;
        List<Tokmoji> ListTokmojiModel;
        public Set setList; 
        public ClassSetModel classSet; ClassSetViewModel classSetVM;
        public GestureDetector gesturedetector; private bool Showingback, isStop;
        public List<TokModel> restokList, TokLists, favList;
        public List<ClassTokModel> ClassTokLists;
        public int cnt = 0; public ProgressBar cardProgress, progressbarTokCardLoading;
        AndroidX.Fragment.App.FragmentTransaction trans; public List<bool> isFavorite;
        public bool isPlayFavorite, isImageVisible = true; bool isSet = true, allowAddTok = true;
        System.Timers.Timer _timer; object _lock = new object(); string IntentTokList = "";
        public TextView cardProgressText; public FloatingActionButton btnNext, btnPrevious;
        public FrameLayout frameTokCardMini; Button btnTokCardsPlay;
        public HomePageViewModel HomeVm => App.Locator.HomePageVM;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.tokcards_minigame_page);
            var tokback_toolbar = this.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.tokcards_toolbar);

            SetSupportActionBar(tokback_toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            var txtTCLoadMore = FindViewById<TextView>(Resource.Id.txtTCLoadMore);
            var btnTokCardFlip = FindViewById<TextView>(Resource.Id.btnTokCardFlip);
            frameTokCardMini = FindViewById<FrameLayout>(Resource.Id.frameTokCardMini);
            var btnTokCardShuffle = FindViewById<Button>(Resource.Id.btnTokCardShuffle);
            btnTokCardsPlay = FindViewById<Button>(Resource.Id.btnTokCardsPlay);
            var btnTokCardOptions = FindViewById<Button>(Resource.Id.btnTokCardOptions);

            Typeface font = Typeface.CreateFromAsset(Application.Assets, "fa_solid_900.otf");
            btnTokCardOptions.Typeface = font;
            btnTokCardShuffle.Typeface = font;
            btnTokCardsPlay.Text = "► Play";

            cardProgressText = FindViewById<TextView>(Resource.Id.cardProgressText);
            cardProgress = FindViewById<ProgressBar>(Resource.Id.cardProgress);
            progressbarTokCardLoading = FindViewById<ProgressBar>(Resource.Id.progressbarTokCardLoading);
            TokLists = new List<TokModel>();

            isSet = Intent.GetBooleanExtra("isSet", isSet);
            //IntentTokList = Intent.GetStringExtra("TokList");
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            IntentTokList = prefs.GetString("TokModelList", "");

            if (isSet == true)
            {
                txtTCLoadMore.Visibility = ViewStates.Gone;

                string StringExtra = Intent.GetStringExtra("setModel");

                if (StringExtra!=null)
                {
                    setList = JsonConvert.DeserializeObject<Set>(StringExtra);
                }
                else
                {
                    StringExtra = Intent.GetStringExtra("classsetModel"); //From Class Set
                    classSet = JsonConvert.DeserializeObject<ClassSetModel>(StringExtra);
                    setList = classSet;

                    classSetVM = JsonConvert.DeserializeObject<ClassSetViewModel>(Intent.GetStringExtra("ClassSetViewModel"));
                }
                
            }
            else
            {
                SupportActionBar.Subtitle = Intent.GetStringExtra("SubTitle");
            }

            if (Settings.FilterTag == 9)
            {
                //Load more stuff here
                txtTCLoadMore.Visibility = ViewStates.Visible;
            }
            else
            {
                txtTCLoadMore.Visibility = ViewStates.Gone;
            }

            ((Activity)this).RunOnUiThread(async () => await InitializeData());

            btnPrevious = FindViewById<FloatingActionButton>(Resource.Id.FabMiniGamePrevious);
            btnNext = FindViewById<FloatingActionButton>(Resource.Id.FabMiniGameNext);
            btnPrevious.Enabled = false;

            _timer = new System.Timers.Timer();
            _timer.Interval = 6000;

            isStop = true; //default is true
            btnTokCardsPlay.Click -= OnPlayCardClick;
            btnTokCardsPlay.Click += OnPlayCardClick;

            //If Load More is clicked
            txtTCLoadMore.Click += async (object sender, EventArgs e) =>
            {
                progressbarTokCardLoading.Visibility = ViewStates.Visible;
                TokQueryValues tokQueryModel = new TokQueryValues();
                tokQueryModel.sortby = Settings.SortByFilter;
                tokQueryModel.token = Settings.ContinuationToken;
                tokQueryModel.loadmore = "yes";
                var restokList = await TokService.Instance.GetAllToks(tokQueryModel);
                foreach (var item in restokList)
                {
                    if (SupportActionBar.Subtitle.ToLower().Contains("category:") && SupportActionBar.Subtitle.ToLower().Contains("user:"))
                    {
                        if (item.IsMegaTok == true || item.TokGroup.ToLower() == "mega") //If Mega
                        {
                            allowAddTok = false;
                        }
                        else
                        {
                            allowAddTok = true;
                        }
                    }

                    if (allowAddTok)
                    {
                        TokLists.Add(item);
                    }
                }

                isFavorite.AddRange(Enumerable.Repeat(false, TokLists.Count).ToList());
                cardProgress.Max = TokLists.Count;
                cardProgressText.Text = cardProgress.Progress + "/" + cardProgress.Max;
                progressbarTokCardLoading.Visibility = ViewStates.Gone;
            };

            btnPrevious.Click += (object sender, EventArgs e) =>
            {
                Animation myAnim = AnimationUtils.LoadAnimation(MainActivity.Instance, Resource.Animation.fab_scaleup);
                frameTokCardMini.StartAnimation(myAnim);
                cnt = cnt - 1;
                cardProgress.IncrementProgressBy(-1);
                cardProgressText.Text = cardProgress.Progress + "/" + cardProgress.Max;

                trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(Resource.Id.frameTokCardMini, new CardFrontFragment());
                trans.AddToBackStack(null);
                trans.Commit();

                if (cnt == 0)
                {
                    btnPrevious.Enabled = false;
                }
                btnNext.Enabled = true;
            };
            btnNext.Click -= OnNextCard;
            btnNext.Click += OnNextCard;
            btnTokCardShuffle.Click += (object sender, EventArgs e) =>
            {
                cnt = 0;
                cardProgress.Progress = cnt + 1;
                cardProgressText.Text = cardProgress.Progress + "/" + cardProgress.Max;
                btnPrevious.Enabled = false;
                if (cnt < TokLists.Count)
                {
                    btnNext.Enabled = true;
                }

                var joined = TokLists.Zip(isFavorite, (x, y) => new { x, y });
                var shuffled = joined.OrderBy(x => Guid.NewGuid()).ToList();
                TokLists = shuffled.Select(pair => pair.x).ToList();
                isFavorite = shuffled.Select(pair => pair.y).ToList();

                trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(Resource.Id.frameTokCardMini, new CardFrontFragment());
                trans.AddToBackStack(null);
                trans.Commit();
            };
            btnTokCardFlip.Click -= OnFlipCard;
            btnTokCardFlip.Click += OnFlipCard;

            btnTokCardOptions.Click += (object sender, EventArgs e) =>
            {
                Bundle args = new Bundle();
                args.PutString("isPlayFavorite", isPlayFavorite.ToString());
                args.PutString("isImageVisible", isImageVisible.ToString());
                AndroidX.AppCompat.App.AppCompatDialogFragment newFragment = new modal_tokcards_options();
                newFragment.Arguments = args;
                newFragment.Show(SupportFragmentManager, "Options");
            };
        }
        public void OnFlipCard(object sender, EventArgs e)
        {
            FlipCard();
        }
        private void OnPlayCardClick(object sender, EventArgs e)
        {
            isStop = !isStop;
            PlayCard();
        }
        public void TriggerFavorite()
        {
            if (isPlayFavorite)
            {
                favList = new List<TokModel>();
                for (int i = 0; i < isFavorite.Count; i++)
                {
                    if (isFavorite[i] == true)
                    {
                        favList.Add(TokLists[i]);
                    }
                }

                cnt = 0;

                if (favList.Count != 0)
                {
                    loadFrontCard();
                }

                cardProgress.Progress = 1;
                cardProgress.Max = favList.Count;
                cardProgressText.Text = cardProgress.Progress + "/" + cardProgress.Max;

                if (favList.Count != 0)
                {
                    isStop = false; //to auto play
                    PlayCard();
                }
            }
            else
            {
                cnt = 0;
                cardProgress.Max = TokLists.Count;
                cardProgress.IncrementProgressBy(1);
                cardProgressText.Text = 1 + "/" + cardProgress.Max;
            }
        }
        private void PlayCard()
        {
            if (isStop) //If Stop
            {
                _timer.Stop();
                btnTokCardsPlay.Text = "► Play";
                btnTokCardsPlay.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor("#007bff"));
            }
            else if (!isStop) //If Play
            {
                _timer.AutoReset = true;
                _timer.Enabled = true;
                _timer.Elapsed += OnTimeEvent;
                btnTokCardsPlay.Text = "■ Stop";
                btnTokCardsPlay.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor("#dc3545"));
            }

            if (cnt == 0)
            {
                btnPrevious.Enabled = false;
            }

            if (cardProgress.Max > 1)
            {
                btnNext.Enabled = true;
            }
        }

        public async Task InitializeData()
        {
            bool isWithMega = false;
            progressbarTokCardLoading.Visibility = ViewStates.Visible;

            //Get Tokmoji
            var tokmojiResult = await TokMojiService.Instance.GetTokmojisAsync(null);
            ListTokmojiModel = tokmojiResult.Results.ToList();

            if (setList != null)
            {
                if (classSetVM == null)
                {
                    restokList = await GetSetToks();
                }
                else
                {
                    ClassTokLists = classSetVM.ClassToks;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(IntentTokList))
                {
                    restokList = JsonConvert.DeserializeObject<List<TokModel>>(IntentTokList);
                }
                else
                {
                    restokList = await HomeVm.GetToksData("", (FilterType)Settings.FilterTag);
                }
            }

            if (classSetVM == null)
            {
                foreach (var item in restokList)
                {
                    //if (SupportActionBar.Subtitle.ToLower().Contains("category:") && SupportActionBar.Subtitle.ToLower().Contains("user:"))
                    //{
                    if (item.IsMegaTok == true || item.TokGroup.ToLower() == "mega") //If Mega
                    {
                        allowAddTok = false;
                        isWithMega = true;
                    }
                    else
                    {
                        allowAddTok = true;
                    }
                    //}

                    if (allowAddTok)
                    {
                        TokLists.Add(item);
                    }
                }
            }
            else
            {
                foreach (var item in ClassTokLists)
                {
                    //if (SupportActionBar.Subtitle.ToLower().Contains("category:") && SupportActionBar.Subtitle.ToLower().Contains("user:"))
                    //{
                    if (item.IsMegaTok == true || item.TokGroup.ToLower() == "mega") //If Mega
                    {
                        allowAddTok = false;
                        isWithMega = true;
                    }

                    if (allowAddTok)
                    {
                        TokLists.Add(item);
                    }
                }
            }

            if (isWithMega)
            {
                //AlertMessage
                var objBuilder = new Android.App.AlertDialog.Builder(this);
                objBuilder.SetTitle("");
                objBuilder.SetMessage("This game cannot be played with Mega Toks.");
                objBuilder.SetCancelable(false);

                Android.App.AlertDialog objDialog = objBuilder.Create();
                objDialog.SetButton((int)(DialogButtonType.Positive), "OK", (s, ev) => { });
                objDialog.Show();
            }

            progressbarTokCardLoading.Visibility = ViewStates.Gone;

            cardProgress.Max = TokLists.Count;
            isFavorite = Enumerable.Repeat(false, TokLists.Count).ToList();
            cardProgress.Progress = 1;
            cardProgressText.Text = cardProgress.Progress + "/" + cardProgress.Max;

            if (TokLists.Count <= 1)
            {
                btnNext.Enabled = false;
            }

            if (TokLists.Count > 2)
            {
                gesturedetector = new GestureDetector(this, new MyGestureListener(this));

                trans = SupportFragmentManager.BeginTransaction();
                trans.Add(Resource.Id.frameTokCardMini, new CardFrontFragment());
                trans.Commit();
            }
        }
        public async Task<List<TokModel>> GetSetToks()
        {
            var list = new List<TokModel>();
            foreach (var tok in setList.TokIds)
            {
                var tokRes = await TokService.Instance.GetTokIdAsync(tok);
                if (tokRes != null)
                    list.Add(tokRes);
            }
            return list;
        }
        private void OnNextCard(object sender, EventArgs e)
        {
            if (cardProgress.Max > 1)
            {
                cnt = cnt + 1;
                loadFrontCard();

                cardProgress.IncrementProgressBy(1);
                cardProgressText.Text = cardProgress.Progress + "/" + cardProgress.Max;
                if (TokLists.Count <= 1 || cardProgress.Progress == TokLists.Count)
                {
                    btnNext.Enabled = false;
                }
                btnPrevious.Enabled = true;
            }
        }
        public void loadFrontCard()
        {
            Animation myAnim = AnimationUtils.LoadAnimation(MainActivity.Instance, Resource.Animation.fab_scaleup);
            frameTokCardMini.StartAnimation(myAnim);

            trans = SupportFragmentManager.BeginTransaction();
            trans.Replace(Resource.Id.frameTokCardMini, new CardFrontFragment());
            trans.AddToBackStack(null);
            trans.CommitAllowingStateLoss();
            Showingback = false;

            cardProgressText.Text = cardProgress.Progress + "/" + cardProgress.Max;
        }
        public void removeFragmentinFL()
        {
            trans = SupportFragmentManager.BeginTransaction();
            trans.Remove(new CardFrontFragment());
            trans.AddToBackStack(null);
            trans.CommitAllowingStateLoss();
        }
        private void OnTimeEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            FlipCard();
            Thread.Sleep(5000);

            RunOnUiThread(() =>
            {
                OnNextCard(sender, e);
                CheckProgress(cnt);
            });
        }
        public void CheckProgress(int progress)
        {
            lock (_lock)
            {
                if (progress >= cardProgress.Max)
                {
                    cnt = 0;
                    cardProgress.Progress = 1;
                    cardProgressText.Text = cardProgress.Progress + "/" + cardProgress.Max;
                }
            }
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void FlipCard()
        {
            if (Showingback)
            {
                SupportFragmentManager.PopBackStack();
                Showingback = false;
            }
            else
            {
                AndroidX.Fragment.App.FragmentTransaction trans = SupportFragmentManager.BeginTransaction();
                trans.SetCustomAnimations(Resource.Animation.card_flip_right_in, Resource.Animation.card_flip_right_out, Resource.Animation.card_flip_left_in, Resource.Animation.card_flip_left_out);
                trans.Replace(Resource.Id.frameTokCardMini, new CardBackFragment());
                trans.AddToBackStack(null);
                trans.Commit();
                Showingback = true;
            }
        }
        private class CardFrontFragment : AndroidX.Fragment.App.Fragment
        {
            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                TokCardsMiniGameActivity tokCardsMiniGameActivity = Activity as TokCardsMiniGameActivity;
                View frontcard_view = inflater.Inflate(Resource.Layout.preview_cardfront, container, false);

                var linear_previewcardfront = frontcard_view.FindViewById<LinearLayout>(Resource.Id.linear_previewcardfront);
                var lblPreviewCardFront = frontcard_view.FindViewById<TextView>(Resource.Id.lblPreviewCardFront);

                List<TokModel> tokList = new List<TokModel>();
                if (tokCardsMiniGameActivity.isPlayFavorite)
                {
                    tokList = tokCardsMiniGameActivity.favList;
                }
                else
                {
                    tokList = tokCardsMiniGameActivity.TokLists;
                }

                if (tokList.Count > 0)
                {
                    if (tokCardsMiniGameActivity.cnt < tokList.Count)
                    {
                        if (!string.IsNullOrEmpty(tokList[tokCardsMiniGameActivity.cnt].PrimaryFieldText))
                        {
                            if (tokList[tokCardsMiniGameActivity.cnt].IsDetailBased)
                            {
                                if (tokList[tokCardsMiniGameActivity.cnt].Details != null)
                                {
                                    string detailstr = "";
                                    for (int i = 0; i < tokList[tokCardsMiniGameActivity.cnt].Details.Count(); i++)
                                    {
                                        if (!string.IsNullOrEmpty(tokList[tokCardsMiniGameActivity.cnt].Details[i]))
                                        {
                                            if (i == 0)
                                            {
                                                detailstr = "• " + tokList[tokCardsMiniGameActivity.cnt].Details[i].ToString();
                                            }
                                            else
                                            {
                                                detailstr += "\n• " + tokList[tokCardsMiniGameActivity.cnt].Details[i].ToString();
                                            }
                                        }
                                    }
                                    //vh.tokcardback.Typeface = Typeface.Default;

                                    tokCardsMiniGameActivity.ssbName = new SpannableStringBuilder(detailstr);
                                    tokCardsMiniGameActivity.spannableResText = SpannableHelper.AddStickersSpannable(MainActivity.Instance, tokCardsMiniGameActivity.ssbName);
                                    lblPreviewCardFront.SetText(tokCardsMiniGameActivity.spannableResText, TextView.BufferType.Spannable);
                                }
                            }
                            else
                            {
                                tokCardsMiniGameActivity.ssbName = new SpannableStringBuilder(tokList[tokCardsMiniGameActivity.cnt].PrimaryFieldText);
                                tokCardsMiniGameActivity.spannableResText = SpannableHelper.AddStickersSpannable(tokCardsMiniGameActivity, tokCardsMiniGameActivity.ssbName);

                                lblPreviewCardFront.SetText(tokCardsMiniGameActivity.spannableResText, TextView.BufferType.Spannable);
                            }
                        }
                    }
                }


                var txt_previewcardfrontstar = frontcard_view.FindViewById<TextView>(Resource.Id.txt_previewcardfrontstar);
                //load favorites
                if (tokCardsMiniGameActivity.isFavorite[tokCardsMiniGameActivity.cnt] == true || tokCardsMiniGameActivity.isPlayFavorite == true)
                {
                    linear_previewcardfront.SetBackgroundResource(Resource.Drawable.linear_yellowborder);
                    txt_previewcardfrontstar.SetBackgroundResource(Resource.Drawable.star_yellow);
                }
                else
                {
                    linear_previewcardfront.SetBackgroundResource(Resource.Drawable.linear_violetborder);
                    txt_previewcardfrontstar.SetBackgroundResource(Resource.Drawable.star_gray);
                }

                //if star is clicked
                txt_previewcardfrontstar.Click += (Object sender, EventArgs e) =>
                {
                    if (tokCardsMiniGameActivity.isFavorite[tokCardsMiniGameActivity.cnt] == true)
                    {
                        tokCardsMiniGameActivity.isFavorite[tokCardsMiniGameActivity.cnt] = false;
                        linear_previewcardfront.SetBackgroundResource(Resource.Drawable.linear_violetborder);
                        txt_previewcardfrontstar.SetBackgroundResource(Resource.Drawable.star_gray);
                    }
                    else
                    {
                        linear_previewcardfront.SetBackgroundResource(Resource.Drawable.linear_yellowborder);
                        txt_previewcardfrontstar.SetBackgroundResource(Resource.Drawable.star_yellow);
                        tokCardsMiniGameActivity.isFavorite[tokCardsMiniGameActivity.cnt] = true;
                    }
                };
                frontcard_view.Touch += Frontcard_view_Touch;
                return frontcard_view;
            }

            private void Frontcard_view_Touch(object sender, View.TouchEventArgs e)
            {
                TokCardsMiniGameActivity myactivity = Activity as TokCardsMiniGameActivity;

                myactivity.gesturedetector.OnTouchEvent(e.Event);
            }
        }
        public class CardBackFragment : AndroidX.Fragment.App.Fragment
        {
            public View backcard_view; public ImageView img_previewcardback;
            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                TokCardsMiniGameActivity tokCardsMiniGameActivity = Activity as TokCardsMiniGameActivity;
                backcard_view = inflater.Inflate(Resource.Layout.preview_cardback, container, false);
                img_previewcardback = backcard_view.FindViewById<ImageView>(Resource.Id.img_previewcardback);
                var linear_previewcardfront = backcard_view.FindViewById<LinearLayout>(Resource.Id.linear_previewcardback);
                var lblPreviewCardBack = backcard_view.FindViewById<TextView>(Resource.Id.lblPreviewCardBack);

                if (tokCardsMiniGameActivity.isImageVisible)
                {
                    img_previewcardback.Visibility = ViewStates.Visible;
                }
                else
                {
                    img_previewcardback.Visibility = ViewStates.Gone;
                }

                List<TokModel> tokList = new List<TokModel>();
                if (tokCardsMiniGameActivity.isPlayFavorite)
                {
                    tokList = tokCardsMiniGameActivity.favList;
                }
                else
                {
                    tokList = tokCardsMiniGameActivity.TokLists;
                }

                if (tokList.Count > 0)
                {
                    if (tokList[tokCardsMiniGameActivity.cnt].IsMegaTok == true || tokList[tokCardsMiniGameActivity.cnt].TokGroup.ToLower() == "mega")
                    {
                        //Mega
                        for (int i = 0; i < tokList[tokCardsMiniGameActivity.cnt].Sections.Count(); i++)
                        {
                            if (!string.IsNullOrEmpty(tokList[tokCardsMiniGameActivity.cnt].Sections[i].Title))
                            {
                                //lblPreviewCardBack.Text = tokList[tokCardsMiniGameActivity.cnt].Sections[i].Title;

                                tokCardsMiniGameActivity.ssbName = new SpannableStringBuilder(tokList[tokCardsMiniGameActivity.cnt].Sections[i].Title);
                                tokCardsMiniGameActivity.spannableResText = SpannableHelper.AddStickersSpannable(tokCardsMiniGameActivity, tokCardsMiniGameActivity.ssbName);

                                lblPreviewCardBack.SetText(tokCardsMiniGameActivity.spannableResText, TextView.BufferType.Spannable);

                                break;
                            }
                        }
                    }
                    else if (tokList[tokCardsMiniGameActivity.cnt].IsDetailBased == true)
                    {
                        tokCardsMiniGameActivity.ssbName = new SpannableStringBuilder(tokList[tokCardsMiniGameActivity.cnt].PrimaryFieldText);
                        tokCardsMiniGameActivity.spannableResText = SpannableHelper.AddStickersSpannable(tokCardsMiniGameActivity, tokCardsMiniGameActivity.ssbName);

                        lblPreviewCardBack.SetText(tokCardsMiniGameActivity.spannableResText, TextView.BufferType.Spannable);

                        /*if (tokCardsMiniGameActivity.TokLists[tokCardsMiniGameActivity.cnt].Details != null)
                        {
                            string detailstr = "";
                            for (int i = 0; i < tokList[tokCardsMiniGameActivity.cnt].Details.Count(); i++)
                            {
                                if (!string.IsNullOrEmpty(tokList[tokCardsMiniGameActivity.cnt].Details[i]))
                                {
                                    if (i == 0)
                                    {
                                        detailstr = "• " + tokList[tokCardsMiniGameActivity.cnt].Details[i].ToString();
                                    }
                                    else
                                    {
                                        detailstr += "\n• " + tokList[tokCardsMiniGameActivity.cnt].Details[i].ToString();
                                    }
                                }
                            }
                            lblPreviewCardBack.Typeface = Typeface.Default;
                            //lblPreviewCardBack.Text = detailstr;

                            tokCardsMiniGameActivity.ssbName = new SpannableStringBuilder(detailstr);
                            tokCardsMiniGameActivity.spannableResText = SpannableHelper.AddStickersSpannable(tokCardsMiniGameActivity, tokCardsMiniGameActivity.ssbName);

                            lblPreviewCardBack.SetText(tokCardsMiniGameActivity.spannableResText, TextView.BufferType.Spannable);
                        }*/
                    }
                    else
                    {
                        //lblPreviewCardBack.Text = tokList[tokCardsMiniGameActivity.cnt].SecondaryFieldText;

                        tokCardsMiniGameActivity.ssbName = new SpannableStringBuilder(tokList[tokCardsMiniGameActivity.cnt].SecondaryFieldText);
                        tokCardsMiniGameActivity.spannableResText = SpannableHelper.AddStickersSpannable(tokCardsMiniGameActivity, tokCardsMiniGameActivity.ssbName);

                        lblPreviewCardBack.SetText(tokCardsMiniGameActivity.spannableResText, TextView.BufferType.Spannable);
                    }

                    //if tok is image
                    if (!string.IsNullOrEmpty(tokList[tokCardsMiniGameActivity.cnt].Image))
                    {
                        string tokimg = tokList[tokCardsMiniGameActivity.cnt].Image + ".jpg";
                        Glide.With(tokCardsMiniGameActivity).Load(tokimg).Into(img_previewcardback);
                    };
                }

                var txt_previewcardbackstar = backcard_view.FindViewById<TextView>(Resource.Id.txt_previewcardbackstar);

                //Load the colors
                if (tokCardsMiniGameActivity.isFavorite[tokCardsMiniGameActivity.cnt] == true || tokCardsMiniGameActivity.isPlayFavorite == true)
                {
                    linear_previewcardfront.SetBackgroundResource(Resource.Drawable.linear_yellowborder);
                    txt_previewcardbackstar.SetBackgroundResource(Resource.Drawable.star_yellow);
                }
                else
                {
                    linear_previewcardfront.SetBackgroundResource(Resource.Drawable.linear_violetborder);
                    txt_previewcardbackstar.SetBackgroundResource(Resource.Drawable.star_gray);
                }

                //If star is clicked
                txt_previewcardbackstar.Click += (Object sender, EventArgs e) =>
                {
                    if (tokCardsMiniGameActivity.isFavorite[tokCardsMiniGameActivity.cnt] == true)
                    {
                        tokCardsMiniGameActivity.isFavorite[tokCardsMiniGameActivity.cnt] = false;
                        linear_previewcardfront.SetBackgroundResource(Resource.Drawable.linear_violetborder);
                        txt_previewcardbackstar.SetBackgroundResource(Resource.Drawable.star_gray);
                    }
                    else
                    {
                        linear_previewcardfront.SetBackgroundResource(Resource.Drawable.linear_yellowborder);
                        txt_previewcardbackstar.SetBackgroundResource(Resource.Drawable.star_yellow);
                        tokCardsMiniGameActivity.isFavorite[tokCardsMiniGameActivity.cnt] = true;
                    }
                };

                if (img_previewcardback.Visibility == ViewStates.Visible)
                {
                    tokCardsMiniGameActivity.isImageVisible = true;
                }
                else
                {
                    tokCardsMiniGameActivity.isImageVisible = false;
                }

                backcard_view.Touch += Backcard_view_Touch;
                return backcard_view;
            }

            private void Backcard_view_Touch(object sender, View.TouchEventArgs e)
            {
                TokCardsMiniGameActivity myactivity = Activity as TokCardsMiniGameActivity;

                myactivity.gesturedetector.OnTouchEvent(e.Event);
            }
        }


        private class MyGestureListener : GestureDetector.SimpleOnGestureListener
        {
            private TokCardsMiniGameActivity mainActivity;

            public MyGestureListener(TokCardsMiniGameActivity Activity)
            {
                mainActivity = Activity;
            }

            public override bool OnSingleTapUp(MotionEvent e)
            {
                mainActivity.FlipCard();
                return true;
            }

            public override void OnLongPress(MotionEvent e)
            {
                Console.WriteLine("Long Press");
            }
            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {

                Console.WriteLine("Fling");
                return base.OnFling(e1, e2, velocityX, velocityY);
            }

            public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
            {
                Console.WriteLine("Scroll");
                return base.OnScroll(e1, e2, distanceX, distanceY);
            }
        }
    }
}