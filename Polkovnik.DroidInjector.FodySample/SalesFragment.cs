using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Polkovnik.DroidInjector;
using Polkovnik.DroidInjector.FodySample;

internal sealed class MySalesFragment : SalesFragment<MySalesFragment>
{
    public static MySalesFragment NewInstance() => NewInstance("");
}

internal abstract class SalesFragment<T> : Fragment where T : SalesFragment<T>, new()
{
    public static T NewInstance(string value) => new T();

    [View] private Button _myButton;

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var view = inflater.Inflate(Resource.Layout.main, container, false);

        Injector.InjectViews(view);
        DirectInject(view);

        return view;
    }

    private void DirectInject(View view)
    {
        _myButton = (Button)view.FindViewById(Resource.Id.myButton);
    }
}