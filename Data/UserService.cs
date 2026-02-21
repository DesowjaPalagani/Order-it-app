using MongoDB.Driver;
using OrderItApp.Models;
using System.Threading.Tasks;

namespace OrderItApp.Data
{
    public interface IUserService
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUserNameAsync(string userName);
        /// <summary>Find a user by either username or email (case‑insensitive).</summary>
        Task<User?> GetByIdentifierAsync(string identifier);

        Task CreateUserAsync(User user);

        /// <summary>Returns true if a user with the given email or username already exists.</summary>
        Task<bool> ExistsByEmailOrUserNameAsync(string email, string userName);
    }

    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserService(IMongoClient mongoClient, IConfiguration configuration)
        {
            var database = mongoClient.GetDatabase(configuration["MongoDb:DatabaseName"]);
            _usersCollection = database.GetCollection<User>(configuration["MongoDb:UsersCollection"]);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            // callers are expected to already normalize to lowercase
            return await _usersCollection
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            // callers are expected to already normalize to lowercase
            return await _usersCollection
                .Find(u => u.UserName == userName)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            // caller should pass a lowercase string; ensure it just in case so the
            // comparison logic stays consistent with how users are stored.
            identifier = identifier.ToLower();

            // try email first then username
            var byEmail = await GetByEmailAsync(identifier);
            if (byEmail != null)
                return byEmail;

            return await GetByUserNameAsync(identifier);
        }

        public async Task CreateUserAsync(User user)
        {
            await _usersCollection.InsertOneAsync(user);
        }

        public async Task<bool> ExistsByEmailOrUserNameAsync(string email, string userName)
        {
            // inputs should already be lowercased by the caller
            var filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Eq(u => u.Email, email),
                Builders<User>.Filter.Eq(u => u.UserName, userName)
            );
            return await _usersCollection.Find(filter).AnyAsync();
        }
    }
}
