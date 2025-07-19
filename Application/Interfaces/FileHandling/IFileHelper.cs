using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.FileHandling
{
    public interface IFileHelper
    {
        string SaveBase64ToFile(string base64String, string fileName, string subFolder = "");
        string GetFileUrl(string relativePath);
        bool DeleteFile(string relativePath);
        bool IsBase64String(string base64);
    }
}
