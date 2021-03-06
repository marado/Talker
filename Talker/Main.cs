using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace Talker
{
	class Talker
	{
		private static TcpListener tcpListener;
		private static Thread listenThread;

		public static void Main(string[] args)
		{
			// Check if the config file is correctly loaded
			if (System.Configuration.ConfigurationManager.AppSettings.Count == 0) {
				Console.WriteLine("ERROR: config file not loaded");
				return;
			}

			int AddressPort = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["port"]);
			//TODO: error message port must be above 1024
			tcpListener = new TcpListener(IPAddress.Any, AddressPort);
			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.Start();
			Console.WriteLine("Started server at " + AddressPort);
		}

		private static void ListenForClients()
		{
			tcpListener.Start();

			while (true) {
				TcpClient client = tcpListener.AcceptTcpClient();

				Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientCommunication));

				
				Random userNamer = new Random();
				int userNumber = userNamer.Next(0, 300);

				User UserObj = new User(client, userNumber);
				UserObj.Logon = DateTime.UtcNow;
				UserObj.Room = Server.LoginRoom;
				UserObj.Room.Users.Add(UserObj);

				Server.ClientList.Add(UserObj);
				clientThread.Start(UserObj);

				List<User> WriteAllButUsers = new List<User>();

				Server.ClientList.ForEach(delegate(User currentUser) {
					if(currentUser.Ignores.HasFlag(User.Ignore.Logons)
					   || currentUser.Ignores.HasFlag(User.Ignore.All)) {
						WriteAllButUsers.Add(currentUser);
					}
				});

				Server.CommandList.ForEach(delegate(ICommand command) {
					if(command.Name.Equals("look")) {
						UserInput newInput = new UserInput(UserObj, "look");
						command.Run(newInput);
					}
				});

				Server.WriteAllBut("[Entering is: " + UserObj.Name + " " + UserObj.Desc + " ] \n", WriteAllButUsers);
			}
		}

		private static void HandleClientCommunication(object client)
		{
			//NOTE since each communication is its own thread it is ok to block for a single user
			//althrough async would be the future plans even for a single user. 
			//this server wouldn't need to be multi threaded if we used async stuff. MONO 3.0!
			User userObj = (User)client;
			NetworkStream clientStream = userObj.Stream;

			byte[] message = new byte[4096];
			int bytesRead;

			while(true) {
				bytesRead = 0;

				try {
					bytesRead = clientStream.Read(message, 0, 4096);
				} catch {
					break;
				}

				UTF8Encoding encoder = new UTF8Encoding();
				string userInput = encoder.GetString(message, 0, bytesRead).Trim();

				UserInput CurrentInput = new UserInput(userObj, userInput);

				if(userInput.StartsWith(".")) {
					if(userInput.Trim().Length == 1) {
						userObj.LastCommand.Run(userObj.LastInput);
						continue;
					}

					//TODO: need to check partial input as well
					ICommand CurrentCommand = Server.CommandList.Find(x => x.Name.Equals(CurrentInput.Args[0].ToLower()));

					if(CurrentCommand != null) {
						userObj.LastCommand = CurrentCommand;
						userObj.LastInput = CurrentInput;

						CurrentCommand.Run(CurrentInput);
					} else {
						userObj.WriteLine("Unknown command.");
					}
				} else {
					//TODO: setup default command thing.. 
					Server.DefaultCommand.Run(CurrentInput);
				}
			}
		}
	}
}
