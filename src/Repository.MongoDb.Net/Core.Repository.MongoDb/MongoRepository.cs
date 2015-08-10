using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Core.Utils;

namespace Core.Repository.MongoDb
{

    /// <summary>
    /// Generic Mongo Repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {
        /// <summary>
        /// Mongo Database
        /// </summary>
        private IMongoDatabase _database;
        /// <summary>
        /// Mongo Collection
        /// </summary>
        private IMongoCollection<T> _collection;

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <value>
        /// The collection.
        /// </value>
        protected IMongoCollection<T> Collection
        {
            get { return _collection; }
        }

        /// <summary>
        /// Initializes the <see cref="MongoRepository{T}"/> class.
        /// Prepare Mappings and Conventions packs
        /// </summary>
        static MongoRepository()
        {
            MongoClassMapHelper.RegisterConventionPacks();
            MongoClassMapHelper.SetupClassMap();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{T}"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="System.ArgumentException">Missing MongoDB connection string</exception>
        public MongoRepository(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentException("Missing MongoDB connection string");
            }

            var client = new MongoClient(connectionString);
            MongoUrl mongoUrl = MongoUrl.Create(connectionString);
            this._database = client.GetDatabase(mongoUrl.DatabaseName);
            this._collection = this.SetupCollection();
        }

        /// <summary>
        /// Setups the collection.
        /// </summary>
        /// <returns></returns>
        protected virtual IMongoCollection<T> SetupCollection()
        {
            try
            {
                var collectionName = this.BuildCollectionName();
                var collection = this._database.GetCollection<T>(collectionName);
                return collection;
            }
            catch (MongoException ex)
            {
                throw new RepositoryException(ex.Message);
            }
        }

        /// <summary>
        /// Builds the name of the collection.
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildCollectionName()
        {
            var pluralizedName = typeof(T).Name.EndsWith("s") ? typeof(T).Name : typeof(T).Name + "s";
            return pluralizedName;
        }


        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="RepositoryException"></exception>
        public async Task<T> Insert(T entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.ThrowIfNull(entity, "entity");

            if (entity.Id == null)
            {
                entity.Id = ObjectId.GenerateNewId().ToString();
            }

            try
            {
                entity.CreatedDate = DateTime.UtcNow;
                entity.UpdatedDate = DateTime.UtcNow;
                await this._collection.InsertOneAsync(entity, cancellationToken);
            }
            catch (MongoWriteException ex)
            {
                throw new EntityDuplicateException(entity, "Insert failed because the entity already exists!", ex);
            }

            return entity;
        }


        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="RepositoryException">Document version conflits. (Is out of date)</exception>
        public async Task<T> Update(T entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.ThrowIfNull(entity, "entity");

            var previousUpdateDate = entity.UpdatedDate;
            var previuosVersion = entity.Version;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.Version++;

            ReplaceOneResult result;

            var idFilter = Builders<T>.Filter.Eq(e => e.Id, entity.Id); //Find entity with same Id

            //Consistency enforcement
            if (!this.IgnoreVersion())
            {
                var versionLowerThan = Builders<T>.Filter.Lt(e => e.Version, entity.Version);

                result = await this.Collection.ReplaceOneAsync(
                    // Consistency enforcement: Where current._id = entity.Id AND entity.Version > current.Version
                    Builders<T>.Filter.And(idFilter, versionLowerThan),
                    entity,
                    null,
                    cancellationToken);

                if (result != null && ((result.IsAcknowledged && result.MatchedCount == 0) || (result.IsModifiedCountAvailable && !(result.ModifiedCount > 0))))
                    throw new EntityConflictException(entity, "Update failed because entity versions conflict!");
            }
            else
            {
                result = await this.Collection.ReplaceOneAsync(idFilter, entity, null, cancellationToken);

                if (result != null && ((result.IsAcknowledged && result.MatchedCount == 0) || (result.IsModifiedCountAvailable && !(result.ModifiedCount > 0))))
                    throw new EntityException(entity, "Entity does not exist.");


            }

            return entity;
        }


        /// <summary>
        /// Gets entity by id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<T> Get(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this._collection.Find(e => e.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes entity by id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<T> Delete(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this._collection.FindOneAndDeleteAsync(e => e.Id == id, null, cancellationToken);
        }


        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this._collection.Find(e => true).ToListAsync(cancellationToken);
        }


        /// <summary>
        /// Paginations the entites.
        /// </summary>
        /// <param name="top">The top.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> Pagination(int top, int skip, Func<T, object> orderBy, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = this._collection.Find(e => true).Skip(skip).Limit(top);

            if (ascending)
                return await query.SortBy(e => e.Id).ToListAsync();
            else
                return await query.SortByDescending(e => e.Id).ToListAsync(cancellationToken);
        }


        /// <summary>
        /// Ignores the document version.
        /// </summary>
        /// <returns></returns>
        public virtual bool IgnoreVersion()
        {
            return false;
        }
    }
}
