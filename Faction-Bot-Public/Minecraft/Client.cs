using Faction_Bot_Public.Faction_Discord;
using MinecraftClient;
using MinecraftClient.Protocol;
using MinecraftClient.Protocol.Handlers.Forge;
using MinecraftClient.Protocol.Session;
using MinecraftClient.WinAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Minecraft {
    class Client {
        public static ulong gId;
        public static string[] args;
        public static McClient client;
        public static string[] startupargs;
        public const string Version = MCHighestVersion;
        public const string MCLowestVersion = "1.4.6";
        public const string MCHighestVersion = "1.16.5";
        public static readonly string BuildInfo = null;
        private static Thread offlinePrompt = null;
        private static bool useMcVersionOnce = false;
        private static Faction_Bots.Faction_Main main;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        public static void run(ulong guildID) {
            gId = guildID;
            var c = JsonConvert.DeserializeObject<Faction_Settings.Settings>(new WebClient().DownloadString($"https://orbitdev.tech/FBP/database/{guildID}.json"));
            ConsoleIO.LogPrefix = "";
            if (args.Length >= 1 && System.IO.File.Exists(args[0]) && System.IO.Path.GetExtension(args[0]).ToLower() == ".ini")
            {
                Settings.LoadSettings(args[0]);
                List<string> args_tmp = args.ToList();
                args_tmp.RemoveAt(0);
                args = args_tmp.ToArray();
            }
            else if (System.IO.File.Exists("MinecraftClient.ini"))
                Settings.LoadSettings("MinecraftClient.ini");
            else Settings.WriteDefaultSettings("MinecraftClient.ini");
            Translations.LoadExternalTranslationFile(Settings.Language);
            Console.WriteLine(c.m_email);
            if (c.m_email == "null")
                new WebClient().DownloadString($"https://orbitdev.tech/FBP/output-buffer.php?userId={c.d_ownerId}&dId={guildID}&code={Faction_Discord.Discord_Commands.sessionID}&data=Minecraft Email is null");
            Settings.Login = c.m_email;
            if (c.m_password == "null")
                new WebClient().DownloadString($"https://orbitdev.tech/FBP/output-buffer.php?userId={c.d_ownerId}&dId={guildID}&code={Faction_Discord.Discord_Commands.sessionID}&data=Minecraft Password is null");
            Settings.Password = c.m_password;
            if (c.m_serverip == "null")
                new WebClient().DownloadString($"https://orbitdev.tech/FBP/output-buffer.php?userId={c.d_ownerId}&dId={guildID}&code={Faction_Discord.Discord_Commands.sessionID}&data=Minecraft ServerIP is null");
            Settings.ServerIP = c.m_serverip;
            Settings.LoginMethod = c.m_logintype;
            Settings.MCSettings_RenderDistance = 2;
            if (c.m_version == "null")
                new WebClient().DownloadString($"https://orbitdev.tech/FBP/output-buffer.php?userId={c.d_ownerId}&dId={guildID}&code={Faction_Discord.Discord_Commands.sessionID}&data=Minecraft Version is null");
            Settings.ServerVersion = c.m_version;
            if (Settings.ConsoleTitle != "")
            {
                Settings.Username = "New Window";
                Console.Title = Settings.ExpandVars(Settings.ConsoleTitle);
            }
            if (Settings.DebugMessages)
                ConsoleIO.WriteLineFormatted(Translations.Get("debug.color_test", "[0123456789ABCDEF]: [§00§11§22§33§44§55§66§77§88§99§aA§bB§cC§dD§eE§fF§r]"));
            if (Settings.SessionCaching == CacheType.Disk)
            {
                bool cacheLoaded = SessionCache.InitializeDiskCache();
                if (Settings.DebugMessages)
                    Translations.WriteLineFormatted(cacheLoaded ? "debug.session_cache_ok" : "debug.session_cache_fail");
            }
            bool useBrowser = Settings.AccountType == ProtocolHandler.AccountType.Microsoft && Settings.LoginMethod == "browser";
            ChatBots();
            startupargs = args;
            InitializeClient();
        }
        public static async void ChatBots() {
            while (true) {
                if (Settings.isBotLaunched) {
                    Console.WriteLine("ok");
                    client.BotLoad(main = new Faction_Bots.Faction_Main());
                    break;
                }
                await Task.Delay(100);
            }
        }
        private static void InitializeClient()
        {
            SessionToken session = new SessionToken();
            ProtocolHandler.LoginResult result = ProtocolHandler.LoginResult.LoginRequired;
            if (Settings.Password == "-")
            {
                Translations.WriteLineFormatted("mcc.offline");
                result = ProtocolHandler.LoginResult.Success;
                session.PlayerID = "0";
                session.PlayerName = Settings.Login;
            }
            else
            {
                if (Settings.SessionCaching != CacheType.None && SessionCache.Contains(Settings.Login.ToLower()))
                {
                    session = SessionCache.Get(Settings.Login.ToLower());
                    result = ProtocolHandler.GetTokenValidation(session);
                    if (result != ProtocolHandler.LoginResult.Success)
                    {
                        Translations.WriteLineFormatted("mcc.session_invalid");
                    }
                    else ConsoleIO.WriteLineFormatted(Translations.Get("mcc.session_valid", session.PlayerName));
                }
                if (result != ProtocolHandler.LoginResult.Success)
                {
                    result = ProtocolHandler.GetLogin(Settings.Login, Settings.Password, Settings.AccountType, out session);
                    if (result == ProtocolHandler.LoginResult.Success && Settings.SessionCaching != CacheType.None)
                        SessionCache.Store(Settings.Login.ToLower(), session);
                }
            }

            if (result == ProtocolHandler.LoginResult.Success)
            {
                Settings.Username = session.PlayerName;
                if (Settings.ServerIP == "")
                {
                    Settings.SetServerIP(Console.ReadLine());
                }
                int protocolversion = 0;
                ForgeInfo forgeInfo = null;
                if (Settings.ServerVersion != "" && Settings.ServerVersion.ToLower() != "auto")
                {
                    protocolversion = MinecraftClient.Protocol.ProtocolHandler.MCVer2ProtocolVersion(Settings.ServerVersion);
                    if (protocolversion != 0)
                        ConsoleIO.WriteLineFormatted(Translations.Get("mcc.use_version", Settings.ServerVersion, protocolversion));
                    else ConsoleIO.WriteLineFormatted(Translations.Get("mcc.unknown_version", Settings.ServerVersion));
                    if (useMcVersionOnce)
                    {
                        useMcVersionOnce = false;
                        Settings.ServerVersion = "";
                    }
                }
                if (protocolversion == 0 || Settings.ServerAutodetectForge || (Settings.ServerForceForge && !ProtocolHandler.ProtocolMayForceForge(protocolversion)))
                {
                    if (protocolversion != 0)
                        Translations.WriteLine("mcc.forge");
                    else Translations.WriteLine("mcc.retrieve");
                    if (!ProtocolHandler.GetServerInfo(Settings.ServerIP, Settings.ServerPort, ref protocolversion, ref forgeInfo))
                    {
                        HandleFailure(Translations.Get("error.ping"), true, MinecraftClient.ChatBots.AutoRelog.DisconnectReason.ConnectionLost);
                        return;
                    }
                }
                if (Settings.ServerForceForge && forgeInfo == null)
                {
                    if (ProtocolHandler.ProtocolMayForceForge(protocolversion))
                    {
                        Translations.WriteLine("mcc.forgeforce");
                        forgeInfo = ProtocolHandler.ProtocolForceForge(protocolversion);
                    }
                    else
                    {
                        HandleFailure(Translations.Get("error.forgeforce"), true, MinecraftClient.ChatBots.AutoRelog.DisconnectReason.ConnectionLost);
                        return;
                    }
                }
                if (protocolversion != 0)
                {
                    try
                    {
                        if (Settings.SingleCommand != "")
                        {
                            client = new McClient(session.PlayerName, session.PlayerID, session.ID, Settings.ServerIP, Settings.ServerPort, protocolversion, forgeInfo, Settings.SingleCommand);
                            Settings.isBotLaunched = true;
                        }
                        else
                        {
                            client = new McClient(session.PlayerName, session.PlayerID, session.ID, protocolversion, forgeInfo, Settings.ServerIP, Settings.ServerPort);
                            Settings.isBotLaunched = true;
                        }
                        if (Settings.ConsoleTitle != "")
                        {
                            Console.Title = $"Faction-Bot-Public";
                            System.Drawing.Bitmap skin = null;
                            return;
                        }
                    }
                    catch (NotSupportedException) { HandleFailure(Translations.Get("error.unsupported"), true); }
                }
                else HandleFailure(Translations.Get("error.determine"), true);
            }
            else
            {
                string failureMessage = Translations.Get("error.login");
                string failureReason = "";
                switch (result)
                {
                    case ProtocolHandler.LoginResult.AccountMigrated: failureReason = "error.login.migrated"; break;
                    case ProtocolHandler.LoginResult.ServiceUnavailable: failureReason = "error.login.server"; break;
                    case ProtocolHandler.LoginResult.WrongPassword: failureReason = "error.login.blocked"; break;
                    case ProtocolHandler.LoginResult.InvalidResponse: failureReason = "error.login.response"; break;
                    case ProtocolHandler.LoginResult.NotPremium: failureReason = "error.login.premium"; break;
                    case ProtocolHandler.LoginResult.OtherError: failureReason = "error.login.network"; break;
                    case ProtocolHandler.LoginResult.SSLError: failureReason = "error.login.ssl"; break;
                    case ProtocolHandler.LoginResult.UserCancel: failureReason = "error.login.cancel"; break;
                    default: failureReason = "error.login.unknown"; break;
                }
                failureMessage += Translations.Get(failureReason);
                if (result == ProtocolHandler.LoginResult.SSLError && isUsingMono)
                {
                    Translations.WriteLineFormatted("error.login.ssl_help");
                    return;
                }
                HandleFailure(failureMessage, false, ChatBot.DisconnectReason.LoginRejected);
            }
        }
        public static void HandleFailure(string errorMessage = null, bool versionError = false, MinecraftClient.ChatBots.AutoRelog.DisconnectReason? disconnectReason = null)
        {
            if (!String.IsNullOrEmpty(errorMessage))
            {
                ConsoleIO.Reset();
                while (Console.KeyAvailable)
                    Console.ReadKey(true);
                Console.WriteLine(errorMessage);

                if (disconnectReason.HasValue)
                    if (MinecraftClient.ChatBots.AutoRelog.OnDisconnectStatic(disconnectReason.Value, errorMessage))
                        return;
            }

            if (Settings.interactiveMode)
            {
                if (versionError)
                {
                    Translations.Write("mcc.server_version");
                    Settings.ServerVersion = Console.ReadLine();
                    if (Settings.ServerVersion != "")
                    {
                        useMcVersionOnce = true;
                        return;
                    }
                }
                if (offlinePrompt == null)
                {
                    offlinePrompt = new Thread(new ThreadStart(delegate {
                        string command = " ";
                        ConsoleIO.WriteLineFormatted(Translations.Get("mcc.disconnected", (Settings.internalCmdChar == ' ' ? "" : "" + Settings.internalCmdChar)));
                        Translations.WriteLineFormatted("mcc.press_exit");
                        while (command.Length > 0)
                        {
                            if (!ConsoleIO.BasicIO)
                                ConsoleIO.Write('>');
                            command = Console.ReadLine().Trim();
                            if (command.Length > 0)
                            {
                                string message = "";
                                if (Settings.internalCmdChar != ' '
                                    && command[0] == Settings.internalCmdChar)
                                    command = command.Substring(1);
                                if (command.StartsWith("reco"))
                                    message = new MinecraftClient.Commands.Reco().Run(null, Settings.ExpandVars(command), null);
                                else if (command.StartsWith("connect"))
                                    message = new MinecraftClient.Commands.Connect().Run(null, Settings.ExpandVars(command), null);
                                else if (command.StartsWith("exit") || command.StartsWith("quit"))
                                    message = new MinecraftClient.Commands.Exit().Run(null, Settings.ExpandVars(command), null);
                                else if (command.StartsWith("help"))
                                {
                                    ConsoleIO.WriteLineFormatted("" + (Settings.internalCmdChar == ' ' ? "" : "" + Settings.internalCmdChar) + new MinecraftClient.Commands.Reco().GetCmdDescTranslated());
                                    ConsoleIO.WriteLineFormatted("" + (Settings.internalCmdChar == ' ' ? "" : "" + Settings.internalCmdChar) + new MinecraftClient.Commands.Connect().GetCmdDescTranslated());
                                }
                                else ConsoleIO.WriteLineFormatted(Translations.Get("icmd.unknown", command.Split(' ')[0]));

                                if (message != "")
                                    ConsoleIO.WriteLineFormatted("§9Orbit§FBot: " + message);
                            }
                        }
                    }));
                    offlinePrompt.Start();
                }
            }
            else
            {
                if (disconnectReason.HasValue)
                {
                    if (disconnectReason.Value == ChatBot.DisconnectReason.UserLogout)
                    if (disconnectReason.Value == ChatBot.DisconnectReason.InGameKick)
                    if (disconnectReason.Value == ChatBot.DisconnectReason.ConnectionLost)
                    if (disconnectReason.Value == ChatBot.DisconnectReason.LoginRejected) { }
                }
            }

        }
        public static bool isUsingMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        }
        public static Type[] GetTypesInNamespace(string nameSpace, Assembly assembly = null)
        {
            if (assembly == null) { assembly = Assembly.GetExecutingAssembly(); }
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }
    }
}
