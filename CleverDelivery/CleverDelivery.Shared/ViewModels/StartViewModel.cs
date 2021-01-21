using CleverDelivery.Pages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms.Core;

namespace CleverDelivery.ViewModels
{
    public class StartViewModel : CoreViewModel
    {
        public ICommand BasicMapCommand { get; set; }
        public ICommand CurrentLocationCommand { get; set; }
        public ICommand AddressLocatorCommand { get; set; }
        public ICommand RoutingCommand { get; set; }

        public StartViewModel()
        {
            BasicMapCommand = new CoreCommand(async (obj) =>
            {
                await Navigation.PushAsync(new MapPage());
            });
            CurrentLocationCommand = new CoreCommand(async (obj) =>
            {
                await Navigation.PushAsync(new CurrentLocationPage());
            });
            AddressLocatorCommand = new CoreCommand(async (obj) =>
            {
                await Navigation.PushAsync(new AddressSearchPage());
            });
            RoutingCommand = new CoreCommand(async (obj) =>
            {
                await Navigation.PushAsync(new OptimizedRoutingPage());
            });
        }
        public override void OnViewMessageReceived(string key, object obj)
        {
        }
    }
}
