using Microsoft.EntityFrameworkCore;
using ReversiRestApi.Model;

namespace ReversiRestApi.DAL
{
    public class SpelRepository : ISpelRepository
    {
        // Lijst met tijdelijke spellen
        public List<Spel> Spellen { get; set; }

        public SpelRepository()
        {
            Spel spel1 = new Spel();
            Spel spel2 = new Spel();
            Spel spel3 = new Spel();
            Spel spel4 = new Spel();
            Spel spel5 = new Spel();

            spel1.Speler1Token = "abcdef";
            spel1.Omschrijving = "Potje snel reveri, dus niet lang nadenken";

            spel2.Speler1Token = "ghijkl";
            spel2.Speler2Token = "mnopqr";
            spel2.Omschrijving = "Ik zoek een gevorderde tegenspeler!";

            spel3.Speler1Token = "stuvwx";
            spel3.Omschrijving = "Na dit spel wil ik er nog een paar spelen tegen zelfde tegenstander";

            spel4.Omschrijving = "Spel met Guid token";
            spel4.Speler1Token = "fnweojfowe";
            spel4.Speler2Token = "nfejfiwejf";
            spel4.Token = Guid.NewGuid().ToString();

            spel5.Omschrijving = "Spel met simpele token";
            spel5.Speler1Token = "ooifdjeiojfij2i";
            spel5.Speler2Token = "nclsdnclsdmcdww";
            spel5.Token = "token";


            Spellen = new List<Spel> { spel1, spel2, spel3, spel4, spel5 };
        }

        public void AddSpel(Spel spel)
        {
            Spellen.Add(spel);
        }

        public List<Spel> GetSpellen()
        {
            return Spellen;
        }

        public Spel GetSpel(string spelToken)
        {
            return Spellen.First(spel => spel.Token == spelToken);
        }

        public Spel GetSpelBySpeler1Token(string speler1Token)
        {
            return Spellen.First(x => x.Speler1Token == speler1Token);
        }

        public Spel GetSpelBySpelerToken(string spelerToken)
        {
            return Spellen.First(x => x.Speler1Token == spelerToken || x.Speler2Token == spelerToken);
        }

        public void UpdateSpel(Spel spel)
        {
            Spel s = Spellen.First(x => x == spel);
            s = spel;
        }

        public void VerwijderSpel(Spel spel)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Save()
        {
            return "";
        }
    }
}
