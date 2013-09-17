using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Datastore.v1beta1;
using CodeProse.Pogo.Mapping;
using Google.Apis.Datastore.v1beta1.Data;

namespace CodeProse.Pogo
{
    internal class DatastoreSession : IDatastoreSession
    {
        private DatastoreService _datastore;
        private string _datasetId;
        private HashSet<object> _entitiesInContext;

        internal DatastoreSession(DatastoreService datastore, string datasetId)
        {
            _datastore = datastore;
            _datasetId = datasetId;
            _entitiesInContext = new HashSet<object>();
        }

        internal DatastoreService Service
        {
            get { return _datastore; }
        }

        internal string DatasetId
        {
            get { return _datasetId; }
        }

        public void Store(object entity)
        {
            _entitiesInContext.Add(entity);
        }

        public void Store(IEnumerable<object> entities)
        {
            entities.ToList().ForEach(t => _entitiesInContext.Add(t));
        }

        public void SaveChanges()
        {
            var request = new BlindWriteRequest();

            request.Mutation = new Mutation();
            request.Mutation.Upsert = _entitiesInContext.Select(t => ObjectEntityMapper.ConvertObjectToEntity(t)).ToList();

            var response = _datastore.Datasets.BlindWrite(request, _datasetId).Fetch();

            //_entitiesInContext = new List<Entity>();
        }

        public IList<T> Load<T>(object[] ids) where T : new()
        {
            var request = new LookupRequest();
            request.Keys = new List<Key>();

            foreach (var id in ids)
            {
                var key = new Key();
                key.Path = new List<KeyPathElement>();
                key.Path.Add(new KeyPathElement { Kind = typeof(T).FullName });
                if (id is string)
                {
                    key.Path[0].Name = id.ToString();
                }
                else
                {
                    key.Path[0].Id = id.ToString(); // TODO this assumes that the ID is integral (verify?)
                }

                request.Keys.Add(key);
            }

            var response = _datastore.Datasets.Lookup(request, _datasetId).Fetch();

            if (response.Found != null)
            {
                // unit of work
                response.Found.Select(t => ObjectEntityMapper.ConvertEntityToObject<T>(t.Entity)).ToList().ForEach(t => _entitiesInContext.Add(t));

                var objectList = response.Found.Select(t => ObjectEntityMapper.ConvertEntityToObject<T>(t.Entity)).ToList();

                return objectList;
            }

            return new List<T>();
        }

        public T Load<T>(object id) where T : new()
        {
            return Load<T>(new object[] { id }).SingleOrDefault();
        }

        public IPogoQueryable<T> Query<T>() where T : new()
        {
            return new PogoQueryable<T>(this);
        }

        public void Dispose()
        {

        }
    }
}
