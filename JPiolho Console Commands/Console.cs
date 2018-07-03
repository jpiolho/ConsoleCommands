using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPiolho.ConsoleCommands
{
    public static class ConsoleCommands
    {
        private static bool stopRequested = false;

        /// <summary>
        /// Starts the console command subsystem.
        /// The calling thread will be blocked by UI reading until the subsystem is stopped.
        /// </summary>
        public static void Initialize()
        {
            stopRequested = false;

            while (!stopRequested)
            {
                try
                {
                    var line = Console.ReadLine().Trim();

                    // Ignore if line is empty
                    if (line.Length == 0)
                        continue;

                    var tokens = GetTokens(line);

                    // Ignore if there was nothing
                    if (tokens.Length == 0)
                        continue;

                    string command = tokens[0].ToLower();
                    string[] arguments = new string[tokens.Length - 1];

                    // Copy arguments
                    if (tokens.Length > 1)
                        Array.Copy(tokens, 1, arguments, 0, tokens.Length - 1);

                    OnCommand?.Invoke(command, arguments);

                    // Call registered command
                    if (registeredCommands.ContainsKey(command))
                    {
                        registeredCommands[command]?.Invoke(arguments);
                    }
                    else // Or unknown command
                    {
                        OnUnknownCommand?.Invoke(command, arguments);
                    }
                }
                catch(Exception ex)
                {
                    if(OnError != null)
                    {
                        OnError.Invoke(ex);
                    }
                    else
                    {
                        Console.WriteLine("Error in handling command: " + ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Stop the console command subsystem
        /// </summary>
        public static void Stop()
        {
            stopRequested = true;
        }



        public delegate void OnRegisteredCommandCallbackHandler(string[] arguments);
        private static Dictionary<string, OnRegisteredCommandCallbackHandler> registeredCommands = new Dictionary<string, OnRegisteredCommandCallbackHandler>();

        /// <summary>
        /// Register a command to a callback. This callback will be called when the command is entered.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="callback"></param>
        public static void RegisterCommand(string command, OnRegisteredCommandCallbackHandler callback)
        {
            registeredCommands.Add(command, callback);
        }


        public delegate void OnCommandEventHandler(string command, string[] arguments);
        public static event OnCommandEventHandler OnCommand;

        public delegate void OnUnknownCommandEventHandler(string command, string[] arguments);
        public static event OnCommandEventHandler OnUnknownCommand;

        public delegate void OnErrorEventHandler(Exception ex);
        public static event OnErrorEventHandler OnError;




        private static string[] GetTokens(string input)
        {
            List<string> tokens = new List<string>();


            string s = "";
            void AddToken()
            {
                if (s == "") return;
                tokens.Add(s);
                s = "";
            }

            bool escaping = false;
            bool quote = false;
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (escaping)
                {
                    s += c;
                    escaping = false;
                }
                else if (c == '\\')
                {
                    escaping = true;
                }
                else if (c == '"')
                {
                    if (!quote)
                    {
                        quote = true;
                    }
                    else
                    {
                        quote = false;
                        AddToken();
                    }
                }
                else if (c == ' ')
                {
                    if (quote)
                    {
                        s += c;
                    }
                    else
                    {
                        AddToken();
                    }
                }
                else
                {
                    s += input[i];
                }

            }

            AddToken();

            return tokens.ToArray();
        }
    }
}
