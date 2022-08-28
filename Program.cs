/*!
 *
 * MineBitcoin - Bitcoin Address Generator
 * Copyright (c) 2022 ActiveTK. <webmaster[at]activetk.cf>
 * License: The MIT License
 *
 */

using System;
using System.IO;
using System.Threading;
using NBitcoin;

namespace MineBitcoin
{
    internal class Program
    {
        public static string LogFile = "";
        static void Main(string[] args)
        {
            Console.Title = "MineBitcoin - Bitcoin Address Generator / Build 2022.08.28";
            WriteLine("*****************************************************************************");
            WriteLine("** " + Console.Title);
            WriteLine("** Copyright (c) 2022 ActiveTK. <webmaster[at]activetk.jp>");
            WriteLine("*****************************************************************************");

            LogFile = ".\\logs\\MineBitcoin " + DateTime.Now.ToString("yyyy_MM_dd HHmmss") + ".log";
            try
            {
                if (!Directory.Exists(".\\logs"))
                    Directory.CreateDirectory(".\\logs");
                if (!Directory.Exists(".\\keys"))
                    Directory.CreateDirectory(".\\keys");
            }
            catch { }

            Console.Write("!* Filters > ");
            Console.ForegroundColor = ConsoleColor.Green;
            string[] filters = Console.ReadLine().Split(' ');
            for (int i = 0; i < filters.Length; i++)
                filters[i] = filters[i].ToLower();
            Console.ResetColor();

            decimal count = 0, match = 0;
            string genlist = "";
            var gen = new ThreadStart(() => {
                var mine = new MineBitcoin();
                mine.filters = filters;
                mine.network = Network.GetNetwork("mainnet");
                while (true)
                {
                    var result = mine.gen();
                    if (result != null)
                    {
                        match++;
                        var pub = result.PubKey;
                        genlist += "** [Found] " + GetTime() + " : " + pub.GetAddress(ScriptPubKeyType.Legacy, mine.network).ToString() + Environment.NewLine;
                        genlist += "**   => Public Key : " + pub.ToString() + Environment.NewLine;
                        genlist += "**   => Private Key (WIF) : " + result.GetWif(mine.network) + Environment.NewLine;
                        try
                        {
                            File.AppendAllText(".\\keys\\" + pub.GetAddress(ScriptPubKeyType.Legacy, mine.network).ToString(),
                                "Public Key : " + pub.ToString() + Environment.NewLine +
                                "Private Key (WIF) : " + result.GetWif(mine.network) + Environment.NewLine);
                        }
                        catch { }
                    }
                    count++;
                }
            });
            Console.Write("!* Threads count (Empty to use ProcessorCount) > ");
            Console.ForegroundColor = ConsoleColor.Green;
            int Threads = Environment.ProcessorCount;
            if (!int.TryParse(Console.ReadLine(), out Threads))
            {
                ViewInfo("ERR", "Fatal->int.TryParse()");
                Environment.Exit(-1);
            }
            Console.ResetColor();
            ViewInfo("INFO", "Filters:" + string.Join(" / ", filters));
            for (int i = 0; i < Threads; i++)
            {
                ViewInfo("INFO", "Starting thread [" + (i + 1) + "/" + Threads + "]");
                new Thread(gen).Start();
            }
            ViewInfo("INFO", "Press [Ctrl] + [C] to exit.");
            var TimerF = new System.Diagnostics.Stopwatch();
            TimerF.Start();
            int clock = 0;
            while (true)
            {
                Thread.Sleep(800);
                Console.Write("$* ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[RUNNING]");
                Console.ForegroundColor = ConsoleColor.Green;
                string SpeedDetail = " Total:" + count + "h, Match:" + match + "h, " +
                    "Time:" + TimerF.Elapsed.TotalSeconds + "s, Speed:" + (int)((double)count / TimerF.Elapsed.TotalSeconds) + "h/s    \n";
                Console.Write(SpeedDetail);
                Console.ResetColor();
                clock++;
                if (clock % 12 == 0)
                  Log("$* [RUNNING]" + SpeedDetail);
                if (genlist != "")
                {
                    Console.Write(genlist);
                    Log(genlist);
                    genlist = "";
                }
            }
        }
        public static void Log(string Message)
        {
            try
            {
                File.AppendAllText(@LogFile, Message);
            } catch { }
        }
        public static void ViewInfo(string type, string msg)
        {
            WriteLine("** [" + type + "] " + GetTime() + " : " + msg);
        }
        public static string GetTime()
        {
            return DateTime.Now.ToUniversalTime().ToString("R");
        }
        public static void WriteLine(string Text)
        {
            Console.WriteLine(Text);
            Log(Text + Environment.NewLine);
        }
    }
    class MineBitcoin
    {
        public string[] filters = null;
        public Network network = null;
        public Key gen()
        {
            var privateKey = new Key();
            string key = privateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, network).ToString();
            foreach (var str in filters)
            {
                if (key.Contains(str))
                    return privateKey;
            }
            return null;
        }
    }
}
