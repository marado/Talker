using System;

namespace Talker.Commands
{
	public class Quit : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			CurrentInput.User.WriteLine("Thank you for coming and Goodbye!");
			Server.ClientList.Remove(CurrentInput.User);
			CurrentInput.User.Save();
			CurrentInput.User.Quit();
		}

		public string Name {
			get {
				return "quit";
			}
		}
	}

	public class ChangeName : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine(".name <new name>");
				return;
			}

			if(CurrentInput.User.Login(CurrentInput.Args[1]) == User.LoginResult.ValidLogin) {
				CurrentInput.User.WriteLine("You are now logged in as \"" + CurrentInput.User.Name + "\"");
			} else if(CurrentInput.User.Login(CurrentInput.Args[1]) == User.LoginResult.OpenUserName) {
				//TODO: save user after name change
				CurrentInput.User.Name = CurrentInput.Args[1];
				CurrentInput.User.WriteLine("Your name has been changed to \"" + CurrentInput.User.Name + "\"");
			}
		}
		
		public string Name {
			get {
				return "name";
			}
		}
	}

	public class InMsg : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine(String.Format("Your current in phrase is: {0}", CurrentInput.User.InMsg));
				return;
			}

			CurrentInput.User.InMsg = CurrentInput.Message;
			CurrentInput.User.WriteLine("In phrase set.");
		}
		
		public string Name {
			get {
				return "inmsg";
			}
		}
	}

	public class OutMsg : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine(String.Format("Your current out phrase is: {0}", CurrentInput.User.InMsg));
				return;
			}
			
			CurrentInput.User.OutMsg = CurrentInput.Message;
			CurrentInput.User.WriteLine("Out phrase set.");
		}
		
		public string Name {
			get {
				return "outmsg";
			}
		}
	}

	public class Desc : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine("Your current description is: " + CurrentInput.User.Desc);
				return;
			}
			
			CurrentInput.User.Desc = CurrentInput.Message;
			CurrentInput.User.WriteLine("Your description has been changed to \"" + CurrentInput.User.Desc + "\"");
		}
		
		public string Name {
			get {
				return "desc";
			}
		}
	}

	public class Who : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
			CurrentInput.User.WriteLine("| Name                                           : Room            : Mins    |"); //TODO: add idle as well
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
			
			string whoLines = "";
			
			DateTime commonTime = DateTime.UtcNow;
			
			foreach(User CurrentUser in Server.ClientList) {
				TimeSpan minsOnline = commonTime - CurrentUser.Logon;
				
				whoLines += String.Format("| {0,-46} : {1,-15} : {2,-6} |\n", CurrentUser.Name + CurrentUser.Desc, CurrentUser.Room.Name, Math.Round(minsOnline.TotalMinutes));
			}
			
			CurrentInput.User.Write(whoLines);
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
			CurrentInput.User.WriteLine(String.Format("|                           Total of {0} user(s)                               |", Server.ClientList.Count));
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
		}
		
		public string Name {
			get {
				return "who";
			}
		}
	}

	public class Help : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			string output = "";
			string line = "";
			short lineLength = 0;

			Server.CommandList.ForEach(delegate(ICommand currentCommand) {
				line += String.Format("{0, -12}", currentCommand.Name.PadRight(10));

				if(lineLength == 5) {
					output += String.Format("| {0, -74} |\n", line);
					line = "";
					lineLength = 0;
				} else {
					lineLength++;
				}
			});

			if(!String.IsNullOrEmpty(line))  { //check for final line
				output += String.Format("| {0, -74} |\n", line);
			}

			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
			CurrentInput.User.WriteLine("| All commands start with a \".\" (when in speech mode) and can be abbreviated |");
			CurrentInput.User.WriteLine("| Remember, a \".\" by itself will repeat your last command or speech          |");
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
			CurrentInput.User.WriteLine("|                    Commands available to you                               |");
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
			CurrentInput.User.Write(output);
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
			CurrentInput.User.WriteLine(String.Format("|  There is a total of {0} commands that you can use                          |", Server.CommandList.Count));
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
		}

		public string Name {
			get {
				return "help";
			}
		}
	}

	public class UserStatus : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			User CurrentUser = CurrentInput.User;

			if (CurrentInput.Args.Length > 1) {
				User NewUser = Server.FindClientByName(CurrentInput.Args[1]);

				if(NewUser != null) {
					CurrentUser = NewUser;
				}
			}
			TimeSpan OnlineFor = DateTime.UtcNow - CurrentUser.Logon;

			string output = "+----- User Info ---------------------------------------------+\n";
			string NameDesc = String.Format("{0} - {1} ",CurrentUser.Name, CurrentUser.Desc);
			output += String.Format("Name   : {0,-30}                Level : \n", NameDesc);
			output += String.Format("Gender : {0,-15} Age : {1, -15}  Online for : {2,-15} mins\n",CurrentUser.Gender, CurrentUser.Age, Math.Round(OnlineFor.TotalMinutes));
			output += String.Format("Email Address : {0}\n", CurrentUser.Email);
			output += String.Format("Total Logins  : {0} \n", CurrentUser.TotalLogins);
			output += "+----------------------------------------------------------------------------+\n";
			CurrentInput.User.WriteLine(output);
			/*
+----- User Info -- (currently logged on) -----------------------------------+
Name   : Test - remove this character                  Level : GOD
Gender : Male          Age : Unknown              Online for : 900 mins
Email Address : Currently unset
Homepage URL  : Currently unset
ICQ Number    : Currently unset
Total Logins  : 8          Total login : 0 days, 16 hours, 8 minutes
+----- General Info ---------------------------------------------------------+
Enter Msg     : Test enters
Exit Msg      : Test goes to the...
Invited to    : <nowhere>      Muzzled : NO             Ignoring : NO           
In Area       : reception      At home : YES            New Mail : NO           
Killed 0 people, and died 0 times.  Energy : 10, Bullets : 6
+----- User Only Info -------------------------------------------------------+
Char echo     : NO             Wrap    : NO             Monitor  : NO           
Colours       : YES            Pager   : 23             Logon rm : NO           
Quick call to : <no one>       Autofwd : NO             Verified : NO           
On from site  : localhost                                   Port : 64638
+----- Wiz Only Info --------------------------------------------------------+
Unarrest Lev  : GOD            Arr lev : Unarrested     Muz Lev  : Unmuzzled    
Logon room    : reception                               Shackled : NO
Last site     : localhost
User Expires  : YES            On date : Sat 2013-01-05 18:49:59
+----------------------------------------------------------------------------+*/
		}

		public string Name {
			get {
				return "ustat";
			}
		}
	}

	public class Examine : ICommand
	{
		public void Run(UserInput currentInput)
		{
			TimeSpan onlineFor = DateTime.UtcNow - currentInput.User.Logon;
			string output = "\n+----------------------------------------------------------------------------+\n";
			output += String.Format("Name   : {0} - {1}                  Level : \n", currentInput.User.Name, currentInput.User.Desc);
//Ignoring all: NO
			output += String.Format("On since    : {0} {1}\n", currentInput.User.Logon.ToLongDateString(), currentInput.User.Logon.ToLongTimeString());
			output += String.Format("On for      : {0} day, {1} hours, {2} minutes\n", onlineFor.Days, onlineFor.Hours, onlineFor.Minutes);

//TODO: add profile
/*
 * Idle for    : 0 minutes
Total login : 2 days, 5 hours, 38 minutes
 *Site        : localhost                                 Port : 50717
/*+----- Profile --------------------------------------------------------------+

User has not yet written a profile.
*/
			output += "+----------------------------------------------------------------------------+\n";

			currentInput.User.WriteLine(output);
		}

		public string Name {
			get {
				return "examine";
			}
		}
	}

	public class ClearScreen : ICommand
	{
		public void Run(UserInput currentInput)
		{
			currentInput.User.ClearScreen();
		}

		public string Name {
			get {
				return "cls";
			}
		}
	}

	public class Rules : ICommand
	{
		public void Run(UserInput currentInput)
		{
			currentInput.User.WriteLine(TalkerFile.GetFile("rules"));
		}

		public string Name {
			get {
				return "rules";
			}
		}
	}

	public class IgnoreAll : ICommand
	{
		public void Run(UserInput currentInput)
		{
			if(currentInput.User.Ignores == User.Ignore.None) {
				currentInput.User.WriteLine("You are now ignoring everyone.");
				currentInput.User.Ignores = User.Ignore.All;
			} else {
				currentInput.User.WriteLine("You will now hear everyone again.");
				currentInput.User.Ignores = User.Ignore.None;
			}
		}

		public string Name {
			get {
				return "ignall";
			}
		}
	}

	public class IgnoreBeeps : ICommand
	{
		public void Run(UserInput currentInput)
		{
			currentInput.User.Ignores ^= User.Ignore.Beeps;
			
			if(currentInput.User.Ignores.HasFlag(User.Ignore.Beeps)) {
				currentInput.User.WriteLine("You are now ignoring beeps.");
			} else {
				currentInput.User.WriteLine("You will now hear beeps again.");
			}
		}
		
		public string Name {
			get {
				return "ignbeeps";
			}
		}
	}
	public class IgnoreGreets : ICommand
	{
		public void Run(UserInput currentInput)
		{
			currentInput.User.Ignores ^= User.Ignore.Greets;
			
			if(currentInput.User.Ignores.HasFlag(User.Ignore.Greets)) {
				currentInput.User.WriteLine("You are now ignoring greets.");
			} else {
				currentInput.User.WriteLine("You will now hear greets again.");
			}
		}
		
		public string Name {
			get {
				return "igngreets";
			}
		}
	}
	public class IgnoreLogons : ICommand
	{
		public void Run(UserInput currentInput)
		{
			currentInput.User.Ignores ^= User.Ignore.Logons;
			
			if(currentInput.User.Ignores.HasFlag(User.Ignore.Logons)) {
				currentInput.User.WriteLine("You are now ignoring logons.");
			} else {
				currentInput.User.WriteLine("You will now see logons again.");
			}
		}
		
		public string Name {
			get {
				return "ignlogons";
			}
		}
	}

	public class IgnorePictures : ICommand
	{
		public void Run(UserInput currentInput)
		{
			currentInput.User.Ignores ^= User.Ignore.Pics;
			
			if(currentInput.User.Ignores.HasFlag(User.Ignore.Pics)) {
				currentInput.User.WriteLine("You are now ignoring pictures.");
			} else {
				currentInput.User.WriteLine("You will see pictures.");
			}
		}
		
		public string Name {
			get {
				return "ignpics";
			}
		}
	}

	public class IgnoreShout : ICommand
	{
		public void Run(UserInput currentInput)
		{
			currentInput.User.Ignores ^= User.Ignore.Shout;
			
			if(currentInput.User.Ignores.HasFlag(User.Ignore.Shout)) {
				currentInput.User.WriteLine("You are now ignoring shouts.");
			} else {
				currentInput.User.WriteLine("You will now hear shouts again.");
			}
		}
		
		public string Name {
			get {
				return "ignshout";
			}
		}
	}

	public class IgnoreTell : ICommand
	{
		public void Run(UserInput currentInput)
		{
			currentInput.User.Ignores ^= User.Ignore.Tell;
			
			if(currentInput.User.Ignores.HasFlag(User.Ignore.Tell)) {
				currentInput.User.WriteLine("You are now ignoring tells.");
			} else {
				currentInput.User.WriteLine("You will now hear tells again.");
			}
		}
		
		public string Name {
			get {
				return "igntell";
			}
		}
	}

	public class IgnoreList : ICommand
	{
		public void Run(UserInput currentInput)
		{	
			bool ignShouts = currentInput.User.Ignores.HasFlag(User.Ignore.Shout) | currentInput.User.Ignores.HasFlag(User.Ignore.All);
			bool ignTells = currentInput.User.Ignores.HasFlag(User.Ignore.Tell) | currentInput.User.Ignores.HasFlag(User.Ignore.All);
			bool ignLogons = currentInput.User.Ignores.HasFlag(User.Ignore.Logons) | currentInput.User.Ignores.HasFlag(User.Ignore.All);
			bool ignPictures = currentInput.User.Ignores.HasFlag(User.Ignore.Pics) | currentInput.User.Ignores.HasFlag(User.Ignore.All);
			bool ignGreets = currentInput.User.Ignores.HasFlag(User.Ignore.Greets) | currentInput.User.Ignores.HasFlag(User.Ignore.All);
			bool ignBeeps = currentInput.User.Ignores.HasFlag(User.Ignore.Beeps) | currentInput.User.Ignores.HasFlag(User.Ignore.All);

			string output = "\n+----------------------------------------------------------------------------+\n";
			output += "| Your ignore states are as follows                                          |\n";
			output += "+----------------------------------------------------------------------------+\n";
			output += String.Format("| Ignoring shouts   : {0, -5} Ignoring tells  : {1, -5} Ignoring logons : {2,-6} |\n", ignShouts.ToYesNoString(), ignTells.ToYesNoString(), ignLogons.ToYesNoString());
			output += String.Format("| Ignoring pictures : {0, -5} Ignoring greets : {1, -5} Ignoring beeps  : {2,-6} |\n", ignPictures.ToYesNoString(), ignGreets.ToYesNoString(), ignBeeps.ToYesNoString());
			output += String.Format("+----------------------------------------------------------------------------+\n");

			currentInput.User.WriteLine(output);
		}
		
		public string Name {
			get {
				return "ignlist";
			}
		}
	}

	public class Listen : ICommand
	{
		public void Run(UserInput currentInput)
		{	
			if(currentInput.User.Ignores == User.Ignore.None) {
				currentInput.User.WriteLine("You are already listening to everything..");
			} else {
				currentInput.User.WriteLine("You listen to everything again.");
				currentInput.User.Ignores = User.Ignore.None;
			}
		}
		
		public string Name {
			get {
				return "listen";
			}
		}
	}
}

