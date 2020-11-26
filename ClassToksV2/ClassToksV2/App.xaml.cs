using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ClassToksV2.Services;
using ClassToksV2.Views;
using ClassToksV2.ViewModels;
using Tokkepedia.Shared.Models;
using ClassToksV2.Models;
using Xamarin.Essentials;

namespace ClassToksV2
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            //DependencyService.Register<MockDataStore>();

            //Check if logged in
            /*var isLoggedIn = CheckCredentials();*/

           /* if (isLoggedIn)*/
                MainPage = new AppShell();
           /* else
                MainPage = new LoginSignUpPage();*/
        }

        public static bool CheckCredentials()
        {
            var idtoken = SecureStorage.GetAsync("idtoken").GetAwaiter().GetResult();
            var refreshtoken = SecureStorage.GetAsync("refreshtoken").GetAwaiter().GetResult();
            var userid = SecureStorage.GetAsync("userid").GetAwaiter().GetResult();

            if (idtoken != null && refreshtoken != null && userid != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static AppShell AppShellInstance { get; set; }
        public static ToksViewModel Toks { get; set; }
        public static ClassTokXF TokParameter { get; set; }
        public static CollectionView TokParameterUIList { get; set; }
        public static ClassTokQueryValues TokFilter { get; set; } = new ClassTokQueryValues();

        public static SetsViewModel Sets { get; set; }
        public static ClassSetXF SetParameter { get; set; }

        public static GroupsViewModel Groups { get; set; }
        public static ClassGroupXF GroupParameter { get; set; }

        public static TokSectionsViewModel TokSections { get; set; }
        public static TokSectionXF TokSectionParameter { get; set; }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
