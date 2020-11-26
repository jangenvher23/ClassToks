using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.ViewPager.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Tabs;
using ImageViews.Photo;
using Newtonsoft.Json;
using Tokkepedia.Adapters;
using Tokkepedia.Fragments;
using Tokkepedia.Helpers;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using XFragment = AndroidX.Fragment.App.Fragment;
using XFragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace Tokkepedia
{
    [Activity(Label = "Class Group", Theme = "@style/AppTheme")]
    public class ClassGroupActivity : BaseActivity
    {
        Intent nextActivity;
        GlideImgListener GListener; float imgScale = 0;
        internal static ClassGroupActivity Instance { get; private set; }
        public AdapterFragmentX fragment { get; private set; }
        public ClassGroupModel model; Typeface font;
        public IMenuItem requesttojoin;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.classgroup_page);

            var tokback_toolbar = this.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.custom_toolbar);

            SetSupportActionBar(tokback_toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            Instance = this;
            Settings.ActivityInt = (int)ActivityType.ClassGroupActivity;
            model = JsonConvert.DeserializeObject<ClassGroupModel>(Intent.GetStringExtra("ClassGroupModel"));

            setupViewPager(viewpager);
            tabLayout.SetupWithViewPager(viewpager);

            Initialize();

            font = Typeface.CreateFromAsset(this.Application.Assets, "fa_solid_900.otf");
            TextCheckRequestSent.SetTypeface(font, TypefaceStyle.Bold);

            viewpager.PageSelected -= Viewpager_PageSelected;
            viewpager.PageSelected += Viewpager_PageSelected;

            FloatingBtn.Click += (object sender, EventArgs e) =>
            {
                if (tabLayout.SelectedTabPosition == 1) //ClassToks
                {
                    nextActivity = new Intent(this, typeof(AddClassTokActivity));
                    var modelConvert = JsonConvert.SerializeObject(model);
                    nextActivity.PutExtra("ClassGroupModel", modelConvert);
                    this.StartActivity(nextActivity);
                }
                else if (tabLayout.SelectedTabPosition == 2) //Class Set
                {
                    nextActivity = new Intent(this, typeof(AddClassSetActivity));
                    var modelConvert = JsonConvert.SerializeObject(model);
                    nextActivity.PutExtra("ClassGroupModel", modelConvert);
                    this.StartActivity(nextActivity);
                }
            };
        }
        public void Initialize()
        {
            this.Title = model.Name;
            TextName.Text = model.Name;
            TextSchool.Text = model.School;
            TextDescription.Text = model.Description;

            GListener = new GlideImgListener();
            GListener.ParentActivity = this;
            Glide.With(this).Load(model.Image).Listener(GListener).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).Error(Resource.Drawable.no_image).FitCenter()).Into(ImgGroup);

        }
        void setupViewPager(ViewPager viewPager)
        {
            List<XFragment> fragments = new List<XFragment>();
            List<string> fragmentTitles = new List<string>();

            //var modelConvert = JsonConvert.SerializeObject(model);
            
            fragments.Add(new classtoks_fragment(model.Id));
            fragments.Add(new myclasstoksets_fragment(model.Id));

            fragmentTitles.Add("Class Toks");
            fragmentTitles.Add("Class Sets");

            //if (model.UserId == Settings.GetUserModel().UserId)
            //{
                fragments.Add(new classgroupmember_fragment(model));
                fragmentTitles.Add("Members");
            //}

            var adapter = new AdapterFragmentX(SupportFragmentManager, fragments, fragmentTitles);
            viewPager.Adapter = adapter;
            viewpager.Adapter.NotifyDataSetChanged();
            fragment = adapter;
        }
        [Java.Interop.Export("OnClickPopUpMenuST")]
        public void OnClickPopUpMenuST(View v)
        {
            int position = 0;
            try { position = (int)v.Tag; } catch { position = int.Parse((string)v.Tag); }
            Android.Widget.PopupMenu menu = new Android.Widget.PopupMenu(this, v);

            // Call inflate directly on the menu:
            menu.Inflate(Resource.Menu.myclasssets_popmenu);

            // A menu item was clicked:
            menu.MenuItemClick += (s1, arg1) => {
                Intent nextActivity; string modelConvert;
                switch (arg1.Item.TitleFormatted.ToString().ToLower())
                {
                    case "edit":
                        nextActivity = new Intent(this, typeof(AddClassSetActivity));
                        modelConvert = JsonConvert.SerializeObject(myclasstoksets_fragment.Instance.ListClassTokSets[position]);
                        nextActivity.PutExtra("ClassTokSetsModel", modelConvert);
                        nextActivity.PutExtra("isSave", false);
                        this.StartActivity(nextActivity);
                        break;
                }
            };

            // Menu was dismissed:
            menu.DismissEvent += (s2, arg2) => {
                //Console.WriteLine("menu dismissed");
            };

            menu.Show();
        }
        [Java.Interop.Export("OnClickImageGroup")]
        public void OnClickImageGroup(View v)
        {
            ImgImageView.SetImageDrawable(ImgGroup.Drawable);
            ImgImageView.SetBackgroundColor(ManipulateColor.manipulateColor(GListener.mColorPalette, 0.62f));
            imgScale = ImgImageView.Scale;
            LinearImageView.Visibility = ViewStates.Visible;

            FloatingBtn.Visibility = ViewStates.Gone;
        }
        [Java.Interop.Export("OnClickCloseImgView")]
        public void OnClickCloseImgView(View v)
        {
            LinearImageView.Visibility = ViewStates.Gone;

            FloatingBtn.Visibility = ViewStates.Visible;
        }
        private void Viewpager_PageSelected(object sender,  AndroidX.ViewPager.Widget.ViewPager.PageSelectedEventArgs e)
        {
            switch (e.Position)
            {
                case 0:
                    FloatingBtn.Visibility = ViewStates.Gone;
                    break;
                case 1:
                    FloatingBtn.Visibility = ViewStates.Visible;
                    break;
                case 2:
                    FloatingBtn.Visibility = ViewStates.Visible;
                    break;
            }
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.classgroup_menu, menu);

            var inviteUsers = menu.FindItem(Resource.Id.menu_InviteUsers);
            requesttojoin = menu.FindItem(Resource.Id.menu_requesttojoin);
            var delete = menu.FindItem(Resource.Id.menu_delete);
            var edit = menu.FindItem(Resource.Id.menu_edit);
            var report = menu.FindItem(Resource.Id.menu_report);
            var requests = menu.FindItem(Resource.Id.menu_requests);

            if (model.HasPendingRequest)
            {
                requesttojoin.SetTitle("Waiting for approval");
            }
            else if (model.IsMember)
            {
                requesttojoin.SetTitle("Leave Group");
                //BtnRequestToJoin.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor("#dc3545"));
            }
            else
            {
                //BtnRequestToJoin.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor("#007bff"));
                requesttojoin.SetTitle("Request to Join");
            }

            if (model.UserId == Settings.GetUserModel().UserId)
            {
                inviteUsers.SetVisible(true);
                delete.SetVisible(true);
                edit.SetVisible(true);
                requests.SetVisible(true);
                report.SetVisible(false);
                requesttojoin.SetVisible(false);
            }
            else
            {
                inviteUsers.SetVisible(false);
                delete.SetVisible(false);
                edit.SetVisible(false);
                report.SetVisible(true);
                requests.SetVisible(false);
            }

            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Android.Support.V7.App.AlertDialog.Builder alertDiag;
            Android.App.Dialog diag;
            string modelConvert;
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
                case Resource.Id.menu_requesttojoin:
                    if (model.IsMember)
                    {
                        alertDiag = new Android.Support.V7.App.AlertDialog.Builder(this);
                        alertDiag.SetTitle("Confirm");
                        alertDiag.SetMessage("Are you sure you want to leave from this group?");
                        alertDiag.SetPositiveButton("Cancel", (senderAlert, args) => {
                            alertDiag.Dispose();
                        });
                        alertDiag.SetNegativeButton(Html.FromHtml("<font color='#dc3545'>Leave</font>", FromHtmlOptions.ModeLegacy), async (senderAlert, args) =>
                        {
                            TextProgressStatus.Text = "Leaving...";
                            LinearProgress.Visibility = ViewStates.Visible;

                            var result = await ClassService.Instance.LeaveClassGroupAsync(model.Id, model.PartitionKey);
                            LinearProgress.Visibility = ViewStates.Gone;
                            TextProgressStatus.Text = "Loading...";

                            if (result)
                            {
                                item.SetTitle("Request to Join");
                            }
                        });
                        diag = alertDiag.Create();
                        diag.Show();
                        diag.SetCanceledOnTouchOutside(false);
                    }
                    else if (model.HasPendingRequest == false)
                    {
                        alertDiag = new Android.Support.V7.App.AlertDialog.Builder(this);
                        alertDiag.SetTitle("Confirm");
                        alertDiag.SetMessage("Are you sure you want to join in this group?");
                        alertDiag.SetPositiveButton("Cancel", (senderAlert, args) => {
                            alertDiag.Dispose();
                        });
                        alertDiag.SetNegativeButton(Html.FromHtml("<font color='#007bff'>Join</font>", FromHtmlOptions.ModeLegacy), async (senderAlert, args) =>
                        {
                            TextProgressStatus.Text = "Requesting...";
                            LinearProgress.Visibility = ViewStates.Visible;

                            var modelitem = new ClassGroupRequestModel();
                            modelitem.ReceiverId = model.UserId;
                            modelitem.SenderId = Settings.GetUserModel().UserId;
                            modelitem.Name = model.Name;
                            modelitem.Status = "0";
                            modelitem.Remarks = "Pending";
                            modelitem.Message = "Request to join group.";
                            modelitem.GroupId = model.Id;
                            modelitem.GroupPartitionKey = model.PartitionKey;
                            var result = await ClassService.Instance.RequestClassGroupAsync(modelitem);
                            LinearProgress.Visibility = ViewStates.Gone;
                            TextProgressStatus.Text = "Loading...";

                            if (result != null)
                            {
                                model.HasPendingRequest = true;
                                TextRequestStatus.Text = "Request Sent";
                                LinearRequestSent.Visibility = ViewStates.Visible;
                                item.SetTitle("Request Sent");
                            }
                        });
                        diag = alertDiag.Create();
                        diag.Show();
                        diag.SetCanceledOnTouchOutside(false);
                    }
                    break;
                case Resource.Id.menu_report:
                    break;
                case Resource.Id.menu_InviteUsers:
                    nextActivity = new Intent(this, typeof(InviteUsersActivity));
                    modelConvert = JsonConvert.SerializeObject(model);
                    nextActivity.PutExtra("ClassGroupModel", modelConvert);
                    this.StartActivity(nextActivity);
                    break;
                case Resource.Id.menu_delete:
                    alertDiag = new Android.Support.V7.App.AlertDialog.Builder(this);
                    alertDiag.SetTitle("Confirm");
                    alertDiag.SetMessage("Are you sure you want to delete this group?");
                    alertDiag.SetPositiveButton("Cancel", (senderAlert, args) => {
                        alertDiag.Dispose();
                    });
                    alertDiag.SetNegativeButton(Html.FromHtml("<font color='#dc3545'>Delete</font>", FromHtmlOptions.ModeLegacy), async (senderAlert, args) => {
                        string message = "";
                        LinearProgress.Visibility = ViewStates.Visible;
                        this.Window.AddFlags(WindowManagerFlags.NotTouchable);

                        var issuccess = await ClassService.Instance.DeleteClassGroupAsync(model.Id, model.PartitionKey); //API

                        this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                        LinearProgress.Visibility = ViewStates.Gone;

                        if (issuccess)
                        {
                            message = "Deleted the group successfully!";
                        }
                        else
                        {
                            message = "Failed to delete group!";
                        }

                        var dialogDelete = new Android.App.AlertDialog.Builder(this);
                        var alertDelete = dialogDelete.Create();
                        alertDelete.SetTitle("");
                        alertDelete.SetIcon(Resource.Drawable.alert_icon_blue);
                        alertDelete.SetMessage(message);
                        alertDelete.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) =>
                        {
                            ClassGroupListActivity.Instance.RemoveClassGroupCollection(model);
                            this.Finish();
                        });
                        alertDelete.Show();
                        alertDelete.SetCanceledOnTouchOutside(false);
                    });
                    diag = alertDiag.Create();
                    diag.Show();
                    diag.SetCanceledOnTouchOutside(false);
                    break;
                case Resource.Id.menu_edit:
                    nextActivity = new Intent(this, typeof(AddClassGroupActivity));
                    modelConvert = JsonConvert.SerializeObject(model);
                    nextActivity.PutExtra("ClassGroupModel", modelConvert);
                    nextActivity.PutExtra("isSaving", false);
                    this.StartActivity(nextActivity);

                    break;
                case Resource.Id.menu_requests:
                    modelConvert = JsonConvert.SerializeObject(model);
                    nextActivity = new Intent(this, typeof(RequestsDialogActivity));
                    nextActivity.PutExtra("classGroupModel", modelConvert);
                    this.StartActivity(nextActivity);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        // Allows us to know if we should use MotionEvent.ACTION_MOVE
        private bool tracking = false;
        // The Position where our touch event started
        private float startY;
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
                    LinearImageView.GetHitRect(hitRect);

                    if (hitRect.Contains((int)ev.GetX(), (int)ev.GetY()))
                    {
                        tracking = true;
                    }
                    startY = ev.GetY();
                    break;
                case MotionEventActions.Move:
                    if (ImgImageView.Scale == imgScale)
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
                                LinearImageView.TranslationY = ev.GetY() - startY;
                            }
                            animateSwipeView(LinearImageView.Height);
                        }
                        else if (startY - ev.GetY() > 1)
                        {
                            if (tracking)
                            {
                                LinearImageView.TranslationY = ev.GetY() - startY;
                            }
                            animateSwipeView(LinearImageView.Height);
                        }
                    }
                    break;
                case MotionEventActions.Up:
                    if (mSwiping)
                    {
                        if (LinearImageView.Visibility == ViewStates.Visible)
                        {
                            int quarterHeight = LinearImageView.Height / 4;
                            float currentPosition = LinearImageView.TranslationY;
                            if (currentPosition < -quarterHeight)
                            {
                                LinearImageView.Visibility = ViewStates.Gone;
                                FloatingBtn.Visibility = ViewStates.Visible;
                                this.SupportActionBar.Show();
                            }
                            else if (currentPosition > quarterHeight)
                            {
                                //Hide LinearImageView
                                LinearImageView.Visibility = ViewStates.Gone;
                                FloatingBtn.Visibility = ViewStates.Visible;
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
            float currentPosition = LinearImageView.TranslationY;
            float animateTo = 0.0f;
            if (currentPosition < -quarterHeight)
            {
                animateTo = -parentHeight;
            }
            else if (currentPosition > quarterHeight)
            {
                animateTo = parentHeight;

                //Hide LinearImageView
                //LinearImageView.Visibility = ViewStates.Gone;
                //FloatingBtn.Visibility = ViewStates.Visible;
                //this.SupportActionBar.Show();
            }
            ObjectAnimator.OfFloat(LinearImageView, "translationY", currentPosition, animateTo)
                    .SetDuration(200)
                    .Start();
        }
        public TextView TextProgressStatus => this.FindViewById<TextView>(Resource.Id.TextProgressStatus);
        public TextView TextName => this.FindViewById<TextView>(Resource.Id.TextName);
        public TextView TextSchool => this.FindViewById<TextView>(Resource.Id.TextSchool);
        public TextView TextDescription => this.FindViewById<TextView>(Resource.Id.TextDescription);
        public TextView TextCheckRequestSent => this.FindViewById<TextView>(Resource.Id.TextCheckRequestSent);
        public TextView TextRequestStatus => this.FindViewById<TextView>(Resource.Id.TextRequestStatus);
        public LinearLayout LinearProgress => this.FindViewById<LinearLayout>(Resource.Id.LinearProgress_ClassGroup);
        public LinearLayout LinearRequestSent => this.FindViewById<LinearLayout>(Resource.Id.LinearRequestSent); 
        public TabLayout tabLayout => this.FindViewById<TabLayout>(Resource.Id.tabLayout);
        public FloatingActionButton FloatingBtn => this.FindViewById<FloatingActionButton>(Resource.Id.fab_AddClassGrouppage);
        public ViewPager viewpager => this.FindViewById<ViewPager>(Resource.Id.viewpagerClassGroup);
        public RelativeLayout LinearImageView => FindViewById<RelativeLayout>(Resource.Id.LinearImageView);
        public PhotoView ImgImageView => FindViewById<PhotoView>(Resource.Id.ImgImageView);
        public ImageView ImgGroup => FindViewById<ImageView>(Resource.Id.ImgGroup);
    }
}