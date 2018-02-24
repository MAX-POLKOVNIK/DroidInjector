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
        [View] private Button _myButton;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.Main);

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            ////Injector.InjectViews();

            //stopwatch.Stop();
            Injector.InjectViews();
        }

        [View(Resource.Id.myButton)] private Button Button { get; set; }
        [View(Resource.Id.myButton)] private Button Button2 { get; }

        private void InjectTest(View view)
        {
            Button = (Button)view.FindViewById(Resource.Id.myButton);
        }
    }
}

