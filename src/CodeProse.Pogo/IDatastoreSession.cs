using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProse.Pogo
{
    public interface IDatastoreSession : IDisposable
    {
        T Load<T>(object id) where T : new();

        IList<T> Load<T>(object[] ids) where T : new();

        void Store(object entity);

        IPogoQueryable<T> Query<T>() where T : new();

        void SaveChanges();
    }
}