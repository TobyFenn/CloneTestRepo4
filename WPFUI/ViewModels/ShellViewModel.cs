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

        Coordinate aircraftPos;
        Presets presets;
        Time time = new Time();
        Packets packets = new Packets();
        Calculations calculations = new Calculations();
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

		public ShellViewModel()
		{

		}

  //      public ShellViewModel()
		//{
		//	People.Add(new PersonModel { FirstName = "Toby", LastName = "Fenner" });
		//	People.Add(new PersonModel { FirstName = "John", LastName = "Smith" });
		//	People.Add(new PersonModel { FirstName = "Jane", LastName = "Doe" });
		//}

		//private string _firstName = "Toby";
  //      private string _lastName;
  //      private BindableCollection<PersonModel> _people = new BindableCollection<PersonModel>();
  //      private PersonModel _selectedPerson;
  //      public string FirstName
		//{
		//	get
		//	{
		//		return _firstName;
		//	}
		//	set
		//	{
		//		_firstName = value;
		//		NotifyOfPropertyChange(() => FirstName);
  //              NotifyOfPropertyChange(() => FullName);
  //          }
		//}

		//public string LastName
		//{
		//	get
		//	{
		//		return _lastName;
		//	}
		//	set
		//	{
		//		_lastName = value;
  //              NotifyOfPropertyChange(() => LastName);
  //              NotifyOfPropertyChange(() => FullName);
  //          }
		//}

		//public string FullName
		//{
		//	get { return $"{ FirstName } { LastName }"; }
		//}

		//public BindableCollection<PersonModel> People
		//{
		//	get { return _people; }
		//	set { _people = value; }
		//}

		//public PersonModel SelectedPerson
		//{
		//	get { return _selectedPerson; }
		//	set
		//	{
		//		_selectedPerson = value;
		//		NotifyOfPropertyChange(() => SelectedPerson);
		//	}
		//}

		//public bool CanClearText(string firstName, string lastName)
		//{

		//	if (String.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
		//	{
		//		return false;
		//	}
		//	else
		//	{
		//		return true;
		//	}
		//}

		//public void ClearText(string firstName, string lastName)
		//{
		//	FirstName = "";
		//	LastName = "";
		//}

		//public void LoadPageOne()
		//{
		//	ActivateItemAsync(new FirstChildViewModel());
		//}

		//public void LoadPageTwo()
		//{
		//	ActivateItemAsync(new SecondChildViewModel());
		//}



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
