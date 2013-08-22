using System;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace MavenBuilder
{
    /// <summary>
    /// Builder.xaml 的交互逻辑
    /// </summary>
    public partial class Builder : Window
    {
        public Builder()
        {
            InitializeComponent();
            ReadSettingFromEnvironment();
            ResetBuildState();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }      

        private void Button_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_max_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else 
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void Button_close_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }      

        private void Button_java_path_Click(object sender, RoutedEventArgs e)
        {
            string javaPath = "./";
            if (!String.IsNullOrEmpty(Textbox_java_path.Text)) 
            {
                javaPath = Textbox_java_path.Text.Trim();
            }

            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "请选择JDK所在的文件夹";
            folderDialog.ShowNewFolderButton = true;            
            //folderDialog.RootFolder = Environment.SpecialFolder.ProgramFiles;
            folderDialog.SelectedPath = javaPath;
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();            
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderName = folderDialog.SelectedPath;
                if (!String.IsNullOrEmpty(folderName))
                {
                    Textbox_java_path.Text = folderName;
                }
            }
           
        }
        
        private void Button_tap_setting_Click(object sender, RoutedEventArgs e)
        {
            Panel_body_setting.Visibility = Visibility.Visible;
            Panel_body_build.Visibility = Visibility.Collapsed;
        }

        private void Button_tap_build_Click(object sender, RoutedEventArgs e)
        {
            Panel_body_setting.Visibility = Visibility.Collapsed;
            Panel_body_build.Visibility = Visibility.Visible;
        }

        private void Button_maven_path_Click(object sender, RoutedEventArgs e)
        {
            string mavenPath = "./";
            if (!String.IsNullOrEmpty(Textbox_maven_path.Text))
            {
                mavenPath = Textbox_maven_path.Text.Trim();
            }

            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "请选择Maven所在的文件夹";
            folderDialog.ShowNewFolderButton = true;
            //folderDialog.RootFolder = Environment.SpecialFolder.ProgramFiles;
            folderDialog.SelectedPath = mavenPath;
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderName = folderDialog.SelectedPath;
                if (!String.IsNullOrEmpty(folderName))
                {
                    Textbox_maven_path.Text = folderName;
                    Textbox_maven_set_path.Text = GetMavenSettingPath(folderName);
                }
            }
            else if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                //do nothing
            }
        }

        private void Button_maven_set_path_Click(object sender, RoutedEventArgs e)
        {
            string mavenSetPath = "";
            if (!String.IsNullOrEmpty(Textbox_maven_set_path.Text))
            {
                mavenSetPath = Textbox_maven_set_path.Text.Trim();
            }
            int index = mavenSetPath.LastIndexOf("\\");
            string path = "", fileName = "";
            if (index > -1) {
                path = mavenSetPath.Substring(index);
                fileName = mavenSetPath.Substring(0, index);    
            }
            

            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.DefaultExt = "xml";
            fileDialog.Filter = "XML files (*.xml)|*.xml|所有文件(*.*)|*.*";
            fileDialog.InitialDirectory = path;
            fileDialog.FileName = null;
            
            DialogResult result = fileDialog.ShowDialog();
            // OK button was pressed.
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //string openFileName = fileDialog.FileName;
                Textbox_maven_set_path.Text = fileDialog.FileName;
            }
            // Cancel button was pressed.
            else if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                //do nothing
            }

        }

        private void Button_project_path_Click(object sender, RoutedEventArgs e)
        {
            string projectPath = "./";
            if (!String.IsNullOrEmpty(Textbox_project_path.Text))
            {
                projectPath = Textbox_project_path.Text.Trim();
            }

            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "请选择项目所在的文件夹";
            folderDialog.ShowNewFolderButton = true;
            //folderDialog.RootFolder = Environment.SpecialFolder.ProgramFiles;
            folderDialog.SelectedPath = projectPath;
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderName = folderDialog.SelectedPath;
                if (!String.IsNullOrEmpty(folderName))
                {
                    Textbox_project_path.Text = folderName;                    
                }
            }
            else if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                //do nothing
            }
        }

        private void Button_project_set_path_Click(object sender, RoutedEventArgs e)
        {
            string projectSetPath = "./";
            if (!String.IsNullOrEmpty(Textbox_project_set_path.Text)) 
            {
                projectSetPath = Textbox_project_set_path.Text.Trim();
            }

            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "请选择项目配置所在的文件夹";
            folderDialog.ShowNewFolderButton = true;
            //folderDialog.RootFolder = Environment.SpecialFolder.ProgramFiles;
            folderDialog.SelectedPath = projectSetPath;
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderName = folderDialog.SelectedPath;
                if (!String.IsNullOrEmpty(folderName))
                {
                    Textbox_project_set_path.Text = folderName;
                }
            }
            else if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                //do nothing
            }
        }

        private void Button_build_Click(object sender, RoutedEventArgs e) 
        {
            //初始化状态
            ResetBuildState();
            
            //检查设置
            Setting setting = InitSetting();
            SetBuildSetpState(Label_setp_check, Setp_state.Processing);
            if (!setting.Check_Setting())
            {
                SetBuildSetpState(Label_setp_check, Setp_state.Failed);
                return;
            }
            else 
            {
                SetBuildSetpState(Label_setp_check, Setp_state.Succeeded);
            }

            //备份文件
            SetBuildSetpState(Label_setp_backup, Setp_state.Processing);
            List<string> filesList = FindBackupFilesPath();
            try 
            {                
                FindAndBackupProjectFiles(filesList);
                SetBuildSetpState(Label_setp_backup, Setp_state.Succeeded);
            }
            catch(Exception e1)
            {
                SetBuildSetpState(Label_setp_backup, Setp_state.Failed);
            }

            //拷贝项目配置文件
            try
            {
                CopyProjectSetFiles(filesList);
                SetBuildSetpState(Label_setp_copy, Setp_state.Succeeded);
            }
            catch (Exception e2) 
            {
                SetBuildSetpState(Label_setp_copy, Setp_state.Failed);
            }
            
            //打包
            buildProject();

            //恢复项目配置文件
            try
            {
                ResetProjectSetFiles(filesList);
                SetBuildSetpState(Label_setp_recovery, Setp_state.Succeeded);
            }
            catch (Exception e3) 
            {
                SetBuildSetpState(Label_setp_recovery, Setp_state.Failed);
            }
            
            //结果

            
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string d = e.Data;

            if (!string.IsNullOrEmpty(d))
            {
                Console.WriteLine("Line = " + d);
            }
        }
        
    }
}
