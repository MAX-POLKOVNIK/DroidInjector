using System.Linq;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;

namespace Polkovnik.DroidInjector.FodySample
{
    internal class MainFragment : Fragment
    {
#pragma warning disable 649
        [View] private RecyclerView _recyclerView;
#pragma warning restore 649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.main_fragment, null);

            Injector.InjectViews(view);

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            _recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));

            var items = Enumerable.Repeat(("title", "subtitle"), 20).ToArray();

            _recyclerView.SetAdapter(new Adapter(items));
        }
    }
}