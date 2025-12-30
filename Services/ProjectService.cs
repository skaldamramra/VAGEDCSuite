using System;
using System.Collections.Generic;
using System.IO;
using System.Data;

namespace VAGSuite.Services
{
    public class ProjectService
    {
        private AppSettings _appSettings;

        public ProjectService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// Opens a project and loads the binary file
        /// </summary>
        public void OpenProject(string projectname, ref string currentFile, ref TransactionLog transactionLog, 
            ref ProjectLog projectLog, ref bool hasTransactionLog)
        {
            if (Directory.Exists(_appSettings.ProjectFolder + "\\" + projectname))
            {
                Tools.Instance.m_CurrentWorkingProject = projectname;
                projectLog.OpenProjectLog(_appSettings.ProjectFolder + "\\" + projectname);
                
                // Load the binary file that comes with this project
                LoadBinaryForProject(projectname, ref currentFile);
                
                if (currentFile != string.Empty)
                {
                    transactionLog = new TransactionLog();
                    hasTransactionLog = true;
                    if (transactionLog.OpenTransActionLog(_appSettings.ProjectFolder, projectname))
                    {
                        transactionLog.ReadTransactionFile();
                        if (transactionLog.TransCollection.Count > 2000)
                        {
                            // Purge needed - handled by caller
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Closes the current project
        /// </summary>
        public void CloseProject(ref string currentFile, ref string workingProject)
        {
            workingProject = string.Empty;
            currentFile = string.Empty;
        }

        /// <summary>
        /// Gets the binary file path for a project
        /// </summary>
        public string GetBinaryForProject(string projectname, string currentFile)
        {
            string binfile = string.Empty;
            try
            {
                DataTable projectprops = new DataTable("T5PROJECT");
                projectprops.ReadXml(_appSettings.ProjectFolder + "\\" + projectname + "\\projectproperties.xml");
                if (projectprops.Rows.Count > 0)
                {
                    binfile = projectprops.Rows[0]["BINFILE"].ToString();
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("GetBinaryForProject error: " + E.Message);
            }
            return binfile;
        }

        /// <summary>
        /// Loads the binary file for a project
        /// </summary>
        public void LoadBinaryForProject(string projectname, ref string currentFile)
        {
            currentFile = GetBinaryForProject(projectname, currentFile);
        }

        /// <summary>
        /// Gets a backup file older than the specified datetime
        /// </summary>
        public string GetBackupOlderThanDateTime(string project, DateTime mileDT, string currentFile)
        {
            string retval = string.Empty;
            try
            {
                string[] files = Directory.GetFiles(_appSettings.ProjectFolder + "\\" + project + "\\Backups", "*.BIN");
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastAccessTime < mileDT)
                    {
                        retval = file;
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("GetBackupOlderThanDateTime error: " + E.Message);
            }
            return retval;
        }

        /// <summary>
        /// Gets the number of backups for a project
        /// </summary>
        public int GetNumberOfBackups(string project)
        {
            int retval = 0;
            try
            {
                if (Directory.Exists(_appSettings.ProjectFolder + "\\" + project + "\\Backups"))
                {
                    string[] files = Directory.GetFiles(_appSettings.ProjectFolder + "\\" + project + "\\Backups", "*.BIN");
                    retval = files.Length;
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("GetNumberOfBackups error: " + E.Message);
            }
            return retval;
        }

        /// <summary>
        /// Gets the number of transactions for a project
        /// </summary>
        public int GetNumberOfTransactions(string project)
        {
            int retval = 0;
            try
            {
                string[] files = Directory.GetFiles(_appSettings.ProjectFolder + "\\" + project, "*.xml");
                foreach (string file in files)
                {
                    if (file.Contains("transactionlog"))
                    {
                        retval++;
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("GetNumberOfTransactions error: " + E.Message);
            }
            return retval;
        }

        /// <summary>
        /// Gets the last access time for a file
        /// </summary>
        public DateTime GetLastAccessTime(string filename)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                return fi.LastAccessTime;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Creates a project backup file
        /// </summary>
        public void CreateProjectBackupFile(string projectFolder, string workingProject, string currentFile, ProjectLog projectLog)
        {
            try
            {
                if (workingProject != "" && currentFile != "")
                {
                    if (!Directory.Exists(projectFolder + "\\" + workingProject + "\\Backups")) 
                        Directory.CreateDirectory(projectFolder + "\\" + workingProject + "\\Backups");
                    
                    string filename = projectFolder + "\\" + workingProject + "\\Backups\\" + Path.GetFileNameWithoutExtension(currentFile) + "-backup-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".BIN";
                    File.Copy(currentFile, filename);
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("CreateProjectBackupFile error: " + E.Message);
            }
        }

        /// <summary>
        /// Gets project properties as a DataTable
        /// </summary>
        public DataTable GetProjectProperties(string project)
        {
            DataTable projectprops = new DataTable("T5PROJECT");
            try
            {
                string xmlPath = _appSettings.ProjectFolder + "\\" + project + "\\projectproperties.xml";
                if (File.Exists(xmlPath))
                {
                    projectprops.ReadXml(xmlPath);
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("GetProjectProperties error: " + E.Message);
            }
            return projectprops;
        }

        /// <summary>
        /// Gets the number of codeblocks in the current file
        /// </summary>
        public int DetermineNumberOfCodeblocks(SymbolCollection symbols)
        {
            List<int> blockIds = new List<int>();
            foreach (SymbolHelper sh in symbols)
            {
                if (!blockIds.Contains(sh.CodeBlock) && sh.CodeBlock != 0) blockIds.Add(sh.CodeBlock);
            }
            return blockIds.Count;
        }

        /// <summary>
        /// Gets valid projects from the project folder
        /// </summary>
        public DataTable GetValidProjects()
        {
            DataTable ValidProjects = new DataTable();
            ValidProjects.Columns.Add("Projectname");
            ValidProjects.Columns.Add("NumberBackups");
            ValidProjects.Columns.Add("NumberTransactions");
            ValidProjects.Columns.Add("DateTimeModified");
            ValidProjects.Columns.Add("Version");
            
            try
            {
                if (!Directory.Exists(_appSettings.ProjectFolder)) 
                    Directory.CreateDirectory(_appSettings.ProjectFolder);
                
                string[] projects = Directory.GetDirectories(_appSettings.ProjectFolder);
                foreach (string project in projects)
                {
                    string[] projectfiles = Directory.GetFiles(project, "projectproperties.xml");
                    if (projectfiles.Length > 0)
                    {
                        DataTable projectprops = new DataTable("T5PROJECT");
                        projectprops.ReadXml((string)projectfiles.GetValue(0));
                        if (projectprops.Rows.Count > 0)
                        {
                            string projectName = projectprops.Rows[0]["NAME"].ToString();
                            ValidProjects.Rows.Add(projectName, 
                                GetNumberOfBackups(projectName), 
                                GetNumberOfTransactions(projectName), 
                                GetLastAccessTime(projectprops.Rows[0]["BINFILE"].ToString()), 
                                projectprops.Rows[0]["VERSION"].ToString());
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("GetValidProjects error: " + E.Message);
            }
            return ValidProjects;
        }

        /// <summary>
        /// Makes a safe directory name from project properties
        /// </summary>
        public string MakeDirName(string dirname)
        {
            string retval = dirname;
            retval = retval.Replace(@"\", "");
            retval = retval.Replace(@"/", "");
            retval = retval.Replace(@":", "");
            retval = retval.Replace(@"*", "");
            retval = retval.Replace(@"?", "");
            retval = retval.Replace(@">", "");
            retval = retval.Replace(@"<", "");
            retval = retval.Replace(@"|", "");
            return retval;
        }
    }
}