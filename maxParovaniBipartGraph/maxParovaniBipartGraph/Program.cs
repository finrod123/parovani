using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BipartGraphCreator;
using System.Diagnostics;

namespace maxParovaniBipartGraph
{
    public class Sit {

        int uzlu, prvniPartita, druhaPartita;
        int indexZdroje, indexSpotrebice;
        int aktualniTok;
        Matrix<int> kapacity,
                    toky,
                    rezervy;

        public Matrix<int> Kapacity { get { return kapacity; } }
        public Matrix<int> Rezervy { get { return rezervy; } }
        public Matrix<int> Toky { get { return toky; } }

        public Sit(string graphText) {
            ZavedGraf(graphText);
        }

        void ZavedGraf(string graphText)
        {
            string[] parts = graphText.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            ETypGrafu typGrafu = (ETypGrafu)int.Parse(parts[0]);

            switch (typGrafu)
            {
                case ETypGrafu.Obecny:
                    ZavedObecnyGraf(parts);
                    break;
                case ETypGrafu.Bipartitni:
                    ZavedBipartitniGraf(parts);
                    break;
            }
        }

        void ZavedBipartitniGraf(string[] parts) {
            
            prvniPartita = int.Parse(parts[1]);
            druhaPartita = int.Parse(parts[2]);
            uzlu = prvniPartita + druhaPartita + 2;
            indexZdroje = 0;
            indexSpotrebice = uzlu - 1;

            InicializujMatice();

            NaplnMaticeBipartitniGraf(parts);
            PridejZdroj();
            PridejSpotrebic();
        }

        void InicializujMatice()
        {
            kapacity = new Matrix<int>(uzlu, uzlu);
            toky = new Matrix<int>(uzlu, uzlu);
            rezervy = new Matrix<int>(uzlu, uzlu);
        }

        void ZavedObecnyGraf(string[] parts)
        {
            uzlu = int.Parse(parts[1]);
            indexZdroje = int.Parse(parts[2]);
            indexSpotrebice = int.Parse(parts[3]);

            InicializujMatice();
            NaplnMaticeObecnyGraf(parts);
        }

        void NaplnMaticeObecnyGraf(string[] parts)
        {
            int len = parts.Length;
            const int obecnyGrafFrom = 4;

            for (int i = obecnyGrafFrom; i < len; i += 3)
            {
                int vert1 = int.Parse(parts[i]);
                int vert2 = int.Parse(parts[i + 1]);
                int kapacita = int.Parse(parts[i + 2]);

                kapacity.SetElement(vert1, vert2, kapacita);
                rezervy.SetElement(vert1, vert2, kapacita);
            }
        }

        void NaplnMaticeBipartitniGraf(string[] sousedi)
        {
            int len = sousedi.Length;
            const int bipartFrom = 3;
            const int kapacitaBipart = 1;

            for (int i = bipartFrom; i < len; i+=2) {
                int vert1 = int.Parse(sousedi[i]);
                int vert2 = int.Parse(sousedi[i + 1]);

                kapacity.SetElement(vert1, vert2, kapacitaBipart);
                rezervy.SetElement(vert1, vert2, kapacitaBipart);
            }
        }

        void PridejZdroj() {
            for (int i = 1; i <= prvniPartita; ++i) {
                kapacity.SetElement(indexZdroje, i, 1);
                rezervy.SetElement(indexZdroje, i, 1);
            }
        }

        void PridejSpotrebic() {
            int mez = uzlu - 2;
            
            for (int i = prvniPartita + 1; i <= mez; ++i) {
                kapacity.SetElement(i, indexSpotrebice, 1);
                rezervy.SetElement(i, indexSpotrebice, 1);
            }
        }

        public DinitzRunSummary SpoctiMaximalniTok() {
            int aktualniTok = 0;
            int prirustekToku = 0;
            DinitzRunSummary summary = new DinitzRunSummary(uzlu);

            while(true)
            {
                SpoctiRezervy();
                //Console.WriteLine("Spoctene rezervy");
                //Console.WriteLine(rezervy.ToString());

                if (!VycistiSit())
                    break;
                
                // pokud se povede protlacit dale 
                if (ZkusZvysitTok(indexZdroje, int.MaxValue, out prirustekToku))
                {
                    aktualniTok += prirustekToku;
                    //Console.WriteLine("Aktualni tok je {0}", aktualniTok);
                    summary.ZvysPocetFazi();
                    summary.ZaznamenejPocetHranVAktualniFazi(aktualniTok);
                }
                else
                {
                    //Console.WriteLine("Koncim s tokem {0}", aktualniTok);
                    break;
                }

            }

            return summary;
        }

        void SpoctiRezervy() {
            for (int i = 0; i < uzlu; ++i) {
                for (int j = 0; j < uzlu; ++j) {
                    rezervy.SetElement(i, j,
                        kapacity.GetElement(i, j) - toky.GetElement(i, j) + toky.GetElement(j, i));
                }
            }
        }

        bool ZkusZvysitTok(int uzel, int limit, out int prirustekToku) {
            bool zvysenTok = false;
            int soused = 0;
            prirustekToku = 0;
            int difPrirustekToku = 0;

            while(soused < uzlu)
            {
                int rezervaI;

                if ((rezervaI = rezervy.GetElement(uzel, soused)) > 0)
                {
                    limit = Math.Min(limit, rezervaI);

                    // narazil jsem na zdroj
                    if (soused == indexSpotrebice)
                    {
                        zvysenTok = true;
                        prirustekToku = difPrirustekToku = limit;
                        rezervy.SetElement(uzel, indexSpotrebice, rezervaI - difPrirustekToku);
                        //rezervy.SetElement(indexSpotrebice, uzel, rezervy.GetElement(indexSpotrebice, uzel) + difPrirustekToku);
                        toky.SetElement(uzel, indexSpotrebice, toky.GetElement(uzel, indexSpotrebice) + difPrirustekToku);
                        break;
                    }
                    // podaril se zvysit tok sousedem
                    else if (ZkusZvysitTok(soused, limit, out difPrirustekToku))
                    {
                        int novaRezerva = rezervaI - difPrirustekToku;
                        prirustekToku += difPrirustekToku;
                        rezervy.SetElement(uzel, soused, novaRezerva);
                        //rezervy.SetElement(soused, uzel, rezervy.GetElement(soused, uzel) + difPrirustekToku);
                        toky.SetElement(uzel, soused, toky.GetElement(uzel, soused) + difPrirustekToku);

                        //Console.WriteLine("Zvysil jsem tok");
                        zvysenTok = true;
                        
                        if (uzel == indexZdroje)
                        {
                            if (novaRezerva == 0)
                                ++soused;
                            
                            continue;
                        }

                        break;
                    }
                    else
                    {
                        // tudy cesta nevede -> zrus hranu
                        rezervy.SetElement(uzel, soused, 0);
                    }

                }

                ++soused;
            }

            return zvysenTok;
        }

        bool VycistiSit()
        {
            int[] dist = new int[uzlu];
            initDist(dist);

            Queue<int> fronta = new Queue<int>();
            dist[indexSpotrebice] = 0;
            pridejSousedy(fronta, indexSpotrebice, dist);
            
            while(fronta.Count > 0)
            {
                int uzel = fronta.Dequeue();
                // vycisti sousedni hrany
                cistiHranyUzlu(uzel, dist);
                // pridej sousedy do fronty
                pridejSousedy(fronta, uzel, dist);
            }
            //Console.WriteLine("Rezervy");
            //Console.WriteLine(rezervy.ToString());
            //Console.ReadLine();
            return dist[indexZdroje] != -1;
        }

        void cistiHranyUzlu(int uzel, int[] dist) {
            for (int i = 0; i < uzlu; ++i) {
                if (dist[i] != (dist[uzel] - 1) &&
                    rezervy.GetElement(uzel, i) > 0)
                        rezervy.SetElement(uzel, i, 0);
            }
        }

        void pridejSousedy(Queue<int> fronta, int uzel, int[] dist) {
            for (int i = 0; i < uzlu; ++i) {
                if (dist[i] == -1 &&
                    rezervy.GetElement(i, uzel) > 0)
                {
                    dist[i] = dist[uzel] + 1;
                    fronta.Enqueue(i);
                }
            }
        }

        void initDist(int[] dist) {
            for (int i = 0; i < uzlu; ++i) {
                dist[i] = -1;
            }
        }
    }

    public class DinitzRunSummary {
        int pocetVrcholuSite;
        int pocetFazi;
        int aktMaxTok;
        List<int> poctyHranParovani;

        public DinitzRunSummary(int vrcholuSite) {
            pocetFazi = 0;
            pocetVrcholuSite = vrcholuSite;
            poctyHranParovani = new List<int>();
        }

        public void ZvysPocetFazi()
        {
            ++pocetFazi;
        }

        public int PocetFazi { get { return pocetFazi; } }
        public int PocetVrcholu { get { return pocetVrcholuSite - 2; } }

        public void ZaznamenejPocetHranVAktualniFazi(int pocet)
        {
            poctyHranParovani.Add(aktMaxTok = pocet);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Pocet vrcholu grafu: {0}\r\nVelikost maximalniho parovani: {1}\r\nPocet fazi: {2}\r\nPocty hran v jednotlivych fazich:", pocetVrcholuSite - 2, aktMaxTok, pocetFazi);
            sb.AppendLine();
            int faze = 1;
            foreach (int pocetHran in poctyHranParovani)
            {
                sb.AppendFormat("Faze {0}\t{1}", faze++, pocetHran);
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    public class Matrix<T> {
        T[] elements;
        readonly int radku, sloupcu;

        public Matrix(int m, int n) {
            radku = m;
            sloupcu = n;
            elements = new T[m * n];

            Clear();
        }

        public T GetElement(int i, int j) {
            return elements[i * sloupcu + j];
        }

        public void SetElement(int i, int j, T value) {
            elements[i * sloupcu + j] = value;
        }

        void Clear() {
            for (int i = 0; i < radku; ++i)
                for (int j = 0; j < sloupcu; ++j)
                    elements[i * sloupcu + j] = default(T);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for(int i = 0; i < radku; ++i)
            {
                for (int j = 0; j < sloupcu; ++j )
                    sb.AppendFormat("{0} ", elements[i * sloupcu + j]);

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
