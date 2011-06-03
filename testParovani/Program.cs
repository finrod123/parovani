using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using maxParovaniBipartGraph;

namespace testParovani {
    class Program
    {
        static void Main(string[] args)
        {
            string inFileName;
            string outFileName;

            if (args.Length != 2 || !File.Exists(inFileName = args[0]))
            {
                Console.WriteLine("Usage: progName <input filename> <output filename>");
                return;
            }

            outFileName = args[1];

            StreamReader sr = new StreamReader(inFileName);
            StreamWriter sw = new StreamWriter(new FileStream(outFileName, FileMode.Create));
            sw.WriteLine();

            int zpracovanoGrafu = 0;
            Dictionary<int, int> grafy2PoctyFazi = new Dictionary<int, int>(),
                                 grafy2PoctyGrafu = new Dictionary<int, int>();

            while (!sr.EndOfStream)
            {
                string graphString = sr.ReadLine().Trim();
                if (graphString.Length == 0)
                    continue;

                Sit sit = new Sit(graphString);
                DinitzRunSummary summary = sit.SpoctiMaximalniTok();

                int pocetVrcholu = summary.PocetVrcholu,
                    pocetFazi = summary.PocetFazi;
                
                if (!grafy2PoctyGrafu.ContainsKey(pocetVrcholu))
                {
                    grafy2PoctyGrafu.Add(pocetVrcholu, 0);
                    grafy2PoctyFazi.Add(pocetVrcholu, 0);
                }

                int staryPocetFazi = grafy2PoctyFazi[pocetVrcholu];

                grafy2PoctyGrafu[pocetVrcholu] = grafy2PoctyGrafu[pocetVrcholu] + 1;
                grafy2PoctyFazi[pocetVrcholu] = Math.Max(staryPocetFazi, pocetFazi);

                Console.WriteLine("Zpracovano grafu: {0}", ++zpracovanoGrafu);
                #if(INDIVIDUALGRAPHS)
                sw.WriteLine(summary.ToString());
                #endif
            }

            //sw.WriteLine();

            foreach(KeyValuePair<int, int> velikostGrafuPocetGrafu in grafy2PoctyGrafu)
            {
                sw.WriteLine("Velikost grafu ve skupine: {0}\tPocet grafu: {1}\tOcekavany nejhorsi pocet fazi: {2}\tNamereny nejhorsi pocet fazi: {3}",
                    new object[]{velikostGrafuPocetGrafu.Key,
                        velikostGrafuPocetGrafu.Value,
                        Math.Sqrt(velikostGrafuPocetGrafu.Key),
                        (double)grafy2PoctyFazi[velikostGrafuPocetGrafu.Key]});
            }

            Console.WriteLine("Done");

            sr.Close();
            sw.Close();

            Console.ReadLine();
        }


    }

}
