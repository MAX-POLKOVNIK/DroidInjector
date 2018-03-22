using System;
using System.Diagnostics;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Text;
using Android.Views;

namespace Polkovnik.DroidInjector.FodySample
{
    [Activity(Label = "Polkovnik.DroidInjector.FodySample", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
#pragma warning disable 649
        [View] private Button _myButton;
#pragma warning restore 649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.main);

            try
            {
                Injector.InjectViews();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Injector.BindViewEvents();

            stopwatch.Stop();
            Console.WriteLine($"TOTAL: {stopwatch.ElapsedMilliseconds}");

            _myButton.Text = "That is working";
        }
        
        [ViewEvent(Resource.Id.myEditText, typeof(EditText), nameof(EditText.TextChanged))]
        private void ButtonClick5(object sender, TextChangedEventArgs args)
        {
            Toast.MakeText(this, args.Text.ToString(), ToastLength.Short).Show();
        }

        [ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        private void ButtonClick5(object sender, EventArgs args)
        {
            StartActivity(typeof(SecondActivity));
        }

        [ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        private void ButtonClick6(object sender, EventArgs args)
        {
            Toast.MakeText(this, "6", ToastLength.Short).Show();
        }
    }
}

