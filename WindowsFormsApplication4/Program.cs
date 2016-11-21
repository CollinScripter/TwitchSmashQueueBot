using System;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using WindowsFormsApplication4;

namespace MainProgram
{
    public class Program
    {
        private static byte[] data;

        public static class StaticVars
        {
            public static string[] stringArray = new string[2];
            public static string[] skipArray = new string[0];
            public static bool start = false;
            public static string channel = WindowsFormsApplication4.Properties.Settings.Default.Channel;
            public static int skipValue = WindowsFormsApplication4.Properties.Settings.Default.skipValue;
            public static int skipTime = WindowsFormsApplication4.Properties.Settings.Default.skipTime;
            public static string username = WindowsFormsApplication4.Properties.Settings.Default.Username;
            public static string oauth = WindowsFormsApplication4.Properties.Settings.Default.oauth;
            public static string skip = WindowsFormsApplication4.Properties.Settings.Default.skipSong;
            public static bool enableSkipping = WindowsFormsApplication4.Properties.Settings.Default.enableSkipping;
            public static int queueLength = WindowsFormsApplication4.Properties.Settings.Default.queueLength;
            public static bool Debug = false;
            public static bool firstRun = WindowsFormsApplication4.Properties.Settings.Default.firstRun;
            public static bool keepQueue = WindowsFormsApplication4.Properties.Settings.Default.keepQueue;
            public static string pastQueue = WindowsFormsApplication4.Properties.Settings.Default.pastQueue;
            public static string version = "0.2.5";
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            if (StaticVars.keepQueue)
            {
                string setting = "SmsHqdDn3808FpoIQdNÆ╥";
                for (int k = 2; k < StaticVars.stringArray.Length; k++)
                {
                    setting += "☼" + StaticVars.stringArray[k];
                }
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(setting);
                var base64 = System.Convert.ToBase64String(plainTextBytes);
                WindowsFormsApplication4.Properties.Settings.Default.pastQueue = base64;
                for (int x = 0; x < 10; ++x)
                {
                    WindowsFormsApplication4.Properties.Settings.Default.Save();
                }
            }
        }

        public void keepRestart()
        {
            string setting = "SmsHqdDn3808FpoIQdNÆ╥";
            for (int k = 2; k < StaticVars.stringArray.Length; k++)
            {
                setting += "☼" + StaticVars.stringArray[k];
            }
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(setting);
            var base64 = System.Convert.ToBase64String(plainTextBytes);
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location, '"' + base64 + '"');
            System.Environment.Exit(1);
        }
        public void RemoveTopPlayer()
        {
            Thread.Sleep(5);
            if (StaticVars.stringArray.Length != 2)
            {
                StaticVars.stringArray = StaticVars.stringArray.Where((source, index) => index != 2).ToArray();
                StaticVars.stringArray = StaticVars.stringArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                Thread.Sleep(169);
            }
            UpdateConsole();
        }

        private static void UpdateConsole()
        {
            if (StaticVars.Debug == false) { Console.Clear(); }
            for (int q = 0; q < StaticVars.stringArray.Length; q++)
            {
                Console.WriteLine(StaticVars.stringArray[q]);
            }
            WriteDebug("Length: " + StaticVars.stringArray.Length);
        }

        [STAThread]
        static void Chat()
        {
            TcpClient client = new TcpClient("irc.chat.twitch.tv", 6667);
            string channel = StaticVars.channel;
            NetworkStream stream = client.GetStream();
            var WriteTwitch = new Action<string>(saystring =>
            {
                saystring = ":" + StaticVars.channel + "!" + StaticVars.channel + "@" + StaticVars.channel + ".tmi.twitch.tv PRIVMSG #" + StaticVars.channel + " :" + saystring + "\r\n";
                Byte[] say = System.Text.Encoding.ASCII.GetBytes(saystring);
                stream.Write(say, 0, say.Length);
            });
            string loginstring = "PASS " + StaticVars.oauth + "\r\nNICK "+ StaticVars.username +"\r\n";
            Byte[] login = System.Text.Encoding.ASCII.GetBytes(loginstring);
            stream.Write(login, 0, login.Length);

            WriteDebug("Sent login.\r\n");
            WriteDebug(loginstring);

            data = new Byte[512];

            String responseData = String.Empty;

            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = Encoding.ASCII.GetString(data, 0, bytes);

            WriteDebug("Received WELCOME: \r\n\r\n" + responseData);

            string joinstring = "JOIN " + "#" + channel + "\r\n";
            Byte[] join = Encoding.ASCII.GetBytes(joinstring);
            stream.Write(join, 0, join.Length);

            WriteDebug("Sent channel join.\r\n");
            WriteDebug(joinstring);

            WriteDebug("Press any key");
            if (StaticVars.Debug == true) { Console.ReadKey(); }
            WriteDebug("Joined the Twitch channel " + channel + "!\r\n\r\n");
            StaticVars.start = true;

            while (true)
            {

                byte[] myReadBuffer = new byte[1024];
                StringBuilder myCompleteMessage = new StringBuilder();
                int numberOfBytesRead = 0;

                do
                {
                    try { numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length); }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error 1, Please report to CollinScripter\r\n{0}", e);
                        Program programz = new Program();
                        programz.keepRestart();
                    }

                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                }

                while (stream.DataAvailable);

                WriteDebug(myCompleteMessage.ToString());

                switch (myCompleteMessage.ToString())
                {

                    case "PING :tmi.twitch.tv\r\n":
                        try
                        {
                            Byte[] say = System.Text.Encoding.ASCII.GetBytes("PONG :tmi.twitch.tv\r\n");
                            stream.Write(say, 0, say.Length);
                            WriteDebug("PONG :tmi.twitch.tv\r\n");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error 2, Please report to CollinScripter\r\n{0}", e);
                            Program programz = new Program();
                            programz.keepRestart();

                        }
                        break;

                    default:
                        try
                        {
                            string messageParser = myCompleteMessage.ToString();
                            string[] message = messageParser.Split(':');
                            string[] preamble = message[1].Split(' ');
                            string tochat;
                            WriteDebug(messageParser);

                            if (preamble[1] == "PRIVMSG")
                            {
                                string[] sendingUser = preamble[0].Split('!');
                                //sendingUser[0] and message[2];
                                WriteDebug(message[2]);
                                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(message[2].ToLower());
                                var message64 = System.Convert.ToBase64String(plainTextBytes);
                                WriteDebug(message64);
                                if (message64 == "IXF1ZXVlDQo=")//!queue
                                    {
                                    string saystring = ":" + channel + "!" + channel + "@" + channel + ".tmi.twitch.tv PRIVMSG #" + channel + " :The next users in the queue are";
                                    int stringlength = 0;
                                    if (StaticVars.stringArray.Length >= StaticVars.queueLength + 2) { stringlength = StaticVars.queueLength + 2; }
                                    else { stringlength = StaticVars.stringArray.Length; }
                                    for (int i = 2; i < stringlength; i++)
                                    {
                                        saystring = saystring + ", " + StaticVars.stringArray[i];
                                    }
                                    saystring = saystring.Replace(", " + StaticVars.stringArray[stringlength - 1], " and " + StaticVars.stringArray[stringlength - 1]);
                                    saystring = saystring.Replace("the queue are,", "the queue are:");
                                    saystring = saystring + "\r\n";
                                    if (StaticVars.stringArray.Length == 2) { saystring = ":" + channel + "!" + channel + "@" + channel + ".tmi.twitch.tv PRIVMSG #" + channel + " :No one is currently in the queue!\r\n"; }
                                    else if (StaticVars.stringArray.Length == 3) { saystring = saystring.Replace("users in the queue are and ","user in the queue is "); }
                                    Byte[] say = System.Text.Encoding.ASCII.GetBytes(saystring);
                                    stream.Write(say, 0, say.Length);
                                    WriteDebug(saystring);
                                }

                                if (message64 == "IXNraXANCg==")//!skip
                                {
                                    if (StaticVars.enableSkipping == true)
                                    {
                                        bool skip = false;
                                        for (int i = 0; i < StaticVars.skipArray.Length; i++)
                                        {
                                            if (StaticVars.skipArray[i].Equals(sendingUser[0]) == true)
                                            {
                                                skip = true;
                                            }
                                        }
                                        if (skip == true)
                                        {
                                            WriteTwitch("@" + sendingUser[0] + "<- You have already voted to skip the song.");
                                        }
                                        else
                                        {
                                            Array.Resize(ref StaticVars.skipArray, StaticVars.skipArray.Length + 1);
                                            StaticVars.skipArray[StaticVars.skipArray.Length - 1] = sendingUser[0];
                                            if (StaticVars.skipArray.Length == 1 && StaticVars.skipValue == 1)
                                            {
                                                WriteTwitch(StaticVars.skipArray.Length + "/1 song skip requested. Now skipping the song...");
                                                WriteTwitch(StaticVars.skip);
                                                for (int q = 0; q < StaticVars.skipArray.Length; q++)
                                                {
                                                    StaticVars.skipArray = StaticVars.skipArray.Where((source, index) => index != q).ToArray();
                                                }
                                                StaticVars.skipArray = StaticVars.skipArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                                Array.Resize(ref StaticVars.skipArray, 0);
                                            }
                                            else if (StaticVars.skipArray.Length == 1 && StaticVars.skipValue != 1)
                                            {
                                                WriteTwitch(StaticVars.skipArray.Length + " song skip requested. You have " + StaticVars.skipTime + " seconds to vote " + StaticVars.skipValue + " times to skip the current song!");
                                                Thread SongSkip = new Thread(new ThreadStart(Program.SongSkip));
                                                SongSkip.Start();
                                            }
                                            else
                                            {
                                                WriteTwitch(StaticVars.skipArray.Length + "/" + StaticVars.skipValue + " song skips requested");
                                            }
                                            if (StaticVars.skipArray.Length == StaticVars.skipValue)
                                            {
                                                WriteTwitch(StaticVars.skip);
                                                for (int q = 0; q < StaticVars.skipArray.Length; q++)
                                                {
                                                    StaticVars.skipArray = StaticVars.skipArray.Where((source, index) => index != q).ToArray();
                                                }
                                                StaticVars.skipArray = StaticVars.skipArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                                Array.Resize(ref StaticVars.skipArray, 0);
                                            }
                                        }
                                    }
                                }

                                if (message64 == "IWpvaW4NCg==")//!join
                                {
                                    WriteDebug("!join was said by " + sendingUser[0] + "\n");
                                    bool person = false;
                                    for (int i = 0; i < StaticVars.stringArray.Length; i++)
                                    {
                                        if (StaticVars.stringArray[i].Equals(sendingUser[0]) == true)
                                        {
                                            person = true;
                                        }
                                    }
                                    if (person == true)
                                    {
                                        WriteDebug(sendingUser[0] + " is already in the queue!\n");
                                        WriteTwitch("@" + sendingUser[0] + " -> You are already in the queue!");
                                    }
                                    else
                                    {
                                        Array.Resize(ref StaticVars.stringArray, StaticVars.stringArray.Length + 1);
                                        StaticVars.stringArray[StaticVars.stringArray.Length - 1] = sendingUser[0];
                                        WriteDebug(sendingUser[0] + " has been added to the queue!\n");
                                        WriteTwitch("@" + sendingUser[0] + " -> You have been added to the queue!");
                                        UpdateConsole();
                                    }

                                }
                                if (message64 == "IWRyb3ANCg==")//!drop
                                {
                                    WriteDebug("!drop was said by " + sendingUser[0] + "\n");
                                    int Iremove = -1;
                                    bool IremoveB = false;
                                    for (int i = 0; i < StaticVars.stringArray.Length; i++)
                                    {
                                        if (StaticVars.stringArray[i].Equals(sendingUser[0]) == true)
                                        {
                                            Iremove = i;
                                            IremoveB = true;
                                        }
                                    }
                                    if (IremoveB == true)
                                    {
                                        StaticVars.stringArray = StaticVars.stringArray.Where((source, index) => index != Iremove).ToArray();
                                        StaticVars.stringArray = StaticVars.stringArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                        WriteTwitch("@" + sendingUser[0] + " -> You have been removed from the queue!");
                                        UpdateConsole();
                                        WriteDebug(sendingUser[0] + " was removed from the queue!\n");
                                    }
                                    else
                                    {
                                        WriteDebug(sendingUser[0] + " is not in the queue!\n");
                                        WriteTwitch("@" + sendingUser[0] + " -> You were not in the queue!");
                                    }
                                }
                                if (message64 == "IWNvbGxpbnNjcmlwdGVyDQo=")//!collinscripter
                                {
                                    if (StaticVars.channel.ToLower() == "tcpixel" || StaticVars.channel.ToLower() == "collinscripter")                                    
                                    {
                                        WriteDebug(StaticVars.username + ": Meow Meow MEOW!! CoolCat\n");
                                        WriteTwitch("Meow Meow MEOW!! CoolCat");
                                    }
                                }
                            }

                            else if (preamble[1] == "JOIN")
                            {
                                string[] sendingUser = preamble[0].Split('!');
                                tochat = "The user " + sendingUser[0] + " has joined.";
                                WriteDebug(tochat);
                            }
                        }

                        catch (Exception e)
                        {
                            Console.WriteLine("Error 3, Please report to CollinScripter\r\n{0}", e);
                            Program programz = new Program();
                            programz.keepRestart();
                        }
                        break;
                }
            }
        }

        static void SongSkip()
        {
            int time = StaticVars.skipTime * 1000;
            Thread.Sleep(time);
            if (StaticVars.skipArray.Length == StaticVars.skipValue) {}
            else 
            {
                for (int q = 0; q < StaticVars.skipArray.Length; q++)
                {
                    StaticVars.skipArray = StaticVars.skipArray.Where((source, index) => index != q).ToArray();
                    Thread.Sleep(10);
                }
                StaticVars.skipArray = StaticVars.skipArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                Array.Resize(ref StaticVars.skipArray, 0);
            }
       }

        static void Start()
        {
            Thread Chat = new Thread(new ThreadStart(Program.Chat));
            Chat.Start();
            StaticVars.stringArray[0] = "Queue Players";
             StaticVars.stringArray[1] = "_____________";
            while (StaticVars.start == false) { Thread.Sleep(10); }
            UpdateConsole();
            if (StaticVars.keepQueue == true) { AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit); }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            System.Environment.Exit(1);
        }

        private static void Update(string version)
        {
            if (StaticVars.keepQueue == false)
            {
                Console.WriteLine("Version " + version + " is out. The version you are on is " + StaticVars.version + ". Update? (Y/N)");
            }
            else
            {
                Console.WriteLine("Version " + version + " is out. The version you are on is " + StaticVars.version + ". Update? This will remove your current queue! (Y/N)");
            }
            ConsoleKeyInfo update = Console.ReadKey();
            if (update.KeyChar == 'Y' || update.KeyChar == 'y')
            {
                string setting = WindowsFormsApplication4.Properties.Settings.Default.Channel + "◄" + WindowsFormsApplication4.Properties.Settings.Default.skipValue.ToString() + "◄" + WindowsFormsApplication4.Properties.Settings.Default.skipTime.ToString() + "◄" + WindowsFormsApplication4.Properties.Settings.Default.Username + "◄" + WindowsFormsApplication4.Properties.Settings.Default.oauth + "◄" + WindowsFormsApplication4.Properties.Settings.Default.skipSong + "◄" + WindowsFormsApplication4.Properties.Settings.Default.enableSkipping.ToString() + "◄" + WindowsFormsApplication4.Properties.Settings.Default.queueLength.ToString() + "◄" + WindowsFormsApplication4.Properties.Settings.Default.keepQueue.ToString();
                if (StaticVars.Debug == true)
                {
                    Console.WriteLine("Debug Update.");
                    Thread.Sleep(5000);
                    var plainTextBytes2 = System.Text.Encoding.UTF8.GetBytes(setting);
                    var base642 = System.Convert.ToBase64String(plainTextBytes2);
                    System.Diagnostics.Process.Start((System.Reflection.Assembly.GetEntryAssembly().Location), '"' + base642 + '"');
                    System.Environment.Exit(1);
                }
                Console.Clear();
                Console.WriteLine("Downloading Version " + version + " Please Wait...");
                string path = (System.Reflection.Assembly.GetEntryAssembly().Location);
                string tempexe = Path.GetTempFileName();
                if (File.Exists(tempexe)) { File.Delete(tempexe); }
                string[] tempexename = tempexe.Split('.');
                tempexe = tempexename[0];
                tempexe = tempexe + ".exe";
                WebClient webClient = new WebClient();
                webClient.DownloadFile("https://github.com/CollinScripter/TwitchSmashQueueBot/raw/master/Updater.exe", tempexe);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(setting);
                var base64 = System.Convert.ToBase64String(plainTextBytes);
                System.Diagnostics.Process.Start(tempexe, '"' + path + '"' + " " + '"' + base64 + '"');
                System.Environment.Exit(1);
            }
            else
            {
                if (update.KeyChar == 'N' || update.KeyChar == 'n')
                {
                    Console.Clear();
                    settings();
                    Start();
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Please press Y for Yes, or N for No");
                    Thread.Sleep(250);
                    Console.Clear();
                    RetryUpdate(version);
                }
            }
        }

        private static void RetryUpdate(string version)
        {
            Update(version);
        }
        
        private static void WriteDebug(string DebugText)
        {
            if (StaticVars.Debug == true) { Console.WriteLine(DebugText); }
        }

        private static void settings()
        {
            if (WindowsFormsApplication4.Properties.Settings.Default.Channel == "" || WindowsFormsApplication4.Properties.Settings.Default.Username == "" || WindowsFormsApplication4.Properties.Settings.Default.oauth == "")
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form2());
                Program programz = new Program();
                programz.keepRestart();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            System.Net.WebClient wc = new System.Net.WebClient();
            try
            {
                string version = wc.DownloadString("https://raw.githubusercontent.com/CollinScripter/TwitchSmashQueueBot/master/Version");
                if (version != StaticVars.version)
                {
                    Update(version);
                }
                else
                {
                    if (StaticVars.firstRun == true)
                    {
                        Console.WriteLine("Before using this program you should unblock it. This can be done by right clicking the file, go to Properties, then check the Unblock box. Then click OK.\r\nPress any key to continue.");
                        Console.ReadKey();
                        WindowsFormsApplication4.Properties.Settings.Default.firstRun = false;
                        for (int x = 0; x < 6; ++x)
                        {
                            WindowsFormsApplication4.Properties.Settings.Default.Save();
                        }
                    }
                    if (args.Length != 0)
                    {
                        byte[] data = Convert.FromBase64String(args[0]);
                        string setting = Encoding.UTF8.GetString(data);
                        if (setting.Contains("SmsHqdDn3808FpoIQdNÆ╥"))
                        {
                            settings();
                            Thread Chat = new Thread(new ThreadStart(Program.Chat));
                            Chat.Start();
                            Thread.Sleep(250);
                            StaticVars.stringArray[0] = "Queue Players";
                            StaticVars.stringArray[1] = "_____________";
                            string[] set = setting.Split('☼');
                            for (int p = 1; p < set.Length; p++)
                            {
                                Array.Resize(ref StaticVars.stringArray, StaticVars.stringArray.Length + 1);
                                StaticVars.stringArray[p + 1] = set[p];
                            }
                            while (StaticVars.start == false) { Thread.Sleep(10); }
                            UpdateConsole();
                            Application.EnableVisualStyles();
                            Application.SetCompatibleTextRenderingDefault(false);
                            Application.Run(new Form1());
                            System.Environment.Exit(1);
                        }
                        else if (setting.Contains("Debug│"))
                        {
                            StaticVars.Debug = true;
                            Update("DEBUG");
                        }
                        else if (setting.Contains("◄"))
                        {
                            string[] set = setting.Split('◄');
                            WindowsFormsApplication4.Properties.Settings.Default.Channel = set[0];
                            WindowsFormsApplication4.Properties.Settings.Default.skipValue = int.Parse(set[1]);
                            WindowsFormsApplication4.Properties.Settings.Default.skipTime = int.Parse(set[2]);
                            WindowsFormsApplication4.Properties.Settings.Default.Username = set[3];
                            WindowsFormsApplication4.Properties.Settings.Default.oauth = set[4];
                            WindowsFormsApplication4.Properties.Settings.Default.skipSong = set[5];
                            WindowsFormsApplication4.Properties.Settings.Default.firstRun = false;
                            bool eskip = true;
                            if (set[6] == "True") { eskip = true; }
                            else { eskip = false; }
                            WindowsFormsApplication4.Properties.Settings.Default.enableSkipping = eskip;
                            if (set.Length != 7) { WindowsFormsApplication4.Properties.Settings.Default.queueLength = int.Parse(set[7]); }
                            if (set.Length != 8)
                            {
                                bool keepQueue = false;
                                if (set[8] == "True") { keepQueue = true; }
                                else { keepQueue = false; }
                                WindowsFormsApplication4.Properties.Settings.Default.keepQueue = keepQueue;
                            }
                            for (int x = 0; x < 6; ++x)
                            {
                                WindowsFormsApplication4.Properties.Settings.Default.Save();
                            }
                        }
                    }
                    if (StaticVars.keepQueue == true)
                    {
                        if (StaticVars.pastQueue != "None")
                        {

                            byte[] data2 = Convert.FromBase64String(StaticVars.pastQueue);
                            string setting2 = Encoding.UTF8.GetString(data2);
                            if (setting2.Contains("SmsHqdDn3808FpoIQdNÆ╥"))
                            {
                                settings();
                                Thread Chat = new Thread(new ThreadStart(Program.Chat));
                                Chat.Start();
                                Thread.Sleep(250);
                                StaticVars.stringArray[0] = "Queue Players";
                                StaticVars.stringArray[1] = "_____________";
                                string[] set2 = setting2.Split('☼');
                                for (int p = 1; p < set2.Length; p++)
                                {
                                    Array.Resize(ref StaticVars.stringArray, StaticVars.stringArray.Length + 1);
                                    StaticVars.stringArray[p + 1] = set2[p];
                                }
                                while (StaticVars.start == false) { Thread.Sleep(10); }
                                UpdateConsole();
                                WindowsFormsApplication4.Properties.Settings.Default.pastQueue = "None";
                                for (int x = 0; x < 6; ++x)
                                {
                                    WindowsFormsApplication4.Properties.Settings.Default.Save();
                                }
                                Application.EnableVisualStyles();
                                Application.SetCompatibleTextRenderingDefault(false);
                                Application.Run(new Form1());
                                System.Environment.Exit(1);
                            }
                        }
                    }
                    else
                    {
                        WindowsFormsApplication4.Properties.Settings.Default.pastQueue = "None";
                        for (int x = 0; x < 6; ++x)
                        {
                            WindowsFormsApplication4.Properties.Settings.Default.Save();
                        }
                    }
                }
                settings();
                Start();
            }
            catch
            {
                Console.WriteLine("Connectivity with the internet has failed. Are you connected to the internet? Is your internet connection very slow?\n\rPress Any Key to Exit");
                Console.ReadKey();
            }
        }
    }
}