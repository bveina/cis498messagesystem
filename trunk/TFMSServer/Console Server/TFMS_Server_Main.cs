using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFMS_Space;
using System.Data.SqlClient;
using System.Configuration;

namespace Console_Server
{
    class TFMS_Server_Main
    {
        static void Main(string[] args)
        {
            TFMS_Server myServer = new TFMS_Server(1000);
            myServer.listRequested += new TFMS_MessageRecieved(handleListRequest);
            myServer.logonRequested += new TFMS_MessageRecieved(handleLoginRequest);
            myServer.logoffRequested += new TFMS_MessageRecieved(handleLogoffRequest);
            myServer.relayRequested += new TFMS_MessageRecieved(handleRelayRequest);
            myServer.startServer();
            string input = Console.ReadLine();
            while (input != "x")
            {
                switch (input)
                {
                    case "c":
                        Console.WriteLine("Clients: {0}", myServer.clientList.Count);
                        Console.WriteLine("-------------------");
                        foreach (TFMS_ClientInfo a in myServer.clientList)
                        {
                            Console.WriteLine("{0}",a.strName);
                        } 
                        Console.WriteLine("-------------------");
                    break;

                }
                input = Console.ReadLine();
            }
        }
        static void handleLoginRequest(TFMS_Data d)
        {
            Console.WriteLine("{0} is trying to login", d.strName);
        }
        static void handleLogoffRequest(TFMS_Data d)
        {
            Console.WriteLine("{0} is trying to logoff", d.strName);
        }
        static void handleListRequest(TFMS_Data d)
        {
            Console.WriteLine("{0} has requested a list of clients", d.strName);
        }

        static void handleRelayRequest(TFMS_Data d)
        {
            Console.WriteLine("{0} has sent a message", d.strName);

//            if (d.strMessage.Length>255)
//                Console.WriteLine("{0} has sent a long message", d.strName );
//            else
//                Console.WriteLine("{0} has sent \"{1}\"", d.strName, d.strMessage);
            
            //logMessage(d);
        }

        static void logMessage(TFMS_Data d)
        {
            Console.WriteLine("Logging {0}'s message to database", d.strName);

            //Log the message to the sql database
            try
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnect"].ToString());
                conn.Open();

                SqlCommand cmd = new SqlCommand("INSERT INTO TFMessage (sender, date, message) VALUES (@sent_by, @date, @msg)", conn);
                cmd.Parameters.AddWithValue("@sent_by", d.strName);
                cmd.Parameters.AddWithValue("@date", d.timeStamp);
                cmd.Parameters.AddWithValue("@msg", d.strMessage);
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("Logged {0}'s message.", d.strName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Logging of {0}'s message failed. {1}", d.strName, e.Message);
            }
        }
    }
}
