using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NDesk.Options;

namespace ConDep.Test.Cli
{
    public class CmdHelpWriter
    {
        protected readonly TextWriter _writer;

        public CmdHelpWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public virtual void WriteHelp(Exception ex, OptionSet optionSet)
        {
            WriteException(ex);
            WriteHelp();
        }

        public virtual void WriteHelp(OptionSet optionSet)
        {

        }

        public virtual void WriteHelp()
        {
            PrintCopyrightMessage();

            var help = @"
Bootstrap servers and execute integration tests

Usage: ConDepTest <command> <options>

Available commands:
    Bootstrap        Bootstrap servers for use with integration testing
    Terminate        Terminate allready bootstrapped servers
    Test             Execute integration tests
    Help <command>   Display help for individual help commands
";
            _writer.Write(help);
        }

        public void PrintCopyrightMessage()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            var version = versionInfo.ProductVersion.Substring(0, versionInfo.ProductVersion.LastIndexOf("."));
            var versionText = string.Format("Version {0} ", version);

            _writer.Write(@"
Copyright (c) Jon Arild Torresdal
  ____            ____           _____         _   
 / ___|___  _ __ |  _ \  ___ _ _|_   _|__  ___| |_ 
| |   / _ \| '_ \| | | |/ _ \ '_ \| |/ _ \/ __| __|
| |__| (_) | | | | |_| |  __/ |_) | |  __/\__ \ |_ 
 \____\___/|_| |_|____/ \___| .__/|_|\___||___/\__|
                            |_|   " + versionText + "\n\n");
        }

        public void WriteException(Exception ex)
        {
            _writer.WriteLine(ex.Message);
            _writer.Write(ex.StackTrace);
            _writer.WriteLine();
        }

        public void WriteHelpForCommand(ConDepCommand command)
        {
            IHandleConDepCommands commandHandler;
            switch (command)
            {
                case ConDepCommand.Bootstrap:
                    commandHandler = null;
                    break;
                case ConDepCommand.Test:
                    commandHandler = null;
                    break;
                case ConDepCommand.Help:
                    commandHandler = null;
                    break;
                default:
                    commandHandler = null;
                    break;
            }

            if (commandHandler == null)
                WriteHelp();
            else
                commandHandler.WriteHelp();

        }
    }
}