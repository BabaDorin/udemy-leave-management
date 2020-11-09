﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Contracts
{
    interface IRepositoryBase<T> where T : class
    {
        // Interface which implements CRUD
        // Every domain class should offer CRUD operations

        ICollection<T> FindAll();
        T FindById(int id);
        bool Create(T entity);
        bool Update(T entity);
        bool Delete(T entity);
        bool Save();
    }
}