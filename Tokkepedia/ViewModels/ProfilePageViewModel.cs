using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using GalaSoft.MvvmLight;
using Tokkepedia.Adapters;
using Tokkepedia.Fragments;
using Tokkepedia.Helpers;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;

namespace Tokkepedia.ViewModels
{
    public class ProfilePageViewModel : ViewModelBase
    {
        #region Properties
        public string UserId { get; set; }
        public TokketUser tokketUser { get; set; }
        public long? userpoints, Coins;
        public string UserDisplayName;
        public AdapterFragment fragment { get; private set; }
        #endregion

        #region Commands
        public ImageView ProfileCoverPhoto { get; set; }
        public ImageView ProfileUserPhoto { get; set; }
        public Activity activity { get; set; }
        public GlideImgListener GListenerCover {get;set;}
        public GlideImgListener GListenerUserPhoto { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        ///     Constructors will be called during the Registration in ViewModelLocator (Applying Dependency Injection or Inversion of Controls)
        /// </summary>
        public ProfilePageViewModel()
        {
            tokketUser = new TokketUser(); // Initialized Model to avoid nullreference exception
        }
        #endregion

        #region Methods/Events
        public async Task Initialize()
        {
            if (Settings.ActivityInt == (int)ActivityType.ProfileTabActivity)
            {
                tokketUser = Settings.GetTokketUser();
            }
            else
            {
                tokketUser = AccountService.Instance.GetUser(UserId);
            }

            string userprofilepic = "", selectedavatar = "";
            bool? isAvatarProfilePic = false;

            if (UserId == Settings.GetTokketUser().Id)
            {
                if (Settings.GetTokketUser().AccountType == "group")
                {
                    selectedavatar = ""; // Settings.GetTokketSubaccount().SelectedAvatar;
                    isAvatarProfilePic = false; // Settings.GetTokketSubaccount().IsAvatarProfilePicture;
                    userpoints = Settings.GetTokketSubaccount().Points;
                    UserDisplayName = Settings.GetTokketSubaccount().SubaccountName;
                    Coins = Settings.GetTokketSubaccount().Coins;
                }
                else
                {
                    selectedavatar = tokketUser.SelectedAvatar;
                    isAvatarProfilePic = tokketUser.IsAvatarProfilePicture;
                    userpoints = tokketUser.Points;
                    UserDisplayName = tokketUser.DisplayName;
                    Coins = tokketUser.Coins;
                }
            }
            else
            {
                isAvatarProfilePic = tokketUser.IsAvatarProfilePicture;
                userpoints = tokketUser.Points;
                UserDisplayName = tokketUser.DisplayName;
                Coins = tokketUser.Coins;
            }

            if (isAvatarProfilePic == true)
            {
                var listavatar = new List<string>();

                listavatar.Add(selectedavatar);
                var result = await AvatarsService.Instance.AvatarsByIdsAsync(listavatar);
                var resultlist = result.Results.ToList();
                userprofilepic = resultlist[0].Image;
            }
            else
            {
                userprofilepic = tokketUser.UserPhoto;
            }

            GListenerUserPhoto = new GlideImgListener();
            GListenerUserPhoto.ParentActivity = activity;
            Glide.With(activity).Load(userprofilepic).Listener(GListenerUserPhoto).Apply(RequestOptions.NoTransformation().Placeholder(Resource.Drawable.Man3)).Into(ProfileUserPhoto);
            //Glide.With(activity).Load(tokketUser.CoverPhoto).Into(ProfileCoverPhoto);

            GListenerCover = new GlideImgListener();
            GListenerCover.ParentActivity = activity;

            Glide.With(activity)
                .Load(tokketUser.CoverPhoto)
                .Listener(GListenerCover)
                .Into(ProfileCoverPhoto);
        }
        #endregion
    }
}