﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReversiRestApi.Model
{
    public class Spel : ISpel
    {
        private const int bordOmvang = 8;
        private readonly int[,] richting = new int[8, 2] {
                                {  0,  1 },         // naar rechts
                                {  0, -1 },         // naar links
                                {  1,  0 },         // naar onder
                                { -1,  0 },         // naar boven
                                {  1,  1 },         // naar rechtsonder
                                {  1, -1 },         // naar linksonder
                                { -1,  1 },         // naar rechtsboven
                                { -1, -1 } };       // naar linksboven

        public int ID { get; set; }
        public string Omschrijving { get; set; }
        public string Token { get; set; }
        public string Speler1Token { get; set; }
        public string? Speler2Token { get; set; }

        [NotMapped]
        public string Winnaar { get; set; }

        private Kleur[,] bord;

        [NotMapped]
        public Kleur[,] Bord
        {
            get => bord; 
            set => JsonBord = boardconvert(value);
        }

        private static string boardconvert(Kleur[,] bord)
        {
            string s = "";
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Kleur kleur = bord[x, y];
                    int v;
                    switch (kleur)
                    {
                        default:
                        case Kleur.Geen:
                            v = 0;
                            break;
                        case Kleur.Wit:
                            v = 1;
                            break;
                        case Kleur.Zwart:
                            v = 2;
                            break;
                    }

                    s += v;
                }

                s += ":";
            }

            return s;
        }

        private static Kleur[,] boardconvert(string str)
        {
            Kleur[,] bord = new Kleur[8, 8];

            int x = 0;
            foreach (string line in str.Split(":"))
            {
                int y = 0;
                foreach (char c in line.ToCharArray())
                {
                    Kleur kleur;

                    switch (c)
                    {
                        default:
                        case '0':
                            kleur = Kleur.Geen;
                            break;
                        case '1':
                            kleur = Kleur.Wit;
                            break;
                        case '2':
                            kleur = Kleur.Zwart;
                            break;
                    }

                    bord[y, x] = kleur;

                    y++;
                }

                x++;
            }

            return bord;
        }

        public string? JsonBord {
            
            get => boardconvert(bord);

            set => bord = boardconvert(value);

        }

        public Kleur AandeBeurt { get; set; }

        [NotMapped]
        public bool Afgerond
        {
            get
            {
                return Afgelopen();
            }
        }
        public Spel()
        {
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            Token = Token.Replace("/", "q");    // slash mijden ivm het opvragen van een spel via een api obv het token
            Token = Token.Replace("+", "r");    // plus mijden ivm het opvragen van een spel via een api obv het token

            Bord = new Kleur[bordOmvang, bordOmvang];
            Bord[3, 3] = Kleur.Wit;
            Bord[4, 4] = Kleur.Wit;
            Bord[3, 4] = Kleur.Zwart;
            Bord[4, 3] = Kleur.Zwart;

            JsonBord = boardconvert(Bord);

            AandeBeurt = Kleur.Geen;
        }

        public void resetbord(Spel spel)
        {
            spel.Bord = new Kleur[bordOmvang, bordOmvang];
            spel.Bord[3, 3] = Kleur.Wit;
            spel.Bord[4, 4] = Kleur.Wit;
            spel.Bord[3, 4] = Kleur.Zwart;
            spel.Bord[4, 3] = Kleur.Zwart;

            spel.JsonBord = boardconvert(spel.Bord);
            spel.AandeBeurt = Kleur.Zwart;
        }

        public void Pas()
        {
            // controleeer of er geen zet mogelijk is voor de speler die wil passen, alvorens van beurt te wisselen.
            if (IsErEenZetMogelijk(AandeBeurt))
                throw new Exception("Passen mag niet, er is nog een zet mogelijk");
            else
                WisselBeurt();
        }


        public bool Afgelopen()     // return true als geen van de spelers een zet kan doen
        {
            if (IsErEenZetMogelijk(Kleur.Zwart) || IsErEenZetMogelijk(Kleur.Wit))
            {
                return false;
            }

            return true;
        }

        public Kleur OverwegendeKleur()
        {
            int aantalWit = 0;
            int aantalZwart = 0;
            for (int rijZet = 0; rijZet < bordOmvang; rijZet++)
            {
                for (int kolomZet = 0; kolomZet < bordOmvang; kolomZet++)
                {
                    if (bord[rijZet, kolomZet] == Kleur.Wit)
                        aantalWit++;
                    else if (bord[rijZet, kolomZet] == Kleur.Zwart)
                        aantalZwart++;
                }
            }
            if (aantalWit > aantalZwart)
                return Kleur.Wit;
            if (aantalZwart > aantalWit)
                return Kleur.Zwart;
            return Kleur.Geen;
        }

        public bool ZetMogelijk(int rijZet, int kolomZet)
        {
            if (!PositieBinnenBordGrenzen(rijZet, kolomZet))
                throw new Exception($"Zet ({rijZet},{kolomZet}) ligt buiten het bord!");
            return ZetMogelijk(rijZet, kolomZet, AandeBeurt);
        }

        public bool DoeZet(int rijZet, int kolomZet)
        {

            if (!ZetMogelijk(rijZet, kolomZet))
            {
                return false;
            }
            for (int i = 0; i < 8; i++)
            {

                DraaiStenenVanTegenstanderInOpgegevenRichtingOmIndienIngesloten(rijZet, kolomZet,
                    AandeBeurt,
                    richting[i, 0], richting[i, 1]);

            }
            Bord[rijZet, kolomZet] = AandeBeurt;
            WisselBeurt();

            return true;

        }

        private static Kleur GetKleurTegenstander(Kleur kleur)
        {
            if (kleur == Kleur.Wit)
                return Kleur.Zwart;
            else if (kleur == Kleur.Zwart)
                return Kleur.Wit;
            else
                return Kleur.Geen;
        }

        private bool IsErEenZetMogelijk(Kleur kleur)
        {
            if (kleur == Kleur.Geen)
                throw new Exception("Kleur mag niet gelijk aan Geen zijn!");
            // controleeer of er een zet mogelijk is voor kleur
            for (int rijZet = 0; rijZet < bordOmvang; rijZet++)
            {
                for (int kolomZet = 0; kolomZet < bordOmvang; kolomZet++)
                {
                    if (ZetMogelijk(rijZet, kolomZet, kleur))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ZetMogelijk(int rijZet, int kolomZet, Kleur kleur)
        {
            // Check of er een richting is waarin een zet mogelijk is. Als dat zo is, return dan true.
            for (int i = 0; i < 8; i++)
            {
                {
                    if (StenenInTeSluitenInOpgegevenRichting(rijZet, kolomZet,
                                                             kleur,
                                                             richting[i, 0], richting[i, 1]))
                        return true;
                }
            }
            return false;
        }

        private void WisselBeurt()
        {
            if (AandeBeurt == Kleur.Wit)
                AandeBeurt = Kleur.Zwart;
            else
                AandeBeurt = Kleur.Wit;
        }

        private static bool PositieBinnenBordGrenzen(int rij, int kolom)
        {
            return (rij >= 0 && rij < bordOmvang &&
                    kolom >= 0 && kolom < bordOmvang);
        }

        private bool ZetOpBordEnNogVrij(int rijZet, int kolomZet)
        {
            // Als op het bord gezet wordt, en veld nog vrij, dan return true, anders false
            return (PositieBinnenBordGrenzen(rijZet, kolomZet) && Bord[rijZet, kolomZet] == Kleur.Geen);
        }

        private bool StenenInTeSluitenInOpgegevenRichting(int rijZet, int kolomZet,
                                                          Kleur kleurZetter,
                                                          int rijRichting, int kolomRichting)
        {
            int rij, kolom;
            Kleur kleurTegenstander = GetKleurTegenstander(kleurZetter);
            if (!ZetOpBordEnNogVrij(rijZet, kolomZet))
                return false;

            // Zet rij en kolom op de index voor het eerst vakje naast de zet.
            rij = rijZet + rijRichting;
            kolom = kolomZet + kolomRichting;

            int aantalNaastGelegenStenenVanTegenstander = 0;
            // Zolang Bord[rij,kolom] niet buiten de bordgrenzen ligt, en je in het volgende vakje 
            // steeds de kleur van de tegenstander treft, ga je nog een vakje verder kijken.
            // Bord[rij, kolom] ligt uiteindelijk buiten de bordgrenzen, of heeft niet meer de
            // de kleur van de tegenstander.
            // N.b.: deel achter && wordt alleen uitgevoerd als conditie daarvoor true is.
            while (PositieBinnenBordGrenzen(rij, kolom) && Bord[rij, kolom] == kleurTegenstander)
            {
                rij += rijRichting;
                kolom += kolomRichting;
                aantalNaastGelegenStenenVanTegenstander++;
            }

            // Nu kijk je hoe je geeindigt bent met bovenstaande loop. Alleen
            // als alle drie onderstaande condities waar zijn, zijn er in de
            // opgegeven richting stenen in te sluiten.
            return (PositieBinnenBordGrenzen(rij, kolom) &&
                    Bord[rij, kolom] == kleurZetter &&
                    aantalNaastGelegenStenenVanTegenstander > 0);
        }

        private bool DraaiStenenVanTegenstanderInOpgegevenRichtingOmIndienIngesloten(int rijZet, int kolomZet,
                                                                                     Kleur kleurZetter,
                                                                                     int rijRichting, int kolomRichting)
        {
            int rij, kolom;
            Kleur kleurTegenstander = GetKleurTegenstander(kleurZetter);
            bool stenenOmgedraaid = false;

            if (StenenInTeSluitenInOpgegevenRichting(rijZet, kolomZet, kleurZetter, rijRichting, kolomRichting))
            {
                rij = rijZet + rijRichting;
                kolom = kolomZet + kolomRichting;

                // N.b.: je weet zeker dat je niet buiten het bord belandt,
                // omdat de stenen van de tegenstander ingesloten zijn door
                // een steen van degene die de zet doet.
                while (Bord[rij, kolom] == kleurTegenstander)
                {
                    Bord[rij, kolom] = kleurZetter;
                    rij += rijRichting;
                    kolom += kolomRichting;
                }
                stenenOmgedraaid = true;
            }
            return stenenOmgedraaid;
        }
    }
}
