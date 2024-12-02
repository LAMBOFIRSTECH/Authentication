using System.Security.Cryptography;

namespace Authentifications.Repositories
{
    public class RandomUserSecret
    {
        private static readonly Dictionary<int, string> keyCache = new Dictionary<int, string>();
        public string GenerateRandomKey(int length)
        {
            if (keyCache.ContainsKey(length))
            {
                // Retourner la clé en cache
                return keyCache[length];
            }
            // Générer une nouvelle clé
            string refreshKey = "";
            var randomNumber = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshKey = Convert.ToBase64String(randomNumber);
            }
            // Stocker la nouvelle clé dans le cache
            keyCache[length] = refreshKey;
            return refreshKey;
        }
    }
}
