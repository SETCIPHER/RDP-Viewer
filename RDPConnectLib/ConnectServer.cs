﻿using RDPCOMAPILib;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Timers;

namespace RDPConnectLib
{
    public class ConnectServer
    {
        public static RDPSession currentSession = null;

        public static void createSession()
        {

            currentSession = new RDPSession();

        }

        public void Connect(RDPSession session)
        {

            session.OnAttendeeConnected += Incoming;
            session.Open();


        }

        public static void Disconnect(RDPSession session)
        {
            session.Close();
        }

        public static string GetConnectionString(RDPSession session, String authString, string group, string password, int clientLimit)
        {
            IRDPSRAPIInvitation invitation = session.Invitations.CreateInvitation
            (authString, group, password, clientLimit);
            return invitation.ConnectionString;
        }

        private static void Incoming(object Guest)
        {
            IRDPSRAPIAttendee MyGuest = (IRDPSRAPIAttendee)Guest;
            MyGuest.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_MAX;

        }


        private void StartServer()
        {            
            createSession();
            Connect(currentSession);

            string pathDir = ConfigurationManager.AppSettings.Get("dir");
            string pathPass = ConfigurationManager.AppSettings.Get("Pass");
            DirectoryInfo dirInfo = new DirectoryInfo(pathDir);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            string serverName = Environment.MachineName;
            string key = GetConnectionString(currentSession, serverName, "", pathPass, 5);

            File.WriteAllText(pathDir + serverName + ".bin", key);
          
        }

        public void RestartServer(object sender, EventArgs e)
        {
            if (Internet.CheckConnection())
            {
                
                if (currentSession == null)
                {
                    StartServer();  
                }

            }
            else
            {
                if (currentSession != null)
                {  
                    Disconnect(currentSession);
                    currentSession = null; 
                    
                }

            }

        }

        public void OpenServer()
        {
            Timer aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler(RestartServer);  
            aTimer.Interval = 1000;
            aTimer.Enabled = true;  
        }

        public string GetLocalIp()
        {
            string ipV4 = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipV4 += ip.ToString();
                    break;

                }
            }

            return ipV4;
        }


        public string GetPublicIp()
        {
            string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            ///var externalIp = IPAddress.Parse(externalIpString);
            return externalIpString;
        }

        public string GetMacAddress()
        {
            string macAddresses = "";
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddresses;
        }
    }

}
