using Application.Common.Settings;
using Application.Interfaces.FileHandling;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.RegularExpressions;

namespace API.Helpers
{
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Hosting;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    namespace Infrastructure.FileHelpers
    {
        public class FileHelpers : IFileHelper
        {
            private readonly FileStorageSettings _settings;
            private readonly IWebHostEnvironment _env;

            public FileHelpers(IOptions<FileStorageSettings> settings, IWebHostEnvironment env)
            {
                _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
                _env = env ?? throw new ArgumentNullException(nameof(env));
            }

            public string SaveBase64ToFile(string base64String, string fileName, string subFolder = "")
            {
                if (string.IsNullOrWhiteSpace(base64String))
                    throw new ArgumentException("Base64 string cannot be empty", nameof(base64String));

                if (string.IsNullOrWhiteSpace(fileName))
                    throw new ArgumentException("File name cannot be empty", nameof(fileName));

                if (!IsBase64String(base64String))
                    throw new FormatException("Invalid Base64 string format");

                try
                {
                    // Remove data URI scheme if present
                    var base64Data = base64String.Contains(",") ?
                        base64String.Split(',')[1] :
                        base64String;

                    byte[] fileBytes = Convert.FromBase64String(base64Data);

                    // Validate file size against configuration
                    if (_settings.MaxFileSize > 0 && fileBytes.Length > _settings.MaxFileSize)
                        throw new InvalidOperationException($"File size exceeds maximum allowed size of {_settings.MaxFileSize} bytes");                    

                    //string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.UploadPath, subFolder);
                   
                    string uploadPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads" , subFolder);
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);
                    
                    // Sanitize filename and generate unique name
                    var sanitizedFileName = SanitizeFileName(fileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
                    var filePath = Path.Combine(uploadPath, uniqueFileName);

                    // Save the file
                    File.WriteAllBytes(filePath, fileBytes);

                    // Return relative path
                    return Path.Combine("uploads", subFolder, uniqueFileName).Replace('\\', '/');
                }
                catch (Exception ex)
                {
                    // Log the error (you should inject ILogger in production)
                    Console.WriteLine($"Error saving file: {ex.Message}");
                    throw; // Re-throw for proper error handling upstream
                }
            }

            public string GetFileUrl(string relativePath)
            {
                if (string.IsNullOrWhiteSpace(relativePath))
                    return string.Empty;

                return $"{_settings.BaseUrl?.TrimEnd('/')}/{_settings.UploadPath?.Trim('/')}/{relativePath.TrimStart('/')}";
            }

            public bool DeleteFile(string relativePath)
            {
                if (string.IsNullOrWhiteSpace(relativePath))
                    return false;

                try
                {
                    var fullPath = Path.Combine(_env.ContentRootPath, _settings.UploadPath, relativePath);

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }

            public  bool IsBase64String(string base64)
            {
                if (string.IsNullOrWhiteSpace(base64))
                    return false;

                // Strip data URI scheme if present
                var data = base64.Contains(",") ? base64.Split(',')[1] : base64;

                // Length must be a multiple of 4
                if (data.Length % 4 != 0)
                    return false;

                // Check valid characters
                return Regex.IsMatch(data, @"^[a-zA-Z0-9\+/]*={0,2}$", RegexOptions.None);
            }

            private static string SanitizeFileName(string fileName)
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries))
                           .Replace(" ", "_");
            }
        }
    }


}
