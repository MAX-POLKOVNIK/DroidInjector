# DroidInjector

Simple injection tool for Xamarin.Android

### Install 

Find package on Nuget or 

`PM> Install-Package Polkovnik.DroidInjector -Version 0.1.1`

### Usage

In Activity:

```csharp
[Activity(Label = "Polkovnik.DroidInjector.Sample", MainLauncher = true, Icon = "@mipmap/icon")]
public class MainActivity : Activity
{
    [View(Resource.Id.myButton)] private Button _myButton;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.main);

        this.InjectViews();

        _myButton.Text = "Text";
    }
}
```

In Fragment:

```csharp
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
```

Fields and properties marked with `View` will be resolved at runtime with specified `Resource.Id`. 

Extension method `InjectViews()` starts resolving views.


### Experimental features

Warning: these features can work wrong. I do not recommend use them.

You must register your `Resource` and `Resource.Id` classes via `Injector.Instance.RegisterResourceClass<TResource, TResourceId>()` before use experimental features.

#### Mark views without specified Resource.Id.

You can mark field or property with attribute `View` without `ResourceId`. In that case injector will try find `ResourceId` by field or property name with trimmed `_` symbol. 

Example:
```xml
<Button android:id="@+id/myButton" 
        android:layout_width="match_parent" 
        android:layout_height="wrap_content" 
        android:text="@string/hello" />
```

```csharp
[View] private Button _myButton;
```

#### Optional views

You can specify `allowMissing` in `View` attribute to allow Injector ignore if ViewContainer doesn't contains view with specified `Resource.Id`. It is useful if you have multiple layouts for one activity or fragment.

Example:
```csharp
[View(Resource.Id.myButton, allowMissing:true)] private Button _myButton2;
```

Also you can specify `allowViewMissing` in `InjectViews()` extension method. In that case all fields and properties marked `View` will be ignored if Injector will not found views.

Example:
```csharp
this.InjectViews(allowViewMissing:true);
```

#### Menu items injection

Injector can find menu items from `Menu`. You must mark field or property with `MenuItem` attribute. 
```csharp
[MenuItem(Resource.Id.action_add)] private IMenuItem _addMenuItem 
```

To resolve marked field call `this.InjectMenuItems()` with `menu` argument:
```csharp
public override bool OnCreateOptionsMenu(IMenu menu)
{
    MenuInflater.Inflate(Resource.Menu.main, menu);
    this.InjectMenuItems(menu);

    return base.OnCreateOptionsMenu(menu);
}
```

#### View events injection
You can mark methods to subscribe to view event like that:
```csharp
[ViewEvent(Resource.Id.myButton, nameof(View.Click))]
private void CustomClick(object sender, EventArgs args)
{
    Toast.MakeText(this, "CustomClick", ToastLength.Short).Show();
}
```

Method signature must be same as event delegate signature, or you get InjectorException. 

To invoke subscription just call `this.BindViewActions()`

You can use exact event attributes. Now available only this one `ViewClickEvent`.
If you use this attribute you can use parameterless method:

```csharp
[ViewClickEvent(Resource.Id.myButton)]
private void Click()
{
    Toast.MakeText(this, "Click", ToastLength.Short).Show();
}
```

### Future 
At this moment you can't use DroidInjector in Android library project. Only in Android app project. 
In feature i'll try to add support Android library project.

Also I will add more special `ViewEvent` attributes like `ViewClickEvent`

### Thanks for reading.

If you have any questions feel free to PM me or open issue.
