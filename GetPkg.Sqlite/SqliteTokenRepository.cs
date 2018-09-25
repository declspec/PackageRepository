using System;
using System.Threading.Tasks;
using Dapper;
using Fiksu.Database;

namespace GetPkg.Sqlite {
    public class SqliteTokenRepository : ITokenRepository {
        private static readonly string SelectQuery = GetSelectQuery();
        private static readonly string InsertQuery = GetInsertQuery();
        private static readonly string UpdateQuery = GetUpdateQuery();

        private readonly IDbConnectionProvider _connectionProvider;

        public SqliteTokenRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task<Token> GetAsync(string tokenId) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                return await connection.QuerySingleAsync<Token>(SelectQuery, new { Id = tokenId }).ConfigureAwait(false);
            }
        }

        public Task<Token> CreateAsync() {
            return CreateAsync(null);
        }

        public async Task<Token> CreateAsync(string userId) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var token = new Token() {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId
                };

                await connection.ExecuteAsync(InsertQuery, token).ConfigureAwait(false);
                return token;
            }
        }

        public async Task AuthenticateAsync(Token token) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var updated = await connection.ExecuteAsync(UpdateQuery, token).ConfigureAwait(false);
                if (updated != 1)
                    throw new InvalidOperationException("cannot authenticate token; invalid token state");
            }
        }

        private static string GetSelectQuery() {
            return $"SELECT id, user_id FROM { Tables.Tokens } WHERE id = @{nameof(Token.Id)};";
        }

        private static string GetInsertQuery() {
            return $"INSERT INTO { Tables.Tokens } (id, user_id) VALUES (@{nameof(Token.Id)}, @{nameof(Token.UserId)};";
        }

        private static string GetUpdateQuery() {
            return $"UPDATE { Tables.Tokens } SET user_id = @{nameof(Token.UserId)}, date_modified = CURRENT_TIMESTAMP WHERE id = @{nameof(Token.Id)} AND user_id IS NULL;";
        }
    }
}
