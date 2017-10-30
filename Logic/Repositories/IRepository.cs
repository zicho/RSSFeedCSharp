using System;
using System.Collections.Generic;
using Logic.Entities;

namespace Logic.Repositories
{
    public interface IRepository<T> where T : IEntity
    {
        void Save();
        void Update(T item);
        void Delete(T item);
        T Add(T item);
        T Find(Guid id);
        IEnumerable<T> GetAll();
    }
}