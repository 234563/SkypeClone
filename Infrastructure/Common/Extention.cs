using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common
{
    public static class DBExtentions 
    {
        public static T SafeGet<T>(this DataRow row, string columnName, T defaultValue = default)
        {
            try
            {
                var value = row[columnName];
                return value == DBNull.Value ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
