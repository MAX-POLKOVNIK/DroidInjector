using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;

namespace Polkovnik.DroidInjector.FodySample
{
    [Activity(Label = "Polkovnik.DroidInjector.FodySample", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        [View(Resource.Id.myButton)] private Button _button;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.Main);

            //InjectTest(GetView(FindViewById<View>(Android.Resource.Id.Content)));
            //
            
            Injector.InjectViews();
            
            //InjectTest(GetRootView());

        }

        private void InjectTest(View view)
        {
            _button = (Button)view.FindViewById(Resource.Id.myButton);
        }

        private View GetRootView()
        {
            return FindViewById(Android.Resource.Id.Content);
        }
    }
}

