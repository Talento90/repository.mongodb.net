using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Core.Repository.MongoDb
{
    public static class MongoClassMapHelper
    {

        private static object _lock = new object();


        /// <summary>
        /// Registers the convention packs.
        /// </summary>
        public static void RegisterConventionPacks()
        {
            lock (_lock)
            {
                var conventionPack = new ConventionPack();
                conventionPack.Add(new IgnoreIfNullConvention(true));
                ConventionRegistry.Register("ConventionPack", conventionPack, t => true);
            }
        }

        /// <summary>
        /// Setups the mappings.
        /// </summary>
        public static void SetupClassMap()
        {
            lock (_lock)
            {

                if (!BsonClassMap.IsClassMapRegistered(typeof(Entity)))
                {
                    BsonClassMap.RegisterClassMap<Entity>(
                        (classMap) =>
                        {
                            classMap.AutoMap();
                            classMap.MapIdProperty(p => p.Id);
                            classMap.MapExtraElementsProperty(p => p.Metadata);
                        });
                }
            }
        }
    }
}
