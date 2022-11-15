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
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media.Animation;

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
        List<CheckBox> interfaceCheckboxes = new List<CheckBox>();
        public List<CheckBox> CBList = new List<CheckBox>();

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
/*                        spNics.Children.Add(cb);*/ //<-- stackpanel in shellview. use binding. commented for build


                        var CB1 = new CheckBox();
                        CB1.Content = "checkbox";
                        //CBList.Add(CB1);  <--- see above. test

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

        //-----
        //working on dynamic view elements w. databinding to vm. adding/removing elements. use itemscontrol.

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void InputBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //temp variables for build
        //databinding overrides these but just keep til viewmodel entirely done

        String dummy = "dummy";
        Button RunButton = new Button();
        Button PauseButton = new Button();
        Button StopButton = new Button();
        Button spNics = new Button();
        Button AirspeedUnitsInputBox = new Button();
        Button CurrentPositionSaveButton = new Button();
        Button CTSCheck = new Button();
        Button CTSCopy = new Button();
        TextBlock CTSInputBox = new TextBlock();
        TextBlock CTSOutputBox = new TextBlock();

        private void setInitialConditions(string[] inputValues)
        {
            initialLat = Convert.ToDouble(inputValues[0]);
            initialLong = Convert.ToDouble(inputValues[1]);
            outputYaw_deg = Convert.ToDouble(inputValues[2]);
            outputPitch_deg = Convert.ToDouble(inputValues[3]);
            outputRoll_deg = Convert.ToDouble(inputValues[4]);
            yawROC_deg = Convert.ToDouble(inputValues[5]);
            pitchROC_deg = Convert.ToDouble(inputValues[6]);
            rollROC_deg = Convert.ToDouble(inputValues[7]);
            altitude_ft = Convert.ToDouble(inputValues[8]);
            unconvertedAirspeed = Convert.ToDouble(inputValues[9]);

            //save a list of initialConditions that were entered when the sim was first run
            initialConditions = new List<double> { initialLat, initialLong, outputYaw_deg, outputPitch_deg, outputRoll_deg, yawROC_deg, pitchROC_deg, rollROC_deg, altitude_ft, unconvertedAirspeed };

            convertedAirspeed = calculations.ConvertAirspeedUnits(unitsInputText, unconvertedAirspeed);
            //time.SetDateTime(CustomTimestampInputBox.Text);
        }

        private void OnPauseButtonClicked(object sender, RoutedEventArgs e)
        {
            //paused is initialized as false
            //pause button is clicked
            if (paused == false)
            {
                PauseButton.Content = "▶";
                valuesOnPause = new List<double> { outputLat_deg, outputLong_deg, outputYaw_deg, outputPitch_deg, outputRoll_deg, yawROC_deg, pitchROC_deg, rollROC_deg, altitude_ft, unconvertedAirspeed };

                prevTime = DateTime.Parse(CTSOutputBox.Text);
                spNics.IsEnabled = true;
                EnableElements(currentTextBoxes);
                EnableElements(copyButtons);
                if (enableCTS)
                {
                    CTSCopy.IsEnabled = true;
                    CTSOutputBox.IsEnabled = true;
                }

                CurrentPositionSaveButton.IsEnabled = true;

                paused = true;
            }
            //unpause
            else
            {
                valuesOnUnpause = new List<double>();

                //check if "current position" textbox values are valid
                //if (getInvalidInputs(GetCurrentPositionValues()).Count == 0)
                //{
                //    foreach (string s in GetCurrentPositionValues())
                //{
                //    valuesOnUnpause.Add(Convert.ToDouble(s));
                //}
                {

                    for (int i = 0; i < valuesOnUnpause.Count; i++)
                    {
                        // if the user has changed a value from its state when it was initially paused
                        if (!calculations.AreEqual(valuesOnPause[i], valuesOnUnpause[i]))
                        {
                            //paused value now equal to that user change
                            valuesOnPause[i] = valuesOnUnpause[i];
                        }
                    }

                    aircraftPos = new CoordinateModel(valuesOnPause[0], valuesOnPause[1]);
                    outputYaw_deg = valuesOnPause[2];
                    outputPitch_deg = valuesOnPause[3];
                    outputRoll_deg = valuesOnPause[4];
                    yawROC_deg = valuesOnPause[5];
                    pitchROC_deg = valuesOnPause[6];
                    rollROC_deg = valuesOnPause[7];
                    altitude_ft = valuesOnPause[8];
                    unconvertedAirspeed = valuesOnPause[9];
                    convertedAirspeed = calculations.ConvertAirspeedUnits(unitsInputText, unconvertedAirspeed);

                    //check if custom timestamp box is still valid
                    if (DateTime.TryParse(CTSOutputBox.Text, out currTime))
                    {
                        if ((currTime - prevTime).TotalMilliseconds != 0)
                        {
                            time.SetDateTime(currTime);
                        }
                        prevTime = currTime;
                    }

                    DisableElements(currentTextBoxes);
                    DisableElements(copyButtons);
                    CurrentPositionSaveButton.IsEnabled = false;
                    CTSCopy.IsEnabled = false;
                    CTSOutputBox.IsEnabled = false;
                    spNics.IsEnabled = false;
                    paused = false;
                    PauseButton.Content = "❚❚";
                }
            }
        }

        private void saveDefaultPresets()
        {
            SavePreset(new string[] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" }, "Zeros", "m/s");
            SavePreset(new string[] { "45.678143", "-121.552", "0", "0", "0", "5", "0", "0", "3000", "50" }, "Orbit Windmaster CW", "m/s");
            SavePreset(new string[] { "45.678143", "-121.552", "0", "0", "0", "-5", "0", "0", "3000", "50" }, "Orbit Windmaster CCW", "m/s");
        }

        private void OnStopButtonClicked(object sender, RoutedEventArgs e)
        {
            RunButton.Visibility = Visibility.Visible;
            PauseButton.Visibility = Visibility.Hidden;
            StopButton.Visibility = Visibility.Hidden;

            EnableElements(initialTextBoxes);
            EnableElements(copyButtons);
            DisableElements(currentTextBoxes);
            spNics.IsEnabled = true;
            AirspeedUnitsInputBox.IsEnabled = true;
            CurrentPositionSaveButton.IsEnabled = true;
            CTSCheck.IsEnabled = true;
            TimerUpdateUI.Stop();
            cancelTokenSource.Cancel();

            if (enableCTS)
            {
                CTSCopy.IsEnabled = true;
                CTSInputBox.IsEnabled = true;
            }

            CTSOutputBox.IsEnabled = false;
        }

        

        /*
         *  Run button pressed
         */
        private void OnRunButtonClick(object sender, RoutedEventArgs e)
        {
            //if (getInvalidInputs(GetInitialPositionValues()).Count > 0)
            {
                canStart = false;
            }
            //else
            {
                canStart = true;
            }

            if (useCTS)
            {
                if (DateTime.TryParse(CTSInputBox.Text, out customDateTime))
                {
                    time.SetDateTime(customDateTime);
                    time.UseCustom = true;
                    validDateTime = true;
                }
                else
                {
                    CreateSimpleAnimation(dummy, Border.OpacityProperty, 0.25, 1, true);
                    validDateTime = false;
                }
            }
            else
            {
                validDateTime = true;
                time.UseCustom = false;
            }


            if (canStart && validDateTime)
            {
                paused = false;
                RunButton.Visibility = Visibility.Hidden;
                PauseButton.Content = "❚❚";
                PauseButton.Visibility = Visibility.Visible;
                StopButton.Visibility = Visibility.Visible;

                DisableElements(initialTextBoxes);
                DisableElements(copyButtons);
                AirspeedUnitsInputBox.IsEnabled = false;
                CurrentPositionSaveButton.IsEnabled = false;
                CTSCheck.IsEnabled = false;
                CTSInputBox.IsEnabled = false;
                CTSCopy.IsEnabled = false;
                spNics.IsEnabled = false;

                //setInitialConditions(GetInitialPositionValues());
                setInterfaces();

                if (TimerUpdateUI == null)
                {
                    TimerUpdateUI = new DispatcherTimer(DispatcherPriority.Send);
                    TimerUpdateUI.Interval = TimeSpan.FromMilliseconds(100);
                    TimerUpdateUI.IsEnabled = true;
                    TimerUpdateUI.Tick += TimerUpdateUI_Tick;
                }

                TimerUpdateUI.Start();

                stopwatch.Reset();
                stopwatch.Start();

                cancelTokenSource = new CancellationTokenSource();
                // Start the receive thread.
                simThread = new Thread(() => simThreadTick(cancelTokenSource.Token))
                {
                    Name = "SimThread"
                };
                simThread.IsBackground = true; // Background threads are disposed automatically when the application is closed.
                simThread.Start();
            }
        }
        private Thread simThread;

        private void simThreadTick(CancellationToken cts)
        {
            while (cts.IsCancellationRequested == false)
            {
                tick();
                Thread.Sleep(updateRate_ms);
            }
        }

        private void tick()
        {
            TimeSpan loopSpan = stopwatch.Elapsed;
            loopMillis = loopSpan.TotalMilliseconds;
            Debug.WriteLine(loopMillis);
            stopwatch.Reset();
            stopwatch.Start();

            runSim();

            if (useCTS)
            {
                //update the custom time
                time.UpdateTime(loopSpan);
            }
        }

        private void TimerUpdateUI_Tick(object sender, EventArgs e)
        {
            updateOutputLabels();
        }

        private void runSim()
        {
            if (paused == false)
            {

                outputYaw_deg = calculations.calculateYaw(outputYaw_deg, yawROC_deg, loopMillis);
                outputPitch_deg = calculations.calculatePitch(outputPitch_deg, pitchROC_deg, loopMillis);
                outputRoll_deg = calculations.calculateRoll(outputRoll_deg, rollROC_deg, loopMillis);

                double distanceTraveled = convertedAirspeed * loopMillis / 1000.0;


                aircraftPos = calculations.moveGeodetic(aircraftPos, distanceTraveled, outputYaw_deg);

                outputLat_deg = aircraftPos.getLat();
                outputLong_deg = aircraftPos.getLon();

                X = calculations.calculateX(outputYaw_deg, convertedAirspeed);
                Y = calculations.calculateY(outputYaw_deg, convertedAirspeed);

                double[] packetDoubles =
                    { X, Y, Z, ConvertToRadians(outputLat_deg), ConvertToRadians(outputLong_deg), calculations.ConvertToMeters(altitude_ft), climbAngle, convertedAirspeed, groundSpeed, groundCourse, ConvertToRadians(outputYaw_deg),
                      ConvertToRadians(outputPitch_deg), ConvertToRadians(outputRoll_deg), ConvertToRadians(yawROC_deg), ConvertToRadians(pitchROC_deg), ConvertToRadians(rollROC_deg)
                    };

                packets.sendPackets(packetDoubles, time);
            }
        }

        private void updateOutputLabels()
        {
            if (paused == false)
            {
                //yawOutputTextBox.Text = outputYaw_deg.ToString("N3");
                //pitchOutputTextBox.Text = outputPitch_deg.ToString("N3");
                //rollOutputTextBox.Text = outputRoll_deg.ToString("N3");
                //altitudeOutputTextBox.Text = altitude_ft.ToString("N0");
                //airspeedOutputTextBox.Text = unconvertedAirspeed.ToString();
                //longOutputTextBox.Text = outputLong_deg.ToString("N6");
                //latOutputTextBox.Text = outputLat_deg.ToString("N6");
                //yawROCOutputTextBox.Text = yawROC_deg.ToString();
                //pitchROCOutputTextBox.Text = pitchROC_deg.ToString();
                //rollROCOutputTextBox.Text = rollROC_deg.ToString();
                //XOutputTextBox.Text = X.ToString("N3");
                //YOutputTextBox.Text = Y.ToString("N3");
                //ZOutputTextBox.Text = Z.ToString("N3");
                //climbAngleOutputTextBox.Text = climbAngle.ToString();
                //groundSpeedOutputTextBox.Text = groundSpeed.ToString();
                //groundCourseOutputTextBox.Text = groundCourse.ToString();
                CTSOutputBox.Text = time.GetNow();
            }
        }

        /*
         * All these are bound to buttons via caliburn. Inside of functions overwritten w binding
         */

        private void LatCopy(object sender, RoutedEventArgs e)
        {
            //LatInputBox.Text = latOutputTextBox.Text;
        }

        private void LongCopy(object sender, RoutedEventArgs e)
        {
            //LongInputBox.Text = longOutputTextBox.Text;
        }

        private void YawCopy(object sender, RoutedEventArgs e)
        {
            //YawInputBox.Text = yawOutputTextBox.Text;
        }

        private void PitchCopy(object sender, RoutedEventArgs e)
        {
            //PitchInputBox.Text = pitchOutputTextBox.Text;
        }

        private void RollCopy(object sender, RoutedEventArgs e)
        {
            //RollInputBox.Text = rollOutputTextBox.Text;
        }

        private void YawROCCopy(object sender, RoutedEventArgs e)
        {
            //YawROCInputBox.Text = yawROCOutputTextBox.Text;
        }

        private void PitchROCCopy(object sender, RoutedEventArgs e)
        {
            //PitchROCInputBox.Text = pitchROCOutputTextBox.Text;
        }
        private void RollROCCopy(object sender, RoutedEventArgs e)
        {
            //RollROCInputBox.Text = rollROCOutputTextBox.Text;
        }

        private void AltitudeCopy(object sender, RoutedEventArgs e)
        {
            //AltitudeInputBox.Text = altitudeOutputTextBox.Text;
        }
        private void AirspeedCopy(object sender, RoutedEventArgs e)
        {
            //AirspeedInputBox.Text = airspeedOutputTextBox.Text;
        }

        private void TimestampCopy(object sender, RoutedEventArgs e)
        {
            CTSInputBox.Text = CTSOutputBox.Text;
        }



        private void CopyAllClicked(object sender, RoutedEventArgs e)
        {

            //binding one way view elements textebox contents --> inputbox text (populate)

        }

        private void InitialPositionSaveButton_Click(object sender, RoutedEventArgs e)
        {
            //string[] positionText = GetInitialPositionValues();
            //string presetName = PresetTextBox.Text;
            //string units = AirspeedUnitsInputBox.Text;

        }

        private void CurrentPositionSaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            //if (presetSelected.Length > 0)
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(Application.Current.MainWindow, "Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    //presets.DeletePreset(presetSelected);
                }
            }

            //Set the trajectories combobox source to the saved presets in Settings.settings
        }

        private void TimestampCheckboxChecked(object sender, RoutedEventArgs e)
        {
            ChangeCustomTSEnabled();
            useCTS = true;
        }

        private void TimestampCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            ChangeCustomTSEnabled();
            useCTS = false;
        }


        private void OnInitialized(object sender, EventArgs e)
        {

        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("NCPA.cpl");
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
        }


        /*
         * Infrastructure method to save a preset 
         */
        private void SavePreset(string[] currentText, string presetName, string units)
        {
            double[] presetValues = new double[10];
            try
            {
                //set presetValues
                for (int i = 0; i < currentText.Length; i++)
                {
                    if (currentText[i].Length > 0)
                    {
                        presetValues[i] = Convert.ToDouble(currentText[i]);
                    }
                    //found null value
                    else
                    {
                        presetValues[i] = 0;
                    }
                }

                if (!presets.ContainsName(presetName))
                {
                    presets.SaveNewPreset(presetName, presetValues, units);
                }
                else
                {
                    presets.UpdatePreset(presetName, presetValues, units);
                }
            }
            catch
            {

            }
        }


        private List<int> getInvalidInputs(string[] inputs)
        {
            double d;
            List<int> nullIndices = new List<int>();
            for (int i = 0; i < inputs.Length; i++)
            {
                if (!Double.TryParse(inputs[i], out d))
                {
                    nullIndices.Add(i);
                }
            }
            return nullIndices;
        }

        private void DisableElements(List<FrameworkElement> elements)
        {
            foreach (FrameworkElement f in elements)
            {
                f.IsEnabled = false;
            }
        }

        private void EnableElements(List<FrameworkElement> elements)
        {
            foreach (FrameworkElement f in elements)
            {
                f.IsEnabled = true;
            }
        }

        // change the enabled or disabled status of the elements in the custom timestamp section
        private void ChangeCustomTSEnabled()
        {
            enableCTS = !enableCTS;
            //CTSLabel.IsEnabled = enableCTS;
            CTSInputBox.IsEnabled = enableCTS;
        }

        /*
         * Create a simple animation by specifying property, name of element being animated, duration, and to value
         */
        private void CreateSimpleAnimation(string name, DependencyProperty animationProperty, double duration_sec, double to, bool autoReverse)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = to;
            animation.Duration = new Duration(TimeSpan.FromSeconds(duration_sec));
            animation.AutoReverse = autoReverse;
            sb.Children.Add(animation);
            Storyboard.SetTargetProperty(animation, new PropertyPath(animationProperty));
            Storyboard.SetTargetName(animation, name);
            //sb.Begin(SimMainWindow);
            //---bind ^
        }

        private double ConvertToRadians(double deg)
        {
            return Math.PI / 180.0 * deg;
        }

    }
}
