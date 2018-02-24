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
        [View(Resource.Id.myButton)] private Button _button2;
        [View(Resource.Id.myButton)] private Button _button3;
        [View(Resource.Id.myButton)] private Button _button4;
        [View(Resource.Id.myButton)] private Button _button5;
        [View(Resource.Id.myButton)] private Button _button6;
        [View(Resource.Id.myButton)] private Button _button7;
        [View(Resource.Id.myButton)] private Button _button8;
        [View(Resource.Id.myButton)] private Button _button9;
        [View(Resource.Id.myButton)] private Button _button10;
        [View(Resource.Id.myButton)] private Button _button11;
        [View(Resource.Id.myButton)] private Button _button13;
        [View(Resource.Id.myButton)] private Button _button14;
        [View(Resource.Id.myButton)] private Button _button15;
        [View(Resource.Id.myButton)] private Button _button16;
        [View(Resource.Id.myButton)] private Button _button17;
        [View(Resource.Id.myButton)] private Button _button18;
        [View(Resource.Id.myButton)] private Button _button19;
        [View(Resource.Id.myButton)] private Button _button110;
        [View(Resource.Id.myButton)] private Button _button111;

        int count = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.Main);
            
            var view = FindViewById<View>(Android.Resource.Id.Content);

            var stopwatch2 = new Stopwatch();
            stopwatch2.Start();

            InjectTest(view);

            stopwatch2.Stop();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Injector.InjectViews(view);

            stopwatch.Stop();
        }

        private void InjectTest(View view)
        {
            _button = (Button)view.FindViewById(Resource.Id.myButton);
            _button2 = (Button)view.FindViewById(Resource.Id.myButton);
            _button3 = (Button)view.FindViewById(Resource.Id.myButton);
            _button4 = (Button)view.FindViewById(Resource.Id.myButton);
            _button5 = (Button)view.FindViewById(Resource.Id.myButton);
            _button6 = (Button)view.FindViewById(Resource.Id.myButton);
            _button7 = (Button)view.FindViewById(Resource.Id.myButton);
            _button8 = (Button)view.FindViewById(Resource.Id.myButton);
            _button9 = (Button)view.FindViewById(Resource.Id.myButton);
            _button10 = (Button)view.FindViewById(Resource.Id.myButton);
            _button11 = (Button)view.FindViewById(Resource.Id.myButton);
            _button13 = (Button)view.FindViewById(Resource.Id.myButton);
            _button14 = (Button)view.FindViewById(Resource.Id.myButton);
            _button15 = (Button)view.FindViewById(Resource.Id.myButton);
            _button16 = (Button)view.FindViewById(Resource.Id.myButton);
            _button17 = (Button)view.FindViewById(Resource.Id.myButton);
            _button18 = (Button)view.FindViewById(Resource.Id.myButton);
            _button19 = (Button)view.FindViewById(Resource.Id.myButton);
            _button110 = (Button)view.FindViewById(Resource.Id.myButton);
            _button111 = (Button)view.FindViewById(Resource.Id.myButton);
        }
    }
}

