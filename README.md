# DroidInjector

Simple injection tool for Xamarin.Android inspired by Cheeseknife and Butterknife.
[Check out speed comparsion](https://github.com/MAX-POLKOVNIK/DroidInjector/wiki/View-injection-speed-benchmark)

### Install 

Install [Nuget package](https://www.nuget.org/packages/Polkovnik.DroidInjector.Fody/)

### Usage

```csharp
[Activity(Label = "Sample", MainLauncher = true, Icon = "@mipmap/icon")]
public class MainActivity : Activity
{
    [View] private TextView _login;
    [View] private TextView _password;
    [View] private TextView _tenant;
    [View] private View _blockInput;
    [View] private Button _enterButton;
    [View] private Button _contactUs;
    [View] private TextInputLayout _tenantInputLayout;
    [View] private TextInputLayout _emailInputLayout;
    [View] private TextInputLayout _passwordInputLayout;
    [View] private Button _logsSecretButton;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.main);

        Injector.InjectViews(); // Will inject views

        //... do something with views
    }
}
```

In Fragment:

```csharp
public class MainFragment : Fragment
{
    [View] private TextView _login;
    [View] private TextView _password;
    [View] private TextView _tenant;
    [View] private View _blockInput;
    [View] private Button _enterButton;
    [View] private Button _contactUs;
    [View] private TextInputLayout _tenantInputLayout;
    [View] private TextInputLayout _emailInputLayout;
    [View] private TextInputLayout _passwordInputLayout;
    [View] private Button _logsSecretButton;

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var view = inflater.Inflate(Resource.Layout.main, container, false);
        
        Injector.InjectViews(view); // Will inject views

        return view;
    }
}
```

Fields and properties marked with `View` will be resolved at runtime. 

Calling method `InjectViews()` starts resolving views.

#### View attribute
There are serveral ways to use `[View]` attribute:
```csharp
// Injector will resolve view with Resource.Id.myButton id
[View] private Button _myButton; 

// If allowMissing false - will throw exception when FindViewById returns null
// If allowMissing true - will ignore when FindViewById returns null
[View(allowMissing:true)] private Button _myButton; 

// Injector will resolve view with Resource.Id.myAwesomeButton id
[View(Resource.Id.myAwesomeButton)] private Button _myAnotherButton;

// Injector will resolve view with Resource.Id.myAwesomeButton id
[View("myAwesomeButton")] private Button _myAnotherButton;
```

In android class library project you can use only `[View]` with id specified as string or empty `[View]` attribute due to in android class library Resource.Id values is not consts, but static int. You can't specify not const argument in attribute.

#### Menu items injection

Injector can find menu items from `Menu`. You must mark field or property with `MenuItem` attribute. 
```csharp
[MenuItem(Resource.Id.action_add)] private IMenuItem _addMenuItem 
```

To resolve marked field call `Injector.InjectMenuItems()` with `menu` argument:
```csharp
public override bool OnCreateOptionsMenu(IMenu menu)
{
    MenuInflater.Inflate(Resource.Menu.main, menu);
    
    Injector.InjectMenuItems(menu);

    return base.OnCreateOptionsMenu(menu);
}
```

#### ~~View events injection~~ [Deprecated. It will be removed in next release]
You can mark methods to subscribe to view event like that:
```csharp
[ViewEvent(Resource.Id.myEditText, typeof(EditText), nameof(EditText.TextChanged))]
private void CustomClick(object sender, EventArgs args)
{
    Toast.MakeText(this, "CustomClick", ToastLength.Short).Show();
}
```

Method signature must be same as event delegate signature, or you get InjectorException. 

To invoke subscription just call `Injector.BindViewActions()`




If you have any questions feel free to PM me or open issue.
