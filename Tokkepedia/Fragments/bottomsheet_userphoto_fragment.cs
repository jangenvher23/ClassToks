using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using System;
using Tokkepedia.Shared.Helpers;
using Android.Graphics;
using Java.IO;
using Tokkepedia.Helpers;
using Android.App;

namespace Tokkepedia.Fragments
{
    public class bottomsheet_userphoto_fragment : BottomSheetDialogFragment
    {
        View v;
        Android.Net.Uri imageUri;
        ImageView PhotoImage;
        Activity activity;
        public bottomsheet_userphoto_fragment(Activity _activity, ImageView _photoImage)
        {
            this.activity = _activity;
            this.PhotoImage = _photoImage;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.row_userphoto_options, container, false);
            
            var btnTakePhoto = v.FindViewById<Button>(Resource.Id.btnTakePhoto);
            var btnAvatars = v.FindViewById<Button>(Resource.Id.btnAvatars);
            var btnChoosePhoto = v.FindViewById<Button>(Resource.Id.btnChoosePhoto);

            if (Settings.ActivityInt != (int)ActivityType.ProfileActivity && Settings.ActivityInt != (int)ActivityType.ProfileTabActivity)
                btnAvatars.Visibility = ViewStates.Gone;

            btnTakePhoto.Click += delegate
            {
                StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
           
                StrictMode.SetVmPolicy(builder.Build());
                
                Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
                
                var dir = Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDcim);
                File _file = new File(dir, String.Format("InspectionPhoto_{0}.jpg", Guid.NewGuid()));
                imageUri = Android.Net.Uri.FromFile(_file);
                intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));
                StartActivityForResult(intent, 0);
            };
            
            btnAvatars.Click += delegate
            {
                Intent nextActivity = new Intent(this.Activity, typeof(AvatarsActivity));
                activity.StartActivityForResult(nextActivity, (int)ActivityType.AvatarsActivity);
                Dismiss();
            };
            
            btnChoosePhoto.Click += delegate
            {
                Intent nextActivity = new Intent();
                nextActivity.SetType("image/*");
                nextActivity.SetAction(Intent.ActionGetContent);
                activity.StartActivityForResult(Intent.CreateChooser(nextActivity, "Select Picture"), Settings.ActivityInt);
                Dismiss();
            };
            
            return v;
        }
        
        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 0 && resultCode == -1)
            {
                try
                {
                    ImageDecoder.Source source = ImageDecoder.CreateSource(this.Context.ContentResolver, imageUri);
                    Bitmap bitmap = ImageDecoder.DecodeBitmap(source); ; //(Bitmap)data.Extras.Get("data");
                    Bitmap scaledBitmap = SpannableHelper.scaleDown(bitmap, 300, true);

                    if (Settings.ActivityInt != (int)ActivityType.AddTokActivityType)
                        //show image
                        activity.FindViewById<ImageView>(PhotoImage.Id).SetImageBitmap(scaledBitmap);
                    
                    //save image to database
                    string base64img = ImageConverter.BitmapToBase64(scaledBitmap);
                    Settings.ImageBrowseCrop = base64img;

                    if (Settings.ActivityInt == (int)ActivityType.ProfileActivity || Settings.ActivityInt == (int)ActivityType.ProfileTabActivity)
                    {
                        MainActivity.Instance.RunOnUiThread(async () => await MainActivity.Instance.SaveUserCoverPhoto(base64img));
                    }
                    else if (Settings.ActivityInt == (int)ActivityType.AddTokActivityType)
                    {
                        AddTokActivity.Instance.displayImageBrowse();
                    }
                    else if(Settings.ActivityInt == (int)ActivityType.AddSetActivityType)
                    {
                        AddSetActivity.Instance.displayImageBrowse(scaledBitmap, base64img);
                    }

                    Dismiss();

                    //todo: need to delete temporary image
                }
                catch (Exception)
                {

                }
            }
            else if ((requestCode == Settings.ActivityInt) && (resultCode == -1) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                Intent nextActivity = new Intent(activity, typeof(CropImageActivity));
                Settings.ImageBrowseCrop = (string)uri;
                this.StartActivityForResult(nextActivity, requestCode);
            }
        }
        

        public override void Dismiss()
        {
            base.Dismiss();
        }

    }
}