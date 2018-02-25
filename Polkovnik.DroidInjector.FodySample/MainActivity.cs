using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
            Injector.InjectViews();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Injector.BindViewEvents();

            stopwatch.Stop();

            _myButton.Text = "That is working";
        }
        
        private void InjectTest(View view)
        {
            var v = view.FindViewById(Resource.Id.myButton);
            var b = v == null;
            if (b)
            {
                //throw new Exception("Can't find view with bla bla ");
            }
            else
            {
                ((Button)v).Click += ButtonClick5;
                ((Button)v).Click += ButtonClick6;
            }
            //v = view.FindViewById(Resource.Id.myButton);
            //b = v == null;
            //if (b)
            //{
            //    //throw new Exception("Can't find view with bla bla ");
            //}
            //else
            //{
            //    ((View)v).Click += ButtonClick7;
            //    ((Button)v).Click += ButtonClick7;
            //}
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

