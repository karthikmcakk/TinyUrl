namespace TinyUrl.Api.Services
{
    public class ShortCodeService
    {
        private const string Characters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        
        public string GenerateShortCode(int length = 6)
        {
            var random = new Random();
            var shortCode = new char[length];
            for (int i = 0; i < length; i++)
            {
                shortCode[i] = Characters[random.Next(Characters.Length)];
            }
            return new string(shortCode);
        }
    }
}
