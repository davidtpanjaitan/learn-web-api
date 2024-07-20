﻿using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using webapp.DAL.DTO;
using webapp.DAL.Models;

namespace webapp.DAL.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseModel
    {
        protected readonly Container _container;
        protected const string partitionKey = "";

        public BaseRepository(CosmosClient client, string databaseName) {
            var containerName = typeof(T).Name;
            _container = client.GetContainer(databaseName, containerName);
        }

        public virtual async Task<T> GetByIdAsync(string id)
        {
            var response =  await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
            return response.Resource;
        }
        public virtual async Task<List<T>> GetAllAsync()
        {
            var query = _container.GetItemQueryIterator<T>("SELECT * FROM c ORDER BY c.createdDate DESC");
            var response = await query.ReadNextAsync();
            return response.ToList();
        }
        public virtual async Task<PagedResult<T>> GetAsyncPaged(int pageSize, int pageNumber)
        {
            var query = _container.GetItemQueryIterator<T>(
                new QueryDefinition($"SELECT * FROM c ORDER BY c.createdDate DESC OFFSET {pageNumber*pageSize} LIMIT {pageSize}"),
                requestOptions: new QueryRequestOptions { MaxItemCount = pageSize, PartitionKey = new PartitionKey(partitionKey) }
            );

            List<T> results = new List<T>();

            FeedResponse<T> response = await query.ReadNextAsync();
            results.AddRange(response);
                    
            return new PagedResult<T>
            {
                Items = results,
                TotalCount = await GetTotalCountAsync()
            };
        }

        private async Task<int> GetTotalCountAsync()
        {
            var countQuery = _container.GetItemQueryIterator<int>(
                new QueryDefinition("SELECT VALUE COUNT(1) FROM c")
            );

            int totalCount = 0;
            while (countQuery.HasMoreResults)
            {
                FeedResponse<int> response = await countQuery.ReadNextAsync();
                totalCount += response.FirstOrDefault();
            }

            return totalCount;
        }

        public virtual async Task<T> CreateAsync(T item, string creator = "")
        {
            try
            {
                item.id = Guid.NewGuid().ToString();
                item.createdBy = creator;
                var response = await _container.CreateItemAsync(item, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new Exception("Username not unique");
            }
        }

        public virtual async Task<T> UpdateAsync(T item)
        {
            var response = await _container.UpsertItemAsync(item, new PartitionKey(partitionKey));
            return response.Resource;
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            try
            {
                await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
