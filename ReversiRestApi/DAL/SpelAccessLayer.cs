using Microsoft.EntityFrameworkCore;
using ReversiRestApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiRestApi.DAL
{
    public class SpelAccessLayer : ISpelRepository
    {
        private SpelContext _spelContext;

        public SpelAccessLayer(SpelContext spelContext)
        {
            _spelContext = spelContext;
        }

        public List<Spel> Spellen { get; set; }

        public List<Spel> GetSpellen() => _spelContext.Spel.ToList();

        public Spel GetSpel(string spelToken) {
            var spel = _spelContext.Spel.FirstOrDefault(spel => spel.Token == spelToken);
            return spel;
                }


        public Spel GetSpelBySpeler1Token(string speler1Token)
        {
            return _spelContext.Spel.First(spel => spel.Speler1Token == speler1Token);
        }

        public Spel GetSpelBySpelerToken(string spelerToken)
        {
            return _spelContext.Spel.FirstOrDefault(spel => spel.Speler1Token == spelerToken || spel.Speler2Token == spelerToken);
            //return _spelContext.Spel.First(x => x.Speler1Token == spelerToken || x.Speler2Token == spelerToken);
        }

        public void AddSpel(Spel spel)
        {
            _spelContext.Spel.Add(spel);
            _spelContext.SaveChanges();
        }

        public void UpdateSpel(Spel spel) => _spelContext.Spel.Update(spel);

        public void VerwijderSpel(Spel spel) => _spelContext.Spel.Remove(spel);

        public async Task<string> Save()
        {
            await _spelContext.SaveChangesAsync();
            return "";
        }
    }
}
