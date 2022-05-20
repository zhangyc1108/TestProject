using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Common.ORM
{
    [Serializable]
    [BsonIgnoreExtraElements(Inherited = true)]
    public abstract class DBEntity
    {
        public DBEntity()
        {
            ID = ObjectId.GenerateNewId().ToString();
        }

        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string ID { get; set; }
    }
}