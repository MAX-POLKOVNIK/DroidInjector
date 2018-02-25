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

        [MenuItem(Resource.Id.action_0)] private IMenuItem _menuItem0;
        [MenuItem(Resource.Id.action_1)] private IMenuItem _menuItem1;
        [MenuItem(Resource.Id.action_2)] private IMenuItem _menuItem2;
        [MenuItem(Resource.Id.action_3)] private IMenuItem _menuItem3 { get; set; }
        [MenuItem(Resource.Id.action_4, true)] private IMenuItem _menuItem4 { get; set; }
#pragma warning restore 649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.main);
            Injector.InjectViews();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //Injector.BindViewEvents();

            stopwatch.Stop();
            Console.WriteLine($"TOTAL: {stopwatch.ElapsedMilliseconds}");

            _myButton.Text = "That is working";
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main, menu);

            Injector.InjectMenuItems(menu);

            return base.OnCreateOptionsMenu(menu);
        }

        private void InjectTest(IMenu menu)
        {
            _menuItem0 = menu.FindItem(Resource.Id.action_0) ?? throw new Exception("Cant find 15");
            _menuItem1 = menu.FindItem(Resource.Id.action_1);
            _menuItem2 = menu.FindItem(Resource.Id.action_2);
            _menuItem3 = menu.FindItem(Resource.Id.action_3);
            _menuItem4 = menu.FindItem(Resource.Id.action_4) ?? throw new Exception("Cant find 15");
        }

        //[ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        //private void ButtonClick(object sender, EventArgs args)
        //{

        //}

        //[ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        //private void ButtonClick2(object sender, EventArgs args)
        //{

        //}

        //[ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        //private void ButtonClick3(object sender, EventArgs args)
        //{

        //}

        //[ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        //private void ButtonClick4(object sender, EventArgs args)
        //{

        //}

        [ViewEvent(Resource.Id.myEditText, typeof(EditText), nameof(EditText.TextChanged))]
        private void ButtonClick5(object sender, TextChangedEventArgs args)
        {
            Toast.MakeText(this, args.Text.ToString(), ToastLength.Short).Show();
        }

        [ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        private void ButtonClick5(object sender, EventArgs args)
        {
            Toast.MakeText(this, "5", ToastLength.Short).Show();
        }

        [ViewEvent(Resource.Id.myButton, typeof(Button), nameof(View.Click))]
        private void ButtonClick6(object sender, EventArgs args)
        {
            Toast.MakeText(this, "6", ToastLength.Short).Show();
        }

        //[ViewEvent(Resource.Id.myButton, typeof(TextView), nameof(TextView.TextChanged))]
        //private void ButtonClick7(object sender, EventArgs args)
        //{

        //}
    }
}

