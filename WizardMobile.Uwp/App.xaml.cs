﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WizardMobile.Core;
using WizardMobile.Uwp.GamePage;

namespace WizardMobile.Uwp
{
    public sealed partial class WizardUwpApp : Application
    {
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        public WizardUwpApp()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        private Frame _rootFrame;

        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            _rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (_rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                _rootFrame = new Frame();

                _rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = _rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (_rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation parameter
                    NavigateToPage(Page.MAIN_MENU);
                }

                ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 500));
                ApplicationView.PreferredLaunchViewSize = new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Height);
                ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// Invoked when Navigation to a certain page fails
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// Invoked when application execution is being suspended.  Application state is saved without knowing
        ///  whether the application will be terminated or resumed with the contents of memory still intact.
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }


        //************* NAVIGATION AND STATE ********************
        public enum Page
        {
            MAIN_MENU,
            PAUSE,
            GAMEPLAY
        }

        private Page _currentPage;

        public bool NavigateToPage(Page page, object navParam = null)
        {
            if (_rootFrame == null) return false;

            switch (page)
            {
                case Page.GAMEPLAY:
                    _rootFrame.Navigate(typeof(GamePage.GamePage), navParam);
                    break;
                case Page.PAUSE:
                    break;
                case Page.MAIN_MENU:
                    _rootFrame.Navigate(typeof(MainMenuPage), navParam);
                    break;
            }
            _currentPage = page;
            return true;
        }
    }
}
