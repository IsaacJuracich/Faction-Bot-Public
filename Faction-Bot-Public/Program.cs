﻿using Faction_Bot_Public.Faction_Discord;
using Faction_Bot_Public.Faction_Settings;
using MinecraftClient;
using MinecraftClient.Protocol;
using MinecraftClient.Protocol.Handlers.Forge;
using MinecraftClient.Protocol.Session;
using MinecraftClient.WinAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Faction_Bot_Public {
    public static class Program {
        public static McClient client;
        public static IMinecraftCom handler;
        public static string[] startupargs;
        public const string Version = MCHighestVersion;
        public const string MCLowestVersion = "1.4.6";
        public const string MCHighestVersion = "1.16.5";
        public static readonly string BuildInfo = null;
        private static Thread offlinePrompt = null;
        private static bool useMcVersionOnce = false;
        // # Faction-Bot Variables & Functions # //
        public static INI config = new INI("");
        public static void discordStart() => Discord_Bot.Start().GetAwaiter().GetResult();

        // # End # //
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleIcon(IntPtr hIcon);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        static void Main(string[] args) {
            return;
            // Will add Minecraft-API when all of Discord Guild is ready
            ConsoleIO.LogPrefix = "";
            if (args.Length == 1 && args[0] == "--keyboard-debug") {
                ConsoleIO.WriteLine("Keyboard debug mode: Press any key to display info");
                ConsoleIO.DebugReadInput();
            }
            if (args.Length >= 1 && args[args.Length - 1] == "BasicIO" || args.Length >= 1 && args[args.Length - 1] == "BasicIO-NoColor") {
                if (args.Length >= 1 && args[args.Length - 1] == "BasicIO-NoColor")
                    ConsoleIO.BasicIO_NoColor = true;
                ConsoleIO.BasicIO = true;
                args = args.Where(o => !ReferenceEquals(o, args[args.Length - 1])).ToArray();
            }
            if (isUsingMono || WindowsVersion.WinMajorVersion >= 10)
                Console.OutputEncoding = Console.InputEncoding = Encoding.UTF8;
            if (args.Length >= 1 && System.IO.File.Exists(args[0]) && System.IO.Path.GetExtension(args[0]).ToLower() == ".ini") {
                Settings.LoadSettings(args[0]);
                List<string> args_tmp = args.ToList();
                args_tmp.RemoveAt(0);
                args = args_tmp.ToArray();
            }
            else if (System.IO.File.Exists("MinecraftClient.ini"))
                Settings.LoadSettings("MinecraftClient.ini");
            else Settings.WriteDefaultSettings("MinecraftClient.ini");
            Translations.LoadExternalTranslationFile(Settings.Language);
            if (args.Length >= 1) {
                Settings.Login = args[0];
                if (args.Length >= 2) {
                    Settings.Password = args[1];
                    if (args.Length >= 3) {
                        Settings.SetServerIP(args[2]);
                        if (args.Length >= 4)
                            Settings.SingleCommand = args[3];
                    }
                }
            }
            if (Settings.ConsoleTitle != "") {
                Settings.Username = "New Window";
                Console.Title = Settings.ExpandVars(Settings.ConsoleTitle);
            }
            if (Settings.DebugMessages)
                ConsoleIO.WriteLineFormatted(Translations.Get("debug.color_test", "[0123456789ABCDEF]: [§00§11§22§33§44§55§66§77§88§99§aA§bB§cC§dD§eE§fF§r]"));
            if (Settings.SessionCaching == CacheType.Disk) {
                bool cacheLoaded = SessionCache.InitializeDiskCache();
                if (Settings.DebugMessages)
                    Translations.WriteLineFormatted(cacheLoaded ? "debug.session_cache_ok" : "debug.session_cache_fail");
            }
            bool useBrowser = Settings.AccountType == ProtocolHandler.AccountType.Microsoft && Settings.LoginMethod == "browser";
            if (Settings.Login == "") {
                if (useBrowser)
                    ConsoleIO.WriteLine("Press Enter to skip session cache checking and continue sign-in with browser");
                Console.Write(ConsoleIO.BasicIO ? Translations.Get("mcc.login_basic_io") + "\n" : Translations.Get("mcc.login"));
                Settings.Login = Console.ReadLine();
            }
            if (Settings.Password == ""
                && (Settings.SessionCaching == CacheType.None || !SessionCache.Contains(Settings.Login.ToLower()))
                && !useBrowser) {
                RequestPassword();
            }
            ChatBots();
            startupargs = args;
            InitializeClient();
        }
        public static async void ChatBots() {
            while (true) {
                if (Settings.isBotLaunched) {
                }
                await Task.Delay(100);
            }
        }
        public enum WinMessages : uint {
            SETICON = 0x0080,
        }
        private static void RequestPassword() {
            Console.Write(ConsoleIO.BasicIO ? Translations.Get("mcc.password_basic_io", Settings.Login) + "\n" : Translations.Get("mcc.password"));
            Settings.Password = ConsoleIO.BasicIO ? Console.ReadLine() : ConsoleIO.ReadPassword();
            if (Settings.Password == "") { Settings.Password = "-"; }
            if (!ConsoleIO.BasicIO) {
                Console.CursorTop--; Console.Write(Translations.Get("mcc.password_hidden", "<******>"));
                for (int i = 19; i < Console.BufferWidth; i++) { Console.Write(' '); }
            }
        }
        private static void SetWindowIcon(System.Drawing.Icon icon) {
            IntPtr mwHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            SendMessage(mwHandle, (int)WinMessages.SETICON, 0, icon.Handle);
            SendMessage(mwHandle, (int)WinMessages.SETICON, 1, icon.Handle);
        }
        private static void InitializeClient() {
            SessionToken session = new SessionToken();
            ProtocolHandler.LoginResult result = ProtocolHandler.LoginResult.LoginRequired;
            if (Settings.Password == "-") {
                Translations.WriteLineFormatted("mcc.offline");
                result = ProtocolHandler.LoginResult.Success;
                session.PlayerID = "0";
                session.PlayerName = Settings.Login;
            }
            else {
                if (Settings.SessionCaching != CacheType.None && SessionCache.Contains(Settings.Login.ToLower())) {
                    session = SessionCache.Get(Settings.Login.ToLower());
                    result = ProtocolHandler.GetTokenValidation(session);
                    if (result != ProtocolHandler.LoginResult.Success) {
                        Translations.WriteLineFormatted("mcc.session_invalid");
                        if (Settings.Password == "")
                            RequestPassword();
                    }
                    else ConsoleIO.WriteLineFormatted(Translations.Get("mcc.session_valid", session.PlayerName));
                }
                if (result != ProtocolHandler.LoginResult.Success) {
                    result = ProtocolHandler.GetLogin(Settings.Login, Settings.Password, Settings.AccountType, out session);
                    if (result == ProtocolHandler.LoginResult.Success && Settings.SessionCaching != CacheType.None)
                        SessionCache.Store(Settings.Login.ToLower(), session);
                }
            }

            if (result == ProtocolHandler.LoginResult.Success) {
                Settings.Username = session.PlayerName;
                if (Settings.ServerIP == "") {
                    Settings.SetServerIP(Console.ReadLine());
                }
                int protocolversion = 0;
                ForgeInfo forgeInfo = null;
                if (Settings.ServerVersion != "" && Settings.ServerVersion.ToLower() != "auto") {
                    protocolversion = MinecraftClient.Protocol.ProtocolHandler.MCVer2ProtocolVersion(Settings.ServerVersion);
                    if (protocolversion != 0)
                        ConsoleIO.WriteLineFormatted(Translations.Get("mcc.use_version", Settings.ServerVersion, protocolversion));
                    else ConsoleIO.WriteLineFormatted(Translations.Get("mcc.unknown_version", Settings.ServerVersion));
                    if (useMcVersionOnce) {
                        useMcVersionOnce = false;
                        Settings.ServerVersion = "";
                    }
                }
                if (protocolversion == 0 || Settings.ServerAutodetectForge || (Settings.ServerForceForge && !ProtocolHandler.ProtocolMayForceForge(protocolversion))) {
                    if (protocolversion != 0)
                        Translations.WriteLine("mcc.forge");
                    else Translations.WriteLine("mcc.retrieve");
                    if (!ProtocolHandler.GetServerInfo(Settings.ServerIP, Settings.ServerPort, ref protocolversion, ref forgeInfo)) {
                        HandleFailure(Translations.Get("error.ping"), true, MinecraftClient.ChatBots.AutoRelog.DisconnectReason.ConnectionLost);
                        return;
                    }
                }
                if (Settings.ServerForceForge && forgeInfo == null) {
                    if (ProtocolHandler.ProtocolMayForceForge(protocolversion)) {
                        Translations.WriteLine("mcc.forgeforce");
                        forgeInfo = ProtocolHandler.ProtocolForceForge(protocolversion);
                    }
                    else {
                        HandleFailure(Translations.Get("error.forgeforce"), true, MinecraftClient.ChatBots.AutoRelog.DisconnectReason.ConnectionLost);
                        return;
                    }
                }
                if (protocolversion != 0) {
                    try {
                        if (Settings.SingleCommand != "") {
                            client = new McClient(session.PlayerName, session.PlayerID, session.ID, Settings.ServerIP, Settings.ServerPort, protocolversion, forgeInfo, Settings.SingleCommand);
                            Settings.isBotLaunched = true;
                        }
                        else {
                            client = new McClient(session.PlayerName, session.PlayerID, session.ID, protocolversion, forgeInfo, Settings.ServerIP, Settings.ServerPort);
                            Settings.isBotLaunched = true;
                        }
                        if (Settings.ConsoleTitle != "") {
                            Console.Title = $"Faction-Bot-Public";
                            System.Drawing.Bitmap skin = null;
                            return;
                            // Add ICON Later
                            SetWindowIcon(System.Drawing.Icon.FromHandle(skin.GetHicon()));
                            SetConsoleIcon(skin.GetHicon());
                        }
                    }
                    catch (NotSupportedException) { HandleFailure(Translations.Get("error.unsupported"), true); }
                }
                else HandleFailure(Translations.Get("error.determine"), true);
            }
            else {
                string failureMessage = Translations.Get("error.login");
                string failureReason = "";
                switch (result) {
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
                if (result == ProtocolHandler.LoginResult.SSLError && isUsingMono) {
                    Translations.WriteLineFormatted("error.login.ssl_help");
                    return;
                }
                HandleFailure(failureMessage, false, ChatBot.DisconnectReason.LoginRejected);
            }
        }
        public static void Restart(int delaySeconds = 0) {
            new Thread(new ThreadStart(delegate {
                if (client != null) { client.Disconnect(); ConsoleIO.Reset(); }
                if (offlinePrompt != null) { offlinePrompt.Abort(); offlinePrompt = null; ConsoleIO.Reset(); }
                if (delaySeconds > 0) {
                    Translations.WriteLine("mcc.restart_delay", delaySeconds);
                    System.Threading.Thread.Sleep(delaySeconds * 1000);
                }
                Translations.WriteLine("mcc.restart");
                InitializeClient();
                Program.ChatBots();
            })).Start();
        }
        public static void Exit(int exitcode = 0) {
            new Thread(new ThreadStart(delegate {
                if (client != null) { client.Disconnect(); ConsoleIO.Reset(); }
                if (offlinePrompt != null) { offlinePrompt.Abort(); offlinePrompt = null; ConsoleIO.Reset(); }
                if (Settings.playerHeadAsIcon) { ConsoleIcon.revertToMCCIcon(); }
                Environment.Exit(exitcode);
            })).Start();
        }
        public static void HandleFailure(string errorMessage = null, bool versionError = false, MinecraftClient.ChatBots.AutoRelog.DisconnectReason? disconnectReason = null) {
            if (!String.IsNullOrEmpty(errorMessage)) {
                ConsoleIO.Reset();
                while (Console.KeyAvailable)
                    Console.ReadKey(true);
                Console.WriteLine(errorMessage);

                if (disconnectReason.HasValue)
                    if (MinecraftClient.ChatBots.AutoRelog.OnDisconnectStatic(disconnectReason.Value, errorMessage))
                        return;
            }

            if (Settings.interactiveMode) {
                if (versionError) {
                    Translations.Write("mcc.server_version");
                    Settings.ServerVersion = Console.ReadLine();
                    if (Settings.ServerVersion != "") {
                        useMcVersionOnce = true;
                        Restart();
                        return;
                    }
                }
                if (offlinePrompt == null) {
                    offlinePrompt = new Thread(new ThreadStart(delegate {
                        string command = " ";
                        ConsoleIO.WriteLineFormatted(Translations.Get("mcc.disconnected", (Settings.internalCmdChar == ' ' ? "" : "" + Settings.internalCmdChar)));
                        Translations.WriteLineFormatted("mcc.press_exit");
                        while (command.Length > 0) {
                            if (!ConsoleIO.BasicIO)
                                ConsoleIO.Write('>');
                            command = Console.ReadLine().Trim();
                            if (command.Length > 0) {
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
                                else if (command.StartsWith("help")) {
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
            else {
                if (disconnectReason.HasValue) {
                    if (disconnectReason.Value == ChatBot.DisconnectReason.UserLogout) Exit(1);
                    if (disconnectReason.Value == ChatBot.DisconnectReason.InGameKick) Exit(2);
                    if (disconnectReason.Value == ChatBot.DisconnectReason.ConnectionLost) Exit(3);
                    if (disconnectReason.Value == ChatBot.DisconnectReason.LoginRejected) Exit(4);
                }
                Exit();
            }

        }
        public static bool isUsingMono {
            get {
                return Type.GetType("Mono.Runtime") != null;
            }
        }
        public static Type[] GetTypesInNamespace(string nameSpace, Assembly assembly = null) {
            if (assembly == null) { assembly = Assembly.GetExecutingAssembly(); }
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }
        static Program()
        {
            AssemblyConfigurationAttribute attribute
             = typeof(Program)
                .Assembly
                .GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false)
                .FirstOrDefault() as AssemblyConfigurationAttribute;
            if (attribute != null)
                BuildInfo = attribute.Configuration;
        }
    }
}
