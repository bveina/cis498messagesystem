using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFMS_Space;
using System.Data.SqlClient;
using System.Configuration;

namespace Console_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            TFMSServer myServer = new TFMSServer(1000);
            myServer.ListRequested += new MessageRecieved(handleListRequest);
            myServer.logonRequested += new MessageRecieved(handleLoginRequest);
            myServer.logoffRequested += new MessageRecieved(handleLogoffRequest);
            myServer.RelayRequested += new MessageRecieved(handleRelayRequest);
            myServer.startServer();
            string input = Console.ReadLine();
            while (input != "x")
            {
                switch (input)
                {
                    case "c":
                        Console.WriteLine("Clients:{0}",myServer.clientList.Count);
                        Console.WriteLine("-------------------");
                        foreach (ClientInfo a in myServer.clientList)
                        {
                            Console.WriteLine("{0}",a.strName);
                        } 
                        Console.WriteLine("-------------------");
                    break;

                }
                input = Console.ReadLine();
            }
        }
        static void handleLoginRequest(Data d)
        {
            Console.WriteLine("{0} is trying to login", d.strName);
        }
        static void handleLogoffRequest(Data d)
        {
            Console.WriteLine("{0} is trying to logoff", d.strName);
        }
        static void handleListRequest(Data d)
        {
            Console.WriteLine("{0} has requested a list of clients", d.strName);
        }
        static void handleRelayRequest(Data d)
        {
            if (d.strMessage.Length>255)
                Console.WriteLine("{0} has sent a long message", d.strName );
            else
                Console.WriteLine("{0} has sent \"{1}\"", d.strName, d.strMessage);
            
            logMessage(d);
        }

        static void logMessage(Data d)
        {
            //Log the message to the sql database
            try
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnect"].ToString());
                conn.Open();

                SqlCommand cmd = new SqlCommand("INSERT INTO TFMessage @SENDER @DATE @MESSAGE");
                cmd.Parameters.AddWithValue("@SENDER", d.strName);
                cmd.Parameters.AddWithValue("@DATE", d.timeStamp);
                cmd.Parameters.AddWithValue("@MESSAGE", d.strMessage);
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Logging of {0}'s message failed. {1}", d.strName, e.Message);
            }
        }
    }
}
