using CommonServiceLocator;
using Facebook.CoreKit;
using Foundation;
using Plugin.GoogleClient;
using UIKit;
using Xamarin.Forms;

namespace Tokkepedia.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register ("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate {

        //[Export("window")]
        public UIWindow window { get; set; }
        //public static UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);
        //public static UIViewController initialViewController;

        // [Export ("application:didFinishLaunchingWithOptions:")]
        public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
        {
            global::Xamarin.Forms.Forms.Init();
            Device.SetFlags(new string[] { "DragAndDrop_Experimental", "Shapes_Experimental" });

            GoogleClientManager.Initialize();

            window = new UIWindow(UIScreen.MainScreen.Bounds);   
            //initialViewController = Storyboard.InstantiateInitialViewController() as UINavigationController;
            //window.RootViewController = initialViewController;   
            App.Window = window;


            // define useragent android like
            string userAgent = "Mozilla/5.0 (Linux; Android 5.1.1; Nexus 5 Build/LMY48B; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/43.0.2357.65 Mobile Safari/537.36";

            // set default useragent
            NSDictionary dictionary = NSDictionary.FromObjectAndKey(NSObject.FromObject(userAgent), NSObject.FromObject("UserAgent"));
            NSUserDefaults.StandardUserDefaults.RegisterDefaults(dictionary);

            //// Color of the selected tab icon:
            //UITabBar.Appearance.SelectedImageTintColor = UIColor.FromRGB(0, 122, 255);

            //// Color of the tabbar background:
            //UITabBar.Appearance.BarTintColor = UIColor.FromRGB(247, 247, 247);

            return true;
        }

       
        //public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        //{
        //    // We need to handle URLs by passing them to their own OpenUrl in order to make the SSO authentication works.
        //    return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        //}
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            base.OpenUrl(app, url, options);
            return GoogleClientManager.OnOpenUrl(app, url, options);
        }
        
    }
}

