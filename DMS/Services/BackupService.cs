using DMS.Data;
using DMS.Models;
using DMS.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using System.IO.Compression;

namespace DMS.Services
{
    public class BackupService : IBackupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly string _backupDirectory;

        public BackupService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
            _backupDirectory = Path.Combine(_environment.ContentRootPath, "Backups");
            
            // Tạo thư mục Backups nếu chưa tồn tại
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, BackupRecord? BackupRecord)> CreateBackupAsync(string userId, string? description = null)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupName = $"Backup_{timestamp}";
                
                // 1. Backup Database
                var dbBackupPath = await BackupDatabaseAsync(backupName);
                if (string.IsNullOrEmpty(dbBackupPath))
                {
                    // Lấy thông tin lỗi chi tiết hơn
                    var connectionString = _configuration.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        return (false, "Không tìm thấy connection string", null);
                    }
                    
                    var builder = new SqlConnectionStringBuilder(connectionString);
                    var databaseName = builder.InitialCatalog;
                    
                    var errorMsg = $"Không thể backup database '{databaseName}'. " +
                                  $"Có thể do:\n" +
                                  $"1. SQL Server service account không có quyền ghi vào thư mục\n" +
                                  $"2. User không có quyền BACKUP DATABASE\n" +
                                  $"3. Đường dẫn backup không hợp lệ\n\n" +
                                  $"Giải pháp:\n" +
                                  $"1. Chạy SQL Server với quyền Administrator\n" +
                                  $"2. Cấp quyền BACKUP DATABASE cho user\n" +
                                  $"3. Kiểm tra Debug Output để xem lỗi chi tiết";
                    
                    return (false, errorMsg, null);
                }

                var dbFileInfo = new FileInfo(dbBackupPath);
                var databaseSize = dbFileInfo.Length;

                // 2. Backup Files (documents)
                var filesBackupPath = await BackupFilesAsync(backupName);
                var filesSize = 0L;
                if (!string.IsNullOrEmpty(filesBackupPath))
                {
                    var filesFileInfo = new FileInfo(filesBackupPath);
                    filesSize = filesFileInfo.Length;
                }

                // 3. Tạo BackupRecord
                var backupRecord = new BackupRecord
                {
                    BackupName = backupName,
                    DatabaseBackupPath = dbBackupPath,
                    FilesBackupPath = filesBackupPath,
                    DatabaseSize = databaseSize,
                    FilesSize = filesSize,
                    Description = description,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                _context.BackupRecords.Add(backupRecord);
                await _context.SaveChangesAsync();

                return (true, null, backupRecord);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo backup: {ex.Message}", null);
            }
        }

        private async Task<string?> BackupDatabaseAsync(string backupName)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Diagnostics.Debug.WriteLine("Connection string is null or empty");
                    return null;
                }

                // Parse connection string để lấy database name
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                var serverName = builder.DataSource;

                if (string.IsNullOrEmpty(databaseName))
                {
                    System.Diagnostics.Debug.WriteLine("Database name is null or empty");
                    return null;
                }

                // Tạo tên file backup
                var fileName = $"{backupName}_Database.bak";
                
                // Lấy đường dẫn backup mặc định của SQL Server
                string sqlServerBackupPath;
                string finalBackupPath = Path.Combine(_backupDirectory, fileName);
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Lấy đường dẫn backup mặc định của SQL Server bằng cách query
                    try
                    {
                        using (var cmd = new SqlCommand("SELECT SERVERPROPERTY('InstanceDefaultBackupPath')", connection))
                        {
                            var result = await cmd.ExecuteScalarAsync();
                            if (result != null && !string.IsNullOrEmpty(result.ToString()))
                            {
                                sqlServerBackupPath = result.ToString()!;
                            }
                            else
                            {
                                // Fallback: Thử lấy từ registry hoặc dùng đường dẫn mặc định
                                using (var cmd2 = new SqlCommand("EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\\Microsoft\\MSSQLServer\\MSSQLServer', N'BackupDirectory'", connection))
                                {
                                    var regResult = await cmd2.ExecuteScalarAsync();
                                    if (regResult != null && !string.IsNullOrEmpty(regResult.ToString()))
                                    {
                                        sqlServerBackupPath = regResult.ToString()!;
                                    }
                                    else
                                    {
                                        // Dùng đường dẫn mặc định cho SQL Server Express
                                        sqlServerBackupPath = "C:\\Program Files\\Microsoft SQL Server\\MSSQL15.MSSQLSERVER\\MSSQL\\Backup";
                                        // Hoặc thử đường dẫn cho SQLEXPRESS
                                        if (!Directory.Exists(sqlServerBackupPath))
                                        {
                                            sqlServerBackupPath = "C:\\Program Files\\Microsoft SQL Server\\MSSQL15.SQLEXPRESS\\MSSQL\\Backup";
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Nếu không lấy được, dùng đường dẫn mặc định
                        sqlServerBackupPath = "C:\\Program Files\\Microsoft SQL Server\\MSSQL15.SQLEXPRESS\\MSSQL\\Backup";
                        if (!Directory.Exists(sqlServerBackupPath))
                        {
                            sqlServerBackupPath = "C:\\Program Files\\Microsoft SQL Server\\MSSQL15.MSSQLSERVER\\MSSQL\\Backup";
                        }
                    }
                    
                    // Tạo đường dẫn backup trên SQL Server
                    var tempBackupPath = Path.Combine(sqlServerBackupPath, fileName);
                    var sqlBackupPath = tempBackupPath.Replace("'", "''");

                    System.Diagnostics.Debug.WriteLine($"SQL Server backup path: {sqlServerBackupPath}");
                    System.Diagnostics.Debug.WriteLine($"Attempting to backup database '{databaseName}' to '{tempBackupPath}'");

                    // SQL command để backup database vào thư mục mặc định của SQL Server
                    var backupQuery = $@"
                        BACKUP DATABASE [{databaseName}]
                        TO DISK = N'{sqlBackupPath}'
                        WITH FORMAT, INIT, NAME = N'{backupName}', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                    using (var command = new SqlCommand(backupQuery, connection))
                    {
                        command.CommandTimeout = 300; // 5 minutes timeout
                        
                        try
                        {
                            await command.ExecuteNonQueryAsync();
                            System.Diagnostics.Debug.WriteLine("Backup command executed successfully");
                        }
                        catch (SqlException sqlEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"SQL Error executing backup: {sqlEx.Message}");
                            System.Diagnostics.Debug.WriteLine($"SQL Error Number: {sqlEx.Number}");
                            
                            // Nếu lỗi do quyền, thử backup trực tiếp vào thư mục project
                            if (sqlEx.Number == 3201 || sqlEx.Number == 3013) // Permission denied or path not found
                            {
                                System.Diagnostics.Debug.WriteLine("Trying to backup directly to project directory...");
                                return await BackupToProjectDirectory(databaseName, backupName, fileName, connectionString);
                            }
                            throw;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error executing backup command: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                            throw;
                        }
                    }
                    
                    // Query để lấy đường dẫn file backup thực tế từ SQL Server
                    string? actualBackupPath = null;
                    try
                    {
                        using (var cmd = new SqlCommand($@"
                            SELECT TOP 1 physical_device_name 
                            FROM msdb.dbo.backupmediafamily 
                            WHERE media_set_id = (
                                SELECT TOP 1 media_set_id 
                                FROM msdb.dbo.backupset 
                                WHERE database_name = '{databaseName}' 
                                AND backup_finish_date IS NOT NULL
                                ORDER BY backup_finish_date DESC
                            )", connection))
                        {
                            var result = await cmd.ExecuteScalarAsync();
                            if (result != null)
                            {
                                actualBackupPath = result.ToString();
                                System.Diagnostics.Debug.WriteLine($"Actual backup file path from SQL Server: {actualBackupPath}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error querying backup file path: {ex.Message}");
                    }
                    
                    // Đợi một chút để đảm bảo file đã được ghi
                    await Task.Delay(3000);

                    // Tìm file backup - thử đường dẫn thực tế từ SQL Server trước
                    string? sourceBackupPath = null;
                    if (!string.IsNullOrEmpty(actualBackupPath) && File.Exists(actualBackupPath))
                    {
                        sourceBackupPath = actualBackupPath;
                        System.Diagnostics.Debug.WriteLine($"Found backup file at actual path: {actualBackupPath}");
                    }
                    else if (File.Exists(tempBackupPath))
                    {
                        sourceBackupPath = tempBackupPath;
                        System.Diagnostics.Debug.WriteLine($"Found backup file at expected path: {tempBackupPath}");
                    }
                    else
                    {
                        // Tìm file trong thư mục backup
                        System.Diagnostics.Debug.WriteLine($"Backup file not found at expected location. Searching in directory: {sqlServerBackupPath}");
                        if (Directory.Exists(sqlServerBackupPath))
                        {
                            var files = Directory.GetFiles(sqlServerBackupPath, $"{backupName}*.bak");
                            if (files.Length > 0)
                            {
                                sourceBackupPath = files[0];
                                System.Diagnostics.Debug.WriteLine($"Found backup file: {sourceBackupPath}");
                            }
                        }
                    }

                    // Copy file từ SQL Server backup directory về project directory
                    if (!string.IsNullOrEmpty(sourceBackupPath) && File.Exists(sourceBackupPath))
                    {
                        try
                        {
                            // Đảm bảo thư mục đích tồn tại
                            if (!Directory.Exists(_backupDirectory))
                            {
                                Directory.CreateDirectory(_backupDirectory);
                            }
                            
                            File.Copy(sourceBackupPath, finalBackupPath, true);
                            System.Diagnostics.Debug.WriteLine($"Backup file copied to: {finalBackupPath}");
                            
                            // Xóa file tạm trên SQL Server (tùy chọn)
                            // File.Delete(sourceBackupPath);
                            
                            return finalBackupPath;
                        }
                        catch (Exception copyEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error copying backup file: {copyEx.Message}");
                            // Nếu không copy được, vẫn trả về đường dẫn trên SQL Server
                            return sourceBackupPath;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Backup file not found. Expected: {tempBackupPath}, Actual: {actualBackupPath ?? "null"}");
                        // Thử backup trực tiếp vào thư mục project như fallback
                        System.Diagnostics.Debug.WriteLine("Trying fallback: backup directly to project directory...");
                        return await BackupToProjectDirectory(databaseName, backupName, fileName, connectionString);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log SQL-specific errors
                System.Diagnostics.Debug.WriteLine($"SQL Error backing up database: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"SQL Error Number: {sqlEx.Number}");
                System.Diagnostics.Debug.WriteLine($"SQL State: {sqlEx.State}");
                
                // Thử backup trực tiếp vào thư mục project
                try
                {
                    var builder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("DefaultConnection")!);
                    var databaseName = builder.InitialCatalog;
                    var fileName = $"{backupName}_Database.bak";
                    return await BackupToProjectDirectory(databaseName, backupName, fileName, _configuration.GetConnectionString("DefaultConnection")!);
                }
                catch
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error backing up database: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        private async Task<string?> BackupToProjectDirectory(string databaseName, string backupName, string fileName, string connectionString)
        {
            try
            {
                var finalBackupPath = Path.Combine(_backupDirectory, fileName);
                var sqlBackupPath = finalBackupPath.Replace("'", "''").Replace("\\", "\\\\");

                System.Diagnostics.Debug.WriteLine($"Attempting direct backup to project directory: {finalBackupPath}");

                var backupQuery = $@"
                    BACKUP DATABASE [{databaseName}]
                    TO DISK = N'{sqlBackupPath}'
                    WITH FORMAT, INIT, NAME = N'{backupName}', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(backupQuery, connection))
                    {
                        command.CommandTimeout = 300;
                        await command.ExecuteNonQueryAsync();
                    }
                }

                await Task.Delay(1000);

                if (File.Exists(finalBackupPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Direct backup successful: {finalBackupPath}");
                    return finalBackupPath;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in BackupToProjectDirectory: {ex.Message}");
                return null;
            }
        }

        private async Task<string?> BackupFilesAsync(string backupName)
        {
            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    return null; // Không có files để backup
                }

                var zipFileName = $"{backupName}_Files.zip";
                var zipPath = Path.Combine(_backupDirectory, zipFileName);

                // Tạo ZIP file từ thư mục uploads
                await Task.Run(() =>
                {
                    if (File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                    }

                    ZipFile.CreateFromDirectory(uploadsPath, zipPath, CompressionLevel.Fastest, false);
                });

                if (File.Exists(zipPath))
                {
                    return zipPath;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error backing up files: {ex.Message}");
                return null;
            }
        }

        public async Task<List<BackupRecord>> GetAllBackupsAsync()
        {
            return await _context.BackupRecords
                .Include(b => b.CreatedByUser)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task<BackupRecord?> GetBackupByIdAsync(int id)
        {
            return await _context.BackupRecords
                .Include(b => b.CreatedByUser)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> DeleteBackupAsync(int id)
        {
            try
            {
                var backup = await _context.BackupRecords.FindAsync(id);
                if (backup == null)
                {
                    return false;
                }

                // Xóa file backup database
                if (!string.IsNullOrEmpty(backup.DatabaseBackupPath) && File.Exists(backup.DatabaseBackupPath))
                {
                    try
                    {
                        File.Delete(backup.DatabaseBackupPath);
                    }
                    catch { }
                }

                // Xóa file backup files
                if (!string.IsNullOrEmpty(backup.FilesBackupPath) && File.Exists(backup.FilesBackupPath))
                {
                    try
                    {
                        File.Delete(backup.FilesBackupPath);
                    }
                    catch { }
                }

                _context.BackupRecords.Remove(backup);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BackupExistsAsync(int id)
        {
            return await _context.BackupRecords.AnyAsync(b => b.Id == id);
        }

        public async Task<(bool Success, string? ErrorMessage)> RestoreBackupAsync(int backupId, string userId, string? notes = null)
        {
            try
            {
                var backup = await GetBackupByIdAsync(backupId);
                if (backup == null)
                {
                    return (false, "Không tìm thấy backup");
                }

                if (!File.Exists(backup.DatabaseBackupPath))
                {
                    return (false, "File backup database không tồn tại");
                }

                // 1. Restore Database
                var restoreSuccess = await RestoreDatabaseAsync(backup.DatabaseBackupPath);
                if (!restoreSuccess)
                {
                    return (false, "Không thể restore database");
                }

                // 2. Restore Files (nếu có)
                if (!string.IsNullOrEmpty(backup.FilesBackupPath) && File.Exists(backup.FilesBackupPath))
                {
                    await RestoreFilesAsync(backup.FilesBackupPath);
                }

                // 3. Cập nhật BackupRecord
                backup.IsRestored = true;
                backup.RestoredDate = DateTime.Now;
                backup.RestoredBy = userId;
                backup.RestoreNotes = notes;

                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi restore: {ex.Message}");
            }
        }

        private async Task<bool> RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    return false;
                }

                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                // SQL command để restore database
                // Cần set database to SINGLE_USER mode trước khi restore
                var restoreQuery = $@"
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [{databaseName}]
                    FROM DISK = '{backupPath}'
                    WITH REPLACE, RECOVERY;
                    ALTER DATABASE [{databaseName}] SET MULTI_USER;";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(restoreQuery, connection))
                    {
                        command.CommandTimeout = 600; // 10 minutes timeout
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring database: {ex.Message}");
                return false;
            }
        }

        private async Task RestoreFilesAsync(string zipPath)
        {
            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                
                // Xóa thư mục uploads cũ (backup trước)
                if (Directory.Exists(uploadsPath))
                {
                    Directory.Delete(uploadsPath, true);
                }
                
                // Tạo lại thư mục
                Directory.CreateDirectory(uploadsPath);

                // Extract ZIP file
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(zipPath, uploadsPath);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring files: {ex.Message}");
            }
        }
    }
}

