using System;
namespace CodeProse.Pogo
{
    public interface IDatastore
    {
        IDatastoreSession OpenSession();
    }
}
