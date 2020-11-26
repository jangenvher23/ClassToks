using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using Java.Lang;

namespace Tokkepedia
{
#if (_TOKKEPEDIA)
    [Activity(Theme = "@style/AppTheme", MainLauncher = true, Icon = "@drawable/tokkepedia_icon", NoHistory = true)]
#endif

#if (_CLASSTOKS)
    [Activity(Theme = "@style/AppTheme", MainLauncher = true, Icon = "@drawable/classtoks", NoHistory = true)]
#endif
    public class SplashActivity : BaseActivity
    {
        Action action;
        public Handler handler;
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.splash_screen);

            action = Callback;
            handler = new Handler(Looper.MainLooper, new CustomHandlerCallback(this));

#if (_CLASSTOKS)
            imgTokkepediaIcon.SetImageResource(Resource.Drawable.classtoks);
            imgTokkepediaLogo.SetImageResource(Resource.Drawable.classtok_text);
            linearBackground.SetBackgroundColor(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));
            linearBackground.SetBackgroundResource(Resource.Drawable.bg_classtok);

            ProgressBar progressSplash = FindViewById<ProgressBar>(Resource.Id.progressSplash);
            progressSplash.IndeterminateDrawable.SetColorFilter(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)), Android.Graphics.PorterDuff.Mode.Multiply);
#endif
            handler.SendEmptyMessageDelayed(0, 1000);
            Log.Debug(TAG, "SplashActivity.OnCreate");
        }

        void Callback()
        {
            // repost itself
            handler.PostDelayed(action, 1000);

            // do other stuff
        }
        public void startSplashActivity()
        {
            handler.SendEmptyMessageDelayed(1, 4000);
        }
        public void startMainActivity()
        {
            Intent mainIntent = new Intent(this, typeof(LoginActivity));
            this.StartActivity(mainIntent);
            this.Finish();
            handler.RemoveCallbacks(action);
            OverridePendingTransition(Resource.Animation.fade, Resource.Animation.hold);
        }
        public override void OnBackPressed()
        {
        }

        private ImageView imgTokkepediaLogo => FindViewById<ImageView>(Resource.Id.imgTokkepediaLogo);
        private ImageView imgTokkepediaIcon => FindViewById<ImageView>(Resource.Id.imgTokkepediaIcon);
        private LinearLayout linearBackground => FindViewById<LinearLayout>(Resource.Id.linearBackground);
    }
    public class CustomHandlerCallback : Java.Lang.Object, Handler.ICallback
    {
        private SplashActivity mainActivity;
        public CustomHandlerCallback(SplashActivity activity)
        {
            this.mainActivity = activity;
        }

        public bool HandleMessage(Message msg)
        {
            switch (msg.What)
            {
                case 0:
                    mainActivity.startSplashActivity();
                    break;
                case 1:
                    mainActivity.startMainActivity();
                    break;
            }
            return true;
        }
    }
}