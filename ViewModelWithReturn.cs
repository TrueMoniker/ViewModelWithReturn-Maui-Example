using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace EntryTest
{
    public partial class ViewModelWithReturn<TReturn> : ObservableObject, IQueryAttributable
    {
        //TODO: I really want ViewModelWithReturn to be an interface, however I cannot find a way to ensure the recipient is correct without the recipientId.
        // and I cannot find a way to get the recipient id without passing it as an argument.        
        private static string RecipientIdName = "MauiApp.RecipientIdName";

        private Guid recipientId;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            this.recipientId = (Guid)query[RecipientIdName];
        }

        /// A command version of GoBackAsync, so that we can bind directly to it from the view xaml
        /// <param name="toReturn">The object to return. Must be passed as a command parameter</param>
        [RelayCommand]
        private async Task GoBack(TReturn toReturn)
        {
            await GoBackAsync(toReturn);
        }

        /// Closes this page and returns an object to whatever opened this page.
        /// In order to receive an object, the reciepient must have navigated to this page using <see cref="GoToAndReturn"/>
        /// <param name="toReturn">The object to return</param>
        public async Task GoBackAsync(TReturn toReturn)
        {
            WeakReferenceMessenger.Default.Send(new ViewModelWithReturnMessage<TReturn>(toReturn, recipientId));
            await Shell.Current.GoToAsync("..");
        }

        //A static helper that can be called from any object that wants to open this view and get a response when it closes. 
        /// Opens a page at the path and sets up to receive an object when the page closes.
        /// On the page closing, <paramref name="action"/> is executed.
        /// The page should use this viewmodel.
        /// <param name="recipient">The recipient to get the return. Should be <code>this</code></param>
        /// <param name="action">The action to execute when  the page closes and returns too where this was called</param>
        /// <param name="path">Path to navigate to. Obviously, don't close this page you are navigating from or it won't be around to get the callback.</param>
        /// <param name="query">Parameters to pass to the viewmodel, <see cref="IQueryAttributable"/> </param>
        public static async Task GoToAndReturn(object recipient, Action<TReturn> action, string path, Dictionary<string, object> query = null)
        {
            Guid thisRecipientId = Guid.NewGuid();

            //Unregister any previous registration. If we don't get a message back, we might try to reopen the page, which would mess with the subscription.
            WeakReferenceMessenger.Default.Unregister<ViewModelWithReturnMessage<TReturn>>(recipient);
            WeakReferenceMessenger.Default.Register<ViewModelWithReturnMessage<TReturn>>(recipient, (r, m) =>
            {
                //The recipientId makes sure we only have one class that handles the response, instead of any waiting for returns. 
                if (m.RecipientId == thisRecipientId)
                {
                    //If this executes, we got the response back, so the view that we were listening for should have closed. We can unsubscribe from this.
                    WeakReferenceMessenger.Default.Unregister<ViewModelWithReturnMessage<TReturn>>(recipient);

                    //Execute the action to assign the result
                    action(m.Value);
                }
            });

            //Add the recipient id to the query parameters so the child can tell who its parent is when sending the response in a message.
            query ??= new Dictionary<string, object>();
            query.Add(RecipientIdName, thisRecipientId);

            await Shell.Current.GoToAsync(path, query);
        }

        /// A message to pass data back from a viewmodel when it closes.
        /// <typeparam name="T">The type of the object that is being returned.</typeparam>
        private class ViewModelWithReturnMessage<T> : ValueChangedMessage<T>
        {
            public ViewModelWithReturnMessage(T value, Guid recipientId) : base(value)
            {
                RecipientId = recipientId;
            }

            /// the id of the object that should handle this message.
            public Guid RecipientId { get; }
        }
    }
}

