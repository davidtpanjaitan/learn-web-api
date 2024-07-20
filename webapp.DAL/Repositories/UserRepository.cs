using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webapp.DAL.Models;
using webapp.DAL.Tools;

namespace webapp.DAL.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository(Microsoft.Azure.Cosmos.CosmosClient client, string databaseName) : base(client, databaseName)
        {
        }

        public async Task<User> CreateWithEncryptedPasswordAsync(User item)
        {
            item.password = EncryptionService.encryptSeeded(item.password);
            return await base.CreateAsync(item);
        }

        public async Task<string> MatchUserPasswordExist(User item)
        {
            var enteredPassword = EncryptionService.encryptSeeded(item.password);
            var existingUserQuery = await _container
                .GetItemQueryIterator<User>($"SELECT * FROM c WHERE (c.id = '{item.id}' OR c.username = '{item.username}') AND c.password = '{enteredPassword}'")
                .ReadNextAsync();
            var existingUser = existingUserQuery.FirstOrDefault();
            
            return existingUser == null ? "" : existingUser.role;
        }

        public override async Task<User> UpdateAsync(User item)
        {
            var user = await base.GetByIdAsync(item.id);
            user.employeeId = item.employeeId;
            user.name = item.name;
            user.username = item.username;
            user.role = item.role;
            user.password = EncryptionService.encryptSeeded(item.password);
            return await base.UpdateAsync(user);
        }
    }
}
