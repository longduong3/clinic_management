using BELibrary.Entity;
using System;

namespace BELibrary.Core.Entity.Repositories
{
    //this.Configuration.LazyLoadingEnabled = false;
    public interface IRecordRepository : IRepository<Record>
    {
        void Put(DetailRecord elm, Guid id);
    }
}