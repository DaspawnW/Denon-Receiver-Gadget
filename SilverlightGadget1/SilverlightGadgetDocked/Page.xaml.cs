// Silverlight Vista Sidebar Gadget Template created by Ioan Lazarciuc
// http://www.lazarciuc.ro/ioan
// Contact form present on website
// Based on the Vista Sidebar Gadget Template created by Tim Heuer 
// http://timheuer.com/blog/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using SilverlightGadgetUtilities;
using System.Windows.Media.Imaging;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;


namespace SilverlightGadgetDocked
{
    public partial class Page : UserControl
    {
        SilverlightGadgetEvents evHelper;

        // Used to hide the Silverlight control from JavaScript when docking/undocking
        [ScriptableMember]
        public bool IsVisible
        {
            get { return this.Visibility == System.Windows.Visibility.Visible; }
            set { this.Visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
        }

        private BitmapImage onImage = new BitmapImage(new Uri("/SilverlightGadgetDocked;component/Images/on.png", UriKind.Relative));

        private BitmapImage offImage = new BitmapImage(new Uri("/SilverlightGadgetDocked;component/Images/off.png", UriKind.Relative));

        private BitmapImage startRefresh = new BitmapImage(new Uri("/SilverlightGadgetDocked;component/Images/refresh.png", UriKind.Relative));

        private BitmapImage stopRefresh = new BitmapImage(new Uri("/SilverlightGadgetDocked;component/Images/stopRefresh.png", UriKind.Relative));

        System.Windows.Threading.DispatcherTimer myTimer = new System.Windows.Threading.DispatcherTimer();

        public string DenonIP = !String.IsNullOrEmpty(SilverlightGadget.Settings["DENONIP"]) ? SilverlightGadget.Settings["DENONIP"] : "192.168.1.22";

        private int refreshIntervall = !String.IsNullOrEmpty(SilverlightGadget.Settings["TickerTime"]) ? (Convert.ToInt32(SilverlightGadget.Settings["TickerTime"]) * 1000) : 60000;

        public Page()
        {
            InitializeComponent();
            evHelper = new SilverlightGadgetEvents("SilverlightGadgetEventsD");
            evHelper.VisibilityChanged += new EventHandler(evHelper_VisibilityChanged);
            evHelper.SettingsClosed += new EventHandler<SettingsClosingEventArgs>(evHelper_SettingsClosed);
            evHelper.SettingsClosing += new EventHandler<SettingsClosingEventArgs>(evHelper_SettingsClosing);
            evHelper.ShowSettings += new EventHandler(evHelper_ShowSettings);
            evHelper.Dock += new EventHandler(evHelper_Dock);

            // Set gadget size according to the one specified in the Silverlight control.
            if (SilverlightGadget.Docked)
                SilverlightGadget.SetGadgetSize(Width, Height);

            HtmlPage.RegisterScriptableObject("dockedGadget", this);
            // Not needed if the pages are set in the javascript code
            //SilverlightGadget.FlyoutPage = "flyout.html";
            //SilverlightGadget.SettingsUI = "Settings.html";
        }

        void evHelper_Dock(object sender, EventArgs e)
        {
            SilverlightGadget.SetGadgetSize(Width, Height);
        }

        public void evHelper_ShowSettings(object sender, EventArgs e)
        {
        }

        void evHelper_SettingsClosing(object sender, SettingsClosingEventArgs e)
        {
        }

        void evHelper_SettingsClosed(object sender, SettingsClosingEventArgs e)
        {
            DenonIP = !String.IsNullOrEmpty(SilverlightGadget.Settings["DENONIP"]) ? SilverlightGadget.Settings["DENONIP"] : "192.168.1.22";
            refreshIntervall = !String.IsNullOrEmpty(SilverlightGadget.Settings["TickerTime"]) ? (Convert.ToInt32(SilverlightGadget.Settings["TickerTime"]) * 1000) : 60000;
        }

        void evHelper_VisibilityChanged(object sender, EventArgs e)
        {
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // If through code the size of the Silverlight control is changed, change the size in the javascript gadget also.
            if (SilverlightGadget.Docked)
                SilverlightGadget.SetGadgetSize(e.NewSize.Width, e.NewSize.Height);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // One can save parameters that need to be passed to the flyout control here
            // The parameters can be save in the gadget settings
            //SilverlightGadget.Settings["temppar"] = value;

            // Open the flyout page
            SilverlightGadget.Flyout();
        }

        private void OnOff_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string url;
            if (OnOff.Tag == "ON")
            {
                //Ausschalten
                OnOff.Source = onImage;
                OnOff.Tag = "OFF";
                url = "http://" + DenonIP + "/MainZone/index.put.asp?cmd0=PutSystem_OnStandby%2FSTANDBY&cmd1=aspMainZone_WebUpdateStatus%2F&_=" + DateTimeURL();
                sourceGrid.Visibility = System.Windows.Visibility.Collapsed;
                VolumeGrid.Visibility = System.Windows.Visibility.Collapsed;
                radioGrid.Visibility = System.Windows.Visibility.Collapsed;
                myTimer.Stop();
                RefreshPlaying.Source = startRefresh;
                RefreshPlaying.Tag = "OFF";
            }
            else
            {
                //einschalten
                OnOff.Source = offImage;
                OnOff.Tag = "ON";
                url = "http://" + DenonIP + "/MainZone/index.put.asp?cmd0=PutSystem_OnStandby%2FON&cmd1=aspMainZone_WebUpdateStatus%2F&_=" + DateTimeURL();
                sourceGrid.Visibility = System.Windows.Visibility.Visible;
                VolumeGrid.Visibility = System.Windows.Visibility.Visible;
            }
            WebClient client = new WebClient();
            client.OpenReadAsync(new Uri(url, UriKind.Absolute));
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(ReceiverStartStop);

        }

        void ReceiverStartStop(object sender, OpenReadCompletedEventArgs e)
        {
            StreamReader myReader = new StreamReader(e.Result);
            //tbData.Text = myReader.ReadToEnd();
            myReader.Close();
            if (OnOff.Tag == "ON")
            {
                //Informationen abfragen
                GetReadSystemInformation();
                
            }
        }

        string DateTimeURL()
        {
            DateTime d = DateTime.Now;
            string value = d.Year + "" + d.Month + "" + d.Day + "" + d.Hour + "" + d.Minute + "" + d.Second + "" + d.Millisecond;
            return value;
        }

        void GetReadSystemInformation()
        {
            string url = "http://" + DenonIP + "/goform/formMainZone_MainZoneXml.xml?_=" + DateTimeURL();
            WebClient client = new WebClient();
            client.OpenReadAsync(new Uri(url, UriKind.Absolute));
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(ReceiverSystemInformation);
        }

        void ReceiverSystemInformation(object sender, OpenReadCompletedEventArgs e)
        {
            radioGrid.Visibility = System.Windows.Visibility.Collapsed;
            textboxNowPlaying.Visibility = System.Windows.Visibility.Collapsed;
            RadioImage.Opacity = 0.2;
            RadioImage.Tag = null;
            TVImage.Opacity = 0.2;
            TVImage.Tag = null;
            DVDImage.Opacity = 0.2;
            DVDImage.Tag = null;
            BDImage.Opacity = 0.2;
            BDImage.Tag = null;
            
            StreamReader myReader = new StreamReader(e.Result);
            XDocument doc = XDocument.Load(myReader);
            string Source = (from d in doc.Descendants("item").Descendants("InputFuncSelect")
                             select d).Single().Value;
            string muted = (from d in doc.Descendants("item").Descendants("Mute")
                            select d).Single().Value;
            string volume = (from d in doc.Descendants("item").Descendants("MasterVolume")
                             select d).Single().Value;
            if (muted == "off")
            {
                MuteImage.Opacity = 0.2;
                MuteImage.Tag = null;
                //Laut
            }
            else
            {
                MuteImage.Opacity = 1.0;
                MuteImage.Tag = "ON";
                //Leise
            }
            if (Source == "Internet Radio" || Source == "NET/USB")
            {
                RadioImage.Opacity = 1.0;
                RadioImage.Tag = "ON";
                getRadioInformation();
                radioGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else if (Source == "TV")
            {
                TVImage.Opacity = 1.0;
                TVImage.Tag = "ON";
                myTimer.Stop();
                RefreshPlaying.Source = startRefresh;
                RefreshPlaying.Tag = "OFF";
            }
            else if (Source == "DVD")
            {
                DVDImage.Opacity = 1.0;
                DVDImage.Tag = "ON";
                myTimer.Stop();
                RefreshPlaying.Tag = "OFF";
                RefreshPlaying.Source = startRefresh;
            }
            else if (Source == "BD")
            {
                BDImage.Opacity = 1.0;
                BDImage.Tag = "ON";
                myTimer.Stop();
                RefreshPlaying.Tag = "OFF";
                RefreshPlaying.Source = startRefresh;
            }
            
            myReader.Close();
        }

        private void getRadioInformation()
        {
            string url = "http://" + DenonIP + "/goform/formNetAudio_StatusXml.xml?_=" + DateTimeURL() + "&ZoneName=MAIN+ZONE";
            WebClient client = new WebClient();
            client.OpenReadAsync(new Uri(url, UriKind.Absolute));
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(RadioSystemInformation);
        }

        void RadioSystemInformation(object sender, OpenReadCompletedEventArgs e)
        {
            StreamReader myReader = new StreamReader(e.Result);
            XDocument doc = XDocument.Load(myReader);
            if (RefreshPlaying.Tag == "ON")
            {
                string AktuellesLiedData = null;
                var gespielteMusik = from d in doc.Descendants("item").Descendants("szLine")
                                     select d;
                foreach (var item in gespielteMusik.Descendants("value"))
                {
                    if (!String.IsNullOrEmpty(item.Value))
                    {
                        if (AktuellesLiedData == null)
                        {
                            AktuellesLiedData = item.Value;
                        }
                        else
                        {
                            AktuellesLiedData += "\r\n" + item.Value;
                        }
                    }
                }
                textboxNowPlaying.Text = AktuellesLiedData;
                textboxNowPlaying.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                textboxNowPlaying.Text = "";
                textboxNowPlaying.Visibility = System.Windows.Visibility.Collapsed;
            }

            var gespeicherteKanaele = from d in doc.Descendants("item").Descendants("PresetLists")
                                      select d;
            List<SenderClass> senderCl = new List<SenderClass>();
            foreach (var item in gespeicherteKanaele.Descendants("value"))
            {
                if (!item.Attribute("table").Value.Contains("P") && !item.Attribute("table").Value.Contains("OFF"))
                {
                    if (!String.IsNullOrEmpty(item.Attribute("param").Value.Replace(" ", "")))
                    {
                        SenderClass clitem = new SenderClass();
                        int index = Convert.ToInt32(item.Attribute("index").Value) - 1;
                        string param = item.Attribute("param").Value;
                        clitem.index = index;
                        clitem.Sendername = param;
                        senderCl.Add(clitem);
                    }
                }
            }
            SenderListe.ItemsSource = senderCl;
            SenderListe.DisplayMemberPath = "Sendername";
        }

        private void TVImage_MouseEnter(object sender, MouseEventArgs e)
        {
            TVImage.Opacity = 1.0;
        }

        private void TVImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TVImage.Tag != "ON")
            {
                TVImage.Opacity = 0.2;
            }
        }

        private void RadioImage_MouseEnter(object sender, MouseEventArgs e)
        {
            RadioImage.Opacity = 1.0;
        }

        private void RadioImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RadioImage.Tag != "ON")
            {
                RadioImage.Opacity = 0.2;
            }
        }

        private void DVDImage_MouseEnter(object sender, MouseEventArgs e)
        {
            DVDImage.Opacity = 1.0;
        }

        private void DVDImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DVDImage.Tag != "ON")
            {
                DVDImage.Opacity = 0.2;
            }
        }

        private void BDImage_MouseEnter(object sender, MouseEventArgs e)
        {
            BDImage.Opacity = 1.0;
        }

        private void BDImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (BDImage.Tag != "ON")
            {
                BDImage.Opacity = 0.2;
            }
        }

        private void LeiserImage_MouseEnter(object sender, MouseEventArgs e)
        {
            LeiserImage.Opacity = 1.0;
        }

        private void LeiserImage_MouseLeave(object sender, MouseEventArgs e)
        {
            LeiserImage.Opacity = 0.2;
        }

        private void MuteImage_MouseEnter(object sender, MouseEventArgs e)
        {
            MuteImage.Opacity = 1.0;
        }

        private void MuteImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (MuteImage.Tag != "ON")
            {
                MuteImage.Opacity = 0.2;
            }
        }

        private void LauterImage_MouseEnter(object sender, MouseEventArgs e)
        {
            LauterImage.Opacity = 1.0;
        }

        private void LauterImage_MouseLeave(object sender, MouseEventArgs e)
        {
            LauterImage.Opacity = 0.2;
        }

        private void LeiserImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string url = "http://" + DenonIP + "/MainZone/index.put.asp?cmd0=PutMasterVolumeBtn/%3C&_=" + DateTimeURL();
            if (MuteImage.Tag == "ON")
            {
                WebClient client = new WebClient();
                client.OpenReadAsync(new Uri(url, UriKind.Absolute));
                client.OpenReadCompleted += new OpenReadCompletedEventHandler(ReceiverStartStop);
            }
            else
            {
                WebClient client = new WebClient();
                client.OpenReadAsync(new Uri(url, UriKind.Absolute));
                client.OpenReadCompleted += new OpenReadCompletedEventHandler(ReceiverSendSettingsWithoutComebach);
            }
        }

        private void MuteImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string url = "http://" + DenonIP + "/NetAudio/index.put.asp?cmd0=PutVolumeMute/";
            if (MuteImage.Tag == "ON")
            {
                //Muted
                url += "off";
                MuteImage.Tag = null;
            }
            else
            {
                //Nicht gemuted
                url += "on";
                MuteImage.Tag = "ON";
            }
            url += "&_=" + DateTimeURL();
            WebClient client = new WebClient();
            client.OpenReadAsync(new Uri(url, UriKind.Absolute));
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(ReceiverStartStop);
        }

        private void LauterImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string url = "http://" + DenonIP + "/MainZone/index.put.asp?cmd0=PutMasterVolumeBtn%2F%3E&_=" + DateTimeURL();
            if (MuteImage.Tag == "ON")
            {
                WebClient client = new WebClient();
                client.OpenReadAsync(new Uri(url, UriKind.Absolute));
                client.OpenReadCompleted += new OpenReadCompletedEventHandler(ReceiverStartStop);
            }
            else
            {
                WebClient client = new WebClient();
                client.OpenReadAsync(new Uri(url, UriKind.Absolute));
                client.OpenReadCompleted += new OpenReadCompletedEventHandler(ReceiverSendSettingsWithoutComebach);
            }
        }

        void ReceiverSendSettingsWithoutComebach(object sender, OpenReadCompletedEventArgs e)
        {
            StreamReader myReader = new StreamReader(e.Result);
            //tbData.Text = myReader.ReadToEnd();
            myReader.Close();
        }

        private void RadioImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetSource("NET/USB");
        }

        private void TVImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetSource("TV");
        }

        private void DVDImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetSource("DVD");
        }

        private void BDImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetSource("BD");
        }

        private void SetSource(string Input)
        {
            string url = "http://" + DenonIP + "/MainZone/index.put.asp?cmd0=PutZone_InputFunction/" + Input + "&=_" + DateTimeURL();
            WebClient client = new WebClient();
            client.OpenReadAsync(new Uri(url, UriKind.Absolute));
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(ReceiverStartStop);
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            sourceGrid.Visibility = System.Windows.Visibility.Collapsed;
            VolumeGrid.Visibility = System.Windows.Visibility.Collapsed;
            radioGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void SenderListe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SenderClass selected = SenderListe.SelectedItem as SenderClass;
            if (selected != null)
            {
                string index = null;
                if (selected.index < 10)
                {
                    index = "0" + selected.index;
                }
                else
                {
                    index = selected.index.ToString();
                }
                string url = "http://" + DenonIP + "/NetAudio/index.put.asp?cmd0=PutNetAudioCommand%2FPresetCall" + index + "&cmd1=aspMainZone_WebUpdateStatus%2F&_=" + DateTimeURL();
                WebClient client = new WebClient();
                client.OpenReadAsync(new Uri(url, UriKind.Absolute));
                client.OpenReadCompleted += new OpenReadCompletedEventHandler(ReceiverSendSettingsWithoutComebach);
            }
        }

        private void RefreshPlaying_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (RefreshPlaying.Tag != "ON")
            {
                RefreshPlaying.Tag = "ON";
                getRadioInformation();
                myTimer.Interval = new TimeSpan(0, 0, 0, 0, refreshIntervall); // 100 Milliseconds
                myTimer.Tick += new EventHandler(Ticker);
                myTimer.Start();
                RefreshPlaying.Source = stopRefresh;
                
                //Starte Timer
            }
            else
            {
                RefreshPlaying.Tag = "OFF";
                myTimer.Stop();
                textboxNowPlaying.Text = "";
                textboxNowPlaying.Visibility = System.Windows.Visibility.Collapsed;
                RefreshPlaying.Source = startRefresh;
                //Beende Timer
            }
            
        }

        private void Ticker(object o, EventArgs sender)
        {
            getRadioInformation();
        }

        


    }
    public class SenderClass
    {
        public int index { get; set; }

        public string Sendername { get; set; }
    }

}
