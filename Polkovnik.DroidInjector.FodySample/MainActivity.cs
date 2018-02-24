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

        int count = 1;

        private View _view;
        private View View { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.Main);

            //InjectTest(GetView(FindViewById<View>(Android.Resource.Id.Content)));
            //

            _view = FindViewById<View>(Android.Resource.Id.Content);
            View = _view;
            //InjectTest(view);

            var view2 = FindViewById<View>(Android.Resource.Id.Content);
            Injector.InjectViews(view2);

            //InjectTest(_view);
            Injector.InjectViews(_view);
            Injector.InjectViews(View);

            Injector.InjectViews(GetView(FindViewById<View>(Android.Resource.Id.Content)));

        }

        private View GetView(View view)
        {
            return view;
        }

        private void InjectTest(View view)
        {
            _button = (Button)view.FindViewById(Resource.Id.myButton);
        }
    }
}

