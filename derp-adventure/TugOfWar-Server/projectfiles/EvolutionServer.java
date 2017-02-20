import java.io.*;
import java.net.*;
import java.util.*;

// Skyler Goodell
// Started: 6/15/2013

// Version 1.0

// This is a separate server to host clients for the Evolution: Survival of the Fittest game.
// All communication goes through this host currently, making it an essential portion of online play.
public class EvolutionServer {
	ServerSocket server;
	
	public void Start(int port)
	{
		try {
			server = new ServerSocket(port);
		}
		catch (IOException e)
		{
			System.out.println(e.toString());
			System.exit(-1);
		}
	}
	
	// /////////
	// The Lobby
	// /////////
	public static HashMap<Long, Client> Lobby = new HashMap<Long, Client>();
	public static void AddToLobby(Client c)
	{
		synchronized (Lobby)
		{
			// Notify everyone else
			// And tell the new person who everyone is in the lobby
			for (Client o : Lobby.values())
			{
				synchronized (o.myOut)
				{
					o.write("AA " + c.myID + " " + c.myName);
				}
				
				synchronized (c.myOut)
				{
					c.write("AA " + o.myID + " " + o.myName);
				}
			}
			
			Lobby.put(c.myID, c);
		}
	}
	
	public static void RemoveFromLobby(Client c)
	{
		synchronized (Lobby)
		{
			if (c != null && Lobby.containsKey(c.myID))
			{
				Lobby.remove(c.myID);
				
				// Notify everyone else
				for (Client o : Lobby.values())
				{
					synchronized (o.myOut)
					{
						o.write("BB " + c.myID);
					}
				}
			}
		}
	}
	
	// This will do a lobby challenge, cancel, accept or decline
	public static void ChallengeLobby(int type, Client from, long to)
	{
		synchronized(Lobby)
		{
			if (Lobby.containsKey(from.myID) && Lobby.containsKey(to))
			{
				Client o = Lobby.get(to);
				
				synchronized (o.myOut)
				{
					if (type == 0)
						o.write("DA " + from.myID);
					else if (type == 1)
						o.write("DB " + from.myID);
					else if (type == 2)
						o.write("DC " + from.myID);
					else if (type == 3)
						o.write("DD " + from.myID);
				}
			}
		}
	}
	
	public boolean CreateGame(Client c1, long c2ID)
	{
		synchronized (Lobby)
		{
			if (Lobby.containsKey(c1.myID) && Lobby.containsKey(c2ID))
			{
				ChallengeLobby(2, c1, c2ID);
				
				Client c2 = Lobby.get(c2ID);
				
				Object gamelock = new Object();
				
				c1.inGame = true;
				c1.opponent = c2;
				c1.gameLock = gamelock;
				
				c2.inGame = true;
				c2.opponent = c1;
				c2.gameLock = gamelock;
				
				RemoveFromLobby(c1);
				RemoveFromLobby(c2);
			}
			else
				return false;
			
			return true;
		}
	}
	
	public static void BroadcastLobby(Client c, String message)
	{
		synchronized (Lobby)
		{
			for (Client o : Lobby.values())
			{
				synchronized (o.myOut)
				{
					o.write("C " + c.myID + " " + message);
				}
			}
		}
	}
	
	// Constantly listen for a new client
	// When we recieve a new client, spin off a thread to handle them
	public void ListenForNewClient()
	{
		try {
			Socket newClient = server.accept();
			
			System.out.println("Accepted new client at " + newClient.getInetAddress().getHostName());
			SocketThread newThread = new SocketThread(newClient);
			newThread.start();
		}
		catch (Exception e)
		{
			System.out.println("Unable to accept connection:\n" + e.toString());
		}
	}
	
	///////////////////////////////////////////////////////////
	// The SocketThread is a dedicated thread for a single user
	// All communication for this user will be handled here
	///////////////////////////////////////////////////////////
	static long heartbeatPace = 10000;
	static boolean CLIENT_DEBUG = true;
	static boolean CLIENT_VERBOSE = true;
	class SocketThread extends Thread {
		Socket socket;

		Client client = null;
		
		long delayTime = 10;
		
		public SocketThread(Socket socket) throws Exception
		{
			this.socket = socket;
			
			PrintWriter out = new PrintWriter(socket.getOutputStream());
			BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream()));
			
			client = new Client("<no name>", out, in);
		}
		
		// The main thread loop
		// Handle all commands here
		// For a list of commands and definitions, see the Server Commands document.
		long lastHeartbeat = System.currentTimeMillis();
		public void run() 
		{
			String inputLine;
			
			// Loop while we handle the client
			try {
				while (client.hasHeartbeat())
				{
					// Check for a message from them
					if (client.hasMessage())
					{
						inputLine = client.getMessage();
						
						if (CLIENT_VERBOSE)
							System.out.println(client.myName + ": Received " + inputLine);
						
						String[] recv = inputLine.split(" ");
						
						// In-Game Commands
						if (client.inGame)
						{
							// RTT initialization
							if (recv[0].equals("R"))
							{
								synchronized (client.gameLock) 
								{
									client.opponent.write(inputLine);
								}
							}
						}
						// Lobby Commands
						else
						{
							// Log In
							if (recv[0].equals("A"))
							{
								String newname = "";
								for (int i = 1; i < recv.length; ++i)
									newname += recv[i] + " ";
								
								client.setName(newname);
								
								synchronized (client.myOut)
								{
									client.write("A " + client.myID);
								}
								
								AddToLobby(client);
								
								if (CLIENT_VERBOSE)
									System.out.println(client.myName + ": LOGGED IN");
							}
							// Log Out
							else if (recv[0].equals("B"))
							{
								RemoveFromLobby(client);
								break;
							}
							// Chat
							else if (recv[0].equals("C"))
							{
								String message = "";
								for (int i = 2; i < recv.length; ++i)
									message = message.concat(recv[i] + " ");
									
								BroadcastLobby(client, message);
							}
							// Challenge
							else if (recv[0].equals("DA"))
							{
								long challengeID = Long.parseLong(recv[2]);
								ChallengeLobby(0, client, challengeID);
							}
							// Cancel Challenge
							else if (recv[0].equals("DB"))
							{
								long challengeID = Long.parseLong(recv[2]);
								ChallengeLobby(1, client, challengeID);
							}
							// Accept Challenge
							else if (recv[0].equals("DC"))
							{
								long challengeID = Long.parseLong(recv[2]);
								
								// Set the opponents and hook these up
								CreateGame(client, challengeID);
							}
							// Decline Challenge
							else if (recv[0].equals("DD"))
							{
								long challengeID = Long.parseLong(recv[2]);
								ChallengeLobby(3, client, challengeID);
							}
						}
						
						// This counts as a heartbeat
						lastHeartbeat = System.currentTimeMillis();
					}
					
					Thread.sleep(delayTime);
					if (System.currentTimeMillis() - lastHeartbeat > heartbeatPace)
					{
						synchronized(client.myOut)
						{
							client.write("H");
						}
						lastHeartbeat = System.currentTimeMillis();
					}
				}
			}
			catch (Exception e)
			{
				if (CLIENT_DEBUG)
				{
					if (client != null)
						System.out.print(client.myName + ": ");
					
					System.out.println("Error in socket run():\n");
					e.printStackTrace(System.out);
				}
				return;
			}
			
			if (CLIENT_VERBOSE)
			{
				if (client != null)
					System.out.println(client.myName + ": LOGGED OUT");
				
				System.out.println("Disconnected client at " + socket.getInetAddress().getHostName());
			}
			RemoveFromLobby(client);
		}
	}
	
	///////////////////////////////////////////////////////////
	// The Client Class
	// hold onto their ID (Assigns ids too) and their PrintStream to talk to them
	///////////////////////////////////////////////////////////
	public static long nextClientID = 1;
	class Client
	{
		public long myID;
		public String myName;
		
		public boolean inGame = false;
		public Client opponent = null;
		public Object gameLock = null;
		
		public PrintWriter myOut;
		public BufferedReader myIn;
		
		public Client(String name, PrintWriter out, BufferedReader in)
		{
			myName = name;
			myID = nextClientID++;
			
			myOut = out;
			myIn = in;
		}
		
		// Set the client's name
		public void setName(String s)
		{
			myName = s;
		}
		
		// Check if we have had a heartbeat error
		public boolean hasHeartbeat()
		{
			return !myOut.checkError();
		}
		
		// See if we have a message
		public boolean hasMessage() throws Exception
		{
			return myIn.ready();
		}
		
		// Get message
		public String getMessage() throws Exception
		{
			return myIn.readLine();
		}
		
		// this will write to this object's print writer
		// will automatically append the null character delimiter (this was causing issues before)
		public void write(String s)
		{
			myOut.write(s + '\0');
		}
	}
	
	// ****************************************************************************************
	static int defaultport = 1903;
	public static void main(String[] args)
	{
		int port = defaultport;
		if (args.length > 1)
			port = Integer.parseInt(args[1]);
		
		System.out.println("********************************************************");
		System.out.println("Booting Evolution: Survival of the Fittest Server");
		
		EvolutionServer a = new EvolutionServer();
		a.Start(port);
		
		System.out.println("Version 1.0...");
		System.out.println("Running...");
		
		for (;;)
		{
			a.ListenForNewClient();
			
			try { Thread.sleep(500); }
			catch (Exception e) {}
		}
	}
}
