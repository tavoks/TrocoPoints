namespace TrocoPoints.Infrastructure.Mongo
{
    public class MongoDbOptions
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
        public required string AuditoriaCollectionName { get; set; }
    }
}
