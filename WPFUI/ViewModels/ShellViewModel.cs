using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;
using WPFUI.Models;
using System.Windows.Documents;

namespace WPFUI.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {

        private readonly SettingsManager<UserSettings> _settingsManager;
        private UserSettings _userSettings;

        private int numberOfUserValues = 10;

        //active sim values
        private double initialLat, initialLong;
        private double outputLat_deg, outputLong_deg;
        private double outputYaw_deg, outputPitch_deg, outputRoll_deg;
        private double yawROC_deg, pitchROC_deg, rollROC_deg;
        private double unconvertedAirspeed, convertedAirspeed, altitude_ft;

        //unused values
        private double X = 0, Y = 0, Z = 0;
        private double climbAngle = 0, groundSpeed = 0, groundCourse = 0;

        private bool canStart = false;
        private bool paused = false;
        private bool useCTS = false; //custom time stamp elements enabled (t/f)
        private bool enableCTS = false;
        private bool validDateTime = false;
        private DateTime customDateTime;

        private const int updateRate_ms = 30;
        private double loopMillis = 0;

        CoordinateModel aircraftPos;
        PresetsModel presets;
        TimeModel time = new TimeModel();
        PacketsModel packets = new PacketsModel();
        CalculationsModel calculations = new CalculationsModel();
        Stopwatch stopwatch = new Stopwatch();
        DispatcherTimer TimerUpdateUI = null;

        private delegate void runSimDelegate();

        private List<double> initialConditions;
        private List<double> valuesOnUnpause;
        private List<double> valuesOnPause;
        private DateTime prevTime;
        private DateTime currTime;

        string unitsInputText;

        private List<FrameworkElement> copyButtons;
        private List<FrameworkElement> initialTextBoxes;
        private List<FrameworkElement> currentTextBoxes;
        static CancellationTokenSource cancelTokenSource;
        List<NetworkInterface> Interfaces = new List<NetworkInterface>();
        public List<CheckBox> interfaceCheckboxes = new List<CheckBox>();

		public ShellViewModel()
		{

            bool first = true;
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up && ni.SupportsMulticast && ni.GetIPProperties().GetIPv4Properties() != null)
                {
                    int id = ni.GetIPProperties().GetIPv4Properties().Index;
                    if (NetworkInterface.LoopbackInterfaceIndex != id)
                    {
                        Interfaces.Add(ni);

                        var cb = new CheckBox();
                        cb.Content = ni.Name;
                        cb.VerticalAlignment = VerticalAlignment.Center;
                        if (first)
                        {
                            first = false;
                            cb.Margin = new Thickness(0, 0, 0, 0);
                        }
                        else
                        {
                            cb.Margin = new Thickness(5, 0, 0, 0);
                        }
                        cb.IsChecked = true;
                        cb.Click += Cb_Click;
                        interfaceCheckboxes.Add(cb);
/*                        spNics.Children.Add(cb);*/ //<-- stackpanel in shellview
                    }
                }
            }
         }

        private void Cb_Click(object sender, RoutedEventArgs e)
        {
            setInterfaces();
        }

        private void setInterfaces()
        {
            List<NetworkInterface> selInterfaces = new List<NetworkInterface>();
            for (int i = 0; i < interfaceCheckboxes.Count; i++)
            {
                if (interfaceCheckboxes[i].IsChecked ?? false)
                {
                    selInterfaces.Add(Interfaces[i]);
                }
            }

            PacketsModel.SetInterfaces(selInterfaces);

        }

        public void DeletePresetButton()
		{


		}

		public void Save()
		{

		}

		public void HyperlinkText()
		{

		}

    }
}
