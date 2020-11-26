using System;
using System.Collections.Generic;
using System.Text;

namespace Tokkepedia.Shared.Helpers
{
    public enum GameScheme { TokBlitz, TokBlast, TokBoom, AlphaGuess }

    public enum NavigationStack
    {
        Login = 0,
        Signup,
        Dashboard,
        Tutorial,
        Main,
        FirstScreen,
        ForgotPassword,
        ResetPassword,
        EmailSignUp,
    }

    public enum EditMode
    {
        Save = 1,
        Update
    }
    public enum ImageType
    {
        Image = 0,
        NonImage,
        Both
    }
    public enum Result
    {
        None = 0,
        Success,
        Failed,
        Error,
        Forbidden,
        NoInternet
    }

    public enum GroupFilter { AllGroup, OwnGroup, JoinedGroup, MyGroup }


    public enum FilterType
    {
        None = 0,
        TokType,
        Text,
        Category,
        Country,
        User,
        Group,
        Featured,
        Set,
        All,
        ClassToks,
        Standard,
        Recent,
        Tag
    }
    public enum FilterToks
    {
        Toks,
        Cards
    }
    public enum Toks
    {
        Category,
        TokGroup,
        TokType
    }
    public enum  NumDisplay
    {
        mobile,
        tablet
    }
    public enum ActivityType
    {
        HomePage,
        AddTokActivityType,
        AddSetActivityType,
        LeftMenuSets,
        MySetsView,
        TokInfo,
        TokSearch,
        AddSectionPage,
        ToksFragment,
        AddStickerDialogActivity,
        ReactionValuesActivity,
        ProfileActivity,
        ProfileTabActivity,
        AddClassTokActivity,
        AddClassGroupActivity,
        AddClassSetActivity,
        SignUpActivity,
        ClassGroupActivity,
        ClassGroupListActivity,
        AddGameSetActivity,
        AvatarsActivity
    }
    public enum MainTab
    {
        Home,
        Search,
        Notification,
        Profile
    }
    public enum PatchesTab
    {
        MyPatches,
        LevelTable,
        PatchColor
    }
    public enum RequestStatus { Pending, PendingInvite, Accepted, Declined, All }
    public enum Actions { None, OpenLink, OpenMedia, OpenHashTag, OpenUser }
    public enum FilterBy { None, Class, Category, Type }
}
