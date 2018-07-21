using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;

namespace Polkovnik.DroidInjector.FodyClassLibrarySample
{
    [Activity(Label = "TestActivity")]
    public class TestActivity : Activity
    {
        [View] private Button _myButton { get; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.screen_test);

            Injector.InjectViews();
            Do(Injector.InjectViews);

            _myButton.SetBackgroundColor(Color.AliceBlue);
        }

        private void Do(Action action)
        {
            action();
        }
    }
}