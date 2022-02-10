using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Persistence
{
    public class FileDataAccess : DataAccess
    {
        /// <summary>
        /// Ez a függvény végzi el aszinkron módon az adatok fájlból való betöltését. Hibát dob, ha nem megfelelő a fájl.
        /// </summary>
        /// <param name="path">string, a betöltendő fájl útvonala</param>
        /// <returns>Task<SimulationTable> </returns>
        public async Task<SimulationTable> loadFromFile(string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line = await reader.ReadLineAsync();
                    string[] data = line.Split(' ');

                    int tableRow = int.Parse(data[0]);
                    int tableCol = int.Parse(data[1]);

                    SimulationTable table = new SimulationTable(tableRow, tableCol);

                    for (int i = 0; i < table.SizeX; i++)
                    {
                        line = await reader.ReadLineAsync();
                        data = line.Split(' ');

                        for (int j = 0; j < table.SizeY; j++)
                        {
                            setData(table,data,i, j);
                        }
                    }
                    line = await reader.ReadLineAsync();
                    data = line.Split(' ');

                    table.RobotMax = int.Parse(data[0]);

                    line = await reader.ReadLineAsync();
                    
                    int podId;
                    int productNumber;
                    while (line != null)
                    {
                        data = line.Split(' ');
                        podId = int.Parse(data[0]);
                        productNumber = int.Parse(data[1]);
                        List<int> productList = new List<int>();

                        int maxType = 0;

                        for (int j = 2; j < productNumber + 2; j++)
                        {
                            productList.Add(int.Parse(data[j]));

                            if (int.Parse(data[j]) > maxType)
                            {
                                  maxType = int.Parse(data[j]);
                            }
                        }
                        table.ProductsTypeNumber = maxType;
                        table.putPod(podId, productList);

                        line = await reader.ReadLineAsync();
                    }

                    if (table.isGood())
                         return table;
                    else
                        throw new FileDataException();
                }
            }
            catch
            {
                throw new FileDataException();
            }
        }
        /// <summary>
        /// Elvégzi a szimuláció végén a naplózást egy napló fájlba
        /// </summary>
        /// <param name="sender">Object, üldő objektum</param>
        /// <param name="args">EndGameEventArgs,ez tartalmazza megfelelő adatokat a szimulációról</param>
        public void log(object sender,EndGameEventArgs args)
        {
            StreamWriter sw = new StreamWriter("log.txt");
            sw.WriteLine(args.steps);

            int sum = 0;
            for (int i = 0; i < args.robotsE.Count; i++)
            {
                sw.WriteLine("Robot " + (i + 1) + ": " + args.robotsE[i]);
                sum += args.robotsE[i];
            }
            sw.WriteLine("SUM: " + sum);
            sw.Close();
        }
        /// <summary>
        /// Beállítja a Simulationtable-ben a tábla mezőinek típusait
        /// </summary>
        /// <param name="table">SimulationTable</param>
        /// <param name="data">string[], a mezők adatai</param>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        private void setData(SimulationTable table, string[] data, int i, int j)
        {
            switch (data[j])
            {
                case "f":
                    table.setFloor(i, j);
                    break;
                case "p":
                    table.setPod(i, j);
                    break;
                case "d":
                    table.setDock(i, j);
                    break;
                case "D":
                    table.setDestination(i, j);
                    break;
                case "r":
                    table.setRobotTrue(i, j); table.setFloor(i, j);
                    break;
            }
        }
    }

    
}

