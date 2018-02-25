using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Polkovnik.DroidInjector.Sample
{
    public class MainFragment : Fragment
    {
        [View(Resource.Id.myButton)] private Button _myButton2;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.main, container, false);
            
            this.InjectViews(view);

            return view;
        }
    }
}