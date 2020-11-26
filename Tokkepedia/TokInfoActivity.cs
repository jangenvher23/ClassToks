using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Newtonsoft.Json;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Helpers;
using System.Threading.Tasks;
using Tokkepedia.Shared.Services;
using Android.Text;
using Tokket.Tokkepedia;
using Android.Webkit;
using AlertDialog = Android.App.AlertDialog;
using Android.Views.Animations;
using Tokkepedia.Shared.ViewModels;
using Tokkepedia.Shared.Extensions;
using Android.Support.V7.Widget;
using Tokkepedia.Listener;
using Tokkepedia.ViewModels;
using Tokkepedia.ViewHolders;
using GalaSoft.MvvmLight.Helpers;
using CachingViewHolder = GalaSoft.MvvmLight.Helpers.CachingViewHolder;
using Android.Support.V4.View;
using static Android.Support.Constraints.ConstraintLayout;
using Com.Github.Aakira.Expandablelayout;
using Android.Text.Style;
using Tokkepedia.Helpers;
using Supercharge;
using System.Threading;
using Android.Views.InputMethods;
using Java.Util;
using Android.Text.Format;
using DE.Hdodenhof.CircleImageViewLib;
using System.Net;
using Tokkepedia.Model;
using System.IO;
using Android.Graphics.Drawables;
using Java.IO;
using Tokkepedia.Fragments;
using static Android.Widget.CompoundButton;
using SharedAccount = Tokkepedia.Shared.Services;
using ImageViews.Photo;
using Android.Animation;
using Android.Util;
using AndroidX.Core.Widget;
using Java.Util.Regex;
using Pattern = Java.Util.Regex.Pattern;
using Android.Text.Method;
using Tokkepedia;
using Xamarin.Essentials;
using Color = Android.Graphics.Color;
using Org.Json;
using AndroidX.SwipeRefreshLayout.Widget;
using System.ComponentModel;
using NetworkAccess = Xamarin.Essentials.NetworkAccess;

namespace Tokkepedia
{
    [Activity(Label = "Tok Info", Theme = "@style/AppTheme")]
    public class TokInfoActivity : BaseActivity, View.IOnTouchListener
    {
        // Allows us to know if we should use MotionEvent.ACTION_MOVE
        private bool tracking = false;
        // The Position where our touch event started
        private float startY;
        float imgScale = 0;
        internal static TokInfoActivity Instance { get; private set; }
        GestureDetector gesturedetector;
        TokModel tokModel; ClassTokModel classTokModel;
        Intent nextActivity; bool isInaccurateAdded = false, changesMade = false;
        List<ReactionModel> CommentList; List<TokMojiDrawableModel> TokMojiDrawables;
        Typeface font; GridLayoutManager mLayoutManager; string hashtagCode = "(^#[a-z0-9_]+|#[a-zA-Z0-9_]+$)";
        GlideImgListener GListener; SpannableString hashtagText;
        ReactionValueViewModel reactionValueVM; ReactionValueModel reactionValue;
        TextView PrimaryFieldText, EnglishPrimaryFieldText; //Separated this one and excluded this from the #regio UI properties because this will return a null valuee when added in the linear.AddView
        private ObservableRecyclerAdapter<ReactionModel, CachingViewHolder> adapterComments;
        public TokInfoViewModel TokInfoVm => App.Locator.TokInfoPageVM;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.tok_info_page);

            var tokback_toolbar = this.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.tokinfo_toolbar);
#if (_CLASSTOKS)
            tokback_toolbar.SetBackgroundResource(Resource.Color.colorAccent);
#endif
            SetSupportActionBar(tokback_toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            Instance = this;
            Settings.ContinuationToken = null;
            RecyclerComments.ContentDescription = "";

            TokInfoVm.CircleProgress = CircleProgress;

            TokMojiDrawables = new List<TokMojiDrawableModel>();
            gesturedetector = new GestureDetector(this, new MyGestureListener(this));
            FrameReactionBtn.Touch -= ReactionTableTouched;
            FrameReactionBtn.Touch += ReactionTableTouched;

            GreenGemContainer.SetOnTouchListener(this);
            YellowGemContainer.SetOnTouchListener(this);
            RedGemContainer.SetOnTouchListener(this);

            BtnInaccurateComment.Click += async(sender, e) =>
            {
                await OnClickAddReaction(BtnInaccurateComment);
            };

            btnTokInfo_SendComment.Click += async (sender, e) =>
            {
                await OnClickAddReaction(btnTokInfo_SendComment);
            };

            BtnMegaAccurate.Click += async (sender, e) =>
            {
                await OnClickAddReaction(BtnMegaAccurate);
            };

            EnglishPrimaryFieldText = FindViewById<TextView>(Resource.Id.lbl_tokTopicConvert1);
            PrimaryFieldText = FindViewById<TextView>(Resource.Id.lbl_tokTopic1);

            swipeRefreshComment.Refresh += RefreshLayout_Refresh;

            CommentEditor.FocusChange += delegate
            {
                if (CommentEditor.IsFocused)
                {
                    BtnSmile.SetImageResource(Resource.Drawable.TOKKET_smiley_1B);
                    ExpandedTokMoji.Expanded = false;
                }
            };

            CommentEditor.Click += delegate
            {
                BtnSmile.SetImageResource(Resource.Drawable.TOKKET_smiley_1B);
                ExpandedTokMoji.Expanded = false;

                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);

                CommentEditor.RequestFocus();

                inputManager.ShowSoftInput(CommentEditor, 0);
            };
            
            EditInaccurateComment.FocusChange += delegate
            {
                if (EditInaccurateComment.IsFocused)
                {
                    BtnSmileInaccurate.SetImageResource(Resource.Drawable.TOKKET_smiley_1B);
                    ExpandedTokMojiInaccurate.Expanded = false;
                }
            };

            EditInaccurateComment.Click += delegate
            {
                BtnSmileInaccurate.SetImageResource(Resource.Drawable.TOKKET_smiley_1B);
                ExpandedTokMojiInaccurate.Expanded = false;

                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);

                EditInaccurateComment.RequestFocus();

                inputManager.ShowSoftInput(EditInaccurateComment, 0);
            };

#if (_CLASSTOKS)
                classTokModel = JsonConvert.DeserializeObject<ClassTokModel>(Intent.GetStringExtra("classtokModel"));
                tokModel = classTokModel;
                if (!string.IsNullOrEmpty(classTokModel.GroupId))
                {
                    ImgPurpleGem.Visibility = ViewStates.Gone;
                    LinearTokInfoReaction.Visibility = ViewStates.Gone;
                    NestedComment.Visibility = ViewStates.Gone;
                }
                LabelTokType.SetText(Resource.String.underClassName);
                LabelTokGroup.SetText(Resource.String.underType);
            

                SupportActionBar.Subtitle = classTokModel.GroupName;
#endif

#if (_TOKKEPEDIA)
            tokModel = JsonConvert.DeserializeObject<TokModel>(Intent.GetStringExtra("tokModel"));
#endif

            font = Typeface.CreateFromAsset(Application.Assets, "fa_solid_900.otf");
            ReactionCheck.Typeface = font;
            ReactionWrong.Typeface = font;
            BtnTokInfoEyeIcon.Typeface = font;
            BtnMegaAccurate.Typeface = font;
            BtnMegaInaccurate.Typeface = font;

            BtnTokInfoEyeIcon.SetOnTouchListener(this);

            txtUserDisplayName.Text = tokModel.UserDisplayName;
            Glide.With(this).Load(Settings.GetTokketUser().UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(imgcomment_userphoto);
            Glide.With(this).Load(tokModel.UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(imgUserPhoto);
            Glide.With(this).Load(tokModel.StickerImage).Into(StickerImage);
            if (string.IsNullOrEmpty(tokModel.StickerImage))
            {
                StickerImage.Visibility = ViewStates.Gone;
            }

            TokDateTimeCreated.Text = tokModel.RelativeTime;

            FillUpFields();

            //If purplegem is clicked
            ImgPurpleGem.Click -= ShowGemsCollClicked;
            ImgPurpleGem.Click += ShowGemsCollClicked;


            tokcategory.Tag = (int)Toks.Category;
            tokcategory.Click -= OnTokButtonClick;
            tokcategory.Click += OnTokButtonClick;

            tokgroup.Tag = (int)Toks.TokGroup;
            tokgroup.Click -= OnTokButtonClick;
            tokgroup.Click += OnTokButtonClick;

            toktype.Tag = (int)Toks.TokType;
            toktype.Click -= OnTokButtonClick;
            toktype.Click += OnTokButtonClick;

            imgUserPhoto.Click += delegate
            {
                nextActivity = new Intent(this, typeof(ProfileUserActivity));
                nextActivity.PutExtra("userid", tokModel.UserId);
                this.StartActivity(nextActivity);
            };

            //Load TokMoji
            RecyclerTokMojis.SetLayoutManager(new GridLayoutManager(this, 2));
            RecyclerTokMojisDummy.SetLayoutManager(new GridLayoutManager(this, 2));
            RecyclerTokMojisInaccurate.SetLayoutManager(new GridLayoutManager(this, 2));

            BtnSmile.Click += (object sender, EventArgs e) =>
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                if (ExpandedTokMoji.Expanded == false)
                {
                    BtnSmile.SetImageResource(Resource.Drawable.TOKKET_smiley_2b);
                    inputManager.HideSoftInputFromWindow(BtnSmile.WindowToken, HideSoftInputFlags.None);
                    ExpandedTokMoji.Expanded = true;
                }
                else
                {
                    BtnSmile.SetImageResource(Resource.Drawable.TOKKET_smiley_1B);
                    ExpandedTokMoji.Expanded = false;
                    View CommentView = CommentEditor;
                    CommentView.RequestFocus();
                    inputManager.ShowSoftInput(CommentView, 0);
                }
            };

            BtnSmileInaccurate.Click += (object sender, EventArgs e) =>
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                if (ExpandedTokMojiInaccurate.Expanded == false)
                {
                    BtnSmileInaccurate.SetImageResource(Resource.Drawable.TOKKET_smiley_2b);
                    inputManager.HideSoftInputFromWindow(BtnSmileInaccurate.WindowToken, HideSoftInputFlags.None);
                    ExpandedTokMojiInaccurate.Expanded = true;
                }
                else
                {
                    BtnSmileInaccurate.SetImageResource(Resource.Drawable.TOKKET_smiley_1B);
                    ExpandedTokMojiInaccurate.Expanded = false;
                    View CommentView = EditInaccurateComment;
                    CommentView.RequestFocus();
                    inputManager.ShowSoftInput(CommentView, 0);
                }
            };

            
            if (tokModel.IsMegaTok == true || tokModel.TokGroup.ToLower() == "mega") //If Mega
            {
                BtnMegaAccurate.Visibility = ViewStates.Visible;
                BtnMegaInaccurate.Visibility = ViewStates.Visible;
                TokBackButton.Visibility = ViewStates.Gone;
                if (tokModel.Sections == null)
                {
                    RunOnUiThread(async () => await loadSections());
                }

                if (tokModel.Sections != null)
                {
                    MegaTokSections();
                }
            }
            else
            {
                TokBackButton.Visibility = ViewStates.Visible;

                isEnglishLinear.RemoveAllViews();

                isEnglishLinear.AddView(EnglishPrimaryFieldText);

                if (tokModel.IsDetailBased)
                {
                    AddTokDetails();

                    if (!tokModel.IsEnglish)
                    {
                        for (int i = 0; i < tokModel.EnglishDetails.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(tokModel.EnglishDetails[i]))
                            {
                                isEnglishLinear.Visibility = ViewStates.Visible;
                                View viewEnglish = LayoutInflater.Inflate(Resource.Layout.tokinfo_isenglish, null);
                                TextView txtEnglish = viewEnglish.FindViewById<TextView>(Resource.Id.lbl_tokConvertAnswer1);
                                txtEnglish.Text = "\u2022 " + tokModel.EnglishDetails[i];
                                isEnglishLinear.AddView(viewEnglish);
                            }
                        }
                    }
                    else
                    {
                        isEnglishLinear.Visibility = ViewStates.Gone;
                    }
                }
                else
                {
                    View view = LayoutInflater.Inflate(Resource.Layout.tokinfo_detail1, null);
                    var btnAccurate = view.FindViewById<Button>(Resource.Id.tokinfo_btnAccurate);
                    var btnInaccurate = view.FindViewById<Button>(Resource.Id.tokinfo_btnInaccurate);
                    btnAccurate.Typeface = font;
                    btnInaccurate.Typeface = font;

#if (_CLASSTOKS)
                    btnAccurate.Visibility = ViewStates.Gone;
                    btnInaccurate.Visibility = ViewStates.Gone;
#endif
                    TextView txtDetail = view.FindViewById<TextView>(Resource.Id.lbl_detail);

                    hashtagText = new SpannableString(tokModel.SecondaryFieldText + "");
                    Matcher matcher = Pattern.Compile(hashtagCode).Matcher(hashtagText);
                    while (matcher.Find())
                    {
                        hashtagText.SetSpan(new ClickableText(Actions.OpenHashTag, matcher.Group()), matcher.Start(), matcher.End(), 0);
                    }

                    txtDetail.Append(hashtagText);
                    txtDetail.MovementMethod = LinkMovementMethod.Instance;

                    //txtDetail.SetText(hashtagText, TextView.BufferType.Spannable);
                    
                    linearParent.AddView(view);

                    if (!tokModel.IsEnglish)
                    {
                        View viewEnglish2 = LayoutInflater.Inflate(Resource.Layout.tokinfo_isenglish, null);
                        TextView txtSecEnglish = viewEnglish2.FindViewById<TextView>(Resource.Id.lbl_tokConvertAnswer1);
                        txtSecEnglish.Text = tokModel.EnglishSecondaryFieldText;
                        isEnglishLinear.AddView(viewEnglish2);
                    }
                }
            }

            //Load Comments
            mLayoutManager = new GridLayoutManager(this, 1);
            RecyclerComments.SetLayoutManager(mLayoutManager);

            //Load RecyclerView
            CommentList = new List<ReactionModel>();
            RunOnUiThread(async () => await LoadComments());

            //LoadMore
            if (RecyclerComments != null)
            {
                RecyclerComments.HasFixedSize = true;
                RecyclerComments.NestedScrollingEnabled = false;

                NestedScroll.ScrollChange += async (object sender, NestedScrollView.ScrollChangeEventArgs e) =>
                {
                    View view = (View)NestedScroll.GetChildAt(NestedScroll.ChildCount - 1);

                    int diff = (view.Bottom - (NestedScroll.Height + NestedScroll.ScrollY));

                    if (diff == 0)
                    {
                        if (!string.IsNullOrEmpty(RecyclerComments.ContentDescription))
                        {
                            await LoadComments(RecyclerComments.ContentDescription);
                        }
                    }

                    if (e.ScrollY > e.OldScrollX)
                    {

                    }
                };
            }

            //Load the comments first before the tokmojis
            RunOnUiThread(async () => await RunTokMojis());
        }
        private void FillUpFields()
        {
            //PrimaryFieldText.Text = tokModel.PrimaryFieldText;

            EnglishPrimaryFieldText.Text = tokModel.EnglishPrimaryFieldText ?? "";
            tokcategory.Text = tokModel.Category;
            tokgroup.Text = tokModel.TokGroup;
            toktype.Text = tokModel.TokType;

            if (!string.IsNullOrEmpty(tokModel.Image))
            {
                GListener = new GlideImgListener();
                GListener.ParentActivity = this;


                if (URLUtil.IsValidUrl(tokModel.Image))
                {
                    Glide.With(this).Load(tokModel.Image).Listener(GListener).Into(tokinfo_imgMain);
                }
                else
                {
                    byte[] imageDetailBytes = Convert.FromBase64String(tokModel.Image.Replace("data:image/jpeg;base64,", ""));
                    tokinfo_imgMain.SetImageBitmap((BitmapFactory.DecodeByteArray(imageDetailBytes, 0, imageDetailBytes.Length)));
                }
            }
        }
        private async Task RunTokMojis()
        {
            TokInfoVm.TokMojiCollection.Clear();

            await TokInfoVm.LoadTokMoji();

            //Show tokmoji in text
            SpannableHelper.ListTokMoji = TokInfoVm.TokMojiCollection.ToList();
            SpannableStringBuilder ssbName = new SpannableStringBuilder(tokModel.PrimaryFieldText);

            var resultSpan = SpannableHelper.AddStickersSpannable(this, ssbName);

            hashtagText = new SpannableString(tokModel.PrimaryFieldText);
            Matcher matcher = Pattern.Compile(hashtagCode).Matcher(hashtagText);
            while (matcher.Find())
            {
                resultSpan.SetSpan(new ClickableText(Actions.OpenHashTag, matcher.Group()), matcher.Start(), matcher.End(), 0);
            }
            
            PrimaryFieldText.Append(resultSpan); //SetText(resultSpan, TextView.BufferType.Spannable);
            PrimaryFieldText.MovementMethod = LinkMovementMethod.Instance;

            var adapterTokMoji = TokInfoVm.TokMojiCollection.GetRecyclerAdapter(BindTokMojiViewHolder, Resource.Layout.tokinfo_tokmoji_row);
            RecyclerTokMojis.SetAdapter(adapterTokMoji);

            RecyclerTokMojisInaccurate.SetAdapter(adapterTokMoji);

            var adapterTokMojiDummy = TokInfoVm.TokMojiCollection.GetRecyclerAdapter(BindTokMojiViewHolderDummy, Resource.Layout.tokinfo_tokmoji_row);
            RecyclerTokMojisDummy.SetAdapter(adapterTokMoji);
            
            RecyclerTokMojisDummy.LayoutChange += (sender, e) =>
            {
                //Created a dummy recyclerTokMoji so that it will show the image.
                for (int i = 0; i < RecyclerTokMojisDummy.ChildCount; i++)
                {
                    View view = RecyclerTokMojisDummy.GetChildAt(i);
                    var ImgTokMoji = view.FindViewById<ImageView>(Resource.Id.imgTokInfoTokMojiRow);

                    if (ImgTokMoji.Drawable != null)
                    {
                        var detail = TokMojiDrawables.FirstOrDefault(a => a.TokmojiId == ImgTokMoji.ContentDescription);
                        if (detail == null)
                        {
                            var tokmojiModel = new TokMojiDrawableModel();
                            tokmojiModel.TokmojiId = ImgTokMoji.ContentDescription;

                            //encode image to base64 string
                            try //if Drawable is displaying the loader animation catch the error
                            {
                                Bitmap imgTokMojiBitmap = ((BitmapDrawable)ImgTokMoji.Drawable).Bitmap;
                                MemoryStream byteArrayOutputStream = new MemoryStream();
                                imgTokMojiBitmap.Compress(Bitmap.CompressFormat.Png, 100, byteArrayOutputStream);
                                byte[] byteArray = byteArrayOutputStream.ToArray();
                                
                                tokmojiModel.TokmojImgBase64 = Android.Util.Base64.EncodeToString(byteArray, Android.Util.Base64Flags.Default);
                                //tokmojiModel.TokmojImg = ImgTokMoji.Drawable;
                                TokMojiDrawables.Add(tokmojiModel);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            };
        }

        private async Task LoadComments(string continuationtoken = "", bool isRefresh = false)
        {
            if (continuationtoken == "")
            {
                TokInfoVm.commentsloaded = 0;
                TokInfoVm.CommentsCollection.Clear();
                ShimmerCommentsList.StartShimmerAnimation();
                ShimmerCommentsList.Visibility = ViewStates.Visible;
                ProgressComments.Visibility = ViewStates.Visible;
            }

            await TokInfoVm.LoadComments(tokModel.Id, continuationtoken);
            RecyclerComments.ContentDescription = Settings.ContinuationToken;
            adapterComments = TokInfoVm.CommentsCollection.GetRecyclerAdapter(BindCommentsViewHolder, Resource.Layout.tokinfo_comments_row);
            RecyclerComments.SetAdapter(adapterComments);

            if (TokInfoVm.CommentsCollection.Count == 0)
            {
                TextTotalComments.Text = " ";
            }
            else
            {
                TextTotalComments.Text = "Comments: " + TokInfoVm.CommentsCollection.Count.ToString();
            }
            
            if (continuationtoken == "")
            {
                TextTotalComments.Visibility = ViewStates.Visible;
                ProgressComments.Visibility = ViewStates.Gone;
                ShimmerCommentsList.Visibility = ViewStates.Gone;

                if (ShimmerCommentsList.Visibility == ViewStates.Gone) //Page is fully loaded
                {
                    if (!isRefresh)
                    {
                        //Writing/recording a view:
                        ReactionModel tokkepediaReaction = new ReactionModel();
                        tokkepediaReaction.ItemId = tokModel.Id;

                        if (Settings.GetUserModel().UserId == tokModel.UserId)
                        {
                            tokkepediaReaction.Kind = "tiletap_views_personal";
                        }
                        else
                        {
                            tokkepediaReaction.Kind = "tiletap_views";
                        }

                        tokkepediaReaction.Label = "reaction";
                        tokkepediaReaction.DetailNum = 0;
                        //tokkepediaReaction.CategoryId = tokModel.CategoryId;
                        //tokkepediaReaction.TokTypeId = tokModel.TokTypeId;
                        tokkepediaReaction.OwnerId = tokModel.UserId;
                        tokkepediaReaction.IsChild = false;

                        //API
                        var result = await ReactionService.Instance.AddReaction(tokkepediaReaction);
                        if (result.ResultEnum == Shared.Helpers.Result.Success)
                        {

                        }
                        await LoadSelectedGems(Id: tokModel.Id);
                    }
                }
            }

            RecyclerComments.LayoutChange += (sender,e) =>
            {
                for (int i = TokInfoVm.commentsloaded; i < RecyclerComments.ChildCount; i++)
                {
                    View viewParent = RecyclerComments.GetChildAt(i);
                    var EditCommentText = viewParent.FindViewById<EditText>(Resource.Id.EditCommentRowContent);
                    var BtnViewMoreClose = viewParent.FindViewById<Button>(Resource.Id.btnViewMoreCloseComment);
                    var txtEllipseComment = viewParent.FindViewById<TextView>(Resource.Id.lblCommentRowContentEllip);
                    var CommentText = viewParent.FindViewById<TextView>(Resource.Id.lblCommentRowContent);
                    var ReplyUserDisplayText = viewParent.FindViewById<TextView>(Resource.Id.TokInfoCommentsReplyText);
                    var spannableStringComment = new SpannableString(CommentText.Text);
                    var spannableStringReply = new SpannableString(ReplyUserDisplayText.Text);

                    Layout layout = txtEllipseComment.Layout;
                    int commentEllipLine = txtEllipseComment.LineCount;

                    if (commentEllipLine > 1)
                    {
                        int ellipsisCount = layout.GetEllipsisCount(commentEllipLine - 1);
                        if (ellipsisCount > 0)
                        {
                            BtnViewMoreClose.Visibility = ViewStates.Visible;
                        }
                    }

                    //LoadTokMoji
                    for (int z = 0; z < TokMojiDrawables.Count; z++)
                    {
                        var loopTokMojiID = ":" + TokMojiDrawables[z].TokmojiId + ":";
                        var indicesComment = spannableStringComment.ToString().IndexesOf(loopTokMojiID);
                        var indicesReply = spannableStringReply.ToString().IndexesOf(loopTokMojiID);

                        foreach (var index in indicesComment)
                        {
                            var set = true;
                            foreach (ImageSpan span in spannableStringComment.GetSpans(index, index + loopTokMojiID.Length, Java.Lang.Class.FromType(typeof(ImageSpan))))
                            {
                                if (spannableStringComment.GetSpanStart(span) >= index && spannableStringComment.GetSpanEnd(span) <= index + loopTokMojiID.Length)
                                    spannableStringComment.RemoveSpan(span);
                                else
                                {
                                    set = false;
                                    break;
                                }
                            }
                            if (set)
                            {
                                byte[] base64TokMoji = Convert.FromBase64String(TokMojiDrawables[i].TokmojImgBase64); 
                                Bitmap decodedByte = BitmapFactory.DecodeByteArray(base64TokMoji, 0, base64TokMoji.Length);
                                spannableStringComment.SetSpan(new ImageSpan(decodedByte), index, index + loopTokMojiID.Length, SpanTypes.ExclusiveExclusive);
                            }
                        }

                        foreach (var index in indicesReply)
                        {
                            var set = true;
                            foreach (ImageSpan span in spannableStringReply.GetSpans(index, index + loopTokMojiID.Length, Java.Lang.Class.FromType(typeof(ImageSpan))))
                            {
                                if (spannableStringReply.GetSpanStart(span) >= index && spannableStringReply.GetSpanEnd(span) <= index + loopTokMojiID.Length)
                                    spannableStringReply.RemoveSpan(span);
                                else
                                {
                                    set = false;
                                    break;
                                }
                            }
                            if (set)
                            {
                                byte[] base64TokMoji = Convert.FromBase64String(TokMojiDrawables[i].TokmojImgBase64);
                                Bitmap decodedByte = BitmapFactory.DecodeByteArray(base64TokMoji, 0, base64TokMoji.Length);
                                spannableStringReply.SetSpan(new ImageSpan(decodedByte), index, index + loopTokMojiID.Length, SpanTypes.ExclusiveExclusive);

                            }
                        }
                    }

                    txtEllipseComment.SetText(spannableStringComment, TextView.BufferType.Spannable);
                    CommentText.SetText(spannableStringComment, TextView.BufferType.Spannable);
                    EditCommentText.SetText(spannableStringComment, TextView.BufferType.Spannable);
                    ReplyUserDisplayText.SetText(spannableStringReply, TextView.BufferType.Spannable);
                }
            };
        }
        private void BindCommentsViewHolder(CachingViewHolder holder, ReactionModel comment, int position)
        {
            var EditCommentText = holder.FindCachedViewById<EditText>(Resource.Id.EditCommentRowContent);
            var BtnCancelComment = holder.FindCachedViewById<Button>(Resource.Id.BtnCancelComment);
            var BtnUpdateComment = holder.FindCachedViewById<Button>(Resource.Id.BtnUpdateComment);
            var PopUpMenuComments = holder.FindCachedViewById<TextView>(Resource.Id.lblCommentPopUpMenu);
            var ImgCommentUserPhoto = holder.FindCachedViewById<ImageView>(Resource.Id.imgcomment_userphoto);
            ImgCommentUserPhoto.ContentDescription = comment.UserId;
            Glide.With(this).Load(comment.UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(ImgCommentUserPhoto);

            var CommentorName = holder.FindCachedViewById<TextView>(Resource.Id.lbl_commentnameuser);
            CommentorName.Text = comment.UserDisplayName;
            CommentorName.ContentDescription = comment.UserId;

            BtnCancelComment.Tag = position;
            BtnUpdateComment.Tag = position;
            PopUpMenuComments.Tag = position;
            EditCommentText.Tag = position;

            holder.FindCachedViewById<TextView>(Resource.Id.lbl_commentdate).Text = DateConvert.ConvertToRelative(comment.CreatedTime).ToString();
            var kind = holder.FindCachedViewById<TextView>(Resource.Id.lblCommentRowKind);
            kind.Text = char.ToUpper(comment.Kind[0]) + comment.Kind.Substring(1);
            if (comment.Kind.ToLower() == "accurate")
            {
                kind.SetBackgroundColor(Color.DarkGreen);
            }
            else if (comment.Kind.ToLower() == "inaccurate")
            {
                kind.SetBackgroundColor(Color.Red);
            }
            else
            {
                kind.Visibility = ViewStates.Gone;
            }

            var BtnCommentReply = holder.FindCachedViewById<Button>(Resource.Id.BtnCommentReply);
            var BtnViewMoreClose = holder.FindCachedViewById<Button>(Resource.Id.btnViewMoreCloseComment);
            var CommentTextEllipsize = holder.FindCachedViewById<TextView>(Resource.Id.lblCommentRowContentEllip);
            var CommentText = holder.FindCachedViewById<TextView>(Resource.Id.lblCommentRowContent);
            CommentText.Text = comment.Text;
            CommentTextEllipsize.Text = comment.Text;
            EditCommentText.Text = comment.Text;

            BtnViewMoreClose.Click += delegate
            {
                if (CommentTextEllipsize.Visibility == ViewStates.Visible)
                {
                    CommentTextEllipsize.Visibility = ViewStates.Gone;
                    CommentText.Visibility = ViewStates.Visible;
                    BtnViewMoreClose.Text = "Close";
                }
                else
                {
                    CommentTextEllipsize.Visibility = ViewStates.Visible;
                    CommentText.Visibility = ViewStates.Gone;
                    BtnViewMoreClose.Text = "View more";
                }
            };

            var LinearTokInfoReplyPreview = holder.FindCachedViewById<LinearLayout>(Resource.Id.LinearTokInfoReplyPreview);
            var BtnShowHideComments = holder.FindCachedViewById<Button>(Resource.Id.btnShowHideComment);
            //var result = await ReactionService.Instance.GetCommentReplyAsync(new ReactionQueryValues() { reaction_id = comment.Id, kind = "comments", detail_number = -1, item_id = tokModel.Id, pagination_id = "" });
            //Settings.ContinuationToken = result.ContinuationToken;
            //var repliesResult = result.Results.ToList();

            //Show 1 reply for preview
            var CircleReplyUserPhoto = holder.FindCachedViewById<CircleImageView>(Resource.Id.CircleReplyUserPhoto);
            var ReplyUserDisplayName = holder.FindCachedViewById<TextView>(Resource.Id.TokInfoCommentsReplyUsername);
            var ReplyUserDisplayText = holder.FindCachedViewById<TextView>(Resource.Id.TokInfoCommentsReplyText);

            if (comment.Children != null)
            {
                if (comment.Children.Count > 0)
                {
                    LinearTokInfoReplyPreview.Visibility = ViewStates.Visible;
                    BtnShowHideComments.Text = "View " + comment.Children.Count + " replies";
                    Glide.With(this).Load(comment.Children[0].UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(CircleReplyUserPhoto);
                    ReplyUserDisplayName.Text = comment.Children[0].UserDisplayName;
                    ReplyUserDisplayText.Text = comment.Children[0].Text;
                }
                else
                {
                    LinearTokInfoReplyPreview.Visibility = ViewStates.Gone;
                    BtnShowHideComments.Visibility = ViewStates.Gone;
                }
            }

            BtnShowHideComments.Click += (object sender, EventArgs e) =>
            {
                GoToRepliesPage(comment,position);
            };

            LinearTokInfoReplyPreview.Click += delegate
            {
                GoToRepliesPage(comment,position);
            };

            BtnCommentReply.Click += (sender,e) =>
            {
                GoToRepliesPage(comment,position);
            };
        }
        private void GoToRepliesPage(ReactionModel comment, int position)
        {
            var tokmojiConvert = JsonConvert.SerializeObject(TokInfoVm.TokMojiCollection);
            var commentConvert = JsonConvert.SerializeObject(comment);
            var repliesConvert = JsonConvert.SerializeObject(comment.Children);
            LocalSettings.TokMojidrawable = JsonConvert.SerializeObject(TokMojiDrawables);

            Settings.ContinuationToken = comment.ChildrenToken;

            nextActivity = new Intent(this, typeof(TokInfoRepliesPageActivity));
            nextActivity.PutExtra("commentReaction", commentConvert);
            nextActivity.PutExtra("repliesCollection", repliesConvert);
            nextActivity.PutExtra("tokMojiCollection", tokmojiConvert);
            nextActivity.PutExtra("commentPosition", position);
            StartActivity(nextActivity);
        }
        public void UpdateReplies(int position, List<ReactionModel> listReplies)
        {
            View view = RecyclerComments.GetChildAt(position);
            //Show 1 reply for preview
            var LinearTokInfoReplyPreview = view.FindViewById<LinearLayout>(Resource.Id.LinearTokInfoReplyPreview);
            var BtnShowHideComments = view.FindViewById<Button>(Resource.Id.btnShowHideComment);
            var CircleReplyUserPhoto = view.FindViewById<CircleImageView>(Resource.Id.CircleReplyUserPhoto);
            var ReplyUserDisplayName = view.FindViewById<TextView>(Resource.Id.TokInfoCommentsReplyUsername);
            var ReplyUserDisplayText = view.FindViewById<TextView>(Resource.Id.TokInfoCommentsReplyText);

            if (listReplies.Count > 0)
            {
                LinearTokInfoReplyPreview.Visibility = ViewStates.Visible;
                BtnShowHideComments.Text = "View " + listReplies.Count + " replies";
                Glide.With(this).Load(listReplies[0].UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(CircleReplyUserPhoto);
                ReplyUserDisplayName.Text = listReplies[0].UserDisplayName;
                ReplyUserDisplayText.Text = listReplies[0].Text;
            }
        }
        private void BindTokMojiViewHolder(CachingViewHolder holder, Tokmoji tokmoji, int position)
        {
            var ImgTokMoji = holder.FindCachedViewById<ImageView>(Resource.Id.imgTokInfoTokMojiRow);
            Glide.With(this).Load(tokmoji.Image).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).FitCenter()).Into(ImgTokMoji);
            
            ImgTokMoji.ContentDescription = tokmoji.Id;
            ImgTokMoji.Click -= displayImageinCommentEditor;
            ImgTokMoji.Click += displayImageinCommentEditor;
        }
        private void BindTokMojiViewHolderDummy(CachingViewHolder holder, Tokmoji tokmoji, int position)
        {
            var ImgTokMoji = holder.FindCachedViewById<ImageView>(Resource.Id.imgTokInfoTokMojiRow);
            Glide.With(this).Load(tokmoji.Image).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).FitCenter()).Into(ImgTokMoji);

            ImgTokMoji.ContentDescription = tokmoji.Id;
            ImgTokMoji.Click -= displayImageinCommentEditor;
            ImgTokMoji.Click += displayImageinCommentEditor;
        }
        private void displayImageinCommentEditor(object sender, EventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder alertDiag = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertDiag.SetTitle("");
            alertDiag.SetIcon(Resource.Drawable.alert_icon_blue);
            alertDiag.SetMessage("This tokmoji costs 3 coins. Continue?");
            alertDiag.SetPositiveButton("Cancel", (senderAlert, args) => {
                alertDiag.Dispose();
            });
            alertDiag.SetNegativeButton(Html.FromHtml("<font color='#007bff'>Proceed</font>", FromHtmlOptions.ModeLegacy), async (senderAlert, args) => {
                
                Android.Text.ISpannable spannableTokMoji = new SpannableString((sender as ImageView).ContentDescription);
                int start = CommentEditor.SelectionStart;
                string tokmojiidx = (sender as ImageView).ContentDescription;
                string tokidx = ":" + tokmojiidx + ":";
                string spaceafter = tokidx + " ";

                //TokMoji Purchase
                txtProgressText.Text = "Purchasing...";
                linearProgress.Visibility = ViewStates.Visible;
                this.Window.AddFlags(WindowManagerFlags.NotTouchable);

                var result = await TokMojiService.Instance.PurchaseTokmojiAsync(tokmojiidx,"tokmoji");

                this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                linearProgress.Visibility = ViewStates.Gone;
                txtProgressText.Text = "Loading...";

                if (result.ResultEnum == Shared.Helpers.Result.Success)
                {
                    //Update Coins
                    Settings.UserCoins -= 3;
                    MainActivity.Instance.TextCoinsToast.Text = string.Format("{0:#,##0.##}", Settings.UserCoins);
                    profile_fragment.Instance.TextCoinsToast.Text = string.Format("{0:#,##0.##}", Settings.UserCoins);

                    SpannableString spannableString;
                    var resultObject = result.ResultObject as PurchasedTokmoji;

                    if (LinearMegaInaccurateComment.Visibility == ViewStates.Visible)
                    {
                        EditInaccurateComment.Text = EditInaccurateComment.Text.Substring(0, start) + spaceafter + EditInaccurateComment.Text.Substring(start);
                        spannableString = new SpannableString(EditInaccurateComment.Text);
                    }
                    else
                    {
                        CommentEditor.Text = CommentEditor.Text.Substring(0, start) + spaceafter + CommentEditor.Text.Substring(start);
                        spannableString = new SpannableString(CommentEditor.Text);
                    }

                    
                    for (int i = 0; i < TokMojiDrawables.Count; i++)
                    {
                        var loopTokMojiID = ":" + TokMojiDrawables[i].TokmojiId + ":";
                        var indices = spannableString.ToString().IndexesOf(loopTokMojiID);

                        foreach (var index in indices)
                        {
                            var set = true;
                            foreach (ImageSpan span in spannableString.GetSpans(index, index + loopTokMojiID.Length, Java.Lang.Class.FromType(typeof(ImageSpan))))
                            {
                                if (spannableString.GetSpanStart(span) >= index && spannableString.GetSpanEnd(span) <= index + loopTokMojiID.Length)
                                    spannableString.RemoveSpan(span);
                                else
                                {
                                    set = false;
                                    break;
                                }
                            }
                            if (set)
                            {
                                if (tokmojiidx == TokMojiDrawables[i].TokmojiId)
                                {
                                    TokMojiDrawables[i].PurchaseIds = resultObject.Id;
                                }

                                byte[] base64TokMoji = Convert.FromBase64String(TokMojiDrawables[i].TokmojImgBase64);
                                Bitmap decodedByte = BitmapFactory.DecodeByteArray(base64TokMoji, 0, base64TokMoji.Length);
                                spannableString.SetSpan(new ImageSpan(decodedByte), index, index + loopTokMojiID.Length, SpanTypes.ExclusiveExclusive);
                            }
                        }
                    }

                    if (LinearMegaInaccurateComment.Visibility == ViewStates.Visible)
                    {
                        EditInaccurateComment.SetText(spannableString, TextView.BufferType.Spannable);
                        EditInaccurateComment.SetSelection(start + spaceafter.Length);
                    }
                    else
                    {
                        CommentEditor.SetText(spannableString, TextView.BufferType.Spannable);
                        CommentEditor.SetSelection(start + spaceafter.Length);
                    }
                }
                else
                {
                    var dialog = new AlertDialog.Builder(this);
                    var alertDialog = dialog.Create();
                    alertDialog.SetTitle("");
                    alertDialog.SetIcon(Resource.Drawable.alert_icon_blue);
                    alertDialog.SetMessage("Not enough coins.");
                    alertDialog.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) =>{});
                    alertDialog.Show();
                    alertDialog.SetCanceledOnTouchOutside(false);
                }
            });
            Dialog diag = alertDiag.Create();
            diag.Show();
            diag.SetCanceledOnTouchOutside(false);
        }
        private async Task loadSections()
        {
            var getTokSectionsResult = await TokService.Instance.GetTokSectionsAsync(tokModel.Id);
            var getTokSections = getTokSectionsResult.Results;
            tokModel.Sections = getTokSections.ToArray();
            MegaTokSections();
        }
        private void AddTokDetails(int calledfromonactivityresult = 0)
        {
            linearParent.RemoveAllViews();
            if (calledfromonactivityresult == 1)
            {
                if (!tokModel.IsDetailBased)
                {
                    View view = LayoutInflater.Inflate(Resource.Layout.tokinfo_detail1, null);
                    var btnAccurate = view.FindViewById<Button>(Resource.Id.tokinfo_btnAccurate);
                    var btnInaccurate = view.FindViewById<Button>(Resource.Id.tokinfo_btnInaccurate);
                    var txtDetail = view.FindViewById<TextView>(Resource.Id.lbl_detail);

                    hashtagText = new SpannableString(tokModel.SecondaryFieldText);
                    Matcher matcher = Pattern.Compile(hashtagCode).Matcher(hashtagText);
                    while (matcher.Find())
                    {
                        hashtagText.SetSpan(new ClickableText(Actions.OpenHashTag, matcher.Group()), matcher.Start(), matcher.End(), 0);
                    }

                    txtDetail.Append(hashtagText);
                    txtDetail.MovementMethod = LinkMovementMethod.Instance;
                    //txtDetail.SetText(hashtagText, TextView.BufferType.Spannable);

                    btnAccurate.Visibility = ViewStates.Gone;
                    btnInaccurate.Visibility = ViewStates.Gone;
                    linearParent.AddView(view);

                    if (!tokModel.IsEnglish)
                    {
                        isEnglishLinear.RemoveAllViews();
                        View viewEnglish2 = LayoutInflater.Inflate(Resource.Layout.tokinfo_isenglish, null);
                        TextView txtSecEnglish = viewEnglish2.FindViewById<TextView>(Resource.Id.lbl_tokConvertAnswer1);
                        txtSecEnglish.Text = tokModel.EnglishSecondaryFieldText;
                        isEnglishLinear.AddView(viewEnglish2);
                    }
                }
            }

            if (tokModel.Details != null)
            {
                for (int i = 0; i < tokModel.Details.Length; i++)
                {
                    if (!string.IsNullOrEmpty(tokModel.Details[i]))
                    {
                        View view = LayoutInflater.Inflate(Resource.Layout.tokinfo_detail1, null);

                        var btnAccurate = view.FindViewById<Button>(Resource.Id.tokinfo_btnAccurate);
                        var btnInaccurate = view.FindViewById<Button>(Resource.Id.tokinfo_btnInaccurate);
                        var DetailDesc = view.FindViewById<TextView>(Resource.Id.lbl_detail);
                        var ImgDetail = view.FindViewById<ImageView>(Resource.Id.tokinfo_imgdetail);
                        var btnComment = view.FindViewById<Button>(Resource.Id.btnTokInfo_detailComment);

#if (_CLASSTOKS)
                        btnAccurate.Visibility = ViewStates.Gone;
                        btnInaccurate.Visibility = ViewStates.Gone;
#endif

                        btnComment.Tag = i + 1; //Add + 1 for detailnum OnClickAddReaction

                        btnAccurate.Typeface = font;
                        btnAccurate.Tag = i + 1; //Add + 1 for detailnum OnClickAddReaction
                        btnInaccurate.Typeface = font;
                        btnInaccurate.Tag = i;

                        hashtagText = new SpannableString(tokModel.Details[i] ?? "");
                        Matcher matcher = Pattern.Compile(hashtagCode).Matcher(hashtagText);
                        while (matcher.Find())
                        {
                            hashtagText.SetSpan(new ClickableText(Actions.OpenHashTag, matcher.Group()), matcher.Start(), matcher.End(), 0);
                        }

                        DetailDesc.Append("\u2022 " + hashtagText);
                        DetailDesc.MovementMethod = LinkMovementMethod.Instance;
                        //DetailDesc.SetText("\u2022 " + hashtagText, TextView.BufferType.Spannable);

#if (_CLASSTOKS)
                         btnAccurate.Visibility = ViewStates.Gone;
                         btnInaccurate.Visibility = ViewStates.Gone;
#endif

                        if (tokModel.DetailImages != null)
                        {
                            if (i < tokModel.DetailImages.Length)
                            {
                                if (URLUtil.IsValidUrl(tokModel.DetailImages[i]))
                                {
                                    Glide.With(this).Load(tokModel.DetailImages[i]).Into(ImgDetail);
                                }
                                else
                                {
                                    if (tokModel.DetailImages[i] != null)
                                    {
                                        tokModel.DetailImages[i] = tokModel.DetailImages[i].Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", "");
                                        byte[] imageDetailBytes = Convert.FromBase64String(tokModel.DetailImages[i]);
                                        ImgDetail.SetImageBitmap((BitmapFactory.DecodeByteArray(imageDetailBytes, 0, imageDetailBytes.Length)));
                                    }
                                }
                            }
                        }

                        linearParent.AddView(view);
                    }
                }
            }
        }
        private void MegaTokSections()
        {
            linearParent.RemoveAllViews();
            for (int i = 0; i < tokModel.Sections.Length; i++)
            {
                View view = LayoutInflater.Inflate(Resource.Layout.tokinfo_megasectiondetail, null);
                //Button
                var btnEdit = view.FindViewById<Button>(Resource.Id.btnTokInfoMegaEditDtl);
                var btnView = view.FindViewById<Button>(Resource.Id.btnTokInfoMegaViewDtl);
                var btnRemove = view.FindViewById<Button>(Resource.Id.btnTokInfoMegaRemoveDtl);
                //tags
                btnEdit.Tag = i;
                btnView.Tag = i;
                btnRemove.Tag = i;

                btnEdit.Typeface = font;
                btnView.Typeface = font;
                btnRemove.Typeface = font;
                //Button End
                var MegaNumber = view.FindViewById<TextView>(Resource.Id.lbl_tokinfo_megaNumber);
                var Title = view.FindViewById<TextView>(Resource.Id.lbl_tokinfo_megaTitle);
                var Content = view.FindViewById<TextView>(Resource.Id.lbl_tokinfo_megaContent);
                var ImgDetail = view.FindViewById<ImageView>(Resource.Id.tokinfo_imgmegadetail);

                MegaNumber.Text = (i + 1).ToString();
                Title.Text = tokModel.Sections[i].Title ?? "";
                Content.Text = tokModel.Sections[i].Content ?? "";

                if (URLUtil.IsValidUrl(tokModel.Sections[i].Image))
                {
                    Glide.With(this).Load(tokModel.Sections[i].Image).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).FitCenter()).Into(ImgDetail);
                }
                else
                {
                    if (tokModel.Sections[i].Image != null)
                    {
                        byte[] imageDetailBytes = Convert.FromBase64String(tokModel.Sections[i].Image);
                        ImgDetail.SetImageBitmap((BitmapFactory.DecodeByteArray(imageDetailBytes, 0, imageDetailBytes.Length)));
                    }
                }

                linearParent.AddView(view);
            }
        }
        private async Task LoadSelectedGems(string reaction = "All", string Id = "", long Index = 0)
        {
            reactionValueVM = new ReactionValueViewModel();

            reactionValue = await ReactionService.Instance.GetReactionsValueAsync(Id);
            if (reactionValue.GemsModel != null)
            {
                reactionValueVM.GemA = (long)GetPropValue(reactionValue.GemsModel, "GemA" + (Index == 0 ? "" : Index.ToString()));
                reactionValueVM.GemB = (long)GetPropValue(reactionValue.GemsModel, "GemB" + (Index == 0 ? "" : Index.ToString()));
                reactionValueVM.GemC = (long)GetPropValue(reactionValue.GemsModel, "GemC" + (Index == 0 ? "" : Index.ToString()));
            }

            if (reactionValue.CommentsModel != null)
            {
                reactionValueVM.Accurate = (long)GetPropValue(reactionValue.CommentsModel, "Accurate" + (Index == 0 ? "" : Index.ToString()));
                reactionValueVM.Inaccurate = (long)GetPropValue(reactionValue.CommentsModel, "Inaccurate" + (Index == 0 ? "" : Index.ToString()));
            }

            if (reactionValue.ViewsModel != null)
            {
                //if (Settings.GetUserModel().UserId == )
                TextTotalViews.Text = (reactionValue.ViewsModel.TileTapViews + reactionValue.ViewsModel.TileTapViewsPersonal + reactionValue.ViewsModel.PageVisitViews).ToString();
                ProgressViews.Visibility = ViewStates.Gone;

                TextToolTotalViews.Text = "Total Views: " + TextTotalViews.Text;
                TextTotalOpened.Text = "Tok was opened: " + reactionValue.ViewsModel.TileTapViews.ToString();
                TextTotalVisited.Text = "Tok was visited: " + reactionValue.ViewsModel.PageVisitViews.ToString();
                TextTotalOpenedByOwner.Text = "Tok was opened by its owner: " + reactionValue.ViewsModel.TileTapViewsPersonal.ToString();
                TextTotalVisitedByOwner.Text = "Tok was visited by its owner: " + reactionValue.ViewsModel.PageViewsPersonal.ToString();
            }

            var getUserReaction = await TokService.Instance.UserReactionsGet(tokModel.Id);
            foreach (var item in getUserReaction.Results)
            {
                var kind = item.Kind.ToLower();
                int i = Convert.ToInt32(item.DetailNum);
                if (i == 0)
                {
                    //#B6B6B6 Gray
                    if (kind == "gema")
                    {
                        GreenGemContainer.TooltipText = "selected";
                        GreenGemHeader.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);
                        ReactionImgGreen.SetBackgroundColor(Color.YellowGreen);
                        GreenGemFooter.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);

                        YellowGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                        ReactionImgYellow.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                        YellowGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                        RedGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                        ReactionImgRed.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                        RedGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                        YellowGemContainer.Enabled = false;
                        RedGemContainer.Enabled = false;
                    }
                    else if (kind == "gemb")
                    {
                        YellowGemContainer.TooltipText = "selected";
                        YellowGemHeader.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);
                        ReactionImgYellow.SetBackgroundColor(Color.YellowGreen);
                        YellowGemFooter.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);

                        GreenGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                        ReactionImgGreen.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                        GreenGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                        RedGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                        ReactionImgRed.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                        RedGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                        GreenGemContainer.Enabled = false;
                        RedGemContainer.Enabled = false;
                    }
                    else if (kind == "gemc")
                    {
                        RedGemContainer.TooltipText = "selected";
                        RedGemHeader.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);
                        ReactionImgRed.SetBackgroundColor(Color.YellowGreen);
                        RedGemFooter.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);

                        GreenGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                        ReactionImgGreen.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                        GreenGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                        YellowGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                        ReactionImgYellow.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                        YellowGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                        GreenGemContainer.Enabled = false;
                        YellowGemContainer.Enabled = false;
                    }
                    else if (kind == "accurate")
                    {
                        //reactionValueVM.Accurate = reactionValueVM.Accurate + 1;
                        BtnMegaAccurate.SetBackgroundColor(Color.YellowGreen);
                    }
                    else if (kind == "inaccurate")
                    {
                        isInaccurateAdded = true;
                        EditInaccurateComment.Text = item.Text;
                        BtnMegaInaccurate.SetBackgroundColor(Color.YellowGreen);
                        //reactionValueVM.Inaccurate = reactionValueVM.Inaccurate + 1;
                    }
                }
                else
                {
                    View view = linearParent.GetChildAt(i - 1);

                    var btnAccurate = view.FindViewById<Button>(Resource.Id.tokinfo_btnAccurate);
                    var btnInaccurate = view.FindViewById<Button>(Resource.Id.tokinfo_btnInaccurate);
                    var txtComment = view.FindViewById<EditText>(Resource.Id.EditTokInfo_detailcomment);
                    var linearComment = view.FindViewById<LinearLayout>(Resource.Id.linearTokInfo_detailComment);
                    linearComment.Visibility = ViewStates.Gone;

                    txtComment.Text = item.Text;

                    if (kind == "accurate")
                    {
                        reactionValueVM.Accurate = reactionValueVM.Accurate + 1;
                        btnAccurate.SetBackgroundColor(Color.YellowGreen);
                    }
                    else if (kind == "inaccurate")
                    {
                        isInaccurateAdded = true;
                        btnInaccurate.SetBackgroundColor(Color.Red);
                        btnInaccurate.SetTextColor(Color.White);
                        reactionValueVM.Inaccurate = reactionValueVM.Inaccurate + 1;
                    }
                }
            }

            TotalGreen.Text = (reactionValueVM.GemA * 5).ToKMB();
            TotalYellow.Text = (reactionValueVM.GemB * 10).ToKMB();
            TotalRed.Text = (reactionValueVM.GemC * 15).ToKMB();
            TotalAccurate.Text = (reactionValueVM.Accurate * 5).ToKMB();
            TotalInaccurate.Text = Math.Abs(reactionValueVM.Inaccurate * -10).ToKMB();

            OverallTotalReactions.Text = ((reactionValueVM.GemA * 5) + (reactionValueVM.GemB * 10) + (reactionValueVM.GemC * 15) + (reactionValueVM.Accurate * 5) + (reactionValueVM.Inaccurate * -10)).ToKMB();
            OverallTotalReactionsDisplay.Text = OverallTotalReactions.Text;
        }
        private void ShowGemsCollClicked(object sender, EventArgs e)
        {
            if (tokModel.IsMegaTok == true || tokModel.TokGroup.ToLower() == "mega") //If Mega
            {
            }
            else
            {
                for (int i = 0; i < linearParent.ChildCount; i++)
                {
                    View childview = linearParent.GetChildAt(i);
                    var LinearComment = childview.FindViewById<LinearLayout>(Resource.Id.linearTokInfo_detailComment);
                    if (LinearComment.Visibility == ViewStates.Visible)
                    {
                        LinearComment.Visibility = ViewStates.Gone;
                    }
                }
            }
            GemsParentContainer.Visibility = ViewStates.Visible;
            PurpleGemContainer.Visibility = ViewStates.Visible;
        }
        [Java.Interop.Export("OnClickParentToCancel")]
        public void OnClickParentToCancel(View v)
        {
            PurpleGemContainer.Visibility = ViewStates.Gone;
            GemsParentContainer.Visibility = ViewStates.Gone;
            FrameViews.Visibility = ViewStates.Gone;
            LinearMegaInaccurateComment.Visibility = ViewStates.Gone;
            NestedComment.Visibility = ViewStates.Visible;
        }
        [Java.Interop.Export("OnClickPopUpMenuComments")]
        public void OnClickPopUpMenuComments(View v)
        {
            int position = (int)v.Tag;
            string message = "";
            Android.Widget.PopupMenu menu = new Android.Widget.PopupMenu(this, v);
            // Call inflate directly on the menu:
            menu.Inflate(Resource.Menu.comment_popmenu);
            var report = menu.Menu.FindItem(Resource.Id.itemReport);
            var edit = menu.Menu.FindItem(Resource.Id.itemEdit);
            var delete = menu.Menu.FindItem(Resource.Id.itemDelete);

            if (Settings.GetTokketUser().Id == TokInfoVm.CommentsCollection[position].UserId)
            {
                edit.SetVisible(true);
                delete.SetVisible(true);
                report.SetVisible(false);
            }
            else
            {
                edit.SetVisible(false);
                delete.SetVisible(false);
                report.SetVisible(true);
            }
            
            // A menu item was clicked:
            menu.MenuItemClick += async(s1, arg1) => {
                switch (arg1.Item.TitleFormatted.ToString().ToLower())
                {
                    case "report":
                        break;
                    case "edit":
                        NestedComment.Visibility = ViewStates.Gone;
                        View view = RecyclerComments.GetChildAt(position);
                        var BtnCommentReply = view.FindViewById<Button>(Resource.Id.BtnCommentReply);
                        var LinearEditComment = view.FindViewById<LinearLayout>(Resource.Id.LinearEditComment);
                        var EditCommentText = view.FindViewById<EditText>(Resource.Id.EditCommentRowContent);
                        var CommentTextEllipsize = view.FindViewById<TextView>(Resource.Id.lblCommentRowContentEllip);
                        var CommentText = view.FindViewById<TextView>(Resource.Id.lblCommentRowContent);

                        LinearEditComment.Visibility = ViewStates.Visible;
                        CommentTextEllipsize.Visibility = ViewStates.Gone;
                        CommentText.Visibility = ViewStates.Gone;
                        BtnCommentReply.Visibility = ViewStates.Gone;

                        break;
                    case "delete":
                        linearProgress.Visibility = ViewStates.Visible;
                        this.Window.AddFlags(WindowManagerFlags.NotTouchable);
                        txtProgressText.Text = "Deleting a comment...";
                        var result = await ReactionService.Instance.DeleteReaction(TokInfoVm.CommentsCollection[position].Id);
                        linearProgress.Visibility = ViewStates.Gone;
                        this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                        txtProgressText.Text = "Loading...";

                        if (result)
                        {
                            message = "Comment deleted.";
                            TokInfoVm.CommentsCollection.RemoveAt(position);
                            adapterComments = TokInfoVm.CommentsCollection.GetRecyclerAdapter(BindCommentsViewHolder, Resource.Layout.tokinfo_comments_row);
                            RecyclerComments.SetAdapter(adapterComments);

                            TextTotalComments.Text = "Comments: " + TokInfoVm.CommentsCollection.Count.ToString();
                        }
                        else
                        {
                            message = "Could not delete comment";
                        }

                        var dialogresult = new AlertDialog.Builder(this);
                        var alertResult = dialogresult.Create();
                        alertResult.SetTitle("");
                        alertResult.SetIcon(Resource.Drawable.alert_icon_blue);
                        alertResult.SetMessage(message);
                        alertResult.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                        alertResult.Show();
                        alertResult.SetCanceledOnTouchOutside(false);
                        break;
                }
            };

            // Menu was dismissed:
            menu.DismissEvent += (s2, arg2) => {
                //Console.WriteLine("menu dismissed");
            };

            menu.Show();
        }
        [Java.Interop.Export("OnClickInaccurate")]
        public void OnClickInaccurate(View v)
        {
            int ndx = 0;
            try { ndx = (int)v.Tag; } catch { ndx = int.Parse((string)v.Tag); }

            View view = linearParent.GetChildAt(ndx);

            var linearComment = view.FindViewById<LinearLayout>(Resource.Id.linearTokInfo_detailComment);
            if (linearComment.Visibility == ViewStates.Visible)
            {
                linearComment.Visibility = ViewStates.Gone;
            }
            else
            {
                //tokinfo_btnInaccurate
                if (v.Background is ColorDrawable || view.FindViewById<Button>(Resource.Id.tokinfo_btnAccurate).Background is ColorDrawable)
                {
                    /*if (((ColorDrawable)v.Background).Color != Color.Red) //Check if reaction is already added based on the button background color
                    {
                        //linearComment.Visibility = ViewStates.Visible;
                    }*/
                }
                else
                {
                    linearComment.Visibility = ViewStates.Visible;
                }
            }
        }
        [Java.Interop.Export("OnClickInaccurateMega")]
        public void OnClickInaccurateMega(View v)
        {
            if (LinearMegaInaccurateComment.Visibility == ViewStates.Visible)
            {
                LinearMegaInaccurateComment.Visibility = ViewStates.Gone;
                NestedComment.Visibility = ViewStates.Visible;
            }
            else
            {
                LinearMegaInaccurateComment.Visibility = ViewStates.Visible;
                GemsParentContainer.Visibility = ViewStates.Visible;
                NestedComment.Visibility = ViewStates.Gone;
            }
        }

        [Java.Interop.Export("OnClickTokInfoCommenter")]
        public void OnClickTokInfoCommenter(View v)
        {
            string commentorid = v.ContentDescription;
            nextActivity = new Intent(this, typeof(ProfileUserActivity));
            nextActivity.PutExtra("userid", commentorid);
            this.StartActivity(nextActivity);
        }

        private async Task OnClickAddReaction(View v)
        {
            txtProgressText.Text = "Loading...";
            string message = "", titlemssg = "", gemValues = "", comment = "";
            decimal gemcost = 0; bool isAddReaction = true;

            var kind = v.ContentDescription.ToString();
            int ndx = 0;
            try { ndx = (int)v.Tag; } catch { ndx = int.Parse((string)v.Tag); }
            
            ReactionModel tokkepediaReaction = new ReactionModel();
            tokkepediaReaction.ItemId = tokModel.Id;
            tokkepediaReaction.Kind = kind.ToLower();
            tokkepediaReaction.Label = "reaction";
            tokkepediaReaction.DetailNum = ndx;
            tokkepediaReaction.CategoryId = tokModel.CategoryId;
            tokkepediaReaction.TokTypeId = tokModel.TokTypeId;
            tokkepediaReaction.OwnerId = tokModel.UserId;
            
            //#B6B6B6 Gray
            if (kind == "gema")
            {
                gemValues = "Valuable";
                reactionValueVM.GemA = reactionValueVM.GemA + 1;
                reactionValueVM.All = reactionValueVM.All + 1;
                gemcost = 0;

                GreenGemHeader.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);
                ReactionImgGreen.SetBackgroundColor(Color.YellowGreen);
                GreenGemFooter.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);

                YellowGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                ReactionImgYellow.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                YellowGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                RedGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                ReactionImgRed.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                RedGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                
                YellowGemContainer.Enabled = false;
                RedGemContainer.Enabled = false;
                GreenGemContainer.TooltipText = "selected";
            }
            else if (kind == "gemb")
            {
                gemValues = "Brilliant";
                reactionValueVM.GemB = reactionValueVM.GemB + 1;
                reactionValueVM.All = reactionValueVM.All + 1;
                gemcost = 5;

                YellowGemHeader.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);
                ReactionImgYellow.SetBackgroundColor(Color.YellowGreen);
                YellowGemFooter.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);

                GreenGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                ReactionImgGreen.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                GreenGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                RedGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                ReactionImgRed.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                RedGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                GreenGemContainer.Enabled = false;
                RedGemContainer.Enabled = false;
                YellowGemContainer.TooltipText = "selected";
            }
            else if (kind == "gemc")
            {
                gemValues = "Precious";
                reactionValueVM.GemC = reactionValueVM.GemC + 1;
                reactionValueVM.All = reactionValueVM.All + 1;
                gemcost = 10;

                RedGemHeader.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);
                ReactionImgRed.SetBackgroundColor(Color.YellowGreen);
                RedGemFooter.Background.SetColorFilter(Color.ParseColor("#9acd32"), PorterDuff.Mode.SrcAtop);

                GreenGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                ReactionImgGreen.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                GreenGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                YellowGemHeader.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);
                ReactionImgYellow.SetBackgroundColor(Color.ParseColor("#B6B6B6"));
                YellowGemFooter.Background.SetColorFilter(Color.ParseColor("#B6B6B6"), PorterDuff.Mode.SrcAtop);

                GreenGemContainer.Enabled = false;
                YellowGemContainer.Enabled = false;
                RedGemContainer.TooltipText = "selected";
            }
            else if (kind == "comment")
            {
                txtProgressText.Text = "Adding a comment...";
                comment = CommentEditor.Text;
                CommentEditor.Text = "";

                tokkepediaReaction.Text = comment; //Comment
                tokkepediaReaction.UserDisplayName = Settings.GetUserModel().DisplayName;
                tokkepediaReaction.UserPhoto = Settings.GetUserModel().UserPhoto;
                tokkepediaReaction.Timestamp = DateTime.Now;
                tokkepediaReaction.IsComment = true;
                
                ////Comment below because it causes to duplicate in display
                ////Show temporarily
                //TokInfoVm.CommentsCollection.Insert(0, tokkepediaReaction);
                //adapterComments = TokInfoVm.CommentsCollection.GetRecyclerAdapter(BindCommentsViewHolder, Resource.Layout.tokinfo_comments_row);
                //RecyclerComments.SetAdapter(adapterComments);
            }
            else
            {
                bool isMarkReaction = true;
                if (ndx == 0) //Mega
                {
                    isAddReaction = !isInaccurateAdded; //Unable to add reaction when inaccurate = true;
                    if (isInaccurateAdded == false) 
                    {
                        comment = EditInaccurateComment.Text;
                        tokkepediaReaction.Text = EditInaccurateComment.Text;
                        LinearMegaInaccurateComment.Visibility = ViewStates.Gone;

                        if (kind == "accurate")
                        {
                            BtnMegaAccurate.SetBackgroundColor(Color.YellowGreen);
                        }
                        else if (kind == "inaccurate")
                        {
                            BtnInaccurateComment.SetBackgroundColor(Color.Red);
                            BtnInaccurateComment.SetTextColor(Color.White);
                        }
                    }
                    else
                    {
                        isMarkReaction = false;
                        var dialognetwork = new AlertDialog.Builder(this);
                        var alertnetwork = dialognetwork.Create();
                        alertnetwork.SetTitle("Error!");
                        alertnetwork.SetMessage("Inaccurate has already been given.");
                        alertnetwork.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                        alertnetwork.Show();
                        alertnetwork.SetCanceledOnTouchOutside(false);
                    }
                }
                else //detail
                {
                    View view = linearParent.GetChildAt(ndx - 1);

                    var btnAccurate = view.FindViewById<Button>(Resource.Id.tokinfo_btnAccurate);
                    var btnInaccurate = view.FindViewById<Button>(Resource.Id.tokinfo_btnInaccurate);
                    var txtComment = view.FindViewById<EditText>(Resource.Id.EditTokInfo_detailcomment);
                    var linearComment = view.FindViewById<LinearLayout>(Resource.Id.linearTokInfo_detailComment);
                    linearComment.Visibility = ViewStates.Gone;

                    comment = txtComment.Text;
                    tokkepediaReaction.Text = txtComment.Text;

                    if (kind == "accurate")
                    {
                        if (btnAccurate.Background is ColorDrawable || btnInaccurate.Background is ColorDrawable)
                        {
                            //if background have already a color, disregard
                            isMarkReaction = false;
                            isAddReaction = false;
                        }
                        else
                        {
                            btnAccurate.SetBackgroundColor(Color.YellowGreen);
                        }
                    }
                    else if (kind == "inaccurate")
                    {
                        btnInaccurate.SetBackgroundColor(Color.Red);
                        btnInaccurate.SetTextColor(Color.White);
                    }
                }

                if (isMarkReaction)
                {
                    if (kind == "accurate")
                    {
                        reactionValueVM.Accurate = reactionValueVM.Accurate + 1;
                        reactionValueVM.All = reactionValueVM.All + 1;
                    }
                    else if (kind == "inaccurate")
                    {
                        reactionValueVM.Inaccurate = reactionValueVM.Inaccurate + 1;
                        reactionValueVM.All = reactionValueVM.All + 1;
                        isInaccurateAdded = true;
                        txtProgressText.Text = "Marking as inaccurate...";

                        //Hide Keyboard
                        var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                        inputManager.HideSoftInputFromWindow(BtnSmile.WindowToken, HideSoftInputFlags.None);
                    }
                }
            }

            //API
            
            if (isAddReaction)
            {
                linearProgress.Visibility = ViewStates.Visible;
                this.Window.AddFlags(WindowManagerFlags.NotTouchable);
            }

            if (!string.IsNullOrEmpty(gemValues))
            {
                //Check if user have enough coins to purchase
                TokketUser tokketUser = await SharedAccount.AccountService.Instance.GetUserAsync(Settings.GetUserModel().UserId);
                Settings.TokketUser = JsonConvert.SerializeObject(tokketUser);

                if (tokketUser.Coins < gemcost)
                {
                    linearProgress.Visibility = ViewStates.Gone;
                    this.Window.ClearFlags(WindowManagerFlags.NotTouchable);

                    //Show error message not enough coins
                    isAddReaction = false;

                    var dialognetwork = new Android.App.AlertDialog.Builder(this);
                    var alertnetwork = dialognetwork.Create();
                    alertnetwork.SetTitle("Could not give gem!");
                    alertnetwork.SetMessage("Not enough coins.");
                    alertnetwork.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                    alertnetwork.Show();
                    alertnetwork.SetCanceledOnTouchOutside(false);
                }
            }

            if (isAddReaction)
            {
                var result = await ReactionService.Instance.AddReaction(tokkepediaReaction);

                linearProgress.Visibility = ViewStates.Gone;
                this.Window.ClearFlags(WindowManagerFlags.NotTouchable);

                if (v.TooltipText == "selected")
                {
                    if (result.ResultEnum == Shared.Helpers.Result.Success)
                    {
                        message = "You have given a " + gemValues.ToLower() + " to this tok.";

                        //Gem Reaction is added. Changes have made in this tok
                        tokModel.HasGemReaction = true;
                        changesMade = true;
                    }
                    else
                    {
                        message = "Could not give gem.";
                        titlemssg = "Error!";

                        //If error, deduct the 
                        if (gemValues.ToLower() == "valuable")
                        {
                            reactionValueVM.GemA = reactionValueVM.GemA - 1;
                            reactionValueVM.All = reactionValueVM.All - 1;
                        }
                        else if (gemValues.ToLower() == "brilliant")
                        {
                            reactionValueVM.GemB = reactionValueVM.GemB - 1;
                            reactionValueVM.All = reactionValueVM.All +- 1;
                        }
                        else if (gemValues.ToLower() == "precious")
                        {
                            reactionValueVM.GemC = reactionValueVM.GemC - 1;
                            reactionValueVM.All = reactionValueVM.All - 1;
                        }
                    }
                    
                    var dialogresult = new AlertDialog.Builder(this);
                    var alertResult = dialogresult.Create();
                    alertResult.SetTitle(titlemssg);
                    alertResult.SetIcon(Resource.Drawable.alert_icon_blue);
                    alertResult.SetMessage(message);
                    alertResult.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                    alertResult.Show();
                    alertResult.SetCanceledOnTouchOutside(false);
                }

                //OverallTotalReactions.Text = reactionValueVM.All.ToKMB();
                TotalGreen.Text = (reactionValueVM.GemA * 5).ToKMB();
                TotalYellow.Text = (reactionValueVM.GemB * 10).ToKMB();
                TotalRed.Text = (reactionValueVM.GemC * 15).ToKMB();
                TotalAccurate.Text = (reactionValueVM.Accurate * 5).ToKMB();
                TotalInaccurate.Text = Math.Abs((reactionValueVM.Inaccurate * -10)).ToKMB();

                OverallTotalReactions.Text = ((reactionValueVM.GemA * 5) + (reactionValueVM.GemB * 10) + (reactionValueVM.GemC * 15) + (reactionValueVM.Accurate * 5) + (reactionValueVM.Inaccurate * -10)).ToKMB();
                OverallTotalReactionsDisplay.Text = OverallTotalReactions.Text;

                if (kind == "comment" && ndx == 0) //Main Comment
                {
                    if (result.ResultEnum == Shared.Helpers.Result.Success)
                    {
                        message = "Comment added.";
                    }
                    else
                    {
                        message = "An error occurred. Please refresh the comment section.";
                    }

                    var dialogresult = new AlertDialog.Builder(this);
                    var alertResult = dialogresult.Create();
                    alertResult.SetTitle("");
                    alertResult.SetIcon(Resource.Drawable.alert_icon_blue);
                    alertResult.SetMessage(message);
                    alertResult.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                    alertResult.Show();
                    alertResult.SetCanceledOnTouchOutside(false);
                }

                if (result.ResultEnum == Shared.Helpers.Result.Success)
                {
                    var serObject = JsonConvert.SerializeObject(result.ResultObject);
                    var resObject = JsonConvert.DeserializeObject<ResultModel>(serObject);
                    tokkepediaReaction.Id = resObject.ResultObject.ToString();

                    //Add or show the commenter
                    tokkepediaReaction.UserDisplayName = Settings.GetUserModel().DisplayName;
                    tokkepediaReaction.UserId = Settings.GetUserModel().UserId;
                    tokkepediaReaction.UserPhoto = Settings.GetUserModel().UserPhoto;

                    TokInfoVm.CommentsCollection.Insert(0, tokkepediaReaction);
                    adapterComments = TokInfoVm.CommentsCollection.GetRecyclerAdapter(BindCommentsViewHolder, Resource.Layout.tokinfo_comments_row);
                    RecyclerComments.SetAdapter(adapterComments);
                    
                    TextTotalComments.Text = "Comments: " + TokInfoVm.CommentsCollection.Count.ToString();
                }
            }
        }

        [Java.Interop.Export("OnClickTokInfoMegaEdit")]
        public void OnClickTokInfoMegaEdit(View v)
        {
            int ndx = (int)v.Tag;
            nextActivity = new Intent(this, typeof(AddSectionPage));
            nextActivity.PutExtra("SectionNo", ndx + 1);
            nextActivity.PutExtra("isAddSection", 1);
            nextActivity.PutExtra("tokModel", JsonConvert.SerializeObject(tokModel));
            nextActivity.PutExtra("tokSection", JsonConvert.SerializeObject(tokModel.Sections[ndx]));
            nextActivity.SetFlags(ActivityFlags.NewTask);
            nextActivity.SetFlags(ActivityFlags.ReorderToFront);
            StartActivityForResult(nextActivity, (int)ActivityType.AddSectionPage);
        }

        [Java.Interop.Export("OnClickTokInfoMegaView")]
        public void OnClickTokInfoMegaView(View v)
        {
            int ndx = (int)v.Tag;
            nextActivity = new Intent(this, typeof(AddSectionPage));
            nextActivity.PutExtra("SectionNo", ndx + 1);
            nextActivity.PutExtra("isAddSection", 2);
            nextActivity.PutExtra("tokModel", JsonConvert.SerializeObject(tokModel));
            nextActivity.PutExtra("tokSection", JsonConvert.SerializeObject(tokModel.Sections[ndx]));
            nextActivity.SetFlags(ActivityFlags.NewTask);
            nextActivity.SetFlags(ActivityFlags.ReorderToFront);
            StartActivity(nextActivity);
        }

        [Java.Interop.Export("OnClickTokInfoMegaRemove")]
        public void OnClickTokInfoMegaRemove(View v)
        {
            int ndx = (int)v.Tag;

            Android.Support.V7.App.AlertDialog.Builder alertDiag = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertDiag.SetTitle("Confirm");
            alertDiag.SetMessage("Are you sure you want to delete this tok?");
            alertDiag.SetPositiveButton("Cancel", (senderAlert, args) => {
                alertDiag.Dispose();
            });
            alertDiag.SetNegativeButton(Html.FromHtml("<font color='#dc3545'>Delete</font>", FromHtmlOptions.ModeLegacy), async (senderAlert, args) => {
                string message = "";
                txtProgressText.Text = "Deleting...";
                linearProgress.Visibility = ViewStates.Visible;
                this.Window.AddFlags(WindowManagerFlags.NotTouchable);
                var issuccess = await TokService.Instance.DeleteTokSectionAsync(tokModel.Sections[ndx]); //API
                this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                linearProgress.Visibility = ViewStates.Gone;

                if (issuccess)
                {
                    message = "Deleted the section successfully!";
                }
                else
                {
                    message = "Failed to delete section!";
                }

                var detail = tokModel.Sections.FirstOrDefault(a => a.Id == tokModel.Sections[ndx].Id);
                if (detail != null)
                {
                    var dialogDelete = new AlertDialog.Builder(this);
                    var alertDelete = dialogDelete.Create();
                    alertDelete.SetTitle("");
                    alertDelete.SetIcon(Resource.Drawable.alert_icon_blue);
                    alertDelete.SetMessage(message);
                    alertDelete.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) =>
                    {
                        var sectionList = tokModel.Sections.ToList();
                        sectionList.Remove(tokModel.Sections[ndx]);
                        tokModel.Sections = sectionList.ToArray();
                        MegaTokSections();
                    });
                    alertDelete.Show();
                    alertDelete.SetCanceledOnTouchOutside(false);
                }
            });
            Dialog diag = alertDiag.Create();
            diag.Show();
            diag.SetCanceledOnTouchOutside(false);
        }
        [Java.Interop.Export("OnClickTokBack")]
        public void OnClickTokBack(View v)
        {
            var modelConvert = JsonConvert.SerializeObject(tokModel);
            Intent nextActivity = new Intent(this, typeof(TokBackActivity));
            nextActivity.PutExtra("tokModel", modelConvert);
            MainActivity.Instance.StartActivity(nextActivity);
        }
        [Java.Interop.Export("OnClickCancelComment")]
        public void OnClickCancelComment(View v)
        {
            int position = (int)v.Tag;
            View view = RecyclerComments.GetChildAt(position);
            var LinearEditComment = view.FindViewById<LinearLayout>(Resource.Id.LinearEditComment);
            var EditCommentText = view.FindViewById<EditText>(Resource.Id.EditCommentRowContent);
            var BtnCancelComment = view.FindViewById<Button>(Resource.Id.BtnCancelComment);
            var BtnUpdateComment = view.FindViewById<Button>(Resource.Id.BtnUpdateComment);
            var CommentTextEllipsize = view.FindViewById<TextView>(Resource.Id.lblCommentRowContentEllip);
            var CommentText = view.FindViewById<TextView>(Resource.Id.lblCommentRowContent);

            LinearEditComment.Visibility = ViewStates.Gone;
            CommentTextEllipsize.Visibility = ViewStates.Visible;
            CommentText.Visibility = ViewStates.Gone;
            NestedComment.Visibility = ViewStates.Visible;
        }
        [Java.Interop.Export("OnClickUpdateComment")]
        public async void OnClickUpdateComment(View v)
        {
            int position = (int)v.Tag;
            View view = RecyclerComments.GetChildAt(position);
            var BtnCommentReply = view.FindViewById<Button>(Resource.Id.BtnCommentReply);
            var LinearEditComment = view.FindViewById<LinearLayout>(Resource.Id.LinearEditComment);
            var EditCommentText = view.FindViewById<EditText>(Resource.Id.EditCommentRowContent);
            var BtnCancelComment = view.FindViewById<Button>(Resource.Id.BtnCancelComment);
            var BtnUpdateComment = view.FindViewById<Button>(Resource.Id.BtnUpdateComment);
            var CommentTextEllipsize = view.FindViewById<TextView>(Resource.Id.lblCommentRowContentEllip);
            var CommentText = view.FindViewById<TextView>(Resource.Id.lblCommentRowContent);

            txtProgressText.Text = "Updating...";
            linearProgress.Visibility = ViewStates.Visible;
            this.Window.AddFlags(WindowManagerFlags.NotTouchable);

            ReactionModel updateReaction = TokInfoVm.CommentsCollection[position];
            updateReaction = TokInfoVm.CommentsCollection[position];
            updateReaction.Text = EditCommentText.Text;
            if (updateReaction._etag == null)
            {
                //400 or bad request is returned when newly added comment and comment is edited, this makes an invalid partitionkey. so set below
                updateReaction.PartitionKey = updateReaction.ItemId + "-reactions0";
            }
            var ResultUpdate = await ReactionService.Instance.UpdateReaction(updateReaction);

            linearProgress.Visibility = ViewStates.Gone;
            this.Window.ClearFlags(WindowManagerFlags.NotTouchable);

            if (ResultUpdate)
            {
                TokInfoVm.CommentsCollection.Remove(TokInfoVm.CommentsCollection[position]);
                TokInfoVm.CommentsCollection.Insert(position, updateReaction);
                CommentTextEllipsize.Text = EditCommentText.Text;
                CommentText.Text = EditCommentText.Text;
                BtnCommentReply.Visibility = ViewStates.Visible;
                LinearEditComment.Visibility = ViewStates.Gone;
                CommentTextEllipsize.Visibility = ViewStates.Visible;
                NestedComment.Visibility = ViewStates.Visible;
            }            
        }
        
        [Java.Interop.Export("OnClickImageToView")]
        public void OnClickImageToView(View v)
        {
            ParentImageViewer.TranslationY = ViewDummyForTouch.TranslationY;

            //ImgUserImageView.SetBackgroundColor(ManipulateColor.manipulateColor(GListener.mColorPalette, 0.62f));
            //ImgUserImageView.SetImageDrawable(tokinfo_imgMain.Drawable);

            ImgUserImageView.SetBackgroundColor(GetDominantColor.GetDominantColorImg(((BitmapDrawable)(v as ImageView).Drawable).Bitmap, 0.62f));
            ImgUserImageView.SetImageDrawable((v as ImageView).Drawable);

            imgScale = ImgUserImageView.Scale;

            ParentImageViewer.Visibility = ViewStates.Visible;
            Animation myAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.fab_scaleup);
            ParentImageViewer.StartAnimation(myAnim);
        }
        [Java.Interop.Export("OnClickCloseImgView")]
        public void OnClickCloseImgView(View v)
        {
            ParentImageViewer.Visibility = ViewStates.Gone;

            this.SupportActionBar.Show();
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.tokinfo_menu, menu);

            MenuCompat.SetGroupDividerEnabled(menu, true);
            var tokinfo_addtoktoset = menu.FindItem(Resource.Id.tokinfo_addtoktoset);
            var tokinfo_addtilesticker = menu.FindItem(Resource.Id.tokinfo_addtilesticker);
            var tokinfo_addsection = menu.FindItem(Resource.Id.tokinfomenu_addsection);
            var tokinfo_share = menu.FindItem(Resource.Id.tokinfo_share);
            var tokinfo_edit = menu.FindItem(Resource.Id.tokinfo_edit);
            var tokinfo_delete = menu.FindItem(Resource.Id.tokinfo_delete);
            var tokinfo_report = menu.FindItem(Resource.Id.tokinfo_report);
            var tokinfo_removefromgroup = menu.FindItem(Resource.Id.tokinfo_removefromgroup);
            tokinfo_removefromgroup.SetVisible(false);
            if (classTokModel != null)
            {
                if (!string.IsNullOrEmpty(classTokModel.GroupId) && tokModel.UserId == Settings.GetUserModel().UserId)
                {
                    tokinfo_removefromgroup.SetVisible(true);
                }
            }

            if (tokModel.UserId == Settings.GetUserModel().UserId)
            {
                tokinfo_addtoktoset.SetVisible(true);
                tokinfo_addtilesticker.SetVisible(true);

                //if tok is mega, set visibility to true
                if (tokModel.IsMegaTok == true || tokModel.TokGroup.ToLower() == "mega") //If Mega
                {
                    tokinfo_addsection.SetVisible(true);
                }
                else
                {
                    tokinfo_addsection.SetVisible(false);
                }

                tokinfo_share.SetVisible(true);
                tokinfo_edit.SetVisible(true);
                tokinfo_delete.SetVisible(true);
                tokinfo_report.SetVisible(false);
            }
            else
            {
                tokinfo_addtoktoset.SetVisible(true);
                tokinfo_addtilesticker.SetVisible(false);
                tokinfo_addsection.SetVisible(false);
                tokinfo_share.SetVisible(true);
                tokinfo_edit.SetVisible(false);
                tokinfo_delete.SetVisible(false);
                tokinfo_report.SetVisible(true);
            }

            return true;
        }
       
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        private void setResult()
        {
#if (_TOKKEPEDIA)
            Intent = new Intent();
            Intent.PutExtra("changesMade", changesMade);
            Intent.PutExtra("tokModel", JsonConvert.SerializeObject(tokModel));
            SetResult(Android.App.Result.Ok, Intent);
            Finish();
#endif
#if (_CLASSTOKS)
           /* Intent = new Intent();
            Intent.PutExtra("changesMade", changesMade);
            Intent.PutExtra("classtokModel", JsonConvert.SerializeObject(classTokModel));
            SetResult(Android.App.Result.Ok, Intent);
            Finish();*/
#endif
        }
        public override void OnBackPressed()
        {
            setResult();
            base.OnBackPressed();
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    setResult();
                    Finish();
                    break;
                case Resource.Id.tokinfo_share:
                    RunOnUiThread(async () => await Share.RequestAsync(new ShareTextRequest
                    {
                        Uri = Shared.Config.Configurations.Url + "tok/" + item.ItemId,
                        Title = tokModel.PrimaryFieldName
                    }));
                    
                    break;
                case Resource.Id.tokinfo_addtoktoset:
                    Settings.ActivityInt = Convert.ToInt16(ActivityType.TokInfo);
                    nextActivity = new Intent(this, typeof(MySetsActivity));
                    nextActivity.PutExtra("isAddToSet", true);
                    nextActivity.PutExtra("TokTypeId", tokModel.TokTypeId);
                    nextActivity.PutExtra("tokModel", JsonConvert.SerializeObject(tokModel));
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    StartActivity(nextActivity);
                    break;
                case Resource.Id.tokinfo_addtilesticker:
                    nextActivity = new Intent(this, typeof(AddStickerDialogActivity));
                    nextActivity.PutExtra("tokModel", JsonConvert.SerializeObject(tokModel));
                    StartActivityForResult(nextActivity, (int)ActivityType.AddStickerDialogActivity);
                    break;
                case Resource.Id.tokinfomenu_addsection:
                    nextActivity = new Intent(this, typeof(AddSectionPage));
                    nextActivity.PutExtra("SectionNo", linearParent.ChildCount + 1);
                    nextActivity.PutExtra("tokModel", JsonConvert.SerializeObject(tokModel));
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    StartActivityForResult(nextActivity, (int)ActivityType.AddSectionPage);
                    break;
                case Resource.Id.tokinfo_edit:
#if (_CLASSTOKS)
                    nextActivity = new Intent(this, typeof(AddClassTokActivity));
                    nextActivity.PutExtra("isSave", false);
                    nextActivity.PutExtra("ClassTokModel", JsonConvert.SerializeObject(classTokModel));
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    StartActivityForResult(nextActivity, (int)ActivityType.AddTokActivityType);
#endif
#if (_TOKKEPEDIA)
                    nextActivity = new Intent(this, typeof(AddTokActivity));
                    nextActivity.PutExtra("isAddTok", false);
                    nextActivity.PutExtra("tokModel", JsonConvert.SerializeObject(tokModel));
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    StartActivityForResult(nextActivity, (int)ActivityType.AddTokActivityType);
#endif
                    break;
                case Resource.Id.tokinfo_delete:
                    Android.Support.V7.App.AlertDialog.Builder alertDiag = new Android.Support.V7.App.AlertDialog.Builder(this);
                    alertDiag.SetTitle("Confirm");
                    alertDiag.SetMessage("Are you sure you want to delete this tok?");
                    alertDiag.SetPositiveButton("Cancel", (senderAlert, args) =>
                    {
                        alertDiag.Dispose();
                    });
                    alertDiag.SetNegativeButton(Html.FromHtml("<font color='#dc3545'>Delete</font>", FromHtmlOptions.ModeLegacy), async (senderAlert, args) =>
                    {
                        //Set Visibility of ProgressBar to Visible
                        txtProgressText.Text = "Deleting...";
                        linearProgress.Visibility = ViewStates.Visible;
                        this.Window.AddFlags(WindowManagerFlags.NotTouchable);

#if (_CLASSTOKS)
                        var result = await ClassService.Instance.DeleteClassToksAsync(classTokModel.Id, classTokModel.PartitionKey);
#endif

#if (_TOKKEPEDIA)
                        var result = await TokService.Instance.DeleteTokAsync(tokModel.Id);
#endif
                        //Set Visibility of ProgressBar to Gone
                        this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                        linearProgress.Visibility = ViewStates.Gone;

                        var builder = new Android.App.AlertDialog.Builder(this);
                        builder.SetMessage("Deleted Successfully!");
                        builder.SetTitle("");
                        var dialog = (Android.App.AlertDialog)null;
                        builder.SetPositiveButton("OK" ?? "OK", (d, index) =>
                        {
                            Intent = new Intent();
                            Intent.PutExtra("isDeleted", true);

#if (_CLASSTOKS)
                            Intent.PutExtra("classtokModel", JsonConvert.SerializeObject(classTokModel));
#endif

#if (_TOKKEPEDIA)
                    Intent.PutExtra("tokModel", JsonConvert.SerializeObject(tokModel));
#endif
                            SetResult(Android.App.Result.Ok, Intent);
                            Finish();
                        });
                        dialog = builder.Create();
                        dialog.Show();
                        dialog.SetCanceledOnTouchOutside(false);
                    });
                    Dialog diag = alertDiag.Create();
                    diag.Show();
                    diag.SetCanceledOnTouchOutside(false);
                    break;
                case Resource.Id.tokinfo_removefromgroup:
                    Android.Support.V7.App.AlertDialog.Builder alertDiagcg = new Android.Support.V7.App.AlertDialog.Builder(this);
                    alertDiagcg.SetTitle("Confirm");
                    alertDiagcg.SetMessage("Are you sure you want to remove this from a group?");
                    alertDiagcg.SetPositiveButton("Cancel", (senderAlert, args) =>
                    {
                        alertDiagcg.Dispose();
                    });
                    alertDiagcg.SetNegativeButton(Html.FromHtml("<font color='#dc3545'>Remove</font>", FromHtmlOptions.ModeLegacy), async (senderAlert, args) =>
                    {
                        //Set Visibility of ProgressBar to Visible
                        txtProgressText.Text = "Removing...";
                        linearProgress.Visibility = ViewStates.Visible;
                        this.Window.AddFlags(WindowManagerFlags.NotTouchable);

                        classTokModel.GroupId = "";
                        var resultClassTokModel = await ClassService.Instance.UpdateClassToksAsync(classTokModel);

                        //Set Visibility of ProgressBar to Gone
                        this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                        linearProgress.Visibility = ViewStates.Gone;

                        var objBuilder = new AlertDialog.Builder(this);
                        objBuilder.SetTitle("");
                        objBuilder.SetMessage(resultClassTokModel.ResultEnum.ToString());
                        objBuilder.SetCancelable(false);

                        AlertDialog objDialog = objBuilder.Create();
                        objDialog.SetButton((int)(DialogButtonType.Positive), "OK", (s, ev) =>
                        {
                            if (resultClassTokModel.ResultEnum == Shared.Helpers.Result.Success)
                            {
                                classtoks_fragment.Instance.RemoveClassTokCollection(classTokModel);
                                Finish();
                            }
                        });
                        objDialog.Show();
                        objDialog.SetCanceledOnTouchOutside(false);
                    });
                    Dialog diagcg = alertDiagcg.Create();
                    diagcg.Show();
                    diagcg.SetCanceledOnTouchOutside(false);

                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            
            if ((requestCode == (int)ActivityType.AddSectionPage) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                var tokdataSection = data.GetStringExtra("toksection");
                if (tokdataSection != null)
                {
                    var toksection = JsonConvert.DeserializeObject<TokSection>(tokdataSection);
                    if (toksection != null)
                    {
                        var ListTokSections = new List<TokSection>();
                        ListTokSections = tokModel.Sections.ToList();

                        var result = ListTokSections.FirstOrDefault(c => c.Id == toksection.Id);
                        if (result != null) //If Edit
                        {
                            int ndx = ListTokSections.IndexOf(result);
                            ListTokSections.Remove(result);

                            ListTokSections.Insert(ndx, toksection);
                        }
                        else //if save
                        {
                            ListTokSections.Add(toksection);
                        }

                        tokModel.Sections = ListTokSections.ToArray();
                        MegaTokSections();
                    }
                }
            }
            else if ((requestCode == (int)ActivityType.AddTokActivityType) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                tokModel = new TokModel();
                tokModel = classTokModel;
                if (classTokModel != null)
                {
                    changesMade = true; //If there are changes made in AddTokActivity
                    if (classTokModel.IsMegaTok == true || classTokModel.TokGroup.ToLower() == "mega") //If Mega
                    {
                        var tokdataSection = data.GetStringExtra("toksection");
                        if (tokdataSection != null)
                        {
                            var toksection = JsonConvert.DeserializeObject<List<TokSection>>(tokdataSection);
                            if (toksection != null)
                            {
                                var ListTokSections = new List<TokSection>();
                                ListTokSections = tokModel.Sections.ToList();
                                foreach (var sec in toksection)
                                {
                                    var result = ListTokSections.FirstOrDefault(c => c.Id == sec.Id);
                                    if (result != null) //If Edit
                                    {
                                        int ndx = ListTokSections.IndexOf(result);
                                        ListTokSections.Remove(result);

                                        ListTokSections.Insert(ndx, sec);
                                    }
                                    else //if save
                                    {
                                        ListTokSections.Add(sec);
                                    }
                                }

                                classTokModel.Sections = ListTokSections.ToArray();
                                tokModel.Sections = classTokModel.Sections;
                                MegaTokSections();
                            }
                        }
                    }
                    else
                    {
                        var tokModelstring = data.GetStringExtra("classtokModel");
                        if (tokModelstring != null)
                        {
                            classTokModel = JsonConvert.DeserializeObject<ClassTokModel>(tokModelstring);
                            tokModel = classTokModel;
                            FillUpFields();
                            AddTokDetails(1);
                        }
                    }
                }
            }
            else if ((requestCode == (int)ActivityType.AddStickerDialogActivity) && (resultCode == Android.App.Result.Ok))
            {
                var dataTokModelstr = data.GetStringExtra("tokModel");
                if (dataTokModelstr != null)
                {
                    var dataTokModel = JsonConvert.DeserializeObject<TokModel>(dataTokModelstr);
                    if (dataTokModel != null)
                    {
                        tokModel.Sticker = dataTokModel.Sticker;
                        tokModel.StickerImage = dataTokModel.StickerImage;
                        Glide.With(this).Load(dataTokModel.StickerImage).Into(StickerImage);
                        StickerImage.Visibility = ViewStates.Visible;
                    }
                }
            }
        }
        [Java.Interop.Export("OnFrameRTClicked")]
        public void OnFrameRTClicked(View v)
        {
            Animation myAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.fab_scaledown);
            GridReactionTable.StartAnimation(myAnim);
            FrameReactionTable.Visibility = ViewStates.Gone;
        }
        void OnTokButtonClick(object sender, EventArgs e)
        {
            //Filter for class toks
            FilterBy filterByClass = FilterBy.None;
            List<string> filterItems = new List<string>();
            //==== end ====

            string titlepage = "";
            string filter = "";
            string headerpage =(sender as TextView).Text;

            if ((int)(sender as TextView).Tag == (int)Toks.Category)
            {
                filter = tokModel.CategoryId;
#if (_CLASSTOKS)
                filterByClass = FilterBy.Category;
#endif

#if (_TOKKEPEDIA)
                Settings.FilterTag = 3;
                titlepage = "Category";
#endif
            }
            else if ((int)(sender as TextView).Tag == (int)Toks.TokGroup)
            {
                filter = tokModel.TokGroup;
#if (_CLASSTOKS)

#endif

#if (_TOKKEPEDIA)
                Settings.FilterTag = 6;
                titlepage = "Tok Group";
#endif
            }
            else if ((int)(sender as TextView).Tag == (int)Toks.TokType)
            {
                filter = tokModel.TokTypeId;
#if (_CLASSTOKS)
                filterByClass = FilterBy.Class;
#endif

#if (_TOKKEPEDIA)
                Settings.FilterTag = 1;
                titlepage = "Tok Type";
#endif
            }

#if (_TOKKEPEDIA)
            Intent nextActivity = new Intent(this, typeof(ToksActivity));
            nextActivity.PutExtra("titlepage", titlepage);
            nextActivity.PutExtra("filter", filter);
            nextActivity.PutExtra("headerpage", headerpage);
            nextActivity.SetFlags(ActivityFlags.ReorderToFront);
            nextActivity.SetFlags(ActivityFlags.NewTask);
            this.StartActivity(nextActivity);
#endif
#if (_CLASSTOKS)

            filterItems.Add(filter);

            Settings.FilterByTypeHome = (int)filterByClass;
            Settings.FilterByItemsHome = JsonConvert.SerializeObject(filterItems);

            Settings.FilterByTypeProfile = (int)filterByClass;
            Settings.FilterByItemsProfile = JsonConvert.SerializeObject(filterItems);

            //classtoks_fragment.Instance.filter = filter;
            //search_fragment.Instance.toksfilter = filter;
            RunOnUiThread(async () => await classtoks_fragment.Instance.InitializeData());
            RunOnUiThread(async () => await profile_fragment.Instance.LoadToks()); //(filter)
            search_fragment.Instance.searchButtonClicked(sender, e);
            this.Finish();
#endif
        }

        private void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                //TextNothingFound.SetTextColor(Color.White);
                //TextNothingFound.SetBackgroundColor(Color.Transparent);
                //TextNothingFound.Visibility = ViewStates.Gone;

                //Data Refresh Place  
                BackgroundWorker work = new BackgroundWorker();
                work.DoWork += Work_DoWork;
                work.RunWorkerCompleted += Work_RunWorkerCompleted;
                work.RunWorkerAsync();
            }
            else
            {
                //TextNothingFound.Text = "No Internet Connection!";
                //TextNothingFound.SetTextColor(Color.White);
                //TextNothingFound.SetBackgroundColor(Color.Black);
                //TextNothingFound.Visibility = ViewStates.Visible;
                swipeRefreshComment.Refreshing = false;
            }
        }
        private void Work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            swipeRefreshComment.Refreshing = false;
        }
        private void Work_DoWork(object sender, DoWorkEventArgs e)
        {
            this.RunOnUiThread(async () => await LoadComments("", true));
            Thread.Sleep(3000);
        }

        private void ReactionTableTouched(object sender, View.TouchEventArgs e)
        {
            gesturedetector.OnTouchEvent(e.Event);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            if (e.Action == MotionEventActions.Up)
            {
                if (v.ContentDescription == "ToolTipViews")
                {
                }
                else
                {
                    if (v.TooltipText == "selected")
                    {
                        var objBuilder = new AlertDialog.Builder(this);
                        objBuilder.SetTitle("");
                        objBuilder.SetMessage("You have already given this tok a gem.");
                        objBuilder.SetCancelable(false);

                        AlertDialog objDialog = objBuilder.Create();
                        objDialog.SetButton((int)(DialogButtonType.Positive), "OK", (s, ev) => { });
                        objDialog.Show();
                        objDialog.SetCanceledOnTouchOutside(false);
                    }
                    else
                    {
                        RunOnUiThread(async () => await OnClickAddReaction(v));
                       ;
                    }
                }
            }
            else
            {
                if (v.ContentDescription == "ToolTipViews")
                {
                    FrameViews.Visibility = ViewStates.Visible;
                    GemsParentContainer.Visibility = ViewStates.Visible;
                }
            }
            return true;
        }

        private class MyGestureListener : GestureDetector.SimpleOnGestureListener
        {
            private TokInfoActivity tokInfoActivity;

            public MyGestureListener(TokInfoActivity Activity)
            {
                tokInfoActivity = Activity;
            }

            public override bool OnDoubleTap(MotionEvent e)
            {
                //Console.WriteLine("Double Tab");
                return true;
            }
            public override bool OnSingleTapUp(MotionEvent e)
            {

                var modelConvert = JsonConvert.SerializeObject(tokInfoActivity.reactionValueVM);
                tokInfoActivity.nextActivity = new Intent(tokInfoActivity, typeof(ReactionValuesActivity));
                tokInfoActivity.nextActivity.PutExtra("reactionValueVM", modelConvert);
                tokInfoActivity.nextActivity.PutExtra("tokId", tokInfoActivity.tokModel.Id);
                tokInfoActivity.StartActivity(tokInfoActivity.nextActivity);
                return true;
            }

            public override void OnLongPress(MotionEvent e)
            {
                tokInfoActivity.FrameReactionTable.Visibility = ViewStates.Visible;
                Animation myAnim = AnimationUtils.LoadAnimation(tokInfoActivity, Resource.Animation.fab_scaleup);
                tokInfoActivity.GridReactionTable.StartAnimation(myAnim);
            }
            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                return base.OnFling(e1, e2, velocityX, velocityY);
            }

            public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
            {
                return base.OnScroll(e1, e2, distanceX, distanceY);
            }
        }
        private int mSlop;
        private float mDownX;
        private float mDownY;
        private bool mSwiping;
        public override bool DispatchTouchEvent(MotionEvent ev)
        {
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
                        
                        if (ParentImageViewer.Visibility == ViewStates.Visible)
                        {
                            int quarterHeight = ParentImageViewer.Height / 4;
                            float currentPosition = ParentImageViewer.TranslationY;
                            if (currentPosition < -quarterHeight)
                            {
                                ParentImageViewer.Visibility = ViewStates.Gone;
                                this.SupportActionBar.Show();
                            }
                            else if (currentPosition > quarterHeight)
                            {
                                ParentImageViewer.Visibility = ViewStates.Gone;
                                this.SupportActionBar.Show();
                            }
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
#region UI Properties
        public RecyclerView RecyclerTokMojisInaccurate => FindViewById<RecyclerView>(Resource.Id.tokinfoRecyclerTokMojisInaccurate);
        public ExpandableLinearLayout ExpandedTokMojiInaccurate => FindViewById<ExpandableLinearLayout>(Resource.Id.expandedTokInfoTokMojiInaccurate);
        public ImageView BtnSmileInaccurate => FindViewById<ImageView>(Resource.Id.btnTokInfoSmileyInaccurate);
        public ImageView BtnSmile => FindViewById<ImageView>(Resource.Id.btnTokInfoSmiley);
        public RecyclerView RecyclerTokMojis => FindViewById<RecyclerView>(Resource.Id.tokinfoRecyclerTokMojis);
        public RecyclerView RecyclerTokMojisDummy => FindViewById<RecyclerView>(Resource.Id.tokinfoRecyclerTokMojisDummy);
        public TextView TotalGreen => FindViewById<TextView>(Resource.Id.reacttable_totalgreen);
        public TextView TotalYellow => FindViewById<TextView>(Resource.Id.reacttable_totalyellow);
        public TextView TotalRed => FindViewById<TextView>(Resource.Id.reacttable_totalred);
        public TextView TotalAccurate => FindViewById<TextView>(Resource.Id.reacttable_totalcheck);
        public TextView TotalInaccurate => FindViewById<TextView>(Resource.Id.reacttable_totalwrong);
        public TextView OverallTotalReactions => FindViewById<TextView>(Resource.Id.reacttable_overalltotal);
        public TextView txtUserDisplayName => FindViewById<TextView>(Resource.Id.lbl_tokinfonameuser);
        public ImageView imgUserPhoto => FindViewById<ImageView>(Resource.Id.img_tokUserPhoto);
        public ImageView imgcomment_userphoto => FindViewById<ImageView>(Resource.Id.imgcomment_userphoto);
        public ImageView StickerImage => FindViewById<ImageView>(Resource.Id.imgtokinfo_stickerimage);
        public LinearLayout isEnglishLinear => FindViewById<LinearLayout>(Resource.Id.linear_ConvertTopic);
        public LinearLayout linear_Reactions => FindViewById<LinearLayout>(Resource.Id.linear_Reactions);
        public LinearLayout linearParent => FindViewById<LinearLayout>(Resource.Id.linear_Topic);
        public TextView lbl_detail => FindViewById<TextView>(Resource.Id.lbl_detail);
        public TextView tokcategory => FindViewById<TextView>(Resource.Id.lblTokInfoTokCategory);
        public TextView tokgroup => FindViewById<TextView>(Resource.Id.lblTokInfoTokGroup);
        public TextView toktype => FindViewById<TextView>(Resource.Id.lblTokInfoTokType);
        public ImageView tokinfo_imgMain => FindViewById<ImageView>(Resource.Id.tokinfo_imgMain);
        public TextView ReactionCheck => FindViewById<TextView>(Resource.Id.reactiontable_check);
        public TextView ReactionWrong => FindViewById<TextView>(Resource.Id.reactiontable_wrong);
        public FrameLayout FrameReactionBtn => FindViewById<FrameLayout>(Resource.Id.frame_btntokinfoReaction);
        public FrameLayout FrameReactionTable => FindViewById<FrameLayout>(Resource.Id.framelayoutRL_reactiontable);
        public GridLayout GridReactionTable => FindViewById<GridLayout>(Resource.Id.gridtokinfo_reactiontable);
        public ImageView ImgPurpleGem => FindViewById<ImageView>(Resource.Id.tokinfo_purplegem);
        public FrameLayout PurpleGemContainer => FindViewById<FrameLayout>(Resource.Id.tokinfo_purplegemscontainer);
        public LinearLayout GreenGemContainer => FindViewById<LinearLayout>(Resource.Id.tokinfo_linear_greengem);
        public LinearLayout YellowGemContainer => FindViewById<LinearLayout>(Resource.Id.tokinfo_linear_yellowgem);
        public LinearLayout RedGemContainer => FindViewById<LinearLayout>(Resource.Id.tokinfo_linear_redgem);
        public LinearLayout TreasureContainer => FindViewById<LinearLayout>(Resource.Id.tokinfo_linear_treasure);
        public LinearLayout GemsParentContainer => FindViewById<LinearLayout>(Resource.Id.LinearGemsParentContainer);
        public TextView GreenGemHeader => FindViewById<TextView>(Resource.Id.greengemheader);
        public TextView TextTotalViews => FindViewById<TextView>(Resource.Id.lblTokInfoViews);
        public ProgressBar ProgressViews => FindViewById<ProgressBar>(Resource.Id.circleprogressViews);
        public Button BtnTokInfoEyeIcon => FindViewById<Button>(Resource.Id.btnTokInfoEyeIcon);
        public TextView GreenGemFooter => FindViewById<TextView>(Resource.Id.greengemfooter);
        public TextView YellowGemHeader => FindViewById<TextView>(Resource.Id.yellowgemheader);
        public TextView YellowGemFooter => FindViewById<TextView>(Resource.Id.yellowgemfooter);
        public TextView RedGemHeader => FindViewById<TextView>(Resource.Id.redgemheader);
        public TextView RedGemFooter => FindViewById<TextView>(Resource.Id.redgemfooter);
        public TextView TreasureHeader => FindViewById<TextView>(Resource.Id.treasureheader);
        public TextView TreasureFooter => FindViewById<TextView>(Resource.Id.treasurefooter);
        public ImageView ReactionImgGreen => FindViewById<ImageView>(Resource.Id.tokinfo_imggreenreaction);
        public ImageView ReactionImgYellow => FindViewById<ImageView>(Resource.Id.tokinfo_imgyellowreaction);
        public ImageView ReactionImgRed => FindViewById<ImageView>(Resource.Id.tokinfo_imgredreaction);
        public ImageView ReactionImgTreasure => FindViewById<ImageView>(Resource.Id.tokinfo_imgtreasurereaction);
        public RecyclerView RecyclerComments => FindViewById<RecyclerView>(Resource.Id.tokinfo_comments_recyclerView);
        public NestedScrollView NestedScroll => FindViewById<NestedScrollView>(Resource.Id.NestedScrollTokInfo);
        public NestedScrollView NestedComment => FindViewById<NestedScrollView>(Resource.Id.NestedTokInfoComment);
        public EditText CommentEditor => FindViewById<EditText>(Resource.Id.tokinfo_txtComment);
        public TextView TokDateTimeCreated => FindViewById<TextView>(Resource.Id.lbl_tokDateCreated);
        public ImageButton TokBackButton => FindViewById<ImageButton>(Resource.Id.btnTokInfoTokBack);
        public ShimmerLayout ShimmerCommentsList => FindViewById<ShimmerLayout>(Resource.Id.ShimmerTokInfoComments);
        public ProgressBar CircleProgress => FindViewById<ProgressBar>(Resource.Id.circleprogressComments);
        public ExpandableLinearLayout ExpandedTokMoji => FindViewById<ExpandableLinearLayout>(Resource.Id.expandedTokInfoTokMoji);
        public FrameLayout FrameViews => FindViewById<FrameLayout>(Resource.Id.frame_tokViews);
        public TextView TextToolTotalViews => FindViewById<TextView>(Resource.Id.TextTotalViews);
        public TextView TextTotalOpened => FindViewById<TextView>(Resource.Id.TextTotalOpened);
        public TextView TextTotalVisited => FindViewById<TextView>(Resource.Id.TextTotalVisited);
        public TextView TextTotalOpenedByOwner => FindViewById<TextView>(Resource.Id.TextTotalOpenedByOwner);
        public TextView TextTotalVisitedByOwner => FindViewById<TextView>(Resource.Id.TextTotalVisitedByOwner); 
        public TextView OverallTotalReactionsDisplay => FindViewById<TextView>(Resource.Id.lbltokinfo_totalReactions); 
        public TextView TextTotalComments => FindViewById<TextView>(Resource.Id.TextTotalComments);
        public ProgressBar ProgressComments => FindViewById<ProgressBar>(Resource.Id.circleprogressComments);
        public Button BtnMegaAccurate => FindViewById<Button>(Resource.Id.tokinfo_btnMegaAccurate);
        public Button BtnMegaInaccurate => FindViewById<Button>(Resource.Id.tokinfo_btnMegaInaccurate);
        public LinearLayout LinearMegaInaccurateComment => FindViewById<LinearLayout>(Resource.Id.LinearMegaInaccurateComment);
        public EditText EditInaccurateComment => FindViewById<EditText>(Resource.Id.EditInaccurateComment);
        public Button BtnInaccurateComment => FindViewById<Button>(Resource.Id.BtnInaccurateComment);
        public ImageView btnTokInfo_SendComment => FindViewById<ImageView>(Resource.Id.btnTokInfo_SendComment);
        public LinearLayout LinearTokInfoReaction => FindViewById<LinearLayout>(Resource.Id.LinearTokInfoReaction);
        public TextView LabelTokType => FindViewById<TextView>(Resource.Id.LabelTokType);
        public TextView LabelTokGroup => FindViewById<TextView>(Resource.Id.LabelTokGroup);
        public PhotoView ImgUserImageView => FindViewById<PhotoView>(Resource.Id.ImgProfileImageView);
        public RelativeLayout ParentImageViewer => FindViewById<RelativeLayout>(Resource.Id.ParentImageViewer);
        public View ViewDummyForTouch => FindViewById<View>(Resource.Id.ViewDummyForTouch);
        public LinearLayout linearProgress => FindViewById<LinearLayout>(Resource.Id.linearProgress);
        public TextView txtProgressText => FindViewById<TextView>(Resource.Id.txtProgressText);
        public SwipeRefreshLayout swipeRefreshComment => FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshComment);
        #endregion
    }
}
public class ClickableText : ClickableSpan
{
    public Actions ClickedAction;
    public string ActionText;

    public ClickableText(Actions ac, string at)
    {
        ClickedAction = ac;
        ActionText = at;
    }

    public override void OnClick(Android.Views.View widget)
    {
        widget.Invalidate();

        //(widget as TextView).SetHighlightColor(Color.ParseColor("#cde9fc"));

        /* var handler = new Handler();
         handler.PostDelayed(() =>
         {
             (widget as TextView).SetHighlightColor(Color.Transparent);
         }, 30L);*/

        if (ClickedAction == Actions.OpenHashTag)
        {
            //(widget as TextView).SetHighlightColor(Color.ParseColor("#cde9fc"));

            var nextActivity = new Intent(TokInfoActivity.Instance, typeof(HashtagActivity));
            nextActivity.PutExtra("hashtag", ActionText);
            TokInfoActivity.Instance.StartActivity(nextActivity);

            //(widget as TextView).SetHighlightColor(Color.Transparent);
        }
    }
    public override void UpdateDrawState(TextPaint ds)
    {
        base.UpdateDrawState(ds);
        //paint.setUnderlineText(true); // set underline if you want to underline

        //ds.BgColor = Color.ParseColor("#cde9fc");
        ds.Color = Color.ParseColor("#3498db"); // set the color to blue
    }
}