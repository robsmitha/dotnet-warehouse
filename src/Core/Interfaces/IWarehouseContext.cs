using Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IWarehouseContext
    {
        DatabaseFacade Database { get; }
        IModel Model { get; }

        DbSet<Catalog> Catalog { get; set; }
        DbSet<DimDate> Dates { get; set; }
        DbSet<Lineage> Lineage { get; set; }

        Task<int> SaveChangesAsync(CancellationToken token = default);
    }
}
