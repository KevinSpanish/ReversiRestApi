using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReversiRestApi.Model
{
    public class SpelInfo
    {
        private Spel _spel;

        public int ID => _spel.ID;
        public string Omschrijving => _spel.Omschrijving;
        public string Token => _spel.Token;
        public string Speler1Token => _spel.Speler1Token;
        public string? Speler2Token => _spel.Speler2Token;
        public string? Bord => JsonConvert.SerializeObject(_spel.Bord); // Convert [,] to string for Json compatibility.
        public Kleur AandeBeurt => _spel.AandeBeurt;
        public string Gewonnen => _spel.Winnaar;

        public SpelInfo(Spel spel) => _spel = spel;

    }
}
