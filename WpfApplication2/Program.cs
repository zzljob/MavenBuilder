using System.Windows;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MavenBuilder
{    
    public partial class Builder : Window
    {
        private enum Setp_state { Ready = 1, Processing = 2, Succeeded = 4, Failed = 8 };
        private Setting setting;

        private void ReadSettingFromEnvironment() 
        {
            string java_home = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(java_home)) 
            {
                Textbox_java_path.Text = java_home;
            }
            
            string maven_home = Environment.GetEnvironmentVariable("M2_HOME");
            if (!string.IsNullOrEmpty(maven_home)) 
            {
                Textbox_maven_path.Text = maven_home;
                Textbox_maven_set_path.Text = GetMavenSettingPath(maven_home);
            }
            
        }     

        private Setting InitSetting() 
        {            
            setting = new Setting();            
            setting.Java_path = Textbox_java_path.Text.Trim();
            setting.Maven_path = Textbox_maven_path.Text.Trim();
            setting.Maven_set_path = Textbox_maven_set_path.Text.Trim();
            setting.Project_path = Textbox_project_path.Text.Trim();
            setting.Project_set_path = Textbox_project_set_path.Text.Trim();
            return setting;
        }

        private void ResetBuildState() 
        {
            SetBuildSetpState(Label_setp_check, Setp_state.Ready);
            SetBuildSetpState(Label_setp_backup, Setp_state.Ready);
            SetBuildSetpState(Label_setp_copy, Setp_state.Ready);
            SetBuildSetpState(Label_setp_build, Setp_state.Ready);
            SetBuildSetpState(Label_setp_recovery, Setp_state.Ready);
            SetBuildSetpState(Label_setp_result, Setp_state.Ready);            
        }

        private bool SetBuildSetpState(Label step, Setp_state state) 
        {
            if(step == null)
            {
                return false;
            }
            BrushConverter brushConverter = new BrushConverter();
            if (state == Setp_state.Ready) 
            {
                step.Background = (Brush)brushConverter.ConvertFromString("#00000000");                
                step.Foreground = (Brush)brushConverter.ConvertFromString("#44444444");                
            }
            else if (state == Setp_state.Processing) 
            {
                step.Background = (Brush)brushConverter.ConvertFromString("#99999999");
                step.Foreground = (Brush)brushConverter.ConvertFromString("#ffffffff");
            }
            else if (state == Setp_state.Succeeded)
            {
                step.Background = (Brush)brushConverter.ConvertFromString("#FF4AA2EA");
                step.Foreground = (Brush)brushConverter.ConvertFromString("#ffffffff");
            }
            else if (state == Setp_state.Failed)
            {
                step.Background = (Brush)brushConverter.ConvertFromString("#ffFF0000");
                step.Foreground = (Brush)brushConverter.ConvertFromString("#ffffffff");
            }
            return true;
        }        

        private void ResetProjectSetFiles(List<string> filesPath) 
        {
            if (filesPath == null || filesPath.Count <= 0)
            {
                return;
            }
            foreach (string path in filesPath)
            {                
                string fullTargetPath = string.Format("{0}\\{1}", setting.Project_path, path);
                string fullSrcPath = string.Format("{0}\\{1}", setting.Backup_path, path);
                if (File.Exists(fullTargetPath))
                {
                    File.Delete(fullTargetPath);
                }
                if (File.Exists(fullSrcPath))
                {
                    File.Copy(fullSrcPath, fullTargetPath, true);
                }
            }
        }

        private void buildProject() 
        {
            Process cmd = null;
            try
            {
                cmd = new Process();
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.CreateNoWindow = false;

                cmd.Start();
                StreamWriter sw = cmd.StandardInput;
                Console.WriteLine(setting.Full_build_cmd);
                sw.WriteLine(setting.Full_build_cmd);
                sw.Flush();
                sw.Close();

                cmd.BeginOutputReadLine();
                cmd.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);
                Console.WriteLine("Finished cmd");
            }
            catch (Exception e4)
            {
                Console.WriteLine("failed cmd");
            }
            finally 
            {
                if (cmd != null) 
                {
                    cmd.Close();
                }
            }

        }

        private void CopyProjectSetFiles(List<string> filesPath) 
        {
            if (filesPath == null || filesPath.Count <= 0)
            {
                return;
            }

            foreach (string path in filesPath)
            {
                string fullSrcPath = string.Format("{0}\\{1}", setting.Project_set_path, path);
                string fullTargetPath = string.Format("{0}\\{1}", setting.Project_path, path);                
                if (File.Exists(fullSrcPath))
                {                    
                    File.Copy(fullSrcPath, fullTargetPath, true);
                }
            }
        
        }

        private void FindAndBackupProjectFiles(List<string> filesPath) 
        {
            if (filesPath == null || filesPath.Count <= 0) 
            {
                return;            
            }
            //先清空临时文件
            string backupPath = setting.Backup_path;
            if(Directory.Exists(backupPath))
            {                
                Directory.Delete(backupPath, true);
            }
            Directory.CreateDirectory(backupPath);
            
            foreach (string path in filesPath) 
            {
                string fullPath = string.Format("{0}\\{1}", setting.Project_path, path);
                if (File.Exists(fullPath)) 
                {
                    string target = string.Format("{0}\\{1}", backupPath, path);
                    File.Copy(fullPath, target);                
                }            
            }
        }

        private List<string> FindBackupFilesPath() 
        {
            DirectoryInfo root = new DirectoryInfo(setting.Project_set_path);
            List<FileInfo> fileList = new List<FileInfo>();
            
            ScanFile(root, fileList);
            
            List<string> filePathList = new List<string>();
            int length = setting.Project_set_path.Length;
            if(fileList != null && fileList.Count > 0)
            {
                foreach(FileInfo info in fileList)
                {                    
                    filePathList.Add(info.FullName.Substring(length));
                }
            }

            return filePathList;
        }

        private void ScanFile(DirectoryInfo root, List<FileInfo> fileList) 
        {
            if (root == null || fileList == null) 
            {
                return;
            }
            
            FileInfo[] files = root.GetFiles();
            if (files != null || files.Length > 0) 
            {
                foreach(FileInfo info in files)
                {
                    fileList.Add(info);
                }
            }

            DirectoryInfo[] dirs = root.GetDirectories();
            if (dirs != null || dirs.Length > 0) 
            {
                foreach (DirectoryInfo info in dirs) 
                {
                    ScanFile(info, fileList);
                }
            }
        }

        private string GetMavenSettingPath(string maven_path) 
        {
            if (!string.IsNullOrEmpty(maven_path)) 
            {
                return maven_path + "\\conf\\settings.xml";
            }
            return "";
        }
    }

    public partial class Setting 
    {
        private string java_path;       
        private string maven_path;     
        private string maven_set_path;     
        private string project_path;       
        private string project_set_path;
        private string home_path;
        private bool debug = false;
        private string maven_cmd;


        public Setting() 
        {
            home_path = string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MavenBuilder");
            if (!Directory.Exists(home_path)) 
            {
                Directory.CreateDirectory(home_path);
            }
        }

        public bool Show_debug_log
        {
            get { return debug; }
            set { debug = value; }
        }

        public string Maven_cmd
        {
            get { return maven_cmd; }
            set { maven_cmd = value; }
        }

        public string Full_build_cmd
        {
            get 
            {
                /**
                StringBuilder cmd = new StringBuilder();
                cmd.Append(string.Format("{0}\\bin\\java.exe", Java_path)).Append(" ");//%MAVEN_JAVA_EXE%
                cmd.Append("-Xdebug -Xnoagent -Djava.compiler=NONE -Xrunjdwp:transport=dt_socket,server=y,suspend=y,address=8000").Append(" ");//%MAVEN_OPTS%

                string CLASSWORLDS_JAR = string.Format("{0}\\boot\\plexus-classworlds-2.4.jar", Maven_path);
                string[] jars = Directory.GetFiles(string.Format("{0}\\boot", Maven_path), "plexus-classworlds-*");
                if (jars != null && jars.Length == 1)
                {
                    CLASSWORLDS_JAR = jars[0];
                }
                cmd.Append(string.Format("-classpath {0}", CLASSWORLDS_JAR)).Append(" ");

                cmd.Append(string.Format("\"-Dclassworlds.conf={0}\\bin\\m2.conf\"", Maven_path)).Append(" ");//classworlds_conf
                cmd.Append(string.Format("\"-Dmaven.home={0}\"", Maven_path)).Append(" ");//"-Dmaven.home=%M2_HOME%"
                cmd.Append("org.codehaus.plexus.classworlds.launcher.Launcher").Append(" ");//CLASSWORLDS_LAUNCHER
                cmd.Append(" clean package ");//%MAVEN_CMD_LINE_ARGS%
                return cmd.ToString();
                 * */
                StringBuilder cmd = new StringBuilder();
                cmd.Append(string.Format("\"{0}\\bin\\mvn.bat\"", maven_path)).Append(" ");
                cmd.Append(string.Format("-f \"{0}\\pom.xml\"", project_path)).Append(" ");
                cmd.Append(string.Format("-s \"{0}\"", maven_set_path)).Append(" ");
                cmd.Append(string.Format("-l \"{0}\\log\\log.log\"", project_path)).Append(" ");
                cmd.Append("clean package");
                return cmd.ToString();
            }
        }

        public string Home_path
        {
            get { return home_path; }
            set { home_path = value; }
        }

        public string Backup_path
        {
            get { return string.Format("{0}\\{1}", home_path, "backup"); }            
        }
       
        public string Java_path
        {
            get { return java_path; }
            set { java_path = value; }
        }
        public string Maven_path
        {
            get { return maven_path; }
            set { maven_path = value; }
        }
        public string Maven_set_path
        {
            get { return maven_set_path; }
            set { maven_set_path = value; }
        }
        public string Project_path
        {
            get { return project_path; }
            set { project_path = value; }
        }
        public string Project_set_path
        {
            get { return project_set_path; }
            set { project_set_path = value; }
        }

        public bool Check_Setting() 
        {
            if (string.IsNullOrEmpty(java_path) || !Directory.Exists(java_path))
            {
               return false;
            }
            if (string.IsNullOrEmpty(maven_path) || !Directory.Exists(maven_path))
            {
                return false;
            }
            if(string.IsNullOrEmpty(maven_set_path) || !File.Exists(maven_set_path))
            {
                return false;
            }
            if (string.IsNullOrEmpty(project_path) || !Directory.Exists(project_path))
            {
                return false;
            }
            if (string.IsNullOrEmpty(project_set_path) || !Directory.Exists(project_set_path))
            {
                return false;
            }
            return true;
        }
       
    }
}