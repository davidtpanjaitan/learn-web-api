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
            item.password = PasswordEncryption.encryptSeeded(item.password);
            return await base.CreateAsync(item);
        }

        public async Task<bool> MatchUserPasswordExist(User item)
        {
            var enteredPassword = PasswordEncryption.encryptSeeded(item.password);
            var existingUser = _container.GetItemLinqQueryable<User>().FirstOrDefault(p => (p.username == item.username || p.id == item.id) && p.password == item.password);
            
            return existingUser != null;
        }
    }
}
