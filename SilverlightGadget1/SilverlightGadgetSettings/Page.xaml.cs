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

namespace SilverlightGadgetSettings
{
    public partial class Page : UserControl
    {
        SilverlightGadgetEvents evHelper;
        public Page()
        {
            InitializeComponent();
            evHelper = new SilverlightGadgetEvents("SilverlightGadgetEvents");
            evHelper.SettingsClosing += new EventHandler<SettingsClosingEventArgs>(evHelper_SettingsClosing);
            evHelper.SettingsClosed += new EventHandler<SettingsClosingEventArgs>(evHelper_SettingsClosed);

            // Set settings page size according to the one specified in the Silverlight control.
            SilverlightGadget.SetPageSize(Width, Height);
        }

        void evHelper_SettingsClosed(object sender, SettingsClosingEventArgs e)
        {
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // If through code the size of the Silverlight control is changed, change the size in the javascript gadget also.
            SilverlightGadget.SetPageSize(e.NewSize.Width, e.NewSize.Height);
        }

        void evHelper_SettingsClosing(object sender, SettingsClosingEventArgs e)
        {
            if (e.Action == CloseAction.Commit)
            {
                // Put code for saving settings here
                SilverlightGadget.Settings["DENONIP"] = ip1tb.Text + "." + ip2tb.Text + "." + ip3tb.Text + "." + ip4tb.Text;
                SilverlightGadget.Settings["TickerTime"] = timerTB.Text;
            }
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(SilverlightGadget.Settings["DENONIP"]))
            {
                string ipDenon = SilverlightGadget.Settings["DENONIP"];
                string[] splittet = ipDenon.Split('.');
                ip1tb.Text = splittet[0];
                ip2tb.Text = splittet[1];
                ip3tb.Text = splittet[2];
                ip4tb.Text = splittet[3];
            }
            else
            {
                ip1tb.Text = "192";
                ip2tb.Text = "168";
                ip3tb.Text = "1";
                ip4tb.Text = "22";
            }
            if (!String.IsNullOrEmpty(SilverlightGadget.Settings["TickerTime"]))
            {
                timerTB.Text = SilverlightGadget.Settings["TickerTime"];
            }
            else
            {
                timerTB.Text = "60";
            }
        }
    }
}
