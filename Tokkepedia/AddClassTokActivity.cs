using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Tokkepedia.Shared.Helpers;
using SharedService = Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using static Android.App.ActionBar;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Result = Tokkepedia.Shared.Helpers.Result;
using Tokkepedia.Shared.Models;
using Tokkepedia.Fragments;
using Newtonsoft.Json;
using Android.Graphics;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Helpers;
using Android.Webkit;
using Tokkepedia.Helpers;
using Android.Support.V7.Widget;
using System.Collections.ObjectModel;
using Android.Animation;
using Tokket.Tokkepedia.Tools;
using Tokkepedia.Shared.Services;
using static Android.Views.View;
using AndroidX.Core.Content;
using System.Threading;
using Color = Android.Graphics.Color;
using Android.Content.Res;
using System.IO;
using Android.Graphics.Drawables;

namespace Tokkepedia
{
    [Activity(Label = "Add Class Tok", Theme = "@style/AppTheme")]
    public class AddClassTokActivity : BaseActivity, View.IOnTouchListener
    {
        // Allows us to know if we should use MotionEvent.ACTION_MOVE
        private bool tracking = false;
        // The Position where our touch event started
        private float startY;
        internal static AddClassTokActivity Instance { get; private set; }
        ObservableCollection<AddTokDetailModel> DetailCollection { get; set; }
        ObservableCollection<TokSection> MegaCollection { get; set; }
        bool isSave = true; bool[] ArrAnswer; TokketUser tokketUser;
        ClassTokModel ClassTokModel; string classGroupId = ""; int selectedGroupPosition = 0;
        ClassGroupModel ClassGroupModel;
        ObservableCollection<ClassGroupModel> ClassGroupCollection;
        Dialog popupGroupDialog; GestureDetector gesturedetector;
        LinearLayout linearDialogParent;
        Dialog tileDialog;
        int COLOR_REQUEST_CODE = 1001;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.addclasstok_page);
            Instance = this;

            gesturedetector = new GestureDetector(this, new MyGestureListener(this));

            Settings.ActivityInt = (int)ActivityType.AddClassTokActivity;
            tokketUser = await SharedService.AccountService.Instance.GetUserAsync(Settings.GetUserModel().UserId);

            ResetAll();

            //This is good for saving.
            this.SetBinding(() => ClassTokModel.TokType, () => TokType.Text, BindingMode.TwoWay);
            this.SetBinding(() => ClassTokModel.Category, () => Category.Text, BindingMode.TwoWay);
            this.SetBinding(() => ClassTokModel.PrimaryFieldText, () => Primary.Text, BindingMode.TwoWay);
            this.SetBinding(() => ClassTokModel.SecondaryFieldText, () => Secondary.Text, BindingMode.TwoWay);
            this.SetBinding(() => ClassTokModel.Notes, () => Notes.Text, BindingMode.TwoWay);

            //Load TokMoji
            RecyclerDetail.SetLayoutManager(new LinearLayoutManager(this));

            TokGroup.ItemSelected -= new EventHandler<AdapterView.ItemSelectedEventArgs>(Cbxtokgroup_ItemSelected);
            TokGroup.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(Cbxtokgroup_ItemSelected);
            string[] arryTokGroup = new string[] { "Basic","Detailed","Mega"};
            var Aadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, arryTokGroup);
            Aadapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            TokGroup.Adapter = Aadapter;

            isSave = Intent.GetBooleanExtra("isSave", true);
            string classgroupmodelstr = Intent.GetStringExtra("ClassGroupModel");
            if (classgroupmodelstr != null)
            {
                ClassGroupModel = JsonConvert.DeserializeObject<ClassGroupModel>(classgroupmodelstr);
                if (ClassGroupModel != null)
                {
                    ClassTokModel.GroupId = ClassGroupModel.Id;
                }
            }
            
            if (isSave == false)
            {
                this.RunOnUiThread(async () => await EditClassTok());
            }
            else
            {
#if (_CLASSTOKS)
                if (MainActivity.Instance.tabLayout.SelectedTabPosition == 0 || MainActivity.Instance.tabLayout.SelectedTabPosition == 1) //Home || Search
                {
                    if (Settings.FilterFeed == (int)FilterType.All)
                    {
                        chkPublic.Checked = true;
                    }
                    else
                    {
                        chkPrivate.Checked = true;
                    }
                }
                else if (MainActivity.Instance.tabLayout.SelectedTabPosition == 2) //Profile
                {
                    chkPrivate.Checked = true;
                }
#endif
            }

            //If Save Button is clicked
            SaveClassTok.Click += SaveClassTok_IsClicked;

            BtnCancelTok.Click += (object sender, EventArgs e) =>
            {
                this.Finish();
            };

            //chkPublic.Checked = ClassTokModel.IsPublic;
            //chkPrivate.Checked = ClassTokModel.IsPrivate;
            //chkGroup.Checked = ClassTokModel.IsGroup;
            txtChkPublic.SetOnTouchListener(this);
            txtChkPrivate.SetOnTouchListener(this);
            txtChkGroup.SetOnTouchListener(this);
            chkPublic.Click += delegate
            {
                ClassTokModel.IsPublic = chkPublic.Checked;
                changeSaveText();
            };

            chkPrivate.Click += delegate
            {
                ClassTokModel.IsPrivate = chkPrivate.Checked;
                changeSaveText();
            };

            chkGroup.Click += delegate
            {
                ClassTokModel.IsGroup = chkGroup.Checked;
                changeSaveText();
            };

            btnSelectColor.Click += delegate
            {
                if (!isSave)
                {
                    if (ClassTokModel.UserId != Settings.GetTokketUser().Id)
                    {
                        return;
                    }
                }
                var nextActivity = new Intent(this, typeof(ColorSelectionActivity));
                nextActivity.PutExtra("className", ClassTokModel.TokType);
                nextActivity.PutExtra("color", ClassTokModel.ColorMainHex);
                nextActivity.PutExtra("keyvalue", ClassTokModel.TokType);
                this.StartActivityForResult(nextActivity, COLOR_REQUEST_CODE);
            };

            btnPreviewTile.Click += btnPreview_IsClicked;

            if (!string.IsNullOrEmpty(ClassTokModel.ColorMainHex))
            {
                txtColor.SetBackgroundColor(Color.ParseColor(ClassTokModel.ColorMainHex));
            }
        }

        private void btnPreview_IsClicked(object sender, EventArgs e)
        {
            tileDialog = new Dialog(this);
            tileDialog.SetContentView(Resource.Layout.listview_row);
            tileDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            tileDialog.Show();

            tileDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            tileDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);

            assignValuesPreviewTile();

        }

        private async void SaveClassTok_IsClicked(object sender, EventArgs e)
        {
            string ClassTokGroup = TokGroup.GetItemAtPosition(TokGroup.FirstVisiblePosition).ToString();
            if (chkGroup.Checked && string.IsNullOrEmpty(classGroupId))
            {
                dialogBoxOk("Group is checked and no Class Group is selected. Double tap the word GROUP next to checkbox to view the list of groups.");
                return;
            }

            if (!chkPublic.Checked && !chkPrivate.Checked && !chkGroup.Checked)
            {
                dialogBoxOk("Please select a feed.");
                return;
            }
            ClassTokModel.IsPublic = chkPublic.Checked;
            ClassTokModel.IsPrivate = chkPrivate.Checked;
            ClassTokModel.IsGroup = chkGroup.Checked;
            ClassTokModel.GroupId = classGroupId;

            if (!string.IsNullOrEmpty(ImageDisplay.ContentDescription))
            {
                if (!URLUtil.IsValidUrl(ImageDisplay.ContentDescription))
                {
                    ClassTokModel.Image = "data:image/jpeg;base64," + ImageDisplay.ContentDescription;
                }
            }

            if (isSave) //If Add
            {
                ClassTokModel.TokGroup = ClassTokGroup;
                ClassTokModel.TokType = TokType.Text;
                ClassTokModel.CategoryId = "category-" + ClassTokModel.Category?.ToIdFormat();
                ClassTokModel.TokTypeId = $"toktype-{ClassTokModel.TokGroup?.ToIdFormat()}-{ClassTokModel.TokType?.ToIdFormat()}";
                ClassTokModel.UserState = Settings.GetTokketUser().State;
            }

            if (ClassTokGroup.ToLower() == "detailed")
            {
                List<string> detailList = new List<string>();
                ClassTokModel.Details = new string[DetailCollection.Count];
                for (int d = 0; d < DetailCollection.Count; d++)
                {
                    detailList.Add(DetailCollection[d].Detail);

                }
                ClassTokModel.Details = detailList.ToArray();
            }

            ResultModel result = new ResultModel();
            showProgress();

            //Image
            if (ClassTokModel.DetailImages != null)
            {
                for (int i = 0; i < ClassTokModel.DetailImages.Length; i++)
                {
                    if (!string.IsNullOrEmpty(ClassTokModel.DetailImages[i]))
                    {
                        if (!URLUtil.IsValidUrl(ClassTokModel.DetailImages[i]))
                        {
                            ClassTokModel.DetailImages[i] = "data:image/jpeg;base64," + ClassTokModel.DetailImages[i];
                        }
                    }
                }
            }

            var taskCompletionSource = new TaskCompletionSource<ResultModel>();
            CancellationToken cancellationToken;
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
            cancellationToken = cancellationTokenSource.Token;

            cancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                taskCompletionSource.TrySetCanceled();
            });

            if (isSave)
            {
                //API
                ClassTokModel.Label = "classtok";
                ProgressText.Text = "Saving...";
                result = await ClassService.Instance.AddClassToksAsync(ClassTokModel, cancellationToken);
                if (result.ResultMessage == "cancelled")
                {
                    showRetryDialog("Task was cancelled.");
                }
                else
                {
                    var serClassTokResult = JsonConvert.SerializeObject(result.ResultObject);
                    var desClassTokResult = JsonConvert.DeserializeObject<ClassTokModel>(serClassTokResult);
                    ClassTokModel = desClassTokResult;
                }
            }
            else
            {
                //API
                ProgressText.Text = "Updating...";
                result = await ClassService.Instance.UpdateClassToksAsync(ClassTokModel, cancellationToken);

                if (result.ResultMessage == "cancelled")
                {
                    showRetryDialog("Task was cancelled.");
                }
            }

            //Saving Sections
            if (result.ResultEnum == Result.Success)
            {
                if (ClassTokGroup.ToLower() == "mega") //If Mega
                {
                    ResultData<TokSection> OrigTokSectionResult =  await TokService.Instance.GetTokSectionsAsync(ClassTokModel.Id);
                    var OrigTokSection = OrigTokSectionResult.Results;
                    bool isSuccess = false;
                    var cnt = 0;
                    foreach (var sec in MegaCollection)
                    {
                        //Progress Text
                        Thread.Sleep(200);
                        double val1 = (double)(cnt + 1) / (double)MegaCollection.Count;
                        var val2 = val1 * 100;
                        int percent = (int)val2;
                        ProgressText.Text = percent.ToString() + " %";

                        if (string.IsNullOrEmpty(sec.Id.Trim())) //Save
                        {
                            if (isSave)
                            {
                                var updatedTok = result.ResultObject as TokModel;
                                ClassTokModel.Id = updatedTok.Id;
                            }
                            var dummySec = new TokSection();
                            sec.Id = dummySec.Id;
                            sec.TokId = ClassTokModel.Id;
                            sec.TokTypeId = ClassTokModel.TokTypeId;
                            sec.UserId = Settings.GetUserModel().UserId;

                            isSuccess = await TokService.Instance.CreateTokSectionAsync(sec, ClassTokModel.Id, 0);
                        }
                        else
                        {
                            var resultSection = OrigTokSection.FirstOrDefault(c => c.Id == sec.Id);
                            if (resultSection != null) //If Edit
                            {
                                if (sec.Title == resultSection.Title && sec.Content == resultSection.Content && sec.Image == resultSection.Image) //Check if changes have been made, disregard update to avoid calling API
                                {
                                    //disregard update
                                }
                                else
                                {
                                    isSuccess = await TokService.Instance.UpdateTokSectionAsync(sec);
                                }
                            }
                        }
                        cnt += 1;
                    }
                }
            }
            hideProgress();

            if (result.ResultMessage != "cancelled")
            {
                var objBuilder = new AlertDialog.Builder(this);
                objBuilder.SetTitle("");
                objBuilder.SetMessage(result.ResultEnum.ToString());
                objBuilder.SetCancelable(false);

                AlertDialog objDialog = objBuilder.Create();
                objDialog.SetButton((int)(DialogButtonType.Positive), "OK", (s, ev) =>
                {
                    if (result.ResultEnum == Result.Success)
                    {
                        if (!isSave) //TokInfo
                        {
                            if (ClassTokGroup.ToLower() == "mega") //If Mega
                            {
                                var modelSerialized = JsonConvert.SerializeObject(MegaCollection.ToList());
                                Intent intent = new Intent();
                                intent.PutExtra("toksection", modelSerialized);
                                SetResult(Android.App.Result.Ok, intent);
                            }
                            else
                            {
                                classtoks_fragment.Instance.AddClassTokCollection(ClassTokModel);
                                var modelSerialized = JsonConvert.SerializeObject(ClassTokModel);
                                Intent intent = new Intent();
                                intent.PutExtra("classtokModel", modelSerialized);
                                SetResult(Android.App.Result.Ok, intent);
                            }
                        }
                        else
                        {
                            classtoks_fragment.Instance.AddClassTokCollection(ClassTokModel);
                            profile_fragment.Instance.AddTokCollection(ClassTokModel, null);
                        }

                        this.Finish();
                    }
                });
                objDialog.Show();
                objDialog.SetCanceledOnTouchOutside(false);
            }
        }

        private void showRetryDialog(string message)
        {
            var builder = new AlertDialog.Builder(MainActivity.Instance)
                            .SetMessage(message)
                            .SetPositiveButton("Cancel", (_, args) =>
                            {

                            })
                            .SetNegativeButton("Retry", (_, args) =>
                            {
                                SaveClassTok_IsClicked(_, args);
                            })
                            .SetCancelable(false)
                            .Show();
        }

        private void changeSaveText()
        {
            int cntCheck = 0;
            if (chkPublic.Checked) cntCheck += 1;
            if (chkPrivate.Checked) cntCheck += 1;
            if (chkGroup.Checked) cntCheck += 1;

            if (cntCheck == 0 || cntCheck == 2 || cntCheck == 3)
            {
                SaveClassTok.Text = "Add Class Tok";
            }
            else
            {
                if (chkPublic.Checked)
                {
                    SaveClassTok.Text = "Add Public Class Tok";
                }
                else if (chkPrivate.Checked)
                {
                    SaveClassTok.Text = "Add Private Class Tok";
                }
                else if (chkGroup.Checked)
                {
                    SaveClassTok.Text = "Add Class Tok to Group";
                }
            }
        }
        private void showProgress()
        {
            LinearProgress.Visibility = ViewStates.Visible;
            this.Window.AddFlags(WindowManagerFlags.NotTouchable);
            ProgressBarCircle.IndeterminateDrawable.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)), Android.Graphics.PorterDuff.Mode.Multiply);
        }
        private void hideProgress()
        {
            this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
            LinearProgress.Visibility = ViewStates.Gone;
        }
        private void ResetAll()
        {
            DetailCollection = new ObservableCollection<AddTokDetailModel>();
            MegaCollection = new ObservableCollection<TokSection>();
            ClassTokModel = new ClassTokModel();

            //User Data
            ClassTokModel.UserDisplayName = tokketUser.DisplayName;
            ClassTokModel.UserId = Settings.GetUserModel().UserId;
            ClassTokModel.UserCountry = tokketUser.Country;
            ClassTokModel.UserPhoto = tokketUser.UserPhoto;
            ImageDisplay.ContentDescription = "";

            AddTokDetailModel TokDetail = new AddTokDetailModel();
            DetailCollection.Add(TokDetail);

            TokDetail = new AddTokDetailModel();
            DetailCollection.Add(TokDetail);

            TokDetail = new AddTokDetailModel();
            DetailCollection.Add(TokDetail);

            var TokDetailMega = new TokSection();
            MegaCollection.Add(TokDetailMega);

            ArrAnswer = new bool[10];
        }
        private void dialogBoxOk(string message)
        {
            var alertDiag = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertDiag.SetTitle("");
            alertDiag.SetIcon(Resource.Drawable.alert_icon_blue);
            alertDiag.SetMessage(message);
            alertDiag.SetPositiveButton("OK", (senderAlert, args) => {
                alertDiag.Dispose();
            });
            Dialog diag = alertDiag.Create();
            diag.Show();
            diag.SetCanceledOnTouchOutside(false);
        }
        private void Cbxtokgroup_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            ClassTokModel.IsMega = false;
            ClassTokModel.IsDetailBased = false;
            ClassTokModel.TokGroup = TokGroup.GetItemAtPosition(TokGroup.FirstVisiblePosition).ToString();
            switch (TokGroup.GetItemAtPosition(e.Position).ToString().ToLower())
            {
                case "basic":
                    LinearDetail.Visibility = ViewStates.Gone;
                    LinearSecondary.Visibility = ViewStates.Visible;
                    linearNotes.Visibility = ViewStates.Visible;
                    break;

                case "detailed":
                    ClassTokModel.IsDetailBased = true;
                    SetDetailRecyclerAdapter();
                    LinearSecondary.Visibility = ViewStates.Gone;
                    LinearDetail.Visibility = ViewStates.Visible;
                    linearNotes.Visibility = ViewStates.Gone;
                    break;

                case "mega":
                    var adapterMega = MegaCollection.GetRecyclerAdapter(BindAddClassTokMega, Resource.Layout.addtokmegadetail_row);
                    RecyclerDetail.SetAdapter(adapterMega);
                    ClassTokModel.IsMega = true;
                    LinearDetail.Visibility = ViewStates.Visible;
                    LinearSecondary.Visibility = ViewStates.Gone;
                    linearNotes.Visibility = ViewStates.Gone;
                    break;
            }
        }
        private void BindAddClassTokDetail(CachingViewHolder holder, AddTokDetailModel model, int position)
        {
            var DeleteImage = holder.FindCachedViewById<Button>(Resource.Id.btnDeltokdtl_img);
            var AddImage = holder.FindCachedViewById<ImageView>(Resource.Id.btnAddtokdtl_img);
            var CheckAnswer = holder.FindCachedViewById<CheckBox>(Resource.Id.chkAddTokChkDtlField);
            var HeaderDetail = holder.FindCachedViewById<TextView>(Resource.Id.lblAddTokDetail1);
            var EnglishHeader = holder.FindCachedViewById<TextView>(Resource.Id.lblAddTokDetailEng1);
            var Detail = holder.FindCachedViewById<EditText>(Resource.Id.txtAddTokDetailField);
            var EnglishDetail = holder.FindCachedViewById<EditText>(Resource.Id.txtAddTokDetailFieldEngTrans);
            var btnDeleteDetail = holder.FindCachedViewById<TextView>(Resource.Id.btnAddTok_deletedtlField1);
            var detailBinding = new Binding<string, string>(model,
                                                  () => model.Detail,
                                                  Detail,
                                                  () => Detail.Text,
                                                  BindingMode.TwoWay);

            var englishdetailBinding = new Binding<string, string>(model,
                                                  () => model.EnglishDetail,
                                                  EnglishDetail,
                                                  () => EnglishDetail.Text,
                                                  BindingMode.TwoWay);

            var chkAnswerBinding = new Binding<bool, bool>(model,
                                                 () => model.ChkAnswer,
                                                 CheckAnswer,
                                                 () => CheckAnswer.Checked,
                                                 BindingMode.TwoWay);
            CheckAnswer.Checked = ArrAnswer[position];
            CheckAnswer.Visibility = ViewStates.Gone;
            AddImage.Tag = position;
            if (position < 3) //Default detail is 3
            {
                btnDeleteDetail.Visibility = ViewStates.Gone;
            }
            else
            {
                btnDeleteDetail.Visibility = ViewStates.Visible;
            }

            holder.FindCachedViewById<CheckBox>(Resource.Id.chkAddTokChkDtlField).Tag = position;
            holder.FindCachedViewById<Button>(Resource.Id.btnDeltokdtl_img).Tag = position;

            if (!CheckAnswer.Checked)
            {
                HeaderDetail.Text = "Detail " + (position + 1);
            }
            else
            {
                HeaderDetail.Text = "Answer";
            }

            holder.FindCachedViewById<TextView>(Resource.Id.btnAddTok_deletedtlField1).Tag = position;

            if (ClassTokModel.DetailImages != null)
            {
                if (position < ClassTokModel.DetailImages.Length)
                {
                    if (!string.IsNullOrEmpty(ClassTokModel.DetailImages[position]))
                    {
                        AddImage.SetImageBitmap(null);
                        DeleteImage.Visibility = ViewStates.Visible;
                    }

                    if (URLUtil.IsValidUrl(ClassTokModel.DetailImages[position]))
                    {
                        ImageDownloaderHelper.AssignImageAsync(AddImage, ClassTokModel.DetailImages[position], this);
                    }
                    else
                    {
                        byte[] imageDetailBytes = Convert.FromBase64String(ClassTokModel.DetailImages[position] ?? "");
                        AddImage.SetImageBitmap((BitmapFactory.DecodeByteArray(imageDetailBytes, 0, imageDetailBytes.Length)));
                    }
                }
            }

            //if (ChkTokEnglish.Checked == false)
            //{
            //    EnglishHeader.Text = "English Translation";
            //    EnglishHeader.Visibility = ViewStates.Visible;
            //    holder.FindCachedViewById<TextInputLayout>(Resource.Id.inputlayoutDetailEnglishTrans).Visibility = ViewStates.Visible;
            //}
            //else
            //{
                EnglishHeader.Visibility = ViewStates.Gone;
                holder.FindCachedViewById<TextInputLayout>(Resource.Id.inputlayoutDetailEnglishTrans).Visibility = ViewStates.Gone;
            //}
        }
        private void BindAddClassTokMega(CachingViewHolder holder, TokSection model, int position)
        {
            var Title = holder.FindCachedViewById<EditText>(Resource.Id.EdittxtAddTokMegaTitle);
            var Content = holder.FindCachedViewById<EditText>(Resource.Id.txtAddTokMegaContent);
            var Image = holder.FindCachedViewById<ImageView>(Resource.Id.btnAddtokmegadtl_displayimg);
            var RelaveAddTokMegaImg = holder.FindCachedViewById<RelativeLayout>(Resource.Id.RelaveAddTokMegaImg);
            var SectionNumber = holder.FindCachedViewById<TextView>(Resource.Id.txtAddTokMegaNumber);

            SectionNumber.Text = (position + 1).ToString();

            var titleBinding = new Binding<string, string>(model,
                                              () => model.Title,
                                              Title,
                                              () => Title.Text,
                                              BindingMode.TwoWay);
            var contentBinding = new Binding<string, string>(model,
                                              () => model.Content,
                                              Content,
                                              () => Content.Text,
                                              BindingMode.TwoWay);

            Image.ContentDescription = position.ToString();
            if (URLUtil.IsValidUrl(model.Image))
            {
                Glide.With(this).Load(model.Image).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).FitCenter()).Into(Image);
            }
            else
            {
                Image.Tag = position;
                byte[] imageDetailBytes = Convert.FromBase64String(model.Image ?? "");
                Image.SetImageBitmap((BitmapFactory.DecodeByteArray(imageDetailBytes, 0, imageDetailBytes.Length)));
            }

            Image.Click += delegate
            {
                ParentImageViewer.TranslationY = LinearProgress.TranslationY;
                ImgProfileImageView.SetImageDrawable(Image.Drawable);

                ParentImageViewer.Visibility = ViewStates.Visible;
            };

            if (model.Image != null)
            {
                Image.SetBackgroundColor(Color.ParseColor("#3498db"));
                RelaveAddTokMegaImg.Visibility = ViewStates.Visible;
            }

            //Adding tag
            holder.FindCachedViewById<ImageView>(Resource.Id.btnAddtokmegadtl_img).Tag = position;
            var DeleteBtn = holder.FindCachedViewById<Button>(Resource.Id.btnAddTokMega_deletedtlField1);
            DeleteBtn.Tag = position;
            holder.FindCachedViewById<TextInputLayout>(Resource.Id.inputlayoutAddTokMegaTitle).Tag = position;
            holder.FindCachedViewById<EditText>(Resource.Id.EdittxtAddTokMegaTitle).Tag = position;
            holder.FindCachedViewById<TextInputLayout>(Resource.Id.inputlayoutAddTokMegaContent).Tag = position;
            Title.Tag = position;
            holder.FindCachedViewById<ImageView>(Resource.Id.btnAddTokMega_deleteImg).Tag = position;
            holder.FindCachedViewById<RelativeLayout>(Resource.Id.RelaveAddTokMegaImg).Tag = position;

            if (position > 0)
            {
                DeleteBtn.Visibility = ViewStates.Visible;
            }
        }
        private async Task EditClassTok()
        {
            ClassTokModel = JsonConvert.DeserializeObject<ClassTokModel>(Intent.GetStringExtra("ClassTokModel"));
            this.SetBinding(() => ClassTokModel.TokType, () => TokType.Text, BindingMode.TwoWay);
            this.SetBinding(() => ClassTokModel.Category, () => Category.Text, BindingMode.TwoWay);
            this.SetBinding(() => ClassTokModel.PrimaryFieldText, () => Primary.Text, BindingMode.TwoWay);
            this.SetBinding(() => ClassTokModel.SecondaryFieldText, () => Secondary.Text, BindingMode.TwoWay);
            this.SetBinding(() => ClassTokModel.Notes, () => Notes.Text, BindingMode.TwoWay);

            chkPrivate.Checked = Convert.ToBoolean(ClassTokModel.IsPrivate);
            chkGroup.Checked = ClassTokModel.IsGroup;
            chkPublic.Checked = ClassTokModel.IsPublic;

            chkPrivate.ButtonTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.btnDisable)));
            chkGroup.ButtonTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.btnDisable)));
            chkPublic.ButtonTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.btnDisable)));
            chkPrivate.Enabled = false;
            chkGroup.Enabled = false;
            chkPublic.Enabled = false;

            //Tok Group
            var spinnerTGPosition = 0;
            if (ClassTokModel.TokGroup.ToLower() == "basic")
            {
                spinnerTGPosition = 0;
            }
            else if (ClassTokModel.TokGroup.ToLower() == "detail" || ClassTokModel.TokGroup.ToLower() == "detailed")
            {
                spinnerTGPosition = 1;
            }
            else if (ClassTokModel.TokGroup.ToLower() == "mega")
            {
                spinnerTGPosition = 2;
            }

            TokGroup.SetSelection(spinnerTGPosition);
            TokGroup.Enabled = false;

            TokType.Text = ClassTokModel.TokType;
            if (!String.IsNullOrEmpty(ClassTokModel.GroupName))
            {
                classGroupName.Visibility = ViewStates.Visible;
                classGroupName.Text = "Class Group: " + ClassTokModel.GroupName;
            }

            Category.Text = ClassTokModel.Category;
            Primary.Text = ClassTokModel.PrimaryFieldText;
            Secondary.Text = ClassTokModel.SecondaryFieldText;
            ImageDisplay.ContentDescription = ClassTokModel.Image;
            if (!string.IsNullOrEmpty(ClassTokModel.Image))
            {
                Glide.With(this).Load(ClassTokModel.Image).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation)).Into(ImageDisplay);
            }
            
            SaveClassTok.Text = "Update Class Tok";

            if (ClassTokModel.IsMegaTok == true || ClassTokModel.TokGroup.ToLower() == "mega") //If Mega
            {
                var GetToksSecResult = await SharedService.TokService.Instance.GetTokSectionsAsync(ClassTokModel.Id);
                var GetToksSec = GetToksSecResult.Results;
                ClassTokModel.Sections = GetToksSec.ToArray();

                MegaCollection.Clear();
                foreach (var item in ClassTokModel.Sections)
                {
                    MegaCollection.Add(item);
                }

                var adapterMega = MegaCollection.GetRecyclerAdapter(BindAddClassTokMega, Resource.Layout.addtokmegadetail_row);
                RecyclerDetail.SetAdapter(adapterMega);
            }
            else if (ClassTokModel.TokGroup.ToLower() == "detailed" || ClassTokModel.TokGroup.ToLower() == "detail")
            {
                DetailCollection.Clear();

                foreach (var item in ClassTokModel.Details)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var tokdetailModel = new AddTokDetailModel();
                        tokdetailModel.Detail = item;
                        DetailCollection.Add(tokdetailModel);
                    }
                }

                //If total image is lesser than the details, add new
                if (ClassTokModel.DetailImages != null)
                {
                    var detailImages = ClassTokModel.DetailImages.ToList();
                    for (int i = detailImages.Count; i < 10; i++)
                    {
                        detailImages.Add(null);
                    }
                    ClassTokModel.DetailImages = detailImages.ToArray();
                }

                SetDetailRecyclerAdapter();
            }
        }
        private void SetDetailRecyclerAdapter()
        {
            var adapterDetail = DetailCollection.GetRecyclerAdapter(BindAddClassTokDetail, Resource.Layout.addtokdetail_row);
            RecyclerDetail.SetAdapter(adapterDetail);
        }
        [Java.Interop.Export("OnClickAddDetail")]
        public void OnClickAddDetail(View v)
        {
            if (TokGroup.GetItemAtPosition(TokGroup.FirstVisiblePosition).ToString().ToLower() == "mega") //If Mega
            {
                var TokDetailMega = new TokSection();
                TokDetailMega.Id = "";
                MegaCollection.Add(TokDetailMega);
                var adapterMega = MegaCollection.GetRecyclerAdapter(BindAddClassTokMega, Resource.Layout.addtokmegadetail_row);
                RecyclerDetail.SetAdapter(adapterMega);
            }
            else
            {
                var TokDetail = new AddTokDetailModel();
                DetailCollection.Add(TokDetail);
                SetDetailRecyclerAdapter();
                LinearDetail.Visibility = ViewStates.Visible;

                AddDetailButton.Visibility = Android.Views.ViewStates.Visible;

                if (RecyclerDetail.ChildCount == 10)
                {
                    AddDetailButton.Visibility = Android.Views.ViewStates.Gone;
                }
            }
        }
        [Java.Interop.Export("OnCheckAddTok")]
        public void OnCheckAddTok(View v)
        {
            //Clear all checked
            int vtag = (int)v.Tag;

            ArrAnswer[vtag] = (v as CheckBox).Checked;
            //SetDetailRecyclerAdapter();
        }
        [Java.Interop.Export("OnDelete")] // The value found in android:onClick attribute.
        public void OnDelete(View v) // Does not need to match value in above attribute.
        {
            int vtag = (int)v.Tag;
            if (TokGroup.GetItemAtPosition(TokGroup.FirstVisiblePosition).ToString().ToLower() == "mega") //If Mega
            {
                MegaCollection.RemoveAt(vtag);
                RecyclerDetail.RemoveViewAt(vtag);

                for (int i = 0; i < RecyclerDetail.ChildCount; ++i)
                {
                    View z = RecyclerDetail.GetChildAt(i);

                    //Adding tag
                    z.FindViewById<ImageView>(Resource.Id.btnAddtokmegadtl_img).Tag = i;
                    z.FindViewById<Button>(Resource.Id.btnAddTokMega_deletedtlField1).Tag = i;
                    z.FindViewById<TextInputLayout>(Resource.Id.inputlayoutAddTokMegaTitle).Tag = i;
                    z.FindViewById<EditText>(Resource.Id.EdittxtAddTokMegaTitle).Tag = i;
                    z.FindViewById<ImageView>(Resource.Id.btnAddtokmegadtl_displayimg).Tag = i;
                    z.FindViewById<TextInputLayout>(Resource.Id.inputlayoutAddTokMegaContent).Tag = i;
                    z.FindViewById<EditText>(Resource.Id.EdittxtAddTokMegaTitle).Tag = i;
                    z.FindViewById<ImageView>(Resource.Id.btnAddTokMega_deleteImg).Tag = i;
                    z.FindViewById<RelativeLayout>(Resource.Id.RelaveAddTokMegaImg).Tag = i;
                    z.FindViewById<TextView>(Resource.Id.txtAddTokMegaNumber).Text = (i + 1).ToString();
                }
            }
            else
            {
                DetailCollection.RemoveAt(vtag);
                RecyclerDetail.RemoveViewAt(vtag);

                for (int i = 0; i < RecyclerDetail.ChildCount; ++i)
                {
                    View view = RecyclerDetail.GetChildAt(i);
                    var CheckAnswer = view.FindViewById<CheckBox>(Resource.Id.chkAddTokChkDtlField);
                    var HeaderDetail = view.FindViewById<TextView>(Resource.Id.lblAddTokDetail1);
                    if (!CheckAnswer.Checked)
                    {
                        HeaderDetail.Text = "Detail " + (i + 1);
                    }
                    else
                    {
                        HeaderDetail.Text = "Answer";
                    }

                    view.FindViewById<CheckBox>(Resource.Id.chkAddTokChkDtlField).Tag = i;
                    view.FindViewById<ImageView>(Resource.Id.btnAddtokdtl_img).Tag = i;
                    view.FindViewById<Button>(Resource.Id.btnDeltokdtl_img).Tag = i;
                    view.FindViewById<TextView>(Resource.Id.btnAddTok_deletedtlField1).Tag = i;
                }

                AddDetailButton.Visibility = Android.Views.ViewStates.Visible;

                if (ClassTokModel.DetailImages != null)
                {
                    if (vtag < ClassTokModel.DetailImages.Length)
                    {
                        ClassTokModel.DetailImages.ToList().RemoveAt(vtag);
                    }
                }
            }
        }
        [Java.Interop.Export("OnDeleteImageDtl")]
        public void OnDeleteImageDtl(View v)
        {
            int vtag = (int)v.Tag;

            for (int x = vtag; x == vtag; ++x)
            {
                View view;
                if (TokGroup.GetItemAtPosition(TokGroup.FirstVisiblePosition).ToString().ToLower() == "mega") //If Mega
                {
                    view = RecyclerDetail.GetChildAt(x);
                    RelativeLayout RelaveAddTokMegaImg = view.FindViewById<RelativeLayout>(Resource.Id.RelaveAddTokMegaImg);
                    RelaveAddTokMegaImg.Visibility = ViewStates.Gone;
                }
                else
                {
                    view = RecyclerDetail.GetChildAt(x);
                    ImageView btnAddtokdtl_img = view.FindViewById<ImageView>(Resource.Id.btnAddtokdtl_img);
                    btnAddtokdtl_img.SetImageResource(Resource.Drawable.add_image_icon);
                    v.Visibility = ViewStates.Gone;

                    if (ClassTokModel.DetailImages.Length <= (vtag + 1))
                    {
                        ClassTokModel.DetailImages[vtag] = null;
                    }
                }
            }
        }
        [Java.Interop.Export("OnClickAddTokImgDetail")]
        public void OnClickAddTokImgDetail(View v)
        {
            Settings.BrowsedImgTag = (int)v.Tag;//(int)v.FindViewById<ImageView>(Resource.Id.btnAddtokdtl_img).Tag;
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), (int)ActivityType.AddTokActivityType);
        }

        [Java.Interop.Export("OnClickAddTokImgMain")]
        public void OnClickAddTokImgMain(View v)
        {
            Settings.BrowsedImgTag = -1;
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), (int)ActivityType.AddTokActivityType);
        }
        [Java.Interop.Export("OnClickRemoveTokImgMain")]
        public void OnClickRemoveTokImgMain(View v)
        {
            ImageDisplay.SetImageBitmap(null);
            ClassTokModel.Image = null;
            ImageDisplay.ContentDescription = "";
            BrowseImgButton.Visibility = ViewStates.Visible;
            RemoveImgButton.Visibility = ViewStates.Gone;
        }
        [Java.Interop.Export("OnClickCloseImgView")]
        public void OnClickCloseImgView(View v)
        {
            ParentImageViewer.Visibility = ViewStates.Gone;
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == (int)ActivityType.AddTokActivityType) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                Intent nextActivity = new Intent(this, typeof(CropImageActivity));
                Settings.ImageBrowseCrop = (string)uri;
                this.StartActivityForResult(nextActivity, requestCode);

                if (Settings.BrowsedImgTag != -1)
                {
                    int vtag = Settings.BrowsedImgTag;

                    if (TokGroup.GetItemAtPosition(TokGroup.FirstVisiblePosition).ToString().ToLower() == "mega") //If Mega
                    {
                    }
                    else
                    {
                        for (int x = vtag; x == vtag; ++x)
                        {
                            View view = RecyclerDetail.GetChildAt(x);
                            Button btnDeltokdtl_img = view.FindViewById<Button>(Resource.Id.btnDeltokdtl_img);
                            btnDeltokdtl_img.Visibility = ViewStates.Visible;
                        }
                    }
                }
            }
            else if ((requestCode == COLOR_REQUEST_CODE) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                var colorHex = data.GetStringExtra("color");
                ClassTokModel.ColorMainHex = colorHex;
                txtColor.SetBackgroundColor(Color.ParseColor(colorHex));
            }
        }
        public void displayImageBrowse()
        {
            //Main Image
            ImageDisplay.SetImageBitmap(null);
            if (Settings.BrowsedImgTag == -1)
            {
                //AddTokVm.TokModel.Image = Settings.ImageBrowseCrop;
                ImageDisplay.ContentDescription = Settings.ImageBrowseCrop;
                byte[] imageMainBytes = Convert.FromBase64String(Settings.ImageBrowseCrop);
                ImageDisplay.SetImageBitmap((BitmapFactory.DecodeByteArray(imageMainBytes, 0, imageMainBytes.Length)));
                BrowseImgButton.Visibility = ViewStates.Gone;
                RemoveImgButton.Visibility = ViewStates.Visible;
            }
            else
            {
                //Detail Image
                int detailpos = Settings.BrowsedImgTag; //Position of Control
                byte[] imageDetailBytes = Convert.FromBase64String(Settings.ImageBrowseCrop);

                if (TokGroup.GetItemAtPosition(TokGroup.FirstVisiblePosition).ToString().ToLower() == "mega") //If Mega
                {
                    View view = RecyclerDetail.GetChildAt(detailpos);
                    view.FindViewById<RelativeLayout>(Resource.Id.RelaveAddTokMegaImg).Visibility = ViewStates.Visible;
                    ImageView btnAddtokdtl_img = view.FindViewById<ImageView>(Resource.Id.btnAddtokmegadtl_displayimg);

                    btnAddtokdtl_img.SetImageBitmap((BitmapFactory.DecodeByteArray(imageDetailBytes, 0, imageDetailBytes.Length)));
                    MegaCollection[detailpos].Image = Settings.ImageBrowseCrop;
                }
                else
                {
                    if (ClassTokModel.DetailImages != null)
                    {
                        var detailImages = ClassTokModel.DetailImages.ToList();
                        for (int i = detailImages.Count; i < detailpos; i++)
                        {
                            detailImages.Add(null);
                        }
                        ClassTokModel.DetailImages = detailImages.ToArray();

                        for (int x = detailpos; x == detailpos; ++x)
                        {
                            View view = RecyclerDetail.GetChildAt(x);
                            ImageView btnAddtokdtl_img = view.FindViewById<ImageView>(Resource.Id.btnAddtokdtl_img);
                            btnAddtokdtl_img.Visibility = ViewStates.Visible;
                            ClassTokModel.DetailImages[x] = Settings.ImageBrowseCrop;
                            btnAddtokdtl_img.SetImageBitmap((BitmapFactory.DecodeByteArray(imageDetailBytes, 0, imageDetailBytes.Length)));

                        }
                    }
                }
            }
            Settings.ImageBrowseCrop = null;
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
                                //Hide ParentImageViewer
                                ParentImageViewer.Visibility = ViewStates.Gone;
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

        public bool OnTouch(View v, MotionEvent e)
        {
            if (e.Action == MotionEventActions.Up)
            {
                if (v.ContentDescription == "privacy")
                {
                    LinearToast.Visibility = ViewStates.Gone;
                    if (v.Id == Resource.Id.txtChkGroup)
                    {
                        if (chkGroup.Checked)
                        {
                            popupGroupDialog = new Dialog(this);
                            popupGroupDialog.SetContentView(Resource.Layout.dialog_classgroup_list);
                            popupGroupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
                            popupGroupDialog.Show();

                            popupGroupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
                            popupGroupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);

                            var btnCancel = popupGroupDialog.FindViewById<Button>(Resource.Id.btnCancel);
                            btnCancel.Click += delegate
                            {
                                classGroupId = "";
                                popupGroupDialog.Dismiss();
                            };

                            dialogRecyclerGroup.SetLayoutManager(new GridLayoutManager(Application.Context, 1));

                            ClassGroupCollection = new ObservableCollection<ClassGroupModel>();
                            RunOnUiThread(async () => await InitializeGroupList());
                        }
                    }
                }
            }
            else if (e.Action == MotionEventActions.Down)
            {
                if (v.ContentDescription == "privacy")
                {
                    LinearToast.Visibility = ViewStates.Visible;

                    if (!isSave)
                    {
                        TextToast.Text = "*Cannot edit the tok's feed.*";
                    }
                    else
                    {
                        switch (v.Id)
                        {
                            case Resource.Id.txtChkPrivate:
                                TextToast.Text = "Only you can see this class tok.";
                                break;
                            case Resource.Id.txtChkPublic:
                                TextToast.Text = "Anyone can see this class tok.";
                                break;
                            case Resource.Id.txtChkGroup:
                                TextToast.Text = "Only users in the selected group can see this class tok.";
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else if (e.Action == MotionEventActions.Cancel)
            {
                if (v.ContentDescription == "privacy")
                {
                    LinearToast.Visibility = ViewStates.Gone;
                }
            }
            return true;
        }
        void recyclerTouchEvent(object sender, TouchEventArgs e)
        {
            int position = (int)(sender as View).Tag;
            selectedGroupPosition = position;
            classGroupId = ClassGroupCollection[position].Id;
            gesturedetector.OnTouchEvent(e.Event);
        }
        private async Task InitializeGroupList()
        {
            dialogProgress.Visibility = ViewStates.Visible;
            GroupFilter filter = (GroupFilter)Settings.FilterGroup;
            ResultData<ClassGroupModel> results = new ResultData<ClassGroupModel>();

            switch (filter)
            {
                case GroupFilter.OwnGroup:
                    results = await ClassService.Instance.GetClassGroupAsync(new ClassGroupQueryValues()
                    {
                        paginationid = null,
                        partitionkeybase = "classgroups",
                        userid = Settings.GetTokketUser().Id,
                        startswith = false,
                        joined = false
                    });
                    break;
                case GroupFilter.JoinedGroup:
                    results = await ClassService.Instance.GetClassGroupAsync(new ClassGroupQueryValues()
                    {
                        paginationid = null,
                        partitionkeybase = "classgroups",
                        userid = Settings.GetTokketUser().Id,
                        joined = true,
                        startswith = false
                    });
                    break;
                case GroupFilter.MyGroup:
                    var myGroups = await ClassService.Instance.GetClassGroupAsync(new ClassGroupQueryValues()
                    {
                        paginationid = null,
                        partitionkeybase = "classgroups",
                        userid = Settings.GetTokketUser().Id,
                        joined = false,
                        startswith = false
                    });

                    var joined = await ClassService.Instance.GetClassGroupAsync(new ClassGroupQueryValues()
                    {
                        paginationid = null,
                        partitionkeybase = "classgroups",
                        userid = Settings.GetTokketUser().Id,
                        joined = true,
                        startswith = false
                    });

                    var combined = myGroups.Results.ToList();
                    combined.AddRange(joined.Results);

                    results.Results = combined;

                    break;
            }

            dialogRecyclerGroup.ContentDescription = results.ContinuationToken;
            var classgroupResult = results.Results.ToList();

            foreach (var item in classgroupResult)
            {
                ClassGroupCollection.Add(item);
            }

            ClassGroupModel items = new ClassGroupModel();
            items.Name = "James";
            items.Description = "SSDF";
            ClassGroupCollection.Add(items);

            SetGroupRecyclerAdapter();
        }

        [Java.Interop.Export("OnAddTokSticker")]
        public void OnAddTokSticker(View v)
        {
            ClassTokModel.UserCountry = tokketUser.UserPhoto;
            ClassTokModel.UserPhoto = tokketUser.UserPhoto;
            ClassTokModel.UserDisplayName = tokketUser.DisplayName;
            if (isSave)
            {
                ClassTokModel.TokGroup = TokGroup.GetItemAtPosition(TokGroup.FirstVisiblePosition).ToString();
            }

            ClassTokModel.TokTypeId = $"toktype-{ClassTokModel.TokGroup?.ToIdFormat()}-{ClassTokModel.TokType?.ToIdFormat()}";

            TokModel tokModel = ClassTokModel;
            var modelConvert = JsonConvert.SerializeObject(tokModel);
            Intent nextActivity = new Intent(this, typeof(AddStickerDialogActivity));
            nextActivity.PutExtra("tokModel", modelConvert);
            this.StartActivity(nextActivity);
        }

        private class MyGestureListener : GestureDetector.SimpleOnGestureListener
        {
            private AddClassTokActivity mainActivity;

            public MyGestureListener(AddClassTokActivity Activity)
            {
                mainActivity = Activity;
            }

            public override bool OnDoubleTap(MotionEvent e)
            {
                //Console.WriteLine("Double Tab");
                var alertDiag = new Android.Support.V7.App.AlertDialog.Builder(mainActivity);
                alertDiag.SetTitle("");
                alertDiag.SetIcon(Resource.Drawable.alert_icon_blue);
                alertDiag.SetMessage("Selected Group: " + mainActivity.ClassGroupCollection[mainActivity.selectedGroupPosition].Name);
                alertDiag.SetPositiveButton("Cancel", (senderAlert, args) => {
                    alertDiag.Dispose();
                });
                alertDiag.SetNegativeButton(Html.FromHtml("<font color='#007bff'>Proceed</font>", FromHtmlOptions.ModeLegacy), (senderAlert, args) => {
                    alertDiag.Dispose();
                    mainActivity.popupGroupDialog.Dismiss();
                });
                Dialog diag = alertDiag.Create();
                diag.Show();
                diag.SetCanceledOnTouchOutside(false);
                return true;
            }
        }
        private void SetGroupRecyclerAdapter()
        {
            var adapterClassGroup = ClassGroupCollection.GetRecyclerAdapter(BindClassGroupViewHolder, Resource.Layout.dialog_classgroup_list_item);
            dialogRecyclerGroup.SetAdapter(adapterClassGroup);

            dialogProgress.Visibility = ViewStates.Invisible;
        }

        private void BindClassGroupViewHolder(CachingViewHolder holder, ClassGroupModel model, int position)
        {
            var txtHeader = holder.FindCachedViewById<TextView>(Resource.Id.txtHeader);
            var txtBody = holder.FindCachedViewById<TextView>(Resource.Id.txtBody);
            linearDialogParent = holder.FindCachedViewById<LinearLayout>(Resource.Id.linearDialogParent);
            linearDialogParent.Tag = position;
            linearDialogParent.Touch -= recyclerTouchEvent;
            linearDialogParent.Touch += recyclerTouchEvent;
            txtHeader.Text = model.Name;
            txtBody.Text = model.Description;
        }

        private void assignValuesPreviewTile()
        {
            Stream sr = null;
            if (!string.IsNullOrEmpty(Settings.GetTokketUser().Country))
            {
                try
                {
                    sr = Assets.Open("Flags/" + Settings.GetTokketUser().Country + ".jpg");
                }
                catch (Exception)
                {

                }
            }
            Bitmap bitmap = BitmapFactory.DecodeStream(sr);
            if (!string.IsNullOrEmpty(ImageDisplay.ContentDescription))
            {
                string tokimg = ImageDisplay.ContentDescription;

                byte[] imageDetailBytes = Convert.FromBase64String(tokimg);
                tileTokImgMain.SetImageBitmap((BitmapFactory.DecodeByteArray(imageDetailBytes, 0, imageDetailBytes.Length)));


                Glide.With(this).Load(ClassTokModel.UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(tileTokImgUserPhoto);
                tileTokImgUserPhoto.SetOnTouchListener(this);
                tileTokImgUserFlag.SetImageBitmap(bitmap);
                tileImgUserDisplayName.Text = ClassTokModel.UserDisplayName;
                tileTokImgPrimaryFieldText.Text = ClassTokModel.PrimaryFieldText;

                tileTokImgCategory.Text = ClassTokModel.Category;
                tileTokImgTokGroup.Text = ClassTokModel.TokGroup;
                tileTokImgTokType.Text = ClassTokModel.TokType;

                tilegridTokImage.SetBackgroundResource(Resource.Drawable.tileview_layout);
                GradientDrawable tokimagedrawable = (GradientDrawable)tiletokimgdrawable.Background;
                tokimagedrawable.SetColor(Color.White);

                tilegridTokImage.Visibility = ViewStates.Visible;
                tilegridBackground.Visibility = ViewStates.Gone;
            }
            else
            {

                Glide.With(this).Load(ClassTokModel.UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(tileUserPhoto);
                tileUserFlag.SetImageBitmap(bitmap);

                tileUserDisplayName.Text = ClassTokModel.UserDisplayName;
                tilePrimaryFieldText.Text = ClassTokModel.PrimaryFieldText;
                tileCategory.Text = ClassTokModel.Category;
                tileTokGroup.Text = ClassTokModel.TokGroup;
                tileTokType.Text = ClassTokModel.TokType;
                if (string.IsNullOrEmpty(ClassTokModel.EnglishPrimaryFieldText))
                {
                    tileEnglishPrimaryFieldText.Visibility = ViewStates.Gone;
                }
                else
                {
                    tileEnglishPrimaryFieldText.Visibility = ViewStates.Visible;
                }
                tileEnglishPrimaryFieldText.Text = ClassTokModel.EnglishPrimaryFieldText;
                tilegridBackground.SetBackgroundResource(Resource.Drawable.tileview_layout);
                GradientDrawable Tokdrawable = (GradientDrawable)tileTokdrawable.Background;

                if (ClassTokModel.ColorMainHex == "#FFFFFF"  || string.IsNullOrEmpty(ClassTokModel.ColorMainHex))
                {
                    Tokdrawable.SetColor(Color.White);
                    setTextColor(Color.Black);
                }
                else
                {
                    Tokdrawable.SetColor(Color.ParseColor(ClassTokModel.ColorMainHex));
                    setTextColor(Color.White);
                }

                tilegridBackground.Visibility = ViewStates.Visible;
                tilegridTokImage.Visibility = ViewStates.Gone;
            }
        }
        private void setTextColor(Color color)
        {
            tileUserDisplayName.SetTextColor(color);
            tilePrimaryFieldText.SetTextColor(color);
            tileCategory.SetTextColor(color);
            tileTokGroup.SetTextColor(color);
            tileTokType.SetTextColor(color);
            tileEnglishPrimaryFieldText.SetTextColor(color);
        }

        #region UI Properties
        public Button btnPreviewTile => FindViewById<Button>(Resource.Id.btnPreviewTile);
        public TextView txtColor => FindViewById<TextView>(Resource.Id.txtColor);
        public Button btnSelectColor => FindViewById<Button>(Resource.Id.btnSelectColor);
        public Spinner TokGroup => FindViewById<Spinner>(Resource.Id.txtAddClassTokType);
        public EditText TokType => FindViewById<EditText>(Resource.Id.txtAddClassTokClass);
        public EditText Category => FindViewById<EditText>(Resource.Id.txtAddClassTokCategory);
        public EditText Primary => FindViewById<EditText>(Resource.Id.txtAddClassTokPrimary);
        public EditText Secondary => FindViewById<EditText>(Resource.Id.txtAddClassTokSecondary);
        public EditText Notes => FindViewById<EditText>(Resource.Id.txtAddClassTokNotes);
        public LinearLayout linearNotes => FindViewById<LinearLayout>(Resource.Id.linearNotes);
        public ImageView ImageDisplay => FindViewById<ImageView>(Resource.Id.addclasstok_imagebrowse);
        public Button BrowseImgButton => FindViewById<Button>(Resource.Id.btnAddClassTok_btnBrowseImage);
        public Button RemoveImgButton => FindViewById<Button>(Resource.Id.btnAddClassTokRemoveImgMain);
        public LinearLayout LinearDetail => FindViewById<LinearLayout>(Resource.Id.LinearAddClassTokDetail); 
        public LinearLayout LinearSecondary => FindViewById<LinearLayout>(Resource.Id.LinearAddClassTokSecondary);
        public RecyclerView RecyclerDetail => FindViewById<RecyclerView>(Resource.Id.RecyclerAddClassTok);
        public Button AddDetailButton => FindViewById<Button>(Resource.Id.btnAddClassTokAddDetail); 
        public TextView SaveClassTok => FindViewById<TextView>(Resource.Id.btnAddClassTokSave);
        public ProgressBar ProgressBarCircle => FindViewById<ProgressBar>(Resource.Id.progressbarAddClassTok);
        public TextView ProgressText => FindViewById<TextView>(Resource.Id.progressBarTextAddClassTok);
        public LinearLayout LinearProgress => FindViewById<LinearLayout>(Resource.Id.LinearProgress_addclasstok);
        public TextView BtnCancelTok => FindViewById<TextView>(Resource.Id.btnAddClassTokCancel);
        public RelativeLayout ParentImageViewer => FindViewById<RelativeLayout>(Resource.Id.ParentImageViewer);
        public ImageView ImgProfileImageView => FindViewById<ImageView>(Resource.Id.ImgProfileImageView);
        public TextView classGroupName => FindViewById<TextView>(Resource.Id.classGroupName);
        public CheckBox chkPublic => FindViewById<CheckBox>(Resource.Id.chkPublic);
        public CheckBox chkPrivate => FindViewById<CheckBox>(Resource.Id.chkPrivate);
        public CheckBox chkGroup => FindViewById<CheckBox>(Resource.Id.chkGroup);
        public TextView txtChkGroup => FindViewById<TextView>(Resource.Id.txtChkGroup);
        public TextView txtChkPublic => FindViewById<TextView>(Resource.Id.txtChkPublic);
        public TextView txtChkPrivate => FindViewById<TextView>(Resource.Id.txtChkPrivate);
        public LinearLayout LinearToast => FindViewById<LinearLayout>(Resource.Id.LinearToast);
        public TextView TextToast => FindViewById<TextView>(Resource.Id.TextToast);
        public RelativeLayout dialogProgress => popupGroupDialog.FindViewById<RelativeLayout>(Resource.Id.relativeProgress);
        public RecyclerView dialogRecyclerGroup => popupGroupDialog.FindViewById<RecyclerView>(Resource.Id.recyclerClassGroupList);
        #endregion

        #region Preview Tile UI
        public GridLayout tilegridBackground =>  tileDialog.FindViewById<GridLayout>(Resource.Id.gridBackground);
        public GridLayout tilegridTokImage => tileDialog.FindViewById<GridLayout>(Resource.Id.gridTokImage);
        public GridLayout tileTokdrawable => tileDialog.FindViewById<GridLayout>(Resource.Id.gridBackground);
        public GridLayout tiletokimgdrawable => tileDialog.FindViewById<GridLayout>(Resource.Id.gridTokImage);
        public ImageView tileUserPhoto => tileDialog.FindViewById<ImageView>(Resource.Id.imageUserPhoto);
        public ImageView tileUserFlag => tileDialog.FindViewById<ImageView>(Resource.Id.imageFlag);
        public TextView tileUserDisplayName => tileDialog.FindViewById<TextView>(Resource.Id.lbl_nameuser);
        public TextView tilePrimaryFieldText => tileDialog.FindViewById<TextView>(Resource.Id.lbl_row);
        public TextView tileSecondaryFieldText => tileDialog.FindViewById<TextView>(Resource.Id.secondarytext_row);
        public TextView tileEnglishPrimaryFieldText => tileDialog.FindViewById<TextView>(Resource.Id.lbl_englishPrimaryFieldText);
        public TextView tileCategory => tileDialog.FindViewById<TextView>(Resource.Id.lblCategory);
        public TextView tileTokGroup => tileDialog.FindViewById<TextView>(Resource.Id.lblTokGroup);
        public TextView tileTokType => tileDialog.FindViewById<TextView>(Resource.Id.lblTokType);
        public ImageView tileImgPurpleGem => tileDialog.FindViewById<ImageView>(Resource.Id.toktile_imgpurplegem);
        public ImageView tileTileSticker =>  tileDialog.FindViewById<ImageView>(Resource.Id.imgtile_stickerimage);
        public TextView tileTokUserTitle => tileDialog.FindViewById<TextView>(Resource.Id.lbl_royaltitle);

        //Tok Image
        public ImageView tileTokImgUserPhoto => tileDialog.FindViewById<ImageView>(Resource.Id.imageTokImgUserPhoto);
        public ImageView tileTokImgUserFlag => tileDialog.FindViewById<ImageView>(Resource.Id.img_tokimgFlag);
        public ImageView tileTokImgMain => tileDialog.FindViewById<ImageView>(Resource.Id.imgTokImgMain);
        public TextView tileImgUserDisplayName => tileDialog.FindViewById<TextView>(Resource.Id.lbl_Imgnameuser);
        public TextView tileTokImgPrimaryFieldText => tileDialog.FindViewById<TextView>(Resource.Id.lbl_tokimgprimarytext);
        public TextView tileTokImgSecondaryFieldText => tileDialog.FindViewById<TextView>(Resource.Id.lbl_tokimgsecondarytext);
        public TextView tileTokImgCategory => tileDialog.FindViewById<TextView>(Resource.Id.lblTokImgCategory);
        public TextView tileTokImgTokGroup => tileDialog.FindViewById<TextView>(Resource.Id.lblTokImgGroup);
        public TextView tileTokImgTokType => tileDialog.FindViewById<TextView>(Resource.Id.lblTokImgType);
        public ImageView tileTileStickerImg => tileDialog.FindViewById<ImageView>(Resource.Id.imgtile_stickerimageImg);
        public TextView tileTokUserTitleImg => tileDialog.FindViewById<TextView>(Resource.Id.lbl_royaltitleImg);
        public ImageView tileImgPurpleGemTokImg => tileDialog.FindViewById<ImageView>(Resource.Id.toktile_imgpurplegemtokimg);
        #endregion
    }
}