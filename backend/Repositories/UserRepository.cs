using MongoDB.Driver;
using backend.Data;
using backend.Models;

namespace backend.Repositories
{
    /// <summary>
    /// Repository implementation for user persistence.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly MongoDbContext _context;

        /// <summary>
        /// Initializes a new instance of the UserRepository class and ensures the username index.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        private readonly backend.Services.ITokenProtector _protector;

        public UserRepository(MongoDbContext context, backend.Services.ITokenProtector protector)
        {
            _context = context;
            _protector = protector;

            // Ensure unique index on Username to avoid duplicates
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Username);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<User>(indexKeys, indexOptions);
            _context.Users.Indexes.CreateOne(indexModel);
        }

        /// <summary>
        /// Upserts a document that contains the username and optional email.
        /// </summary>
        /// <param name="username">The username to upsert.</param>
        public Task UpsertByUsernameAsync(string username, string? email = null)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Username, username);
            var update = Builders<User>.Update.Set(u => u.Username, username);
            if (!string.IsNullOrEmpty(email))
            {
                update = update.Set(u => u.Email, email);
            }
            var options = new UpdateOptions { IsUpsert = true };
            return _context.Users.UpdateOneAsync(filter, update, options);
        }

        /// <summary>
        /// Finds a user document by username.
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Username, username);
            var user = await _context.Users.Find(filter).FirstOrDefaultAsync();
            if (user?.GoogleAccount != null)
            {
                user.GoogleAccount.RefreshToken = _protector.Unprotect(user.GoogleAccount.RefreshToken);
                user.GoogleAccount.AccessToken = _protector.Unprotect(user.GoogleAccount.AccessToken);
            }
            return user;
        }

        /// <summary>
        /// Finds a user document by email.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var user = await _context.Users.Find(filter).FirstOrDefaultAsync();
            if (user?.GoogleAccount != null)
            {
                user.GoogleAccount.RefreshToken = _protector.Unprotect(user.GoogleAccount.RefreshToken);
                user.GoogleAccount.AccessToken = _protector.Unprotect(user.GoogleAccount.AccessToken);
            }
            return user;
        }

        /// <summary>
        /// Upserts the Google account subdocument for the specified username.
        /// </summary>
        public Task UpsertGoogleAccountAsync(string username, GoogleAccount account)
        {
            // Protect tokens before persisting
            if (account != null)
            {
                account.RefreshToken = _protector.Protect(account.RefreshToken);
                account.AccessToken = _protector.Protect(account.AccessToken);
            }

            var filter = Builders<User>.Filter.Eq(u => u.Username, username);
            var update = Builders<User>.Update.Set(u => u.GoogleAccount, account);
            var options = new UpdateOptions { IsUpsert = false };
            return _context.Users.UpdateOneAsync(filter, update, options);
        }

        /// <summary>
        /// Removes the Google account linkage for a user.
        /// </summary>
        public Task RemoveGoogleAccountAsync(string username)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Username, username);
            var update = Builders<User>.Update.Unset(u => u.GoogleAccount);
            return _context.Users.UpdateOneAsync(filter, update);
        }
    }
}
