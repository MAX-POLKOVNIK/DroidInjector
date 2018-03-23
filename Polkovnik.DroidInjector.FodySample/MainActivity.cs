using System;
using System.Diagnostics;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Text;
using Android.Views;
using CheeseBind;
using Genetics;
using Genetics.Attributes;
using Polkovnik.DroidInjector.FodyClassLibrarySample;

namespace Polkovnik.DroidInjector.FodySample
{
    [Activity(Label = "Polkovnik.DroidInjector.FodySample", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
#pragma warning disable 649
        [BindView(Resource.Id.myButton)] [Splice(Resource.Id.myButton)] [View] private Button _myButton;
        //[BindView(Resource.Id.myEditText1)] [Splice(Resource.Id.myEditText1)] [View] private EditText myEditText1;
        //[BindView(Resource.Id.myEditText2)] [Splice(Resource.Id.myEditText2)] [View] private EditText myEditText2;
        //[BindView(Resource.Id.myEditText3)] [Splice(Resource.Id.myEditText3)] [View] private EditText myEditText3;
        //[BindView(Resource.Id.myEditText4)] [Splice(Resource.Id.myEditText4)] [View] private EditText myEditText4;
        //[BindView(Resource.Id.myEditText5)] [Splice(Resource.Id.myEditText5)] [View] private EditText myEditText5;
        //[BindView(Resource.Id.myEditText6)] [Splice(Resource.Id.myEditText6)] [View] private EditText myEditText6;
        //[BindView(Resource.Id.myEditText7)] [Splice(Resource.Id.myEditText7)] [View] private EditText myEditText7;
        //[BindView(Resource.Id.myEditText8)] [Splice(Resource.Id.myEditText8)] [View] private EditText myEditText8;
        //[BindView(Resource.Id.myEditText9)] [Splice(Resource.Id.myEditText9)] [View] private EditText myEditText9;
        //[BindView(Resource.Id.myEditText10)][Splice(Resource.Id.myEditText10)] [View] private EditText myEditText10;
        //[BindView(Resource.Id.myEditText11)] [Splice(Resource.Id.myEditText11)] [View] private EditText myEditText11;

        [MenuItem(Resource.Id.action_0)] private IMenuItem item;
#pragma warning restore 649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.main);
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //DirectInjection();
            Injector.InjectViews();
            Injector.BindViewEvents();
            //Geneticist.Splice(this);
            //Cheeseknife.Bind(this);

            //InjectWrapper("1", Injector.InjectViews);

            stopwatch.Stop();
            Console.WriteLine($"TOTAL: {stopwatch.ElapsedMilliseconds} ms");

            //_myButton.Text = $"TOTAL: {stopwatch.ElapsedMilliseconds} ms";
            _myButton.Click += (sender, args) => StartActivity(typeof(TestActivity));
            FragmentManager.BeginTransaction().Replace(Resource.Id.contentLayout, MySalesFragment.NewInstance())
                .Commit();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main, menu);
            Injector.InjectMenuItems(menu);
            return true;
        }

        private void InjectWrapper(string nane, Action action)
        {
            action();
        }

        //[ViewEvent(Resource.Id.myButton, typeof(View), nameof(View.Click))]
        //private void ButtonClick(object sender, EventArgs eventArgs)
        //{
        //    Toast.MakeText(this, "Clicked", ToastLength.Short).Show();
        //}

        //private void DirectInjection()
        //{
        //    _myButton = FindViewById<Button>(Resource.Id.myButton);
        //    myEditText1 = FindViewById<EditText>(Resource.Id.myEditText1);
        //    myEditText2 = FindViewById<EditText>(Resource.Id.myEditText2);
        //    myEditText3 = FindViewById<EditText>(Resource.Id.myEditText3);
        //    myEditText4 = FindViewById<EditText>(Resource.Id.myEditText4);
        //    myEditText5 = FindViewById<EditText>(Resource.Id.myEditText5);
        //    myEditText6 = FindViewById<EditText>(Resource.Id.myEditText6);
        //    myEditText7 = FindViewById<EditText>(Resource.Id.myEditText7);
        //    myEditText8 = FindViewById<EditText>(Resource.Id.myEditText8);
        //    myEditText9 = FindViewById<EditText>(Resource.Id.myEditText9);
        //    myEditText10 = FindViewById<EditText>(Resource.Id.myEditText10);
        //    myEditText11 = FindViewById<EditText>(Resource.Id.myEditText11);
        //}
    }
}

