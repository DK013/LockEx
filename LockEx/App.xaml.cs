﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using LockEx.Resources;
using LockEx.Models.Main;
using System.Threading;
using System.Globalization;

namespace LockEx
{

    public partial class App : Application
    {

        public static PhoneApplicationFrame RootFrame { get; private set; }
        public static MainView MainViewModel { get; private set; }

        public App()
        {
            UnhandledException += Application_UnhandledException;
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
           // Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            InitializeComponent();
            InitializePhoneApplication();
            InitializeLanguage();
            MainViewModel = new MainView();
            RootFrame.DataContext = MainViewModel;
            RootFrame.Obscured += RootFrame_Obscured;
            RootFrame.Unobscured += RootFrame_Unobscured;
            if (Debugger.IsAttached)
            {
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }

        void RootFrame_Unobscured(object sender, EventArgs e)
        {
            MainViewModel.UnFreeze();
        }

        void RootFrame_Obscured(object sender, ObscuredEventArgs e)
        {
            MainViewModel.Freeze();
        }

        private void Application_ContractActivated(object sender, Windows.ApplicationModel.Activation.IActivatedEventArgs e)
        {
        }

        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        #region Initialisierung der Phone-Anwendung

        private bool phoneApplicationInitialized = false;

        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;
            RootFrame.Navigated += CheckForResetNavigation;
            PhoneApplicationService.Current.ContractActivated += Application_ContractActivated;
            phoneApplicationInitialized = true;
        }

        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            RootFrame.Navigated -= ClearBackStackAfterReset;
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;
            while (RootFrame.RemoveBackEntry() != null) { ; }
        }

        #endregion

        private void InitializeLanguage()
        {
            try
            {
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                throw;
            }
        }

    }

}