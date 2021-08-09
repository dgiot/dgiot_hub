using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace dgiot_hub
{
    class dgiot_hub
    {
        static void Main(string[] args)
        {
            Process[] process1 = Process.GetProcesses();
            foreach (Process prc in process1)
            {
                if (prc.ProcessName == "erl")
                    prc.Kill();
            }

            foreach (Process prc in process1)
            {
                if (prc.ProcessName == "node")
                    prc.Kill();
            }
          
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            
            String cmdType = args[0];
            string AppPath = System.Environment.CurrentDirectory;
            string PgData = AppPath + "/datacenter/pgdata";
            string PgCmd = AppPath + "/pgsql/bin/pg_ctl.exe";
            string PgInit = AppPath + "/pgsql/bin/initdb.exe";
            string ParseCmd = AppPath + "/node/pm2.cmd";
            string NSSMCmd = AppPath + "/node/nssm.exe";

            switch (cmdType)
            {
                case "start":
                    if (Directory.Exists(PgData) == false)
                    {
                        string[] pgInitArgs = new string[] { "-D", PgData, "-E", "UTF8" };
                        StartProcess(PgInit, pgInitArgs);
                    };
                    string[] pgInstallArgs = new string[] { "register", "-N", "\"pgsql\"", "-D", PgData };
                    StartProcess(PgCmd, pgInstallArgs);

                    string[] pgStartArgs = new string[] { "start", "\"pgsql\"" };
                    StartProcess("net", pgStartArgs);

                    string[] filesInstallArgs = new string[] { "install", "\"DGIoTFS\"", AppPath + "/datacenter/file/file.exe" };
                    StartProcess(NSSMCmd, filesInstallArgs);

                    string[] filesStartArgs = new string[] { "start", "\"DGIoTFS\"" };
                    StartProcess("net", filesStartArgs);

                    string[] parseStartArgs = new string[] { "start", AppPath + "/parse/server/index.js" };
                    StartProcess(ParseCmd, parseStartArgs);

                    string FilePath = System.Environment.CurrentDirectory + "/emqx/bin/emqx.cmd";
                    StartProcess(FilePath, args);
                    Thread.Sleep(1000);
                    System.Diagnostics.Process.Start("http://127.0.0.1:5080");


                    break;
                case "stop":
                    string[] parseStopArgs = new string[] { "delete", "index" };
                    StartProcess(ParseCmd, parseStopArgs);

                    string[] filesStopArgs = new string[] { "stop", "\"DGIoTFile\"" };
                    StartProcess("net", filesStopArgs);

                    string[] pgStopArgs = new string[] { "stop", "\"pgsql\"" };
                    StartProcess("net", pgStopArgs);


                    string[] filesRemoveArgs = new string[] { "remove", "\"DGIoTFS\"", "confirm" };
                    StartProcess(NSSMCmd, filesRemoveArgs);

                    string[] pgUnregisterArgs = new string[] { "unregister", "-N", "\"pgsql\"" };
                    StartProcess(PgCmd, pgUnregisterArgs);

                    break;
                default:
                    break; 
            }
        }

        static public bool StartProcess(string filename, string[] args)
        {
            try
            {
                string s = "";
                foreach (string arg in args)
                {
                    s = s + arg + " ";
                }
                s = s.Trim();
                Process myprocess = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo(filename, s);
                myprocess.StartInfo = startInfo;

                //通过以下参数可以控制exe的启动方式，具体参照 myprocess.StartInfo.下面的参数，如以无界面方式启动exe等
                myprocess.StartInfo.UseShellExecute = false;
                myprocess.Start();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("启动应用程序时出错！原因：" + ex.Message);
            }
            return false;
         }
        }
    }
