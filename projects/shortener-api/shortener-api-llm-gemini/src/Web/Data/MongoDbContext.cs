﻿using MongoDB.Driver;
using Web.Common.Models.Options;
using Web.Data.Configurations;
using Web.Data.Entities;

namespace Web.Data;

public class MongoDbContext
{
    public MongoDbContext(IMongoClient client, AppSettingModel appSettingModel)
    {
        var databaseName = appSettingModel.MongoDb.Database;
        Database = client.GetDatabase(databaseName);

        ConfigureEntities();
    }

    private void ConfigureEntities()
    {
        UrlTokenConfiguration.Configure();
        UrlShortenConfiguration.Configure();

        UrlTokens.Indexes.CreateMany(UrlTokenConfiguration.CreateIndexes);
        UrlShortens.Indexes.CreateMany(UrlShortenConfiguration.CreateIndexes);
    }

    private IMongoDatabase Database { get; }

    public IMongoCollection<UrlToken> UrlTokens => GetCollection<UrlToken>();

    public IMongoCollection<UrlShorten> UrlShortens => GetCollection<UrlShorten>();

    private IMongoCollection<T> GetCollection<T>(string? name = null)
    {
        name ??= typeof(T).Name;
        return Database.GetCollection<T>(name);
    }
}