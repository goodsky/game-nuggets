import java.util.*;

import javolution.util.FastMap;
import org.apache.commons.io.filefilter.IOFileFilter;
import org.jwebsocket.api.*;
import org.jwebsocket.config.*;
import org.jwebsocket.kit.*;
import org.jwebsocket.listener.*;
import org.jwebsocket.token.*;
import org.jwebsocket.server.*;
import org.jwebsocket.factory.*;

public class JHost implements WebSocketServerTokenListener {
  private TokenServer tokenServer;

  public TokenServer getTokenServer()
  {
    return tokenServer;
  }
  public void init()
  {
    try {
      JWebSocketFactory.printCopyrightToConsole();
      String[] args = new String[0];
      JWebSocketConfig.initForConsoleApp(args);

      JWebSocketFactory.start();
      tokenServer = (TokenServer) JWebSocketFactory.getServer("ts0");
      if (tokenServer != null) {
        System.out.println("Server was found!!! :D");
        tokenServer.addListener(this);
      } else {
        System.out.println("Awww snap. I couldn't find a server.");
      }
    } catch (Exception e) {
      System.out.println("init exception...");
      e.printStackTrace();
    }
  }

  public void processOpened(WebSocketServerEvent event)
  {
    System.out.println("\n***");
    System.out.println("Open! \n" + event.toString());
    System.out.println("***\n");
  } 
  public void processClosed(WebSocketServerEvent event)
  {
    System.out.println("\n***");
    System.out.println("Closed! \n" + event.toString());
    System.out.println("***\n");
  }
  public void processToken(WebSocketServerTokenEvent event, Token token)
  {
    System.out.println("\n***");
    System.out.println("Got Token: \n" + token.toString());
    System.out.println("***\n");
	
	// Respond depending on what they sent
	Token response = event.createResponse(token);
	
	String itype = token.getString("itype");
	if (itype != null && itype.equals("sendMove"))
	{
		response.setString("msg", "SendMoveResponse");
		event.sendToken(response);
	}
  }
  public void processPacket(WebSocketServerEvent event, WebSocketPacket packet)
  {
    // System.out.println("\n***");
    // System.out.println("Got Packet: \n" + packet.toString());
    // System.out.println("***\n");
  }

  public static void main(String[] args)
  {
    JHost host = new JHost();
    host.init();
  }
}
