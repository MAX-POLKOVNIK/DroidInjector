using System.Diagnostics;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;

namespace Polkovnik.DroidInjector.FodySample
{
    [Activity(Label = "Polkovnik.DroidInjector.FodySample", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        [View] private Button _myButton;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.Main);

            //InjectTest(GetView(FindViewById<View>(Android.Resource.Id.Content)));
            //

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Injector.InjectViews();
            
            stopwatch.Stop();

            //InjectTest(GetRootView());

        }

        private void InjectTest(View view)
        {
            _myButton = (Button)view.FindViewById(Resource.Id.myButton);
        }
    }
}

