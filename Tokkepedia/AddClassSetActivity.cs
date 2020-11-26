using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Com.Bumptech.Glide;
using GalaSoft.MvvmLight.Helpers;
using Newtonsoft.Json;
using Tokkepedia.Fragments;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia.Tools;
using Xamarin.Essentials;

namespace Tokkepedia
{
    [Activity(Label = "Add Class Set", Theme = "@style/AppTheme")]
    public class AddClassSetActivity : BaseActivity
    {
        internal static AddClassSetActivity Instance { get; private set; }
        ObservableCollection<string> PrivacyList;
        ObservableCollection<ClassGroupModel> ClassGroupCollection;
        ClassSetModel model; ClassGroupModel ClassGroupModel;
        string UserId; bool isSave = true;
        int COLOR_REQUEST_CODE = 1001;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.addclassset_page);
            Settings.ActivityInt = Convert.ToInt16(ActivityType.AddClassSetActivity);
            Instance = this;
            UserId = Settings.GetUserModel().UserId;
            int numcol = 1;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                numcol = (int)NumDisplay.tablet + 1;
            }
            RecyclerGroupList.SetLayoutManager(new GridLayoutManager(Application.Context, numcol));

            ClassGroupCollection = new ObservableCollection<ClassGroupModel>();

            model = new ClassSetModel();

            var stringClassGroup = Intent.GetStringExtra("ClassGroupModel");
            if (stringClassGroup != null)
            {
                ClassGroupModel = JsonConvert.DeserializeObject<ClassGroupModel>(stringClassGroup);
                model.GroupId = ClassGroupModel.Id;
                TextGroupName.ContentDescription = ClassGroupModel.Id;
                TextGroupName.Text = ClassGroupModel.Name;
            }

            LoadSpinners();
            RunOnUiThread(async () => await LoadClassGroup());

            SpinTokGroup.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(SpinTokGroup_ItemSelected);
            SpinPrivacy.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(SpinPrivacy_ItemSelected);

            var ArrTokGroup = new string[] { "Basic", "Detail", "Mega" };
            var Aadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, ArrTokGroup);
            Aadapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            SpinTokGroup.Adapter = null;
            SpinTokGroup.Adapter = Aadapter;

            var SpinPrivacyadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, PrivacyList);
            SpinPrivacyadapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            SpinPrivacy.Adapter = null;
            SpinPrivacy.Adapter = SpinPrivacyadapter;

            SaveClassSet.Click += async(sender,e) =>
            {
                if (isSave)
                {
                    await AddClassSetFunction();
                }
                else
                {
                    await EditClassSetFunction();
                }
            };

            CancelClassSet.Click += delegate
            {
                Settings.ActivityInt = Convert.ToInt16(ActivityType.LeftMenuSets);
                Finish();
            };

            isSave = Intent.GetBooleanExtra("isSave", true);
            if (!isSave)
            {
                SaveClassSet.Text = "Update Set";
                LinearGroup.Visibility = ViewStates.Gone;
                model = JsonConvert.DeserializeObject<ClassSetModel>(Intent.GetStringExtra("ClassTokSetsModel"));

                if (model.Group != null)
                {
                    classGroupName.Visibility = ViewStates.Visible;
                    classGroupName.Text = "Class Group: " + model.Group.Name;
                }

                //Tok Group
                var spinnerTGPosition = 0;
                switch (model.TokGroup.ToLower())
                {
                    case "basic":
                        spinnerTGPosition = 0;
                        break;
                    case "detail":
                        spinnerTGPosition = 1;
                        break;
                    case "mega":
                        spinnerTGPosition = 2;
                        break;
                }
                SpinTokGroup.SetSelection(spinnerTGPosition);

                EditClassName.Text = model.TokType;

                //Privacy
                for (int i = 0; i < PrivacyList.Count; i++)
                {
                    if (PrivacyList[i].ToString() == model.Privacy)
                    {
                        SpinPrivacy.SetSelection(i);
                        break;
                    }
                }

                SpinTokGroup.ContentDescription = model.TokGroup;
                EditClassName.Text = model.TokType;
                SpinPrivacy.ContentDescription = model.Privacy;
                EditClassSetName.Text = model.Name;
                EditDescription.Text = model.Description;
                TextGroupName.ContentDescription = model.GroupId;

                Glide.With(this).Load(model.Image).Into(ImgClassSet);

                var resultRequest = ClassGroupCollection.FirstOrDefault(c => c.Id == model.GroupId);
                if (resultRequest != null) //If Edit
                {
                    TextGroupName.Text = resultRequest.Name;
                }
            }

            BtnBrowse.Click += BtnBrowseImageClick;

            btnSelectColor.Click += delegate
            {
                if (!isSave)
                {
                    if (model.UserId != Settings.GetTokketUser().Id)
                    {
                        return;
                    }
                }

                var nextActivity = new Intent(this, typeof(ColorSelectionActivity));
                nextActivity.PutExtra("className", model.TokType);
                nextActivity.PutExtra("color", model.ColorMainHex);
                nextActivity.PutExtra("keyvalue", model.TokType);
                this.StartActivityForResult(nextActivity, COLOR_REQUEST_CODE);
            };

            if (!string.IsNullOrEmpty(model.ColorMainHex))
            {
                txtColor.SetBackgroundColor(Color.ParseColor(model.ColorMainHex));
            }
        }
        private async Task AddClassSetFunction()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            CancellationToken cancellationToken;

            cancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                taskCompletionSource.TrySetCanceled();
            });

            model.UserId = UserId;
            model.TokGroup = SpinTokGroup.ContentDescription;
            model.TokType = EditClassName.Text;
            model.Privacy = SpinPrivacy.ContentDescription;
            model.Name = EditClassSetName.Text;
            model.Description = EditDescription.Text;

            if (!string.IsNullOrEmpty(ImgClassSet.ContentDescription))
            {
                if (!URLUtil.IsValidUrl(ImgClassSet.ContentDescription))
                {
                    model.Image = "data:image/jpeg;base64," + ImgClassSet.ContentDescription;
                }
            }

            model.GroupId = TextGroupName.ContentDescription;
            model.TokTypeId = $"toktype-{model.TokGroup.ToIdFormat()}-{model.TokType.ToIdFormat()}";

            TextProgress.Text = "Adding class set...";
            LinearProgress.Visibility = ViewStates.Visible;
            Instance.Window.AddFlags(WindowManagerFlags.NotTouchable);

            bool result = true;
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
                cancellationToken = cancellationTokenSource.Token;

                result = await ClassService.Instance.AddClassSetAsync(model, cancellationToken);

            }

            Instance.Window.ClearFlags(WindowManagerFlags.NotTouchable);
            LinearProgress.Visibility = ViewStates.Gone;

            if (!result)
            {
                showRetryDialog("Failed to save.");
            }
            else
            {
                string alertmessage = "";
                if (result)
                {
                    alertmessage = "Class set added!";
                }
                else
                {
                    alertmessage = "Failed to save.";
                }

                var builder = new AlertDialog.Builder(Instance);
                builder.SetMessage(alertmessage);
                builder.SetTitle("");
                var dialog = (Android.App.AlertDialog)null;
                builder.SetPositiveButton("OK" ?? "OK", (d, index) =>
                {
                    if (result == true)
                    {
                        myclasstoksets_fragment.Instance.PassItemClassSetsFromAddClassSet(model);
                        Settings.ActivityInt = Convert.ToInt16(ActivityType.LeftMenuSets);
                        this.Finish();
                    }
                });
                dialog = builder.Create();
                dialog.Show();
                if (result)
                {
                    dialog.SetCanceledOnTouchOutside(false);
                }
            }
        }

        private async Task EditClassSetFunction()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            CancellationToken cancellationToken;

            cancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                taskCompletionSource.TrySetCanceled();
            });

            model.TokGroup = SpinTokGroup.ContentDescription;
            model.TokType = EditClassName.Text;
            model.Privacy = SpinPrivacy.ContentDescription;
            model.Name = EditClassSetName.Text;
            model.Description = EditDescription.Text;

            if (!string.IsNullOrEmpty(ImgClassSet.ContentDescription))
            {
                if (!URLUtil.IsValidUrl(ImgClassSet.ContentDescription))
                {
                    model.Image = "data:image/jpeg;base64," + ImgClassSet.ContentDescription;
                }
                else
                {
                    model.Image = ImgClassSet.ContentDescription;
                }
            }

            model.GroupId = TextGroupName.ContentDescription;

            TextProgress.Text = "Updating class set...";
            LinearProgress.Visibility = ViewStates.Visible;
            Instance.Window.AddFlags(WindowManagerFlags.NotTouchable);

            bool result = true;
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
                cancellationToken = cancellationTokenSource.Token;

                result = await ClassService.Instance.UpdateClassSetAsync(model, cancellationToken);
            }

            this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
            LinearProgress.Visibility = ViewStates.Gone;

            if (!result)
            {
                showRetryDialog("Failed to update.");
            }
            else
            {
                string alertmessage = "";
                if (result)
                {
                    alertmessage = "Class set updated.";
                }
                else
                {
                    alertmessage = "Failed to update.";
                }

                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(alertmessage);
                builder.SetTitle("");
                var dialog = (AlertDialog)null;
                builder.SetPositiveButton("OK" ?? "OK", (d, index) =>
                {
                    if (result)
                    {
                        myclasstoksets_fragment.Instance.PassItemClassSetsFromAddClassSet(model);
                        Settings.ActivityInt = Convert.ToInt16(ActivityType.LeftMenuSets);
                        this.Finish();
                    }
                });
                dialog = builder.Create();
                dialog.Show();
                if (result)
                {
                    dialog.SetCanceledOnTouchOutside(false);
                }
            }
        }
        private void showRetryDialog(string message)
        {
            var builder = new Android.Support.V7.App.AlertDialog.Builder(this)
                            .SetMessage(message)
                            .SetPositiveButton("Cancel", (_, args) =>
                            {

                            })
                            .SetNegativeButton("Retry", async (_, args) =>
                            {
                                if (isSave)
                                {
                                    await AddClassSetFunction();
                                }
                                else
                                {
                                    await EditClassSetFunction();
                                }
                            })
                            .SetCancelable(false)
                            .Show();
        }

        private void LoadSpinners()
        {
            PrivacyList = new ObservableCollection<string>();
            PrivacyList.Clear();
            PrivacyList.Add("Public");
            PrivacyList.Add("Private");
        }
        private async Task LoadClassGroup()
        {
            var resultGroup = await ClassService.Instance.GetClassGroupAsync(new ClassGroupQueryValues() { partitionkeybase = "classgroups", startswith = false, userid = UserId });
            RecyclerGroupList.ContentDescription = resultGroup.ContinuationToken;
            var classgroupResult = resultGroup.Results.ToList();

            foreach (var item in classgroupResult)
            {
                ClassGroupCollection.Add(item);
            }
            var adapterClassGroup = ClassGroupCollection.GetRecyclerAdapter(BindClassGroupViewHolder, Resource.Layout.addclasssetgroup_row);
            RecyclerGroupList.SetAdapter(adapterClassGroup);

            if (!isSave) //Edit
            {
                var resultRequest = ClassGroupCollection.FirstOrDefault(c => c.Id == model.GroupId);
                if (resultRequest != null) //If Edit
                {
                    TextGroupName.Text = resultRequest.Name;
                }
            }
        }
        private void BindClassGroupViewHolder(CachingViewHolder holder, ClassGroupModel model, int position)
        {
            var ClassGroupHeader = holder.FindCachedViewById<TextView>(Resource.Id.TextClassSetGroupName);
            var ClassGroupBody = holder.FindCachedViewById<TextView>(Resource.Id.TextClassSetGroupDescription);
            var LinearRow = holder.FindCachedViewById<LinearLayout>(Resource.Id.LinearClassSetGroupRow);

            LinearRow.Tag = position;
            ClassGroupHeader.Text = model.Name;
            ClassGroupBody.Text = model.Description;

            LinearRow.Click -= RowClicked;
            LinearRow.Click += RowClicked;
        }
        private void RowClicked(object sender, EventArgs e)
        {
            int position = 0;
            try { position = (int)(sender as LinearLayout).Tag; } catch { position = int.Parse((string)(sender as LinearLayout).Tag); }
            TextGroupName.Text = ClassGroupCollection[position].Name;
            TextGroupName.ContentDescription = ClassGroupCollection[position].Id;
        }
        private void SpinTokGroup_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            SpinTokGroup.ContentDescription = SpinTokGroup.GetItemAtPosition(SpinTokGroup.FirstVisiblePosition).ToString();
        }
        public void SpinPrivacy_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            SpinPrivacy.ContentDescription = SpinPrivacy.GetItemAtPosition(SpinPrivacy.FirstVisiblePosition).ToString();
        }

        private void BtnBrowseImageClick(object sender, EventArgs e)
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), (int)ActivityType.AddClassSetActivity);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == (int)ActivityType.AddClassSetActivity) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                Intent nextActivity = new Intent(this, typeof(CropImageActivity));
                Settings.ImageBrowseCrop = (string)uri;
                StartActivityForResult(nextActivity, requestCode);
            }
            else if ((requestCode == COLOR_REQUEST_CODE) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                var colorHex = data.GetStringExtra("color");
                model.ColorMainHex = colorHex;
                txtColor.SetBackgroundColor(Color.ParseColor(colorHex));
            }
        }
        public void displayImageBrowse()
        {
            byte[] imageByte = null;
            imageByte = Convert.FromBase64String(Settings.ImageBrowseCrop);
            model.Image = "data:image/jpeg;base64," +  Settings.ImageBrowseCrop;
            //ImgClassSet.SetImageBitmap((BitmapFactory.DecodeByteArray(imageByte, 0, imageByte.Length)));
            Glide.With(this).AsBitmap().Load(imageByte).Into(ImgClassSet);
            Settings.ImageBrowseCrop = null;
        }
        public LinearLayout LinearToolbar => FindViewById<LinearLayout>(Resource.Id.LinearClassSetToolbar);
        public Spinner SpinTokGroup => FindViewById<Spinner>(Resource.Id.txtAddClassSetTokGroup);
        public TextView TextTokGroupDescription => FindViewById<TextView>(Resource.Id.lblAddSetDescription);
        public EditText EditClassName => FindViewById<EditText>(Resource.Id.EditAddClassSetClassName);
        public TextView TextGroupName => FindViewById<TextView>(Resource.Id.TextGroupName);
        public RecyclerView RecyclerGroupList => FindViewById<RecyclerView>(Resource.Id.RecyclerGroupList);
        public Spinner SpinPrivacy => FindViewById<Spinner>(Resource.Id.txtAddClassSetPrivacy);
        public EditText EditClassSetName => FindViewById<EditText>(Resource.Id.txtAddClassSetName);
        public EditText EditDescription => FindViewById<EditText>(Resource.Id.txtAddClassSetDescription); 
        public ImageView ImgClassSet => FindViewById<ImageView>(Resource.Id.addClassset_imagebrowse);
        public Button BtnBrowse => FindViewById<Button>(Resource.Id.btnAddClassSet_btnBrowseImage);
        public TextView CancelClassSet => FindViewById<TextView>(Resource.Id.btnAddClassSetCancel);
        public TextView SaveClassSet => FindViewById<TextView>(Resource.Id.btnAddClassSetSave);
        public LinearLayout LinearProgress => FindViewById<LinearLayout>(Resource.Id.linear_addClasssetprogress);
        public TextView TextProgress => FindViewById<TextView>(Resource.Id.progressBarTextAddClassSet);
        public LinearLayout LinearGroup => FindViewById<LinearLayout>(Resource.Id.LinearGroup);
        public TextView classGroupName => FindViewById<TextView>(Resource.Id.classGroupName);
        public TextView txtColor => FindViewById<TextView>(Resource.Id.txtColor);
        public Button btnSelectColor => FindViewById<Button>(Resource.Id.btnSelectColor);
    }
}