using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFMS_Space;
using System.Data.SqlClient;
using System.Configuration;

namespace Console_Server
{
    /// <summary>
    /// The TFMS_Server_Main class provides one of the Main methods for running the TFMS program
    /// TFMS_Server_Main starts a TFMS_Server object, which is intended to remain running for the duration of the product
    /// The server is supposed to be "always on"
    /// </summary>
    public class TFMS_Server_Main
    {
        public static void Main(string[] args)
        {
            // create the server object
            TFMS_Server myServer = new TFMS_Server();

            // initialize the server delegates
            myServer.listRequested += new TFMS_MessageRecieved(handleListRequest);
            myServer.logonRequested += new TFMS_MessageRecieved(handleLoginRequest);
            myServer.logoffRequested += new TFMS_MessageRecieved(handleLogoffRequest);
            myServer.relayRequested += new TFMS_MessageRecieved(handleRelayRequest);

            // start the server
            myServer.startServer();
            
            string input = Console.ReadLine();
            
            // while the server is running, only listen for 2 different console commands
            // "x" exits the server program
            // "c" displays the current client list
            while (input != "x")
            {
                switch (input)
                {
                    case "c":
                        Console.WriteLine("Clients: {0}", myServer.getClientList().Count);
                        Console.WriteLine("-------------------");
                        foreach (TFMS_ClientInfo a in myServer.getClientList())
                        {
                            Console.WriteLine("{0}",a.strName);
                        } 
                        Console.WriteLine("-------------------");
                    break;

                }
                input = Console.ReadLine();
            }
        }

        #region TFMS_Server_Main private helper methods

        /// <summary>
        /// If the current TFMS_Data objects contains the Login command, echo the status (and perform the command elsewhere)
        /// </summary>
        /// <param name="data">TFMS_Data object containing the Login command</param>
        private static void handleLoginRequest(TFMS_Data data)
        {
            Console.WriteLine("{0} is logging in", data.strName);
        }

        /// <summary>
        /// If the current TFMS_Data objects contains the Logoff command, echo the status (and perform the command elsewhere)
        /// </summary>
        /// <param name="data">TFMS_Data object containing the Logoff command</param>
        private static void handleLogoffRequest(TFMS_Data data)
        {
            Console.WriteLine("{0} is logging off", data.strName);
        }

        /// <summary>
        /// If the current TFMS_Data objects contains the List command, echo the status (and perform the command elsewhere)
        /// </summary>
        /// <param name="data">TFMS_Data object containing the List command</param>
        private static void handleListRequest(TFMS_Data data)
        {
            Console.WriteLine("{0} has requested a list of clients", data.strName);
        }

        /// <summary>
        /// If the current TFMS_Data objects contains the Relay command, echo the status (and perform the command elsewhere)
        /// When a message is sent to be broadcast to other uses, attempt to log it in the SQL database
        /// </summary>
        /// <param name="data">TFMS_Data object containing the Relay command</param>
        private static void handleRelayRequest(TFMS_Data data)
        {
            Console.WriteLine("{0} has sent a message", data.strName);
            
            // log the message in the SQL database
            //logMessage(data);
        }

        /// <summary>
        /// Attempt to log the current message in the SQL database
        /// </summary>
        /// <param name="data">TFMS_Data object containing the current message</param>
        private static void logMessage(TFMS_Data data)
        {
            Console.WriteLine("Logging {0}'s message to database", data.strName);

            // Attmpt to log the message to the SQL database
            try
            {
                // open a new SQL connection
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnect"].ToString());
                conn.Open();

                // add the message to the database
                SqlCommand cmd = new SqlCommand("INSERT INTO TFMessage (sender, date, message) VALUES (@sent_by, @date, @msg)", conn);
                cmd.Parameters.AddWithValue("@sent_by", data.strName);
                cmd.Parameters.AddWithValue("@date", data.timeStamp);
                cmd.Parameters.AddWithValue("@msg", data.strMessage);
                cmd.ExecuteNonQuery();

                Console.WriteLine("Logged {0}'s message.", data.strName);

                // close the connection
                conn.Close();
            }
            catch (Exception e)
            {
                // print to the console if the logging failed
                Console.WriteLine("Logging of {0}'s message failed. {1}", data.strName, e.Message);
            }
        }

        #endregion
    }
}
