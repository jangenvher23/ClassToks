using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Com.Github.Aakira.Expandablelayout;
using GalaSoft.MvvmLight.Helpers;
using Newtonsoft.Json;
using Supercharge;
using Tokkepedia.Helpers;
using Tokkepedia.Model;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Xamarin.Essentials;
using static Android.Support.Constraints.ConstraintLayout;
using Color = Android.Graphics.Color;

namespace Tokkepedia
{

#if (_CLASSTOKS)
    [Activity(Label = "Replies", Theme = "@style/CustomAppThemeBlue")]
#endif
#if (_TOKKEPEDIA)
    [Activity(Label = "Replies", Theme = "@style/CustomAppTheme")]
#endif
    public class TokInfoRepliesPageActivity : BaseActivity
    {
        int loadedreplies = 0, commentPosition = 0;
        ReactionModel commentReaction;
        Intent nextActivity;
        List<TokMojiDrawableModel> TokMojiDrawables;
        ObservableCollection<Tokmoji> TokMojiCollection;
        ObservableCollection<ReactionModel> RepliesCollection;
        ObservableRecyclerAdapter<ReactionModel, CachingViewHolder> adapterReplies;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.tokinfo_replies_page);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            swipeRefresh.Refresh += RefreshLayout_Refresh;

            commentPosition = Intent.GetIntExtra("commentPosition", 0);
            commentReaction = JsonConvert.DeserializeObject<ReactionModel>(Intent.GetStringExtra("commentReaction"));
            var repliesList = JsonConvert.DeserializeObject<List<ReactionModel>>(Intent.GetStringExtra("repliesCollection"));
            TokMojiDrawables = LocalSettings.GetTokMojiDrawable();
            RepliesCollection = new ObservableCollection<ReactionModel>();
            if (repliesList != null)
            {
                foreach (var itemReply in repliesList)
                {
                    RepliesCollection.Add(itemReply);
                }
            }
            var repliesLayoutManager = new GridLayoutManager(this, 1);
            RecyclerTokreplies.SetLayoutManager(repliesLayoutManager);
            SetRepliesAdapter();

            TokMojiCollection = new ObservableCollection<Tokmoji>();
            var tokMojilist = JsonConvert.DeserializeObject<List<Tokmoji>>(Intent.GetStringExtra("tokMojiCollection"));
            foreach (var tokmojiitem in tokMojilist)
            {
                TokMojiCollection.Add(tokmojiitem);
            }

            RecyclerTokMoji.SetLayoutManager(new GridLayoutManager(this, 2));
            var adapterTokMoji = TokMojiCollection.GetRecyclerAdapter(BindTokMojiViewHolder, Resource.Layout.tokinfo_tokmoji_row);
            RecyclerTokMoji.SetAdapter(adapterTokMoji);



            View view = LayoutInflater.Inflate(Resource.Layout.tokinfo_comments_row, null);
            view.FindViewById<TextView>(Resource.Id.lblCommentPopUpMenu).Visibility = ViewStates.Gone; //Hide the ⋮

            var ImgCommentUserPhoto = view.FindViewById<ImageView>(Resource.Id.imgcomment_userphoto);
            ImgCommentUserPhoto.ContentDescription = commentReaction.UserId;
            Glide.With(this).Load(commentReaction.UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(ImgCommentUserPhoto);

            var CommentorName = view.FindViewById<TextView>(Resource.Id.lbl_commentnameuser);
            CommentorName.Text = commentReaction.UserDisplayName;
            CommentorName.ContentDescription = commentReaction.UserId;

            view.FindViewById<TextView>(Resource.Id.lbl_commentdate).Text = DateConvert.ConvertToRelative(commentReaction.CreatedTime).ToString();
            var kind = view.FindViewById<TextView>(Resource.Id.lblCommentRowKind);
            kind.Text = char.ToUpper(commentReaction.Kind[0]) + commentReaction.Kind.Substring(1);
            if (commentReaction.Kind.ToLower() == "accurate")
            {
                kind.SetBackgroundColor(Color.DarkGreen);
            }
            else if (commentReaction.Kind.ToLower() == "inaccurate")
            {
                kind.SetBackgroundColor(Color.Red);
            }
            var btnLoadMore = view.FindViewById<Button>(Resource.Id.btncommentsreply_loadmore);
            var BtnCommentReply = view.FindViewById<Button>(Resource.Id.BtnCommentReply);
            var BtnViewMoreClose = view.FindViewById<Button>(Resource.Id.btnViewMoreCloseComment);
            var CommentTextEllipsize = view.FindViewById<TextView>(Resource.Id.lblCommentRowContentEllip);
            var CommentText = view.FindViewById<TextView>(Resource.Id.lblCommentRowContent);
            CommentText.Text = commentReaction.Text;
            CommentTextEllipsize.Text = commentReaction.Text;

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

            var BtnShowHideComments = view.FindViewById<Button>(Resource.Id.btnShowHideComment);
            BtnShowHideComments.Visibility = ViewStates.Gone;
            RecyclerTokreplies.Visibility = ViewStates.Visible;

            LinearReplies.ContentDescription = Settings.ContinuationToken;
            btnLoadMore.Click += async (object sender, EventArgs e) =>
            {
                if (!string.IsNullOrEmpty(LinearReplies.ContentDescription))
                {
                    await LoadReplies(LinearReplies.ContentDescription);
                }
            };

            BtnCommentReply.Click += delegate
            {
                CommentEditor.RequestFocus();
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager.ShowSoftInput(CommentEditor, 0);
            };

            LinearReplies.AddView(view, new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent));

            RecyclerTokreplies.LayoutChange += (sender1, e1) =>
            {
                for (int i = loadedreplies; i < RecyclerTokreplies.ChildCount; i++)
                {
                    View viewParent = RecyclerTokreplies.GetChildAt(i);
                    var ReplyContentEllipL = viewParent.FindViewById<TextView>(Resource.Id.lblReplyRowContentEllip);
                    var ReplyContent = viewParent.FindViewById<TextView>(Resource.Id.lblReplyRowContent);
                    var BtnViewMoreCloseReply1 = viewParent.FindViewById<Button>(Resource.Id.btnViewMoreCloseReply);

                    Layout layoutReply = ReplyContentEllipL.Layout;
                    int replyEllipLine = BtnViewMoreCloseReply1.LineCount;

                    if (replyEllipLine > 1)
                    {
                        int ellipsisCount = layoutReply.GetEllipsisCount(replyEllipLine - 1);
                        if (ellipsisCount > 0)
                        {
                            BtnViewMoreCloseReply1.Visibility = ViewStates.Visible;

                            BtnViewMoreCloseReply1.Click += delegate
                            {
                                if (ReplyContentEllipL.Visibility == ViewStates.Visible)
                                {
                                    ReplyContentEllipL.Visibility = ViewStates.Gone;
                                    ReplyContent.Visibility = ViewStates.Visible;
                                    BtnViewMoreCloseReply1.Text = "Close";
                                }
                                else
                                {
                                    ReplyContentEllipL.Visibility = ViewStates.Visible;
                                    ReplyContent.Visibility = ViewStates.Gone;
                                    BtnViewMoreCloseReply1.Text = "View more";
                                }
                            };
                        }
                    }
                }
            };

            SmileyIcon.Click += (object sender, EventArgs e) =>
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                if (ExpandedTokMoji.Expanded == false)
                {
                    SmileyIcon.SetImageResource(Resource.Drawable.TOKKET_smiley_2b);
                    inputManager.HideSoftInputFromWindow(SmileyIcon.WindowToken, HideSoftInputFlags.None);
                    ExpandedTokMoji.Expanded = true;
                }
                else
                {
                    SmileyIcon.SetImageResource(Resource.Drawable.TOKKET_smiley_1B);
                    ExpandedTokMoji.Expanded = false;
                    View CommentView = CommentEditor;
                    CommentView.RequestFocus();
                    inputManager.ShowSoftInput(CommentView, 0);
                }
            };

            CommentEditor.FocusChange += delegate
            {
                if (CommentEditor.IsFocused)
                {
                    SmileyIcon.SetImageResource(Resource.Drawable.TOKKET_smiley_1B);
                    ExpandedTokMoji.Expanded = false;
                }
            };

            CommentEditor.Click += delegate
            {
                SmileyIcon.SetImageResource(Resource.Drawable.TOKKET_smiley_1B);
                ExpandedTokMoji.Expanded = false;

                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);

                CommentEditor.RequestFocus();

                inputManager.ShowSoftInput(CommentEditor, 0);
            };
        }
        private void BindRepliesViewHolder(CachingViewHolder holder, ReactionModel reply, int position)
        {
            var EditCommentText = holder.FindCachedViewById<EditText>(Resource.Id.EditCommentRowContent);
            var BtnCancelComment = holder.FindCachedViewById<Button>(Resource.Id.BtnCancelComment);
            var BtnUpdateComment = holder.FindCachedViewById<Button>(Resource.Id.BtnUpdateComment);
            var PopUpMenuComments = holder.FindCachedViewById<TextView>(Resource.Id.lblCommentPopUpMenu);

            var ImgUserReply = holder.FindCachedViewById<ImageView>(Resource.Id.imgreply_userphoto);
            var NameUserReply = holder.FindCachedViewById<TextView>(Resource.Id.lbl_replynameuser);
            var DateReplied = holder.FindCachedViewById<TextView>(Resource.Id.lbl_replydate);
            var replyContent = holder.FindCachedViewById<TextView>(Resource.Id.lblReplyRowContent);
            var replyContentEllip = holder.FindCachedViewById<TextView>(Resource.Id.lblReplyRowContentEllip);

            Glide.With(this).Load(reply.UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(ImgUserReply);
            NameUserReply.Text = reply.UserDisplayName;
            DateReplied.Text = DateConvert.ConvertToRelative(reply.CreatedTime).ToString();
            replyContent.Text = reply.Text;
            replyContentEllip.Text = reply.Text;
            EditCommentText.Text = reply.Text;

            holder.FindCachedViewById<View>(Resource.Id.ViewrepliesTopLine).Visibility = ViewStates.Gone;
            holder.FindCachedViewById<View>(Resource.Id.ViewrepliesBottomLine).Visibility = ViewStates.Visible;
        }
        private void SetRepliesAdapter()
        {
            adapterReplies = RepliesCollection.GetRecyclerAdapter(BindRepliesViewHolder, Resource.Layout.tokinfo_comments_replies_row);
            RecyclerTokreplies.SetAdapter(adapterReplies);
        }
        [Java.Interop.Export("OnClickAddReaction")]
        public async void OnClickAddReaction(View v)
        {
            ReactionModel tokkepediaReaction = new ReactionModel();
            tokkepediaReaction.ParentItem = commentReaction.Id;
            tokkepediaReaction.ItemId = commentReaction.ItemId;
            tokkepediaReaction.Kind = "comment";
            tokkepediaReaction.Label = "reaction";
            tokkepediaReaction.DetailNum = commentReaction.DetailNum;
            tokkepediaReaction.CategoryId = commentReaction.CategoryId;
            tokkepediaReaction.TokTypeId = commentReaction.TokTypeId;
            tokkepediaReaction.OwnerId = commentReaction.OwnerId;
            tokkepediaReaction.Text = CommentEditor.Text; //Comment
            tokkepediaReaction.IsChild = true;
            tokkepediaReaction.UserDisplayName = Settings.GetUserModel().DisplayName;
            tokkepediaReaction.UserPhoto = Settings.GetUserModel().UserPhoto;
            tokkepediaReaction.Timestamp = DateTime.Now;
            tokkepediaReaction.IsComment = true;

            CommentEditor.Text = "";

            //Show data that is added
            RepliesCollection.Insert(0, tokkepediaReaction);
            adapterReplies = RepliesCollection.GetRecyclerAdapter(BindRepliesViewHolder, Resource.Layout.tokinfo_comments_replies_row);
            RecyclerTokreplies.SetAdapter(adapterReplies);

            TextProgressReply.Text = "Adding a reply...";
            LinearProgress.Visibility = ViewStates.Visible;
            this.Window.AddFlags(WindowManagerFlags.NotTouchable);

            var result = await ReactionService.Instance.AddReaction(tokkepediaReaction);

            LinearProgress.Visibility = ViewStates.Gone;
            this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
            TextProgressReply.Text = "Loading...";

            if (result.ResultEnum == Shared.Helpers.Result.Success)
            {
                tokkepediaReaction.Id = result.ResultObject.ToString();
                //Replace with tokkepediaReaction.Id
                RepliesCollection.RemoveAt(0);
                RepliesCollection.Insert(0, tokkepediaReaction);

                adapterReplies = RepliesCollection.GetRecyclerAdapter(BindRepliesViewHolder, Resource.Layout.tokinfo_comments_replies_row);
                RecyclerTokreplies.SetAdapter(adapterReplies);

                TokInfoActivity.Instance.UpdateReplies(commentPosition, RepliesCollection.ToList());
            }
            else
            {
                string message = "An error occurred. Please refresh this section.";

                var dialogresult = new Android.App.AlertDialog.Builder(this);
                var alertResult = dialogresult.Create();
                alertResult.SetTitle("");
                alertResult.SetIcon(Resource.Drawable.alert_icon_blue);
                alertResult.SetMessage(message);
                alertResult.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                alertResult.Show();
                alertResult.SetCanceledOnTouchOutside(false);
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
        [Java.Interop.Export("OnClickCancelComment")]
        public void OnClickCancelComment(View v)
        {
            int position = (int)v.Tag;
            View view = RecyclerTokreplies.GetChildAt(position);
            var LinearEditComment = view.FindViewById<LinearLayout>(Resource.Id.LinearEditComment);
            var EditCommentText = view.FindViewById<EditText>(Resource.Id.EditCommentRowContent);
            var BtnCancelComment = view.FindViewById<Button>(Resource.Id.BtnCancelComment);
            var BtnUpdateComment = view.FindViewById<Button>(Resource.Id.BtnUpdateComment);
            var replyContent = view.FindViewById<TextView>(Resource.Id.lblReplyRowContent);
            var replyContentEllip = view.FindViewById<TextView>(Resource.Id.lblReplyRowContentEllip);

            LinearEditComment.Visibility = ViewStates.Gone;
            replyContentEllip.Visibility = ViewStates.Visible;
            replyContent.Visibility = ViewStates.Gone;
            NestedReplyBox.Visibility = ViewStates.Visible;
        }
        [Java.Interop.Export("OnClickUpdateComment")]
        public async void OnClickUpdateComment(View v)
        {
            int position = (int)v.Tag;
            View view = RecyclerTokreplies.GetChildAt(position);
            var LinearEditComment = view.FindViewById<LinearLayout>(Resource.Id.LinearEditComment);
            var EditCommentText = view.FindViewById<EditText>(Resource.Id.EditCommentRowContent);
            var BtnCancelComment = view.FindViewById<Button>(Resource.Id.BtnCancelComment);
            var BtnUpdateComment = view.FindViewById<Button>(Resource.Id.BtnUpdateComment);
            var replyContent = view.FindViewById<TextView>(Resource.Id.lblReplyRowContent);
            var replyContentEllip = view.FindViewById<TextView>(Resource.Id.lblReplyRowContentEllip);

            ReactionModel updateReaction = new ReactionModel();
            updateReaction = RepliesCollection[position];
            updateReaction.Text = EditCommentText.Text;

            LinearProgress.Visibility = ViewStates.Visible;
            this.Window.AddFlags(WindowManagerFlags.NotTouchable);

            //var origcomment = await ReactionService.Instance.GetCommentAsync(TokInfoVm.CommentsCollection[position].Id);
            //origcomment.Text = EditCommentText.Text;
            var ResultUpdate = await ReactionService.Instance.UpdateReaction(updateReaction);

            LinearProgress.Visibility = ViewStates.Gone;
            this.Window.ClearFlags(WindowManagerFlags.NotTouchable);

            if (ResultUpdate)
            {
                RepliesCollection.Remove(RepliesCollection[position]);
                RepliesCollection.Insert(position, updateReaction);
                replyContentEllip.Text = EditCommentText.Text;
                replyContent.Text = EditCommentText.Text;
                LinearEditComment.Visibility = ViewStates.Gone;
                replyContentEllip.Visibility = ViewStates.Visible;
                NestedReplyBox.Visibility = ViewStates.Visible;
            }
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

            if (Settings.GetTokketUser().Id == RepliesCollection[position].UserId)
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
            menu.MenuItemClick += async (s1, arg1) => {
                switch (arg1.Item.TitleFormatted.ToString().ToLower())
                {
                    case "report":
                        break;
                    case "edit":
                        NestedReplyBox.Visibility = ViewStates.Gone;
                        View view = RecyclerTokreplies.GetChildAt(position);
                        var LinearEditComment = view.FindViewById<LinearLayout>(Resource.Id.LinearEditComment);
                        var EditCommentText = view.FindViewById<EditText>(Resource.Id.EditCommentRowContent);
                        var replyContent = view.FindViewById<TextView>(Resource.Id.lblReplyRowContent);
                        var replyContentEllip = view.FindViewById<TextView>(Resource.Id.lblReplyRowContentEllip);
                        
                        LinearEditComment.Visibility = ViewStates.Visible;
                        replyContentEllip.Visibility = ViewStates.Gone;
                        replyContent.Visibility = ViewStates.Gone;

                        break;
                    case "delete":
                        LinearProgress.Visibility = ViewStates.Visible;
                        this.Window.AddFlags(WindowManagerFlags.NotTouchable);
                        TextProgressReply.Text = "Deleting a reply...";
                        var result = await ReactionService.Instance.DeleteReaction(RepliesCollection[position].Id);
                        if (result)
                        {
                            message = "Reply deleted.";
                            RepliesCollection.RemoveAt(position);
                            SetRepliesAdapter();

                            //TextTotalComments.Text = "Number of Comments: " + RepliesCollection.Count.ToString();
                        }
                        else
                        {
                            message = "Could not delete comment";
                        }

                        var dialogresult = new Android.App.AlertDialog.Builder(this);
                        var alertResult = dialogresult.Create();
                        alertResult.SetTitle("");
                        alertResult.SetIcon(Resource.Drawable.alert_icon_blue);
                        alertResult.SetMessage(message);
                        alertResult.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                        alertResult.Show();
                        alertResult.SetCanceledOnTouchOutside(false);

                        LinearProgress.Visibility = ViewStates.Gone;
                        this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                        TextProgressReply.Text = "Loading...";
                        break;
                }
            };

            // Menu was dismissed:
            menu.DismissEvent += (s2, arg2) => {
                //Console.WriteLine("menu dismissed");
            };

            menu.Show();
        }
        private void BindTokMojiViewHolder(CachingViewHolder holder, Tokmoji tokmoji, int position)
        {
            var ImgTokMoji = holder.FindCachedViewById<ImageView>(Resource.Id.imgTokInfoTokMojiRow);
            Glide.With(this).Load(tokmoji.Image).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).Error(Resource.Drawable.no_image).FitCenter()).Into(ImgTokMoji);
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
            alertDiag.SetPositiveButton("Cancel", (senderAlert, args) =>
            {
                alertDiag.Dispose();
            });
            alertDiag.SetNegativeButton(Html.FromHtml("<font color='#007bff'>Proceed</font>", FromHtmlOptions.ModeLegacy), async (senderAlert, args) =>
            {

                Android.Text.ISpannable spannableTokMoji = new SpannableString((sender as ImageView).ContentDescription);
                int start = CommentEditor.SelectionStart;
                string tokmojiidx = (sender as ImageView).ContentDescription;
                string tokidx = ":" + (sender as ImageView).ContentDescription + ":";
                string spaceafter = tokidx + " ";

                //TokMoji Purchase
                TextProgress.Text = "Purchasing...";
                FrameProgress.Visibility = ViewStates.Visible;
                this.Window.AddFlags(WindowManagerFlags.NotTouchable);
                var result = await TokMojiService.Instance.PurchaseTokmojiAsync(tokmojiidx, "tokmoji");
                this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                FrameProgress.Visibility = ViewStates.Gone;

                if (result.ResultEnum == Shared.Helpers.Result.Success)
                {
                    var resultObject = result.ResultObject as PurchasedTokmoji;
                    CommentEditor.Text = CommentEditor.Text.Substring(0, start) + spaceafter + CommentEditor.Text.Substring(start);

                    var spannableString = new SpannableString(CommentEditor.Text);
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

                    CommentEditor.SetText(spannableString, TextView.BufferType.Spannable);
                    CommentEditor.SetSelection(start + spaceafter.Length);
                }
                else
                {
                    var dialogDelete = new Android.App.AlertDialog.Builder(this);
                    var alertDelete = dialogDelete.Create();
                    alertDelete.SetTitle("");
                    alertDelete.SetIcon(Resource.Drawable.alert_icon_blue);
                    alertDelete.SetMessage("Not enough coins.");
                    alertDelete.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                    alertDelete.Show();
                    alertDelete.SetCanceledOnTouchOutside(false);
                }
            });
            Dialog diag = alertDiag.Create();
            diag.Show();
            diag.SetCanceledOnTouchOutside(false);
        }

        public async Task LoadReplies(string paginationId = "")
        {
            var result = await ReactionService.Instance.GetCommentReplyAsync(new ReactionQueryValues() { reaction_id = commentReaction.Id, kind = "comments", detail_number = -1, item_id = commentReaction.ItemId, pagination_id =  paginationId});
            LinearReplies.ContentDescription = result.ContinuationToken;
            var repliesResult = result.Results.ToList();
            loadedreplies += repliesResult.Count();

            foreach (var reply in repliesResult)
            {
                RepliesCollection.Add(reply);
            }
            adapterReplies = RepliesCollection.GetRecyclerAdapter(BindRepliesViewHolder, Resource.Layout.tokinfo_comments_replies_row);
            RecyclerTokreplies.SetAdapter(adapterReplies);
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
                swipeRefresh.Refreshing = false;
            }
        }
        private void Work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            swipeRefresh.Refreshing = false;
        }
        private void Work_DoWork(object sender, DoWorkEventArgs e)
        {
            this.RunOnUiThread(async () => await LoadReplies(LinearReplies.ContentDescription));
            Thread.Sleep(3000);
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

        #region Properties
        public SwipeRefreshLayout swipeRefresh => FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefresh);
        public LinearLayout LinearReplies => FindViewById<LinearLayout>(Resource.Id.LinearTokInfoRepliesPage);
        public EditText CommentEditor => FindViewById<EditText>(Resource.Id.tokinforeplies_txtComment);
        public ExpandableLinearLayout ExpandedTokMoji => FindViewById<ExpandableLinearLayout>(Resource.Id.expandedTokInfoRepliesTokMoji);
        public RecyclerView RecyclerTokMoji => FindViewById<RecyclerView>(Resource.Id.tokinfoRepliesRecyclerTokMojis);
        public ImageView SendReply => FindViewById<ImageView>(Resource.Id.btnTokInfoReply_SendComment);
        public ImageView SmileyIcon => FindViewById<ImageView>(Resource.Id.btnTokInfoRepliesSmiley);
        public RecyclerView RecyclerTokreplies => FindViewById<RecyclerView>(Resource.Id.RecyclerTokInfoReplies);
        public ProgressBar CircleProgressLoadMore => FindViewById<ProgressBar>(Resource.Id.circleprogressRepliesPage);
        public FrameLayout FrameProgress => FindViewById<FrameLayout>(Resource.Id.frame_addsetprogress);
        public TextView TextProgress => FindViewById<TextView>(Resource.Id.progressBarText);
        public LinearLayout LinearProgress => FindViewById<LinearLayout>(Resource.Id.linear_replyprogress);
        public TextView TextProgressReply => FindViewById<TextView>(Resource.Id.progressBarTextReply);
        public NestedScrollView NestedReplyBox => FindViewById<NestedScrollView>(Resource.Id.NestedReplyBox);
#endregion
    }
}