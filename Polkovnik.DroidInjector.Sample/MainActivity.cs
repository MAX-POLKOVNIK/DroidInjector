using System;
using System.Diagnostics;
using Android.App;
using Android.Widget;
using Android.OS;

namespace Polkovnik.DroidInjector.Sample
{
    [Activity(Label = "Polkovnik.DroidInjector.Sample", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        [View] private Button _myButton;

        [View(Resource.Id.myButton)] private Button _myButton2;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Injector.Instance.RegisterResourceClass<Resource, Resource.Id>();

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.main);

            _stopwatch.Start();

            this.InjectViews();
            this.BindViewActions();

            _stopwatch.Stop();
            Toast.MakeText(this, $"Injection takes: {_stopwatch.ElapsedMilliseconds} ms", ToastLength.Long).Show();

            _myButton.Text = "TExt";
        }

        [ViewClickEvent(Resource.Id.myButton)]
        private void Click(/*object a, EventArgs b*/)
        {
            Toast.MakeText(this, "Click", ToastLength.Short).Show();
        }

        [ViewEvent(Resource.Id.myButton, nameof(Button.Click))]
        private void CustomClick(object a, EventArgs b)
        {
            Toast.MakeText(this, "CustomClick", ToastLength.Short).Show();
        }
    }
}

