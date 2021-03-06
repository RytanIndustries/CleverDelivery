﻿using CleverDelivery.Pages;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CleverDelivery
{
    public class App : Xamarin.Forms.Application
    {
        public App()
        {
            Device.SetFlags(new string[] { "Markup_Experimental" });
            // Deployed applications must be licensed at the Lite level or greater. 
            // See https://developers.arcgis.com/licensing for further details.
            
            //ArcGISRuntimeEnvironment.SetLicense(licenseKey:licencekey);

            // Initialize the ArcGIS Runtime before any components are created.
            ArcGISRuntimeEnvironment.Initialize();
            
            // The root page of your application
            MainPage = new NavigationPage(new StartPage());
        }
    }
}
