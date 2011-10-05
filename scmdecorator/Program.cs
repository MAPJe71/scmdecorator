﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace scmdecorator
{
    class Program
    {
        private const string ApplicationName = "scmdecorator";

        private class ScmInfo
        {
            internal string Scm;
            internal string Folder;
            internal string Icon;
        }

        private static readonly List<ScmInfo> ScmInfos =
            new List<ScmInfo>
                {
                    new ScmInfo { Scm = "Subversion", Folder = ".svn", Icon = "svn.ico" },
                    new ScmInfo { Scm = "Git", Folder = ".git", Icon = "git.ico" },
                    new ScmInfo { Scm = "Mercurial", Folder = ".hg", Icon = "hg.ico" }
                };

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("usage: {0} [scm-folder]", ApplicationName);
                return;
            }

            string scmFolder;
            if (args.Length == 1)
            {
                scmFolder = args[0];
            }
            else
            {
                Console.Write("Enter name of SCM folder: ");
                scmFolder = Console.ReadLine() ?? String.Empty;
            }

            ScmInfo currentScmInfo;
            try
            {
                if (!Directory.Exists(scmFolder))
                {
                    Console.WriteLine("{0}: Folder '{1}' does not exist.", ApplicationName, scmFolder);
                    return;
                }

                var dirInfo = new DirectoryInfo(scmFolder);
                currentScmInfo = ScmInfos.SingleOrDefault(info => dirInfo.GetDirectories(info.Folder).Length > 0);
                if (currentScmInfo == null)
                {
                    Console.WriteLine("{0}: Folder '{1}' does not appear to be an SCM folder; support directory is missing", ApplicationName, scmFolder);
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}: Error when accessing folder, message: {1}", ApplicationName, e.Message);
                return;
            }

            try
            {
                var iconExists = File.Exists(currentScmInfo.Icon);
                var desktopIniText = String.Format(CultureInfo.InvariantCulture,
                                                   "[.ShellClassInfo]{0}" +
                                                   "ConfirmFileOp=0{0}" +
                                                   "NoSharing=1{0}" +
                                                   (iconExists ? "IconFile={1}{0}IconIndex=0{0}" : String.Empty) +
                                                   "InfoTip={2} version controlled folder.", 
                                                   Environment.NewLine, currentScmInfo.Icon, currentScmInfo.Scm);

                var destDesktopIni = Path.Combine(scmFolder, "desktop.ini");
                File.WriteAllText(destDesktopIni, desktopIniText);
                File.SetAttributes(destDesktopIni, File.GetAttributes(destDesktopIni) | FileAttributes.Hidden);

                var destScmIcon = Path.Combine(scmFolder, currentScmInfo.Icon);
                if (iconExists && !File.Exists(destScmIcon))
                {
                    File.Copy(currentScmInfo.Icon, destScmIcon, false);
                    File.SetAttributes(destScmIcon, File.GetAttributes(destScmIcon) | FileAttributes.Hidden);
                }
            }
            catch (Exception e)
            {
                
                throw;
            }
        }
    }
}
