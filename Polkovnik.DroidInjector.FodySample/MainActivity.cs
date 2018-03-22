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

        [MenuItem(Resource.Id.action_0)] private IMenuItem menuItem;
        [MenuItem(Resource.Id.action_1)] private IMenuItem menuItem1;
        [MenuItem(Resource.Id.action_2)] private IMenuItem menuItem2;
        [MenuItem(Resource.Id.action_3)] private IMenuItem menuItem3;
        [MenuItem(Resource.Id.action_4)] private IMenuItem menuItem4;
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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main, menu);

            Injector.InjectMenuItems(menu);

            menuItem.SetTitle("TitleSET");
            menuItem1.SetTitle("TitleSET");
            menuItem2.SetTitle("TitleSET");
            menuItem3.SetTitle("TitleSET");
            menuItem4.SetTitle("TitleSET");


            return true;
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

