using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GalaSoft.MvvmLight.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokkepedia.Shared.Helpers;
using Android.Graphics;
using Android.Text;
using Newtonsoft.Json;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Android.Webkit;

namespace Tokkepedia
{
    [Activity(Label = "Class Group", Theme = "@style/AppTheme")]
    public class AddClassGroupActivity : BaseActivity
    {
        bool isSaving = true;
        internal static AddClassGroupActivity Instance { get; private set; }
        ClassGroupModel ClassModel;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.addclassgroup_page);
            Settings.ActivityInt = (int)ActivityType.AddClassGroupActivity;
            isSaving = Intent.GetBooleanExtra("isSaving", true);
            Instance = this;
            ClassModel = new ClassGroupModel();

            CancelButton.Click += delegate
            {
                Finish();
            };

            SaveButton.Click += async(sender,e) =>
            {
                if (isSaving)
                {
                    await SaveClassGroup();
                }
                else
                {
                    await EditClassGroup();
                }
            };

            ButtonBrowse.Click += delegate
            {
                Intent = new Intent();
                Intent.SetType("image/*");
                Intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), 1001);
            };

            ButtonRemoveBrowse.Click += delegate
           {
               ImgBrowse.ContentDescription = "";
               ImgBrowse.SetImageBitmap(null);
               ButtonRemoveBrowse.Visibility = ViewStates.Gone;
               ButtonBrowse.Visibility = ViewStates.Visible;
           };

            if (!isSaving)
            {
                SaveButton.Text = "Update";
                ClassModel = JsonConvert.DeserializeObject<ClassGroupModel>(Intent.GetStringExtra("ClassGroupModel"));
                EditGroupName.Text = ClassModel.Name;
                EditDescription.Text = ClassModel.Description;
                EditSchool.Text = ClassModel.School;
                ImgBrowse.ContentDescription = ClassModel.Image;
                Glide.With(this).Load(ClassModel.Image).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).Error(Resource.Drawable.no_image).FitCenter()).Into(ImgBrowse);
            }
        }
        private async Task SaveClassGroup()
        {
            ClassModel.UserId = Settings.GetUserModel().UserId;
            ClassModel.Name = EditGroupName.Text;
            ClassModel.Description = EditDescription.Text;
            ClassModel.School = EditSchool.Text;
            ClassModel.Image = ImgBrowse.ContentDescription;

            if (!string.IsNullOrEmpty(ClassModel.Image))
            {
                ClassModel.Image = "data:image/jpeg;base64," + ClassModel.Image;
            }

            LinearProgress.Visibility = ViewStates.Visible;
            this.Window.AddFlags(WindowManagerFlags.NotTouchable);

            var result = await ClassService.Instance.AddClassGroupAsync(ClassModel);
            
            this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
            LinearProgress.Visibility = ViewStates.Gone;

            if (result != null)
            {
                var dialog = new AlertDialog.Builder(this);
                var alertDialog = dialog.Create();
                alertDialog.SetTitle("");
                alertDialog.SetIcon(Resource.Drawable.alert_icon_blue);
                alertDialog.SetMessage("Save Successfully.");
                alertDialog.SetButton((int)(DialogButtonType.Positive), Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => 
                {
                    ClassGroupListActivity.Instance.AddClassGroupCollection(result);
                    Finish();
                });
                alertDialog.Show();
                alertDialog.SetCanceledOnTouchOutside(false);
            }
        }
        private async Task EditClassGroup()
        {
            ClassModel.Name = EditGroupName.Text;
            ClassModel.Description = EditDescription.Text;
            ClassModel.School = EditSchool.Text;
            ClassModel.Image = ImgBrowse.ContentDescription;

            if (!string.IsNullOrEmpty(ClassModel.Image))
            {
                if (!URLUtil.IsValidUrl(ClassModel.Image))
                {
                    ClassModel.Image = "data:image/jpeg;base64," + ClassModel.Image;
                }
            }

            LinearProgress.Visibility = ViewStates.Visible;
            this.Window.AddFlags(WindowManagerFlags.NotTouchable);

            var result = await ClassService.Instance.UpdateClassGroupAsync(ClassModel);
            this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
            LinearProgress.Visibility = ViewStates.Gone;

            string alertmssg = "Failed to update.";
            if (result)
            {
                alertmssg = "Updated Successfully.";
            }

            var dialog = new AlertDialog.Builder(this);
            var alertDialog = dialog.Create();
            alertDialog.SetTitle("");
            alertDialog.SetIcon(Resource.Drawable.alert_icon_blue);
            alertDialog.SetMessage(alertmssg);
            alertDialog.SetButton((int)(DialogButtonType.Positive), Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) =>
            {
                if (result)
                {
                    ClassGroupActivity.Instance.model = ClassModel;
                    ClassGroupActivity.Instance.Initialize();
                    ClassGroupListActivity.Instance.AddClassGroupCollection(ClassModel, false);
                    Finish();
                }
            });
            alertDialog.Show();
            alertDialog.SetCanceledOnTouchOutside(false);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == 1001) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                Intent nextActivity = new Intent(this, typeof(CropImageActivity));
                Settings.ImageBrowseCrop = (string)uri;
                this.StartActivityForResult(nextActivity, requestCode);
            }

        }
        public void displayImageBrowse()
        {
            //Main Image
            //AddTokVm.TokModel.Image = Settings.ImageBrowseCrop;
            ImgBrowse.ContentDescription = Settings.ImageBrowseCrop;
            byte[] imageMainBytes = Convert.FromBase64String(Settings.ImageBrowseCrop);
            ImgBrowse.SetImageBitmap((BitmapFactory.DecodeByteArray(imageMainBytes, 0, imageMainBytes.Length)));
            ButtonBrowse.Visibility = ViewStates.Gone;
            ButtonRemoveBrowse.Visibility = ViewStates.Visible;
        }
        public EditText EditGroupName => FindViewById<EditText>(Resource.Id.EditACGGroupName);
        public EditText EditDescription => FindViewById<EditText>(Resource.Id.EditACGDescription);
        public EditText EditSchool => FindViewById<EditText>(Resource.Id.EditACGSchool);
        public ImageView ImgBrowse => FindViewById<ImageView>(Resource.Id.addclassgroup_imagebrowse);
        public Button ButtonBrowse => FindViewById<Button>(Resource.Id.btnAddClassGroup_btnBrowseImage);
        public Button ButtonRemoveBrowse => FindViewById<Button>(Resource.Id.btnAddClassGroupRemoveImgMain);
        public TextView CancelButton => FindViewById<TextView>(Resource.Id.btnAddClassGroupCancel);
        public TextView SaveButton => FindViewById<TextView>(Resource.Id.btnAddClassGroupSave);
        public LinearLayout LinearProgress => FindViewById<LinearLayout>(Resource.Id.LinearProgress_addclassgroup);
        public ProgressBar ProgressCircle => FindViewById<ProgressBar>(Resource.Id.progressbarAddClassGroup);
        public TextView ProgressText => FindViewById<TextView>(Resource.Id.progressBarTextAddClassGroup);
    }
}