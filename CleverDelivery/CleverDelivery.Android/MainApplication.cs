using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.CurrentActivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms.Core;

namespace CleverDelivery
{
    [Application]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        public static Activity AppContext
        {
            get { return CrossCurrentActivity.Current.Activity; }
        }

        public MainApplication(IntPtr handle, JniHandleOwnership transer)
          : base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);
            InitBuildSettings();
            InitGlobalLibraries();
#if DEBUG
            CoreSettings.CurrentBuild = "dev";
#elif QA
            CoreSettings.CurrentBuild = "qa";
#elif RELEASE
            CoreSettings.CurrentBuild = "prod";
#endif
        }

        public override void OnTerminate()
        {

            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityDestroyed(Activity activity)
        {
        }

        public void OnActivityPaused(Activity activity)
        {
        }

        public void OnActivityResumed(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityStarted(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityStopped(Activity activity)
        {
        }

        private void InitGlobalLibraries()
        {
            //CoreSettings.AppIcon = Resource.Drawable.icon;
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
        }

        private void InitBuildSettings()
        {


        }
    }
}