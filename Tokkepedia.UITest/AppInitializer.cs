using System;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Tokkepedia.UITest
{
    public class AppInitializer
    {
        public static IApp StartApp(Platform platform)
        {
            // TODO: If the iOS or Android app being tested is included in the solution 
            // then open the Unit Tests window, right click Test Apps, select Add App Project
            // and select the app projects that should be tested.
            //
            // The iOS project should have the Xamarin.TestCloud.Agent NuGet package
            // installed. To start the Test Cloud Agent the following code should be
            // added to the FinishedLaunching method of the AppDelegate:
            //
            //    #if ENABLE_TEST_CLOUD
            //    Xamarin.Calabash.Start();
            //    #endif
            if (platform == Platform.Android)
            {
                // TODO: Update this path to point to your Android app and uncomment the
                // code if the app is not included in the solution.
                return ConfigureApp
                    .Android.ApkFile("C:/Users/bonqu/source/github/TokkepediaMobile/Tokkepedia/Tokkepedia/bin/Debug/com.tokket.tokkepedia.apk").StartApp(); //This depends on your computer File Explorer
                    //.StartApp();
            }

            return ConfigureApp
                .iOS
                // TODO: Update this path to point to your iOS app and uncomment the
                // code if the app is not included in the solution.
                //.AppBundle("../../../iOS/bin/iPhoneSimulator/Debug/TokBlitz.iOS.app")
                .StartApp();
        }
    }
}