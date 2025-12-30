using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using VAGSuite;

namespace VAGSuite.Services
{
    /// <summary>
    /// Service for handling project transactions (rollback, forward, project management)
    /// Extracted from frmMain.cs to improve maintainability
    /// </summary>
    public class TransactionService
    {
        private AppSettings _appSettings;
        
        public TransactionService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        #region Rollback

        /// <summary>
        /// Rolls back a transaction entry
        /// </summary>
        public void RollBack(TransactionEntry entry, string currentFile, EDCFileType currentFileType, 
            TransactionLog transactionLog, ProjectLog projectLog)
        {
            int addressToWrite = entry.SymbolAddress;
            Tools.Instance.savedatatobinary(addressToWrite, entry.SymbolLength, entry.DataBefore, 
                currentFile, false, currentFileType);
            
            if (transactionLog != null)
            {
                transactionLog.SetEntryRolledBack(entry.TransactionNumber);
            }
            
            if (!string.IsNullOrEmpty(Tools.Instance.m_CurrentWorkingProject))
            {
                projectLog.WriteLogbookEntry(LogbookEntryType.TransactionRolledback, 
                    Tools.Instance.GetSymbolNameByAddress(entry.SymbolAddress) + " " + 
                    entry.Note + " " + entry.TransactionNumber.ToString());
            }
        }

        #endregion

        #region Roll Forward

        /// <summary>
        /// Rolls forward a transaction entry
        /// </summary>
        public void RollForward(TransactionEntry entry, string currentFile, EDCFileType currentFileType, 
            TransactionLog transactionLog, ProjectLog projectLog)
        {
            int addressToWrite = entry.SymbolAddress;
            Tools.Instance.savedatatobinary(addressToWrite, entry.SymbolLength, entry.DataAfter, 
                currentFile, false, currentFileType);
            
            if (transactionLog != null)
            {
                transactionLog.SetEntryRolledForward(entry.TransactionNumber);
            }
            
            if (!string.IsNullOrEmpty(Tools.Instance.m_CurrentWorkingProject))
            {
                projectLog.WriteLogbookEntry(LogbookEntryType.TransactionRolledforward, 
                    Tools.Instance.GetSymbolNameByAddress(entry.SymbolAddress) + " " + 
                    entry.Note + " " + entry.TransactionNumber.ToString());
            }
        }

        #endregion

        #region Update Rollback Forward Controls

        /// <summary>
        /// Updates the enabled state of rollback/forward controls based on transaction state
        /// </summary>
        public void UpdateRollbackForwardControls(TransactionLog transactionLog, 
            ref bool rollbackEnabled, ref bool rollforwardEnabled, ref bool showTransactionLogEnabled)
        {
            rollbackEnabled = false;
            rollforwardEnabled = false;
            showTransactionLogEnabled = false;
            
            if (transactionLog != null)
            {
                for (int t = transactionLog.TransCollection.Count - 1; t >= 0; t--)
                {
                    if (!showTransactionLogEnabled) showTransactionLogEnabled = true;
                    if (transactionLog.TransCollection[t].IsRolledBack)
                    {
                        rollforwardEnabled = true;
                    }
                    else
                    {
                        rollbackEnabled = true;
                    }
                }
            }
        }

        #endregion

        #region Open Project

        /// <summary>
        /// Opens a project and loads the associated binary file
        /// </summary>
        public void OpenProject(string projectname, string projectFolder, 
            TransactionLog transactionLog, ref string currentFile)
        {
            string projectPath = Path.Combine(projectFolder, projectname);
            if (Directory.Exists(projectPath))
            {
                Tools.Instance.m_CurrentWorkingProject = projectname;
                Tools.Instance.m_ProjectLog.OpenProjectLog(projectFolder + "\\" + projectname);
                
                // Load the binary file that comes with this project
                LoadBinaryForProject(projectname, projectFolder, ref currentFile);
                
                if (!string.IsNullOrEmpty(currentFile))
                {
                    Tools.Instance.m_ProjectTransactionLog = new TransactionLog();
                    if (Tools.Instance.m_ProjectTransactionLog.OpenTransActionLog(projectFolder, projectname))
                    {
                        Tools.Instance.m_ProjectTransactionLog.ReadTransactionFile();
                        if (Tools.Instance.m_ProjectTransactionLog.TransCollection.Count > 2000)
                        {
                            // Trigger purge dialog - this would need UI interaction
                            Console.WriteLine("Transaction log has " + Tools.Instance.m_ProjectTransactionLog.TransCollection.Count + " entries");
                        }
                    }
                }
            }
        }

        #endregion

        #region Close Project

        /// <summary>
        /// Closes the current project
        /// </summary>
        public void CloseProject(ref string currentFile, ref string currentWorkingProject)
        {
            currentFile = string.Empty;
            currentWorkingProject = string.Empty;
        }

        #endregion

        #region Create Project Backup File

        /// <summary>
        /// Creates a backup file for the current project
        /// </summary>
        public void CreateProjectBackupFile(string projectFolder, string projectName, string currentFile, ProjectLog projectLog)
        {
            string backupPath = Path.Combine(projectFolder, projectName);
            backupPath = Path.Combine(backupPath, "Backups");
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }
            
            string filename = Path.Combine(backupPath, 
                Path.GetFileNameWithoutExtension(currentFile) + "-backup-" + 
                DateTime.Now.ToString("MMddyyyyHHmmss") + ".BIN");
            
            File.Copy(currentFile, filename);
            
            if (!string.IsNullOrEmpty(projectName))
            {
                projectLog.WriteLogbookEntry(LogbookEntryType.BackupfileCreated, filename);
            }
        }

        #endregion

        #region Load Binary For Project

        /// <summary>
        /// Loads the binary file associated with a project
        /// </summary>
        public void LoadBinaryForProject(string projectname, string projectFolder, ref string currentFile)
        {
            string projectFile = Path.Combine(projectFolder, projectname);
            projectFile = Path.Combine(projectFile, "projectproperties.xml");
            if (File.Exists(projectFile))
            {
                DataTable projectprops = new DataTable("T5PROJECT");
                projectprops.Columns.Add("CARMAKE");
                projectprops.Columns.Add("CARMODEL");
                projectprops.Columns.Add("CARMY");
                projectprops.Columns.Add("CARVIN");
                projectprops.Columns.Add("NAME");
                projectprops.Columns.Add("BINFILE");
                projectprops.Columns.Add("VERSION");
                projectprops.ReadXml(projectFile);
                
                if (projectprops.Rows.Count > 0)
                {
                    currentFile = projectprops.Rows[0]["BINFILE"].ToString();
                }
            }
        }

        #endregion

        #region Get Binary For Project

        /// <summary>
        /// Gets the binary file path for a project
        /// </summary>
        public string GetBinaryForProject(string projectname, string projectFolder, string currentFile)
        {
            string retval = currentFile;
            string projectFile = Path.Combine(projectFolder, projectname);
            projectFile = Path.Combine(projectFile, "projectproperties.xml");
            
            if (File.Exists(projectFile))
            {
                DataTable projectprops = new DataTable("T5PROJECT");
                projectprops.Columns.Add("CARMAKE");
                projectprops.Columns.Add("CARMODEL");
                projectprops.Columns.Add("CARMY");
                projectprops.Columns.Add("CARVIN");
                projectprops.Columns.Add("NAME");
                projectprops.Columns.Add("BINFILE");
                projectprops.Columns.Add("VERSION");
                projectprops.ReadXml(projectFile);
                
                if (projectprops.Rows.Count > 0)
                {
                    retval = projectprops.Rows[0]["BINFILE"].ToString();
                }
            }
            
            return retval;
        }

        #endregion

        #region Get Backup Older Than DateTime

        /// <summary>
        /// Finds the backup file older than a specified datetime
        /// </summary>
        public string GetBackupOlderThanDateTime(string project, DateTime mileDT, string projectFolder, string currentFile)
        {
            string retval = currentFile;
            string backupPath = Path.Combine(projectFolder, project);
            backupPath = Path.Combine(backupPath, "Backups");
            DateTime MaxDateTime = DateTime.MinValue;
            string foundBackupfile = string.Empty;
            
            if (Directory.Exists(backupPath))
            {
                string[] backupfiles = Directory.GetFiles(backupPath, "*.bin");
                foreach (string backupfile in backupfiles)
                {
                    FileInfo fi = new FileInfo(backupfile);
                    if (fi.LastAccessTime > MaxDateTime && fi.LastAccessTime <= mileDT)
                    {
                        MaxDateTime = fi.LastAccessTime;
                        foundBackupfile = backupfile;
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(foundBackupfile))
            {
                retval = foundBackupfile;
            }
            
            return retval;
        }

        #endregion

        #region Roll Forward On File

        /// <summary>
        /// Applies a transaction to a specific file
        /// </summary>
        public void RollForwardOnFile(string file2Rollback, TransactionEntry entry, EDCFileType currentFileType)
        {
            FileInfo fi = new FileInfo(file2Rollback);
            int addressToWrite = entry.SymbolAddress;
            while (addressToWrite > fi.Length) addressToWrite -= (int)fi.Length;
            
            Tools.Instance.savedatatobinary(addressToWrite, entry.SymbolLength, entry.DataAfter, 
                file2Rollback, false, currentFileType);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the number of backups for a project
        /// </summary>
        public int GetNumberOfBackups(string project, string projectFolder)
        {
            int retval = 0;
            string dirname = Path.Combine(projectFolder, project);
            dirname = Path.Combine(dirname, "Backups");
            if (!Directory.Exists(dirname)) Directory.CreateDirectory(dirname);
            
            string[] backupfiles = Directory.GetFiles(dirname, "*.bin");
            retval = backupfiles.Length;
            
            return retval;
        }

        /// <summary>
        /// Gets the number of transactions for a project
        /// </summary>
        public int GetNumberOfTransactions(string project, string projectFolder)
        {
            int retval = 0;
            string filename = Path.Combine(projectFolder, project);
            filename = Path.Combine(filename, "TransActionLogV2.ttl");
            
            if (File.Exists(filename))
            {
                TransactionLog translog = new TransactionLog();
                translog.OpenTransActionLog(projectFolder, project);
                translog.ReadTransactionFile();
                retval = translog.TransCollection.Count;
            }
            
            return retval;
        }

        /// <summary>
        /// Gets the last access time for a file
        /// </summary>
        public DateTime GetLastAccessTime(string filename)
        {
            DateTime retval = DateTime.MinValue;
            
            if (File.Exists(filename))
            {
                FileInfo fi = new FileInfo(filename);
                retval = fi.LastAccessTime;
            }
            
            return retval;
        }

        #endregion
    }
}
