﻿using MongoDB.Driver;

namespace Campaign.Watch.Infra.Data.Factories.Common
{
    /// <summary>
    /// Define uma interface para a fábrica (factory) responsável por criar e fornecer instâncias de IMongoDatabase.
    /// </summary>
    public interface IMongoDbFactory
    {
        /// <summary>
        /// Obtém uma instância do banco de dados MongoDB com base em uma chave de conexão e um nome de banco de dados.
        /// </summary>
        /// <param name="connectionKey">A chave usada para obter a string de conexão (ex: a partir de um arquivo de configuração).</param>
        /// <param name="databaseName">O nome do banco de dados ao qual se conectar.</param>
        /// <returns>Uma instância de IMongoDatabase configurada para o banco de dados especificado.</returns>
        IMongoDatabase GetDatabase(string connectionKey, string databaseName);
    }
}