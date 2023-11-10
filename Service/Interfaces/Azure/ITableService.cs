using Azure.Data.Tables;
using Domain.Azure.TableEntities;

namespace Service.Interfaces.Azure
{
    public interface ITableService
    {
        Task AddEntityAsync<T>(T item) where T : ITableEntity;
        Task UpdateEntityAsync<T>(T item) where T : ITableEntity;
        JobTableEntity GetEntityByRowKey<T>(string rowKey) where T : ITableEntity;
    }
}
