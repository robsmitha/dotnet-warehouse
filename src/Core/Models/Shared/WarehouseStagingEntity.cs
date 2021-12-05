using Core.Customization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Shared
{
    public abstract class WarehouseStagingEntity
    {
        public abstract WarehouseEntity MapToEntity(int lineageKey);
    }
}
