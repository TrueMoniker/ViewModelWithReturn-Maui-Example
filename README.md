# ViewModelWithReturn-Maui-Example
An example of implementing ViewModelWithReturn for .NET Maui. Repo created from my [original github post](https://github.com/dotnet/maui/discussions/18972#discussioncomment-10236159)

I would like a consistent way to return a value to a previous page when a page is closed. For this discussion I'll call the original page the parent page and the page that was opened and is closing the child page. For me, it's a very common use case of: being in a parent page, opening a new child page to get a specific value, then needing that value returned to the parent page when the child page is closed.  I would like to see an explicit interface for returning values added to the framework.

**Most of this is an example. The ViewModelWithReturn.cs file is the only code you need for implementation.**

# How it works:
It's passing the return value from the child to the parent through the WeakReferenceMessenger. It handles the setup for the messenger for me, I only have to remember to open the child page using the `GoToAndReturn` method, and close the child page using the `GoBackAsync` method.
To use, inherit the class  `ViewModelWithReturn` in your viewmodel. Set the TReturn generic parameter to the type of the object returned.
```C#
public partial class MyViewModel : ViewModelWithReturn<MyResponseObject>
```
Then, when you want the page to close, call `GoBackAsync` with the return object.
```C#
await GoBackAsync(toReturn);
```
#### Override Back Button
If you want to return something when the back button is pressed, you can override the back button behavior in the page xaml to call a command you define. That command should call the `GoBackAsync` method.
```Xaml
<Shell.BackButtonBehavior>
    <BackButtonBehavior Command="{Binding MyMethodCommand}"/>   
</Shell.BackButtonBehavior>
```
Alternatively, `ViewModelWithReturn` provides the GoBackCommand to close the page and return the command parameter.
```Xaml
<Shell.BackButtonBehavior>
    <BackButtonBehavior Command="{Binding GoBackCommand}"
                        CommandParameter="{Binding ToReturn}"/>   
</Shell.BackButtonBehavior>
```

### Usage
When navigating to the page that should return a parameter, call the static methdo on the viewmodel `GoToAndReturn`. </br>
For the parameters:
- the recipient should always be `this`
- the action is the callback to handle the returned object 
- the path is the same as the Maui app shell navigation path
- the query is the same as the Maui app shell navigation query parameter

```C#
await ViewModels.MyViewModel.GoToAndReturn(this,
    (response) =>
        MyResponse = response,
   "MyPagePath");
```

> **_IMPORTANT:_** The page you are navigating to with GoToAndReturn must be registered in the AppShell routes rather than created in the app shell xaml at startup. This is because I am using IQueryAttributable to identify the returned to page and pass the parameter.

# Pros:
- It's type safe. When used properly, the callback tells you the return type. Changing the return type of a child page without changing the call from the parent would be obvious to the developer because it would cause a compiler error.
- It guarantees uniqueness. If you open several child pages in quick succession, the callback for a closing child will only trigger the one time that is set from where it opened. Similarly, if you open a large stack of similar pages (like parent => child => parent => child), when the deepest child page closes, the callback will only trigger for the parent that opened the child, not the higher level ancestors.
- It can be used anywhere, not just from a page on the Shell. If you need to get a return value into a popup or service, it can. Just make sure you do not dispose of the parent before the child returns.  

# Cons:
- This still allows me to navigate to the child page or close the child page using the standard shell navigation, without handling the return value. Doing this would make the child page useless. Ideally this solution would enforce it's proper usage.
- It's heavy. It has it's own boiler plate code and it has to use the resources for passing around messages. Ideally a solution wouldn't need this many resources because roundabout way through the messenger. 

# Why I do it this way:
Maui doesn't have an explicit way to return a value when a page is closed. When I search around, I find two options recommended. 
## Option 1: Use the messaging center 
_based on [this](https://stackoverflow.com/questions/72806361/net-maui-mvvm-how-to-catch-a-return-from-a-different-page-using-shell-navigati) discussion_
Use the messaging center _,now changed to `WeakReferenceMessenger` and `StrongReferenceMessenger`,_ to pass the return value back to parent page in a message. The parent page has to subscribe to the message the child is sending. This is the method I went with, though with my own layer on top. The messenger has a lot of boiler plate code, it is not type safe, it is easy to accidentally call an orphaned subscriber that hasn't been garbage collected yet, and can be difficult to understand why the subscription is there for anyone reading the code for the first time. My implementation uses the messenger option, but attempts to resolve these issues.
## Option 2: Pass the value as a parameter from the child page back to the parent page
_based on [this](https://stackoverflow.com/questions/74561572/pass-data-back-from-maui-shell-navigation) discussion_
I did not use this option. I have qualms with the way Maui handles parameters, discussed [here](https://github.com/dotnet/maui/discussions/18968).  But besides that, this method of returning a value requires the parent page to handle the returned value as a parameter. If the parent page already accepts parameters, this adds another step for checking and casting the parameter that will only be there sometimes. If the parent page uses several child pages in it's lifetime, the problem is worse as each one will have a parameter condition in the parent page. 
