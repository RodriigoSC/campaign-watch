﻿using Campaign.Watch.Infra.Data.Factories;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Data.Repository.Common
{
    /// <summary>
    /// Uma classe base abstrata para repositórios que encapsula a lógica comum de acesso a uma coleção MongoDB.
    /// </summary>
    /// <typeparam name="TEntity">O tipo da entidade com a qual este repositório trabalha.</typeparam>
    public abstract class CommonRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// A coleção MongoDB para a entidade <typeparamref name="TEntity"/>.
        /// Acessível pelas classes derivadas para realizar operações de banco de dados.
        /// </summary>
        protected readonly IMongoCollection<TEntity> _collection;

        /// <summary>
        /// Inicializa uma nova instância da classe CommonRepository.
        /// </summary>
        /// <param name="persistenceFactory">A fábrica para obter a instância do banco de dados de persistência.</param>
        /// <param name="collectionName">O nome da coleção a ser utilizada no banco de dados.</param>
        protected CommonRepository(IPersistenceMongoFactory persistenceFactory, string collectionName)
        {
            var database = persistenceFactory.GetDatabase();
            _collection = database.GetCollection<TEntity>(collectionName);
        }

        /// <summary>
        /// Cria múltiplos índices na coleção de forma assíncrona.
        /// Este método é geralmente chamado no construtor de uma classe derivada para garantir que os índices existam.
        /// </summary>
        /// <param name="indexes">Uma coleção de modelos de criação de índice.</param>
        protected async Task CreateIndexesAsync(IEnumerable<CreateIndexModel<TEntity>> indexes)
        {
            if (indexes != null && indexes.Any())
            {
                await _collection.Indexes.CreateManyAsync(indexes);
            }
        }
    }
}