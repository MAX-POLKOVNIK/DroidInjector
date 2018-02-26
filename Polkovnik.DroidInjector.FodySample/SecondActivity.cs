using Android.App;
using Android.OS;

namespace Polkovnik.DroidInjector.FodySample
{
    [Activity(Label = "SecondActivity")]
    public class SecondActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_second);

            FragmentManager.BeginTransaction().Replace(Resource.Id.frameLayoyt, new MainFragment())
                .CommitAllowingStateLoss();
        }
    }
}