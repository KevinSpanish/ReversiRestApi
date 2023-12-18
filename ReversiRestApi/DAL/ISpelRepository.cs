using ReversiRestApi.Model;

namespace ReversiRestApi.DAL
{
    public interface ISpelRepository
    {
        public List<Spel> GetSpellen();

        Spel GetSpel(string spelToken);

        Spel GetSpelBySpeler1Token(string speler1Token);

        Spel GetSpelBySpelerToken(string speler1Token);

        void AddSpel(Spel spel);

        void UpdateSpel(Spel spel);

        Task<string> Save();

        void VerwijderSpel(Spel spel);
    }
}
