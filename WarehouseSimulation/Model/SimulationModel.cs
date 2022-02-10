using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Persistence;

namespace Model
{
    public class SimulationModel
    {
        #region Members
        private SimulationTable simTable; 
        private DataAccess persistence; 
        private CentralUnit cu;
        #endregion

        #region Properties
        public SimulationTable SimTable { get { return simTable; } }
        public CentralUnit CU { get { return cu; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Létrehozza a modell-t. Paraméterként van átadva neki a perzisztencia
        /// </summary>
        /// <param name="persistence">DataAccess típusú</param>
        public SimulationModel(DataAccess persistence)
        {
            this.persistence = persistence;
            this.End += this.persistence.log;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Ez a függvény hívja meg a betöltést végző függvényt és átadja a táblát a Central Unitnak.
        /// Illetve létrehozza az összes szükséges objektumot.
        /// </summary>
        /// <param name="path">string típusú, a fájl elérési útja amiből betöltünk</param>
        /// <returns></returns>
        public async Task load(string path)
        {
            if (persistence == null)
                throw new InvalidOperationException("No data access is provided.");

            simTable = (await persistence.loadFromFile(path));
            makeCU(simTable);

            //make objects like pod, robots etc.
            maker();

        }

        public void load(SimulationTable _table)
        {
            if (persistence == null)
                throw new InvalidOperationException("No data access is provided.");

            simTable = _table;
            makeCU(simTable);

            //make objects like pod, robots etc.
            maker();

        }
        /// <summary>
        /// Megviszgálja hogy vége van-e a szimulációnak. Ha igen akkor eseményt küld a view-nak 
        /// </summary>
        public void isEnd()
        {
            if (cu.isEnd())
            {
                int rCount = cu.Robots.Count;
                List<int> robotsE = new List<int>();

                for (int i = 0; i < rCount; i++)
                {
                    robotsE.Add(cu.Robots[i].SumEnergy);
                }

                onEnd(cu.Steps, robotsE);
            }
        }

        public Field[,] getTable()
        {
            return simTable.Table;
        }
        /// <summary>
        /// Elindítja a szimulációt
        /// </summary>
        public void start()
        {
            cu.start();
        }
        /// <summary>
        /// A tick hatására ez a függvény hívódik meg. Ez végzi a robotok léptetését
        /// </summary>
        public void move()
        {
            cu.stepAllRobotsWhenTheTimerTick();
            isEnd();
        }
        /// <summary>
        /// Visszadja a parméterként megadott koordinátán lévő robot id-ját.
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">Egész szám</param>
        /// <returns>Egész szám, a koordinátákkal megadott robot id-ja</returns>
        public int getRobotNumber(int x, int y)
        {
            int id = -1;
            foreach (Robot r in cu.Robots)
            {
                if (x == r.Position.x && y == r.Position.y)
                {
                    id = r.Id;
                }
            }
            return id;
        }
        /// <summary>
        /// Visszaadja a koordinátákkal megadott polcon lévő termékeket.
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">Egész szám</param>
        /// <returns>List<int> típusú, a koordinátákkal megadott polcon lévő termékek listája</returns>
        public List<int> getPodProducts(int x, int y)
        {
            foreach (Pod p in cu.Pods)
            {
                if (x == p.Position.x && y == p.Position.y)
                {
                    return p.Products;
                }
            }

            return null;
        }
        /// <summary>
        /// Visszadja a koordinátákkal megadott helyen lévő leadási hely id-ját.
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">Egész szám</param>
        /// <returns>Egész szám, a koordinátával megadott helyen lévő leadási hely id-ja</returns>
        public int getDestinationId(int x, int y)
        {
            foreach (Destination d in cu.Destinations)
            {
                if (d.Position.x == x && d.Position.y == y)
                {
                    return d.Id;
                }
            }

            return -1;
        }
        /// <summary>
        /// Visszadja a koordinátákkal megadott helyen lévő töltőhely id-ját.
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">Egész szám</param>
        /// <returns>Egész szám, a koordinátával megadott helyen lévő töltőhely id-ja</returns>
        public int getDockId(int x, int y)
        {
            foreach (Dock d in cu.Docks)
            {
                if (d.Position.x == x && d.Position.y == y)
                {
                    return d.Id;
                }
            }

            return -1;
        }

        /// <summary>
        /// Visszaadja a robotok számát.
        /// </summary>
        /// <returns>Egész szám, a robotok száma</returns>
        public int getRobotCount()
        {
            return cu.Robots.Count;
        }
        /// <summary>
        /// Visszaadja a robotok maximális töltöttségét. 
        /// </summary>
        /// <returns>Egész szám, max töltöttség.</returns>
        public int getRobotMax()
        {
            return simTable.RobotMax;
        }
        /// <summary>
        /// Visszaadja az id-val megadott robot töltöttségét
        /// </summary>
        /// <param name="id">Egész szám</param>
        /// <returns>Egész szám, töltöttség</returns>
        public int getEnergy(int id)
        {
            foreach (Robot r in cu.Robots)
            {
                if (r.Id == id)
                {
                    return r.Energy;
                }
            }

            return -1;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Meghívja az egyes objektumokot létrehozó függvényeket.
        /// </summary>
        private void maker()
        {
            cu.podMaker(simTable.PodPositions);
            cu.robotMaker(simTable.RobotPositions, simTable.RobotMax, simTable.RobotTable);
            cu.dockMaker(simTable.DockPositions);
            cu.destinationMaker(simTable.DestinationPositions);
        }
        /// <summary>
        /// Létrehozza a Central Unitot és átadja neki a táblát.
        /// </summary>
        /// <param name="simTable">SimulationTable típusú</param>
        private void makeCU(SimulationTable simTable)
        {
            cu = new CentralUnit(simTable.Table);
            cu.RefreshTable += new EventHandler(onRefreshTable);
        }
        #endregion

        #region Events
        /// <summary>
        /// Biztonságosan kiváltja a Refresh eseményt.
        /// </summary>
        public void onRefresh()
        {
            if (Refresh != null)
            {
                Refresh(this, new EventArgs());
            }
        }
        /// <summary>
        /// Biztonságosan kiváltja az End eseményt.
        /// </summary>
        /// <param name="steps"></param>
        /// <param name="robotsE"></param>
        public void onEnd(int steps, List<int> robotsE)
        {
            if (End != null)
            {
                End(this, new EndGameEventArgs(steps, robotsE));
            }
        }

        public event EventHandler Refresh;
        public event EventHandler<EndGameEventArgs> End;

        #endregion

        #region EventHandlers
        /// <summary>
        /// Frissíti az itt tárolt tábla adatait a Centarl unit adatai alapján.
        /// </summary>
        /// <param name="sender">Object típusú </param>
        /// <param name="e">Eventargs típusú</param>
        private void onRefreshTable(object sender, EventArgs e)
        {
            simTable.resetRobotTable();

            foreach (Robot r in cu.Robots)
            {
                if (simTable.isFreeSpot(r.Position.x, r.Position.y))
                {
                    simTable.setRobotTrue(r.Position.x, r.Position.y);
                }
            }

            foreach (Destination D in cu.Destinations)
            {
                simTable.setValue(D.Position.x, D.Position.y, FieldEnum.DESTINATION, D.Id, null);
            }

            foreach (Dock d in cu.Docks)
            {
                simTable.setValue(d.Position.x, d.Position.y, FieldEnum.DOCK, d.Id, null);
            }

            simTable.resetPodTable();

            foreach (Pod p in cu.Pods)
            {
                simTable.setValue(p.Position.x, p.Position.y, FieldEnum.POD, p.Id, p.Products);
            }

            onRefresh();
        }


        #endregion

    }
}
