using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using RikardLib.AspLog;
using RikardWeb.Lib.Db.Service;

namespace RikardWeb.Lib.Identity
{
    public class MongoRoleStore : IRoleStore<IdentityRole>
    {
        private const string ROLES_COLLECTION_NAME = "roles";

        private IMongoCollection<IdentityRole> roles;

        private readonly IAspLogger logger;

        private static volatile bool ManagedIndicies = false;

        public MongoRoleStore(IAspLogger logger, IMongoDbService mongoDbService)
        {
            this.logger = logger;

            roles = mongoDbService.GetDatabase().MongoDatabase.GetCollection<IdentityRole>(ROLES_COLLECTION_NAME);

            if(!ManagedIndicies)
            {
                ManagedIndicies = true;

                roles.Indexes.CreateOne(
                    Builders<IdentityRole>.IndexKeys.Ascending(r => r.NormalizedName), new CreateIndexOptions
                    {
                        Unique = true,
                        Sparse = false,
                        Background = true
                    });

                logger.Info("MongoRoleStore has been initialized.");
            }
        }

        public void Dispose()
        {
            roles = null;
        }

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            await roles.InsertOneAsync(role, null, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            await roles.FindOneAndReplaceAsync(r => r.Id == role.Id, role, cancellationToken: cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            await roles.DeleteOneAsync(r => r.Id == role.Id, cancellationToken);
            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            return Task.Run(() => role.Name = roleName, cancellationToken);
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.Run(() => role.NormalizedName = normalizedName, cancellationToken);
        }

        public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return (await roles.FindAsync(r => r.Id == roleId, cancellationToken: cancellationToken)).SingleOrDefault();
        }

        public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return (await roles.FindAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken: cancellationToken)).SingleOrDefault();
        }
    }
}
