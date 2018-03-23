using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
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
            //DirectInject(null);

            _myButton.SetBackgroundColor(Color.AliceBlue);
        }

        private void DirectInject(View view)
        {
            //_myButtonC = view.FindViewById(Resource.Id.myButtonC);
        }
    }
}