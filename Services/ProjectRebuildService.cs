using System;
using System.IO;
using System.Windows.Forms;
using VAGSuite;

namespace VAGSuite.Services
{
    /// <summary>
    /// Service for handling project file rebuild operations.
    /// Extracted from frmMain to separate rebuild logic from UI concerns.
    /// </summary>
    public class ProjectRebuildService
    {
        private readonly AppSettings _appSettings;
        private readonly TransactionService _transactionService;

        public ProjectRebuildService(AppSettings appSettings, TransactionService transactionService)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }

        /// <summary>
        /// Result of a rebuild operation
        /// </summary>
        public class RebuildResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string RebuiltFilePath { get; set; }
        }

        /// <summary>
        /// Rebuilds a project file by applying transactions up to a specified datetime.
        /// </summary>
        /// <param name="selectedDateTime">The cutoff datetime for transactions</param>
        /// <param name="useAsNewProjectFile">If true, replace the current project file; otherwise prompt for destination</param>
        /// <param name="currentFile">Path to the current binary file</param>
        /// <param name="currentWorkingProject">Name of the current working project</param>
        /// <param name="projectLog">Project log for writing entries</param>
        /// <param name="transactionLog">Transaction log containing all transactions</param>
        /// <returns>RebuildResult with success status and details</returns>
        public RebuildResult RebuildFile(DateTime selectedDateTime, bool useAsNewProjectFile,
            string currentFile, string currentWorkingProject, ProjectLog projectLog, TransactionLog transactionLog)
        {
            var result = new RebuildResult
            {
                Success = false,
                Message = ""
            };

            try
            {
                // Get the backup file older than selected datetime
                string file2Process = _transactionService.GetBackupOlderThanDateTime(
                    currentWorkingProject, selectedDateTime, _appSettings.ProjectFolder, currentFile);

                if (string.IsNullOrEmpty(file2Process))
                {
                    result.Message = "No backup file found for the selected datetime";
                    return result;
                }

                // Create temporary rebuild file
                string tempRebuildFile = Path.Combine(_appSettings.ProjectFolder, 
                    currentWorkingProject + "rebuild.bin");

                if (File.Exists(tempRebuildFile))
                {
                    File.Delete(tempRebuildFile);
                }

                // Create backup of current state
                CreateBackupBeforeRebuild(currentWorkingProject, currentFile);

                // Copy the backup file to rebuild location
                File.Copy(file2Process, tempRebuildFile);

                // Apply all transactions newer than the backup file and older than selected datetime
                FileInfo fi = new FileInfo(file2Process);
                foreach (TransactionEntry te in transactionLog.TransCollection)
                {
                    if (te.EntryDateTime >= fi.LastAccessTime && te.EntryDateTime <= selectedDateTime)
                    {
                        // Apply this change
                        RollForwardOnFile(tempRebuildFile, te);
                    }
                }

                // Handle the output file
                if (useAsNewProjectFile)
                {
                    // Replace current file
                    File.Delete(currentFile);
                    File.Copy(tempRebuildFile, currentFile);
                    File.Delete(tempRebuildFile);
                    result.RebuiltFilePath = currentFile;
                }
                else
                {
                    // Prompt for destination file
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Title = "Save rebuild file as...";
                        sfd.Filter = "Binary files|*.bin";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                            File.Copy(tempRebuildFile, sfd.FileName);
                            File.Delete(tempRebuildFile);
                            result.RebuiltFilePath = sfd.FileName;
                        }
                    }
                }

                // Log the rebuild operation
                if (!string.IsNullOrEmpty(currentWorkingProject))
                {
                    projectLog.WriteLogbookEntry(LogbookEntryType.ProjectFileRecreated,
                        "Reconstruct upto " + selectedDateTime.ToString("dd/MM/yyyy") + " selected file " + file2Process);
                }

                result.Success = true;
                result.Message = "Rebuild completed successfully";
            }
            catch (Exception ex)
            {
                result.Message = "Rebuild failed: " + ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Creates a backup before performing rebuild operations.
        /// </summary>
        private void CreateBackupBeforeRebuild(string currentWorkingProject, string currentFile)
        {
            if (!string.IsNullOrEmpty(currentWorkingProject))
            {
                string backupPath = Path.Combine(Path.Combine(_appSettings.ProjectFolder,
                    currentWorkingProject), "Backups");
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                string filename = Path.Combine(backupPath,
                    Path.GetFileNameWithoutExtension(currentFile) + "-backup-" + 
                    DateTime.Now.ToString("MMddyyyyHHmmss") + ".BIN");
                File.Copy(currentFile, filename);
            }
        }

        /// <summary>
        /// Applies a single transaction to a file.
        /// </summary>
        private void RollForwardOnFile(string file2Rollback, TransactionEntry entry)
        {
            FileInfo fi = new FileInfo(file2Rollback);
            int addressToWrite = entry.SymbolAddress;
            while (addressToWrite > fi.Length) addressToWrite -= (int)fi.Length;

            Tools.Instance.savedatatobinary(addressToWrite, entry.SymbolLength, entry.DataAfter,
                file2Rollback, false, Tools.Instance.m_currentFileType);
        }
    }
}