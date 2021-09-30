using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IWarehouseAction
    {
        Task StageAsync(DateTime loadDate, DateTime lastLoadDate);
        Task LoadAsync(string tableName, int lineageKey);
    }
}
