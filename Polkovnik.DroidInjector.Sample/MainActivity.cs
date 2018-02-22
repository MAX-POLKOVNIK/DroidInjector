using Android.App;
using Android.Widget;
using Android.OS;

namespace Polkovnik.DroidInjector.Sample
{
    [Activity(Label = "Polkovnik.DroidInjector.Sample", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        [InjectView(Resource.Id.myButton)] private Button _myButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.main);
            this.InjectViews();
            this.BindViewActions();
        }

        [ViewClickEventHandler(Resource.Id.myButton)]
        public void Click()
        {
            Toast.MakeText(this, "Click", ToastLength.Short).Show();
        }

        [ViewEventHandler(Resource.Id.myButton, "Click")]
        public void CustomClick(object a, object b)
        {
            Toast.MakeText(this, "CustomClick", ToastLength.Short).Show();
        }
    }
}

