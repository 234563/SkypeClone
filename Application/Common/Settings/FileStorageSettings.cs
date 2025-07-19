using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Settings
{
    public class FileStorageSettings
    {
        public string UploadPath { get; set; }
        public int MaxFileSize { get; set; }
        public string BaseUrl { get; set; }
    }
}
