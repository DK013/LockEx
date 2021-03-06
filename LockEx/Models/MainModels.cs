﻿using System;
using LockEx.Models.WeatherControl;
using LockEx.Models.BadgesControl;
using LockEx.Models.DateTimeControl;
using LockEx.Models.DetailedTextControl;
using LockEx.Models.MusicControl;
using LockEx.Models.NewsControl;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.IO.IsolatedStorage;
using Windows.Phone.System.LockScreenExtensibility;
using LockEx.Resources;
using Windows.Phone.System.UserProfile;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using LockEx.Hardware;
using RTComponent;
using RTComponent.NotificationsSnapshot;
using System.Windows.Data;
using Windows.Phone.System;
using System.Diagnostics;

namespace LockEx.Models.Main
{

    public class MainView : INotifyPropertyChanged
    {

        public enum LeftControls
        {
            WeatherControl, NewsControl
        }
        public enum LongTextModes
        {
            Always, Never, Auto
        }

        #region Maps

        private static Dictionary<bool, Uri> FlashlightImageMap = new Dictionary<bool, Uri>()
        {
            { true, new Uri("/Assets/Icons/lightbulb2.png", UriKind.Relative) },
            { false, new Uri("/Assets/Icons/lightbulb.png", UriKind.Relative) }
        };
        public static Dictionary<LeftControls, string> LeftControlsCharMap = new Dictionary<LeftControls, string>()
        {
            { LeftControls.NewsControl, "N" },
            { LeftControls.WeatherControl, "W" }
        };
        private static Dictionary<string, LeftControls> LeftControlsReverseCharMap = new Dictionary<string, LeftControls>()
        {
            { "N", LeftControls.NewsControl },
            { "W", LeftControls.WeatherControl }
        };
        public static Dictionary<LongTextModes, string> LongTextModeCharMap = new Dictionary<LongTextModes, string>()
        {
            { LongTextModes.Always, "E" },
            { LongTextModes.Never, "D" },
            { LongTextModes.Auto, "A" }
        };
        private static Dictionary<string, LongTextModes> LongTextModeReverseCharMap = new Dictionary<string, LongTextModes>()
        {
            { "E", LongTextModes.Always },
            { "D", LongTextModes.Never },
            { "A", LongTextModes.Auto }
        };

        #endregion

        private DispatcherTimer _secondsTimer;
        public DispatcherTimer SecondsTimer
        {
            get
            {
                return _secondsTimer;
            }
        }

        private WeatherControlView _weatherView;
        public WeatherControlView WeatherView
        {
            get
            {
                return _weatherView;
            }
            set
            {
                _weatherView = value;
                RaisePropertyChanged("WeatherView");
            }
        }
        private BadgesControlView _badgesView;
        public BadgesControlView BadgesView
        {
            get
            {
                return _badgesView;
            }
            set
            {
                _badgesView = value;
                RaisePropertyChanged("BadgesView");
            }
        }
        private DateTimeControlView _dateTimeView;
        public DateTimeControlView DateTimeView
        {
            get
            {
                return _dateTimeView;
            }
            set
            {
                _dateTimeView = value;
                RaisePropertyChanged("DateTimeView");
            }
        }
        private DetailedTextControlView _detailedTextView;
        public DetailedTextControlView DetailedTextView
        {
            get
            {
                return _detailedTextView;
            }
            set
            {
                _detailedTextView = value;
                RaisePropertyChanged("DetailedTextView");
            }
        }
        private MusicControlView _musicView;
        public MusicControlView MusicView
        {
            get
            {
                return _musicView;
            }
            set
            {
                _musicView = value;
                RaisePropertyChanged("MusicView");
            }
        }
        private NewsControlView _newsView;
        public NewsControlView NewsView
        {
            get
            {
                return _newsView;
            }
            set
            {
                _newsView = value;
                RaisePropertyChanged("NewsView");
            }
        }
        private Uri _imageUri;
        public Uri ImageUri
        {
            get
            {
                if (DesignerProperties.IsInDesignTool) return _imageUri;
                return (IsolatedStorageSettings.ApplicationSettings.Contains("ImageUri")) ?
                   new Uri((string)IsolatedStorageSettings.ApplicationSettings["ImageUri"]) :
                   DefaultImageUri;
            }
            set
            {
                _imageUri = value;
                if (!DesignerProperties.IsInDesignTool)
                {
                    IsolatedStorageSettings.ApplicationSettings["ImageUri"] = value.AbsolutePath;
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
                RaisePropertyChanged("ImageUri");
                RaisePropertyChanged("CustomImage");
            }
        }
        private bool _isLockscreen;
        public bool IsLockscreen
        {
            get
            {
                if (DesignerProperties.IsInDesignTool) return _isLockscreen;
                return ExtensibilityApp.IsLockScreenApplicationRegistered();
            }
            set
            {
                if (DesignerProperties.IsInDesignTool) _isLockscreen = value;
                if (value && !ExtensibilityApp.IsLockScreenApplicationRegistered())
                {
                    ExtensibilityApp.RegisterLockScreenApplication();
                    RaisePropertyChanged("IsLockscreen");
                }
                else if (!value && ExtensibilityApp.IsLockScreenApplicationRegistered())
                {
                    ExtensibilityApp.UnregisterLockScreenApplication();
                    RaisePropertyChanged("IsLockscreen");
                }
            }
        }
        private LongTextModes _longTextMode;
        public LongTextModes LongTextMode
        {
            get
            {
                if (DesignerProperties.IsInDesignTool) return _longTextMode;
                return (IsolatedStorageSettings.ApplicationSettings.Contains("LongTextMode")) ?
                   LongTextModeReverseCharMap[(string)IsolatedStorageSettings.ApplicationSettings["LongTextMode"]] :
                   LongTextModeReverseCharMap[AppResources.DefaultLongTextMode];
            }
            set
            {
                _longTextMode = value;
                if (!DesignerProperties.IsInDesignTool)
                {
                    IsolatedStorageSettings.ApplicationSettings["LongTextMode"] = LongTextModeCharMap[value];
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
                RaisePropertyChanged("LongTextMode");
                RaisePropertyChanged("RightPanelRowSpan");
                RaisePropertyChanged("LowerPanelVisibility");
            }
        }
        public int RightPanelRowSpan
        {
            get
            {
                return (LongTextMode == LongTextModes.Always || (LongTextMode == LongTextModes.Auto && !MusicView.HasMusic)) ? 3 : 1;
            }
        }
        public Visibility LowerPanelVisibility
        {
            get
            {
                return (LongTextMode != LongTextModes.Always && MusicView.HasMusic) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private double _globalYOffset;
        public double GlobalYOffset
        {
            get
            {
                return _globalYOffset;
            }
            set
            {
                if (value <= 0)
                {
                    if (HasPasscode && -value > SwipeMax)
                    {
                        _globalYOffset = -SwipeMax;
                    }
                    else
                    {
                        _globalYOffset = value;
                    }
                    RaisePropertyChanged("GlobalYOffset");
                }
            }
        }
        private double swipeMaxDefault = 400;
        public double SwipeMax
        {
            get
            {
                var cont = Application.Current.Host.Content;
                double height = ExtensibilityApp.GetLockPinpadHeight() / (Application.Current.Host.Content.ScaleFactor / 100.0);
                if (height == 0) return swipeMaxDefault;
                return height;
            }
        }
        public bool HasPasscode
        {
            get
            {
                return ExtensibilityApp.GetLockPinpadHeight() != 0;
            }
        }
        private bool _flashlightVisibleBool;
        public bool FlashlightVisibleBool
        {
            get
            {
                if (DesignerProperties.IsInDesignTool) return _flashlightVisibleBool;
                return (IsolatedStorageSettings.ApplicationSettings.Contains("FlashlightVisible")) ?
                    (Convert.ToBoolean(IsolatedStorageSettings.ApplicationSettings["FlashlightVisible"])) :
                    (Convert.ToBoolean(AppResources.DefaultFlashlightVisible));
            }
            set
            {
                _flashlightVisibleBool = value;
                if (!DesignerProperties.IsInDesignTool)
                {
                    IsolatedStorageSettings.ApplicationSettings["FlashlightVisible"] = value.ToString();
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
                RaisePropertyChanged("FlashlightVisibleBool");
                RaisePropertyChanged("FlashlightVisible");
            }
        }
        public Visibility FlashlightVisible
        {
            get
            {
                return (FlashlightVisibleBool) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private Flashlight _flashlight;
        public Flashlight Flashlight
        {
            get
            {
                return _flashlight;
            }
            set
            {
                _flashlight = value;
                RaisePropertyChanged("Flashlight");
                RaisePropertyChanged("FlashlightImageUri");
            }

        }
        public Uri FlashlightImageUri
        {
            get
            {
                return FlashlightImageMap[Flashlight.IsTurnedOn];
            }
        }
        private LeftControls _leftControl;
        public LeftControls LeftControl
        {
            get
            {
                if (DesignerProperties.IsInDesignTool) return _leftControl;
                return (IsolatedStorageSettings.ApplicationSettings.Contains("LeftControl")) ?
                    LeftControlsReverseCharMap[(string)IsolatedStorageSettings.ApplicationSettings["LeftControl"]] :
                   LeftControlsReverseCharMap[AppResources.DefaultLeftControl];
            }
            set
            {
                _leftControl = value;
                if (!DesignerProperties.IsInDesignTool)
                {
                    IsolatedStorageSettings.ApplicationSettings["LeftControl"] = LeftControlsCharMap[value];
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
            }
        }
        public Visibility WeatherControlVisible
        {
            get
            {
                return LeftControl == LeftControls.WeatherControl ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Visibility NewsControlVisible
        {
            get
            {
                return LeftControl == LeftControls.NewsControl ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        /*private bool _glanceEnabled;
        public bool GlanceEnabled
        {
            get
            {
                if (DesignerProperties.IsInDesignTool) return _glanceEnabled;
                return (IsolatedStorageSettings.ApplicationSettings.Contains("GlanceEnabled")) ?
                    (Convert.ToBoolean(IsolatedStorageSettings.ApplicationSettings["GlanceEnabled"])) :
                    (Convert.ToBoolean(AppResources.DefaultGlance));
            }
            set
            {
                _glanceEnabled = value;
                if (!DesignerProperties.IsInDesignTool)
                {
                    IsolatedStorageSettings.ApplicationSettings["GlanceEnabled"] = value.ToString();
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
                RaisePropertyChanged("GlanceEnabled");
            }
        }
        private Glance _glance;
        public Glance Glance
        {
            get
            {
                return _glance;
            }
            set
            {
                _glance = value;
                RaisePropertyChanged("Glance");
            }
        }*/

        private Array _leftControlsStrings = Enum.GetValues(typeof(LeftControls));
        public Array LeftControlsStrings
        {
            get
            {
                return _leftControlsStrings;
            }
        }
        private Array _longTextModesStrings = Enum.GetValues(typeof(LongTextModes));
        public Array LongTextModesStrings
        {
            get
            {
                return _longTextModesStrings;
            }
        }
        public Uri DefaultImageUri = new Uri("/Assets/Backgrounds/blue_mountains_lq.jpg", UriKind.Relative);
        public Uri DefaultImageUriSystem = new Uri("/Assets/Backgrounds/blue_mountains_lq_system.jpg", UriKind.Relative);

        public NativeAPI NAPI;
        private const string UIXMARPrefix = "res://UIXMobileAssets{ScreenResolution}!";
        private const string FilePrefix = "file://";
        private Snapshot lastSnapshot = null;

        public MainView()
        {
            _weatherView = new WeatherControlView();
            _badgesView = new BadgesControlView();
            _dateTimeView = new DateTimeControlView();
            _detailedTextView = new DetailedTextControlView();
            _musicView = new MusicControlView();
            _newsView = new NewsControlView();
            _flashlight = new Flashlight();
            /*_glance = new Glance();
            _glance.GraceTime = TimeSpan.FromSeconds(1);
            _glance.GlanceEvent += Glance_GlanceEvent;*/
            if (!DesignerProperties.IsInDesignTool)
            {
                NAPI = new NativeAPI();
                var cont = Application.Current.Host.Content;
                NAPI.InitUIXMAResources((int)Math.Ceiling(cont.ActualWidth * cont.ScaleFactor * 0.01), (int)Math.Ceiling(cont.ActualHeight * cont.ScaleFactor * 0.01));
                _secondsTimer = new DispatcherTimer();
                _secondsTimer.Interval = new TimeSpan(0, 0, 1);
                _secondsTimer.Tick += secondsTimer_Tick;
            }
            else
            {
                PopulateDesignerData();
            }
            MusicView.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (e.PropertyName == "HasMusic")
                {
                    RaisePropertyChanged("LowerPanelVisibility");
                    RaisePropertyChanged("RightPanelRowSpan");
                }
            };
            Flashlight.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (e.PropertyName == "IsTurnedOn")
                {
                    RaisePropertyChanged("FlashlightImageUri");
                }
            };
        }

        void Glance_GlanceEvent(object sender, Glance.GlanceEventArgs args)
        {
            //Debug.WriteLine(args);
           // NAPI.TurnScreenOn(args ==Glance.GlanceEventArgs.Light);
        }

        void secondsTimer_Tick(object sender, EventArgs e)
        {
            if (!SystemProtection.ScreenLocked) return;
            DateTimeView.Value = DateTime.Now;
            PopulateShellChromeData();
            MusicView.RaisePropertyChanged("Position");
        }

        public void PopulateDesignerData()
        {
            _imageUri = DefaultImageUri;
            _isLockscreen = true;
            _longTextMode = LongTextModes.Auto;
            _globalYOffset = 0;
            _flashlightVisibleBool = true;
            _flashlight.IsTurnedOn = true;
            _leftControl = LeftControls.NewsControl;
            /*_glanceEnabled = true;
            Glance.Dark = 4;*/
            WeatherView.Entries = new ObservableCollection<WeatherControlEntry>()
            {
                new WeatherControlEntry(DateTime.Today, WeatherControlEntry.WeatherStates.Clear, "Klarer Himmel", 20.4, 30.6),
                new WeatherControlEntry(DateTime.Today.AddDays(1), WeatherControlEntry.WeatherStates.FewClouds, "Einige Wolken", -10.4, 3),
                new WeatherControlEntry(DateTime.Today.AddDays(2), WeatherControlEntry.WeatherStates.Rain, "Starker Regen", -10.4, 3),
                new WeatherControlEntry(DateTime.Today.AddDays(3), WeatherControlEntry.WeatherStates.Snow, "Schnee", -20, 23.5),
                new WeatherControlEntry(DateTime.Today.AddDays(7), WeatherControlEntry.WeatherStates.Thunderstorm, "Ay caramba!", 3, 11),
                new WeatherControlEntry(DateTime.Today.AddDays(14), WeatherControlEntry.WeatherStates.BrokenClouds, "Dichte Wolken", 14, 18.7)
            };
            WeatherControlView.City = "München";
            WeatherControlView.TempSuffix = WeatherControlView.TempSuffixes.Celsius;
            ObservableCollection<BadgesControlEntry> badgesEntries = new ObservableCollection<BadgesControlEntry>();
            BitmapImage placeholder = new BitmapImage(new Uri("/Assets/ApplicationIcon.png", UriKind.Relative));
            for (int i = 0; i < 5; i++) badgesEntries.Add(new BadgesControlEntry(placeholder, "0"));
            BadgesView.Entries = badgesEntries;
            DateTimeView.Value = DateTime.Now;
            DateTimeControlView.HourFormat = "24";
            DateTimeControlView.SecondsVisible = Visibility.Visible;
            DetailedTextView.Entries = new ObservableCollection<DetailedTextControlEntry>()
            {
                new DetailedTextControlEntry("Lockscreen fertig programmieren, und diese Zeile ist auch sehr lang", true, false),
                new DetailedTextControlEntry("Zuhause, am Rechner", false, true),
                new DetailedTextControlEntry("Morgen: 12:00 - 15:00 Uhr", false, false)
            };
            MusicView.Song = "Are We The Waiting";
            MusicView.Artist = "Green Day";
            MusicView.PlayState = Microsoft.Xna.Framework.Media.MediaState.Playing;
            MusicView.Position = 60.5;
            WeatherView.ErrorVisible = Visibility.Collapsed;
            WeatherView.LoadingVisible = Visibility.Collapsed;
            NewsView.Entries = new ObservableCollection<NewsControlEntry>()
            {
                new NewsControlEntry("Frieden im nahen Osten für immer und ewig und alle Zeiten", 
@"Heute wurde ewiger Friede zwischen Christen, Juden, Muslimen und allen Völkern in der Welt beschlossen. 
Ein Sprecher vor Ort bestätigte, der Grund sei eine rote Kuh, die eine zentrale Rolle in allen Religionen spiele."
                    , null, DateTime.Now),
                new NewsControlEntry("Benzinprise unter 0,05€", 
@"Nach langen Verhandlungen einigte sich die OPEC wiederholt nicht auf eine Begrenzung der Fördermengen. 
Ohje! Rufen die Grünen - ihrer Meinung nach sollte mehr mit Fahrädern in Bussen gefahren werden."
                    , null, DateTime.Now.AddDays(-1)),
                new NewsControlEntry("NSA spionierte Toiletten aus", 
@"Wie nun aus internen Berichten des BND hervorgeht, beobachtete der amerikanische Geheimdienst NSA jahrelang Toiletten im Bundestag. 
Aus den Dokumenten geht lediglich hervor was für ein 'bschissener Job' die Überwachung war."
                    , null, DateTime.Now.AddDays(-2)),
                new NewsControlEntry("Kindergruppe findet Einbrecher", 
@"Wie die Polizei Hinterdupfing nun bekanntgab, ist der Fall um die im April gestohlene Aktentasche aufgeklärt - nur
durch die Hilfe einer Gruppe lokaler Kinderdedektive unter der Führung eines gewissen Emils."
                    , null, DateTime.Now.AddDays(-3))
            };
            NewsView.ErrorVisible = Visibility.Collapsed;
            NewsView.LoadingVisible = Visibility.Collapsed;
            NewsView.Source = new Uri("http://www.example.com");
            NewsView.Title = "Ernsthafte Nachrichten.de";
        }

        public void PopulateShellChromeData()
        {
            Snapshot snap = NAPI.GetNotificationsSnapshot();
            if (SnapshotsEqual(snap, lastSnapshot)) return;
            lastSnapshot = snap;
            ObservableCollection<DetailedTextControlEntry> newTextControlEntries = new ObservableCollection<DetailedTextControlEntry>();
            for (int i = 0; i < 3; i++)
            {
                newTextControlEntries.Add(new DetailedTextControlEntry(
                    snap.DetailedTexts[i].DetailedText,
                    snap.DetailedTexts[i].IsBoldText,
                    i == 1
                ));
            }
            DetailedTextView.Entries = newTextControlEntries;
            ObservableCollection<BadgesControlEntry> newBadgeControlEntries = new ObservableCollection<BadgesControlEntry>();
            for (int i = 0; i < snap.BadgeCount && i < 5; i++)
            {
                if (snap.Badges[i].Type != BadgeValueType.None)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    if (snap.Badges[i].IconUri.StartsWith(UIXMARPrefix))
                    {
                        bitmapImage.SetSource(new MemoryStream(NAPI.GetUIXMAResource(snap.Badges[i].IconUri.Substring(UIXMARPrefix.Length))));
                    }
                    else if (snap.Badges[i].IconUri.StartsWith(FilePrefix))
                    {
                        MemoryStream ms = new MemoryStream();
                        using (FileStream fs = new FileStream(snap.Badges[i].IconUri.Substring(FilePrefix.Length), FileMode.Open))
                        {
                            byte[] bytes = new byte[fs.Length];
                            fs.Read(bytes, 0, (int)fs.Length);
                            ms.Write(bytes, 0, (int)fs.Length);
                        }
                        bitmapImage.SetSource(ms);
                    }
                    newBadgeControlEntries.Add(new BadgesControlEntry(bitmapImage, snap.Badges[i].Value));
                }
            }
            BadgesView.Entries = newBadgeControlEntries;
            /*string[] iconUris = new string[] { snap.AlarmIconUri, snap.DoNotDisturbIconUri, snap.DrivingModeIconUri };
            for (int i = 0; i < 3; i++)
            {
                if (iconUris[i] != "" && iconUris[i].StartsWith(UIXMARPrefix))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(new MemoryStream(NAPI.GetUIXMAResource(iconUris[i].Substring(UIXMARPrefix.Length))));
                    indicatorImages[i].Source = bitmapImage;
                }
                else
                {
                    //testImages2[i].Source = null
                }
            }*/
        }

        private static bool SnapshotsEqual(Snapshot snapA, Snapshot snapB)
        {
            if (snapA == null || snapB == null) return false;
            if (snapA.AlarmIconUri != snapB.AlarmIconUri) return false;
            if (snapA.BadgeCount != snapB.BadgeCount) return false;
            if (snapA.DetailedTextCount != snapB.DetailedTextCount) return false;
            if (snapA.DoNotDisturbIconUri != snapB.DoNotDisturbIconUri) return false;
            if (snapA.DrivingModeIconUri != snapB.DrivingModeIconUri) return false;
            if (snapA.Size != snapB.Size) return false;
            for (int i = 0; i < snapA.Badges.Count; i++)
            {
                if (snapA.Badges[i].IconUri != snapB.Badges[i].IconUri) return false;
                if (snapA.Badges[i].Type != snapB.Badges[i].Type) return false;
                if (snapA.Badges[i].Value != snapB.Badges[i].Value) return false;
            }
            for (int i = 0; i < snapA.DetailedTexts.Count; i++)
            {
                if (snapA.DetailedTexts[i].DetailedText != snapB.DetailedTexts[i].DetailedText) return false;
                if (snapA.DetailedTexts[i].IsBoldText != snapB.DetailedTexts[i].IsBoldText) return false;
            }
            return true;
        }

        private object freezeLock = new object();

        public void Freeze()
        {
            lock (freezeLock)
            {
                Flashlight.UnInitCamera();
                SecondsTimer.Stop();
                MusicView.XNAFrameworkDispatcher.Stop();
            }
        }

        public void UnFreeze()
        {
            lock (freezeLock)
            {
                Flashlight.InitCamera();
                secondsTimer_Tick(null, null);
                SecondsTimer.Start();
                MusicView.XNAFrameworkDispatcher.Start();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainView GetCopy()
        {
            MainView copy = (MainView)this.MemberwiseClone();
            return copy;
        }

    }

    public class LeftControlsIntConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)(MainView.LeftControls)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.GetValues(typeof(MainView.LeftControls)).GetValue((int)value);
        }

    }

    public class LongTextModesIntConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)(MainView.LongTextModes)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.GetValues(typeof(MainView.LongTextModes)).GetValue((int)value);
        }

    }

}