using System;
using System.Collections.Generic;

namespace Persistence
{
    public struct Coordinate
    {
        public int x;
        public int y;
    }
    public class SimulationTable
    {
        #region Members
        private List<Coordinate> podPositions;
        private List<Coordinate> robotPositions;
        private List<Coordinate> dockPositions;
        private List<Coordinate> destinationPositions;

        private int sizeX;
        private int sizeY;
        private Field[,] table;
        private RobotEnum[,] robotTable;

        private int robotMax;

        private int podCounter;
        private int robotCounter;
        private int destinationCounter;
        private int dockCounter;
        private int productsTypeNumber;
        #endregion

        #region Properties
        public List<Coordinate> PodPositions { get { return podPositions; } }
        public List<Coordinate> RobotPositions { get { return robotPositions; } }
        public List<Coordinate> DockPositions { get { return dockPositions; } }
        public List<Coordinate> DestinationPositions { get { return destinationPositions; } }

        public int SizeX { get { return sizeX; } set { sizeX = value; } }
        public int SizeY { get { return sizeY; } set { sizeY = value; } }

        public Field[,] Table { get { return table; } set { table = value; } }
        public RobotEnum[,] RobotTable { get { return robotTable; } set { robotTable = value; } }

        public int RobotMax { get { return robotMax; } set { robotMax = value; } }
        public int ProductsTypeNumber { get { return productsTypeNumber; } set { productsTypeNumber = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Konstruktor, létrehoz egy SimulationTable objektumot és inicializálja a táblaméretet. Emellett 0-val inicializálja a 
        /// benne található változókat és létrehozza az egyes elemek helyeit tartalmazó listákat
        /// </summary>
        /// <param name="X">Egész szám, a tábla szélessége</param>
        /// <param name="Y">Egész szám, a tábla magassága</param>
        public SimulationTable(int X, int Y)
        {
            table = new Field[X, Y];//csere
            robotTable = new RobotEnum[X, Y];
            sizeX = X;
            sizeY = Y;

            podPositions = new List<Coordinate>();
            robotPositions = new List<Coordinate>();
            dockPositions = new List<Coordinate>();
            destinationPositions = new List<Coordinate>();

            robotCounter = 0;
            destinationCounter = 0;
            podCounter = 0;
            dockCounter = 0;
            productsTypeNumber = 0;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Visszaadja a megadott mezőn lévő Field objektumot
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">Egész szám</param>
        /// <returns>Field típusú</returns>
        public Field getValue(int x, int y)
        {
            if (x < 0 || x >= table.GetLength(0))
                throw new ArgumentOutOfRangeException("x", "The X coordinate is out of range.");
            if (y < 0 || y >= table.GetLength(1))
                throw new ArgumentOutOfRangeException("y", "The Y coordinate is out of range.");

            return table[x, y];
        }
        /// <summary>
        /// Beállítja az összes mező típusát a táblán Nothing értékre
        /// </summary>
        public void setFields()
        {
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    setValue(i, j, FieldEnum.NOTHING, -1, null);  
                }
            }
        }
        /// <summary>
        /// Visszaadja a megadott koordinátán lévő RobotEnum értéket.
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">Egész szám</param>
        /// <returns>RobotEnum, a mező értéke</returns>
        public RobotEnum getRobot(int x, int y)
        {
            return robotTable[x, y];
        }
        /// <summary>
        /// Beállítja a robot táblában a megadott koordinátán található mező értékét robotra és hozzáadja a koordinátát a robotok listájához.
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">Egész szám</param>
        public void setRobotTrue(int x, int y)
        {
            robotTable[x, y] = RobotEnum.ROBOT;
            robotCounter++;
            Coordinate c;
            c.x = x;
            c.y = y;
            robotPositions.Add(c);
        }
        /// <summary>
        /// Visszaadja hogy a megadott koordináta szabad-e, vagyis hogy nincs ott robot.
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">Egész szám</param>
        /// <returns>Logikai érték, a mező nem robot</returns>
        public bool isFreeSpot(int x, int y)
        {
            return robotTable[x, y] != RobotEnum.ROBOT;
        }
        /// <summary>
        /// Ellenőrzi a betölteni kívánt táblát, hogy megfelel-e a feltételeknek. Ha igen akkor igazat ad, ha nem akkor hamisat. 
        /// </summary>
        /// <returns>Logikai érték, megfelelő-e a tábla</returns>
        public bool isGood()
        {
            return (! (somethingInTheCorners() || crowdedPods() || podsInTheEdge() || docksInTheEdge() ||
                       destinationsInTheEdge() || badDocksOrDestinPositions()|| isTooSmallTable() ||
                       isTooBigTable() || isBadMaxEnergy()  || isTooManyRobots() || isNotEnoughDestinations()) );
        }

        public void setPodTrue(int x, int y)
        {
            table[x, y].Type = FieldEnum.POD;
        }
        /// <summary>
        /// A robot tábla összes mezőjének értékét Nothing-ra állítja és törli a robotok helyeinek listájából az értékeket.
        /// </summary>
        public void resetRobotTable ()
        {
            for (int i = 0; i < robotTable.GetLength(0); i++)
            {
                for (int j = 0; j < robotTable.GetLength(1); j++)
                {
                    robotTable[i, j] = RobotEnum.NOTHING;
                }
            }
            robotCounter = 0;
            robotPositions.Clear();
        }
        /// <summary>
        /// Annak a táblának, ami a robotokon kívül minden más értéket tárol, az összes mezőjének az értékét Nothing-ra állítja.
        /// </summary>
        public void resetPodTable()
        {
            for (int i = 0; i < table.GetLength(0); i++)
            {
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    if (table[i, j].Type == FieldEnum.POD)
                    {
                        table[i, j].Type = FieldEnum.NOTHING;
                    }
                }
            }
        }
        /// <summary>
        /// Létrehoz egy új Field objektumot a paraméterek alapján és ezt állítja be a tábla x,y-ik elemének értékéül
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">Egész szám</param>
        /// <param name="value">Fieldenum, a mező értéke</param>
        /// <param name="id">Egész szám, a mező id-ja, ha van</param>
        /// <param name="products">Egész számok listája, tartalmazza a termékeket,ha vannak </param>
        public void setValue(int x, int y, FieldEnum value, int id, List<int> products)
        {
            if (x < 0 || x >= table.GetLength(0))
                throw new ArgumentOutOfRangeException("x", "The X coordinate is out of range.");
            if (y < 0 || y >= table.GetLength(1))
                throw new ArgumentOutOfRangeException("y", "The Y coordinate is out of range.");
            if (value < 0)
                throw new ArgumentOutOfRangeException("value", "The value is out of range.");

            table[x, y] = new Field(value, id, products);
        }
        /// <summary>
        /// Átadja a polcnak a termékek listáját
        /// </summary>
        /// <param name="podNumber">Egész szám, polc id</param>
        /// <param name="products">Egész számok listája, tartalmazza a termékeket</param>
        public void putPod(int podNumber, List<int> products)
        {
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    if (table[i, j].Id == podNumber)
                    {
                        table[i, j].Products = products;
                    }
                }
            }
        }
        /// <summary>
        /// Beállítja az adott mező értékét Nothing-ra, ami azt jelenti hogy ott csak padló van.
        /// </summary>
        /// <param name="i">Egész szám </param>
        /// <param name="j">Egész szám</param>
        public void setFloor(int i, int j)
        {
            setValue(i, j, FieldEnum.NOTHING, -1, null); 
        }
        /// <summary>
        /// Beállítja az adott mező értékét Pod-ra, ami azt jelenti hogy ott polc van. Illetve hozzáadja a koordinátát a polcok koordinátáit 
        /// tartalmazó listához
        /// </summary>
        /// <param name="i">Egész szám </param>
        /// <param name="j">Egész szám</param>
        public void setPod(int i, int j)
        {
            podCounter++;
            setValue(i, j, FieldEnum.POD, podCounter, null);  
            Coordinate c;
            c.x = i;
            c.y = j;
            PodPositions.Add(c);
        }
        /// <summary>
        /// Beállítja az adott mező értékét Dock-ra, ami azt jelenti hogy ott töltő van. Illetve hozzáadja a koordinátát a töltők koordinátáit
        /// tartalmazó listához
        /// </summary>
        /// <param name="i">Egész szám </param>
        /// <param name="j">Egész szám</param>
        public void setDock(int i, int j)
        {
            dockCounter++;
            setValue(i, j, FieldEnum.DOCK, dockCounter, null);  
            Coordinate c;
            c.x = i;
            c.y = j;
            dockPositions.Add(c);
        }
        /// <summary>
        /// Beállítja az adott mező értékét Destination-re, ami azt jelenti hogy ott leadási hely van. 
        /// Illetve hozzáadja a koordinátát a leadási helyek koordinátáit
        /// tartalmazó listához
        /// </summary>
        /// <param name="i">Egész szám </param>
        /// <param name="j">Egész szám</param>
        public void setDestination(int i, int j)
        {
            destinationCounter++;
            setValue(i, j, FieldEnum.DESTINATION, destinationCounter, null); 
            Coordinate c;
            c.x = i;
            c.y = j;
            destinationPositions.Add(c);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Elvégzi a töltők és a leadási helyek helyzetére való megszorítások ellenőrzését. 
        /// Visszaadja hogy megfelelő helyen vannak-e a leadási helyek és a töltők.
        /// </summary>
        /// <returns>Logikai érték, jó helyen vannak a töltők és a leadási helyek</returns>
        private bool badDocksOrDestinPositions()
        {
            int firstDesx = destinationPositions[0].x;
            int firstDesy = destinationPositions[0].y;
            int lastDesx = destinationPositions[destinationPositions.Count - 1].x;
            int lastDesy = destinationPositions[destinationPositions.Count - 1].y;
            bool commonDesX = false;
            bool commonDesY = false;

            int firstDockx = dockPositions[0].x;
            int firstDocky = dockPositions[0].y;
            int lastDockx = dockPositions[dockPositions.Count - 1].x;
            int lastDocky = dockPositions[dockPositions.Count - 1].y;
            bool commonDockX = false;
            bool commonDockY = false;

            return ((!commonDestinationLine(firstDesx, lastDesx, firstDesy, lastDesy, ref commonDesX, ref commonDesY)) ||
                 (!commonDockLine(firstDockx, lastDockx, firstDocky, lastDocky, ref commonDockX, ref commonDockY)) ||
                 (!correctSameColumnDockAndDes(commonDesY, commonDockY, firstDesy, firstDocky, lastDesy, lastDocky)) ||
                 (!correctSameRowDockAndDes(commonDesX, commonDockX, firstDesx, firstDockx, lastDesx, lastDockx)) ||
                 (!notCrowdedDestinations(commonDesX, commonDesY, firstDesx, lastDesx, firstDesy, lastDesy)) ||
                 (!notCrowdedDocks(commonDockX, commonDockY, firstDockx, lastDockx, firstDocky, lastDocky)) ||
                 (!docksCorrectInSameRow(commonDockX, commonDockY, firstDockx, lastDockx, firstDocky, lastDocky)) ||
                 (!docksCorrectInSameColumn(commonDockX, commonDockY, firstDockx, lastDockx, firstDocky, lastDocky)) ||
                 (!destinationsCorrectInSameRow(commonDesX, commonDesY, firstDesx, lastDesx, firstDesy, lastDesy)) ||
                 (!destinationsCorrectInSameColumn(commonDesX, commonDesY, firstDesx, lastDesx, firstDesy, lastDesy)));
        }
        /// <summary>
        /// Ellenőrzi hogy a leadási helyek egy oszlopban vannak-e
        /// </summary>
        /// <param name="commonDesX">Logikai érték, a leadási helyek egy sorban vannak-e</param>
        /// <param name="commonDesY">Logikai érték, a leadási helyek egy oszlopban vannak-e</param>
        /// <param name="firstDesx">Egész szám</param>
        /// <param name="lastDesx">Egész szám</param>
        /// <param name="firstDesy">Egész szám</param>
        /// <param name="lastDesy">Egész szám</param>
        /// <returns>Logikai érték, egy oszlopban vannak-e a leadási helyek</returns>
        private bool destinationsCorrectInSameColumn(bool commonDesX, bool commonDesY, int firstDesx, int lastDesx, int firstDesy, int lastDesy)
        {
            if (commonDesY)
            {
                if (firstDesy == 0)
                {
                    for (int i = firstDesx; i <= lastDesx; i++)
                    {
                        if (table[i, firstDesy].Type == FieldEnum.DESTINATION)
                        {
                            if (table[i, firstDesy + 1].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }
                }
                else if (firstDesy == sizeY - 1)
                {
                    for (int i = firstDesx; i <= lastDesx; i++)
                    {
                        if (table[i, firstDesy].Type == FieldEnum.DESTINATION)
                        {
                            if (table[i, firstDesy - 1].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Ellenőrzi hogy a leadási helyek egy sorban vannak-e.
        /// </summary>
        /// <param name="commonDesX">Logikai érték, a leadási helyek egy sorban vannak-e</param>
        /// <param name="commonDesY">Logikai érték, a leadási helyek egy oszlopban vannak-e</param>
        /// <param name="firstDesx">Egész szám</param>
        /// <param name="lastDesx">Egész szám</param>
        /// <param name="firstDesy">Egész szám</param>
        /// <param name="lastDesy">Egész szám</param>
        /// <returns>Logikai érték, egy sorban vannak-e a leadási helyek</returns>
        private bool destinationsCorrectInSameRow(bool commonDesX, bool commonDesY, int firstDesx, int lastDesx, int firstDesy, int lastDesy)
        {
            if (commonDesX)
            {
                if (firstDesx == 0)
                {
                    for (int i = firstDesy; i <= lastDesy; i++)
                    {
                        if (table[firstDesx, i].Type == FieldEnum.DESTINATION)
                        {
                            if (table[firstDesx + 1, i].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }
                }
                else if (firstDesx == sizeX - 1)
                {
                    for (int i = firstDesy; i <= lastDesy; i++)
                    {
                        if (table[firstDesx, i].Type == FieldEnum.DESTINATION)
                        {
                            if (table[firstDesx - 1, i].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Visszaadja hogy a töltők között van-e hely kihagyva ha egy oszlopban vannak
        /// </summary>
        /// <param name="commonDockX">Logikai érték, a töltők egy sorban vannak-e</param>
        /// <param name="commonDockY">Logikai érték, a töltők egy oszlopban vannak-e</param>
        /// <param name="firstDockx">Egész szám</param>
        /// <param name="lastDockx">Egész szám</param>
        /// <param name="firstDocky">Egész szám</param>
        /// <param name="lastDocky">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool docksCorrectInSameColumn(bool commonDockX, bool commonDockY, int firstDockx, int lastDockx, int firstDocky, int lastDocky)
        {
            if (commonDockY)
            {
                if (firstDocky == 0)
                {
                    for (int i = firstDockx; i <= lastDockx; i++)
                    {
                        if (table[i, firstDocky].Type == FieldEnum.DOCK)
                        {
                            if (table[i, firstDocky + 1].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }
                }
                else if (firstDocky == sizeY - 1)
                {
                    for (int i = firstDockx; i <= lastDockx; i++)
                    {
                        if (table[i, firstDocky].Type == FieldEnum.DOCK)
                        {
                            if (table[i, firstDocky - 1].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }

                }
            }
            return true;

        }
        /// <summary>
        /// Visszaadja hogy a töltők között van-e hely ha egy sorban vannak
        /// </summary>
        /// <param name="commonDockX">Logikai érték, a töltők egy sorban vannak-e</param>
        /// <param name="commonDockY">Logikai érték, a töltők egy oszlopban vannak-e</param>
        /// <param name="firstDockx">Egész szám</param>
        /// <param name="lastDockx">Egész szám</param>
        /// <param name="firstDocky">Egész szám</param>
        /// <param name="lastDocky">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool docksCorrectInSameRow(bool commonDockX, bool commonDockY, int firstDockx, int lastDockx, int firstDocky, int lastDocky)
        {
            if (commonDockX)
            {
                if (firstDockx == 0)
                {
                    for (int i = firstDocky; i <= lastDocky; i++)
                    {
                        if (table[firstDockx, i].Type == FieldEnum.DOCK)
                        {
                            if (table[firstDockx + 1, i].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }

                        }
                    }
                }
                else if (firstDockx == sizeX - 1)
                {
                    for (int i = firstDocky; i <= lastDocky; i++)
                    {
                        if (table[firstDockx, i].Type == FieldEnum.DOCK)
                        {
                            if (table[firstDockx - 1, i].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        ///Visszaadja hogy a töltők között van-e ures hely.
        /// </summary>
        /// <param name="commonDockX">Logikai érték, a töltők egy sorban vannak-e</param>
        /// <param name="commonDockY">Logikai érték, a töltők egy oszlopban vannak-e</param>
        /// <param name="firstDockx">Egész szám</param>
        /// <param name="lastDockx">Egész szám</param>
        /// <param name="firstDocky">Egész szám</param>
        /// <param name="lastDocky">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool notCrowdedDocks(bool commonDockX, bool commonDockY, int firstDockx, int lastDockx, int firstDocky, int lastDocky)
        {
            if (commonDockX)
            {
                for (int i = firstDocky; i <= lastDocky; i++)
                {
                    if (table[firstDockx, i].Type == FieldEnum.DOCK)
                    {
                        if (i != lastDocky)
                        {
                            if (table[firstDockx, i + 1].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (commonDockY)
            {
                for (int i = firstDockx; i <= lastDockx; i++)
                {
                    if (table[i, firstDocky].Type == FieldEnum.DOCK)
                    {
                        if (i != lastDockx)
                        {
                            if (table[i + 1, firstDocky].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Visszaadja hogy a leadási helyek között van-e ures hely.
        /// </summary>
        /// <param name="commonDockX">Logikai érték, a töltők egy sorban vannak-e</param>
        /// <param name="commonDockY">Logikai érték, a töltők egy oszlopban vannak-e</param>
        /// <param name="firstDockx">Egész szám</param>
        /// <param name="lastDockx">Egész szám</param>
        /// <param name="firstDocky">Egész szám</param>
        /// <param name="lastDocky">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool notCrowdedDestinations(bool commonDesX, bool commonDesY, int firstDesx, int lastDesx, int firstDesy, int lastDesy)
        {
            if (commonDesX)
            {
                for (int i = firstDesy; i <= lastDesy; i++)
                {
                    if (table[firstDesx, i].Type == FieldEnum.DESTINATION)
                    {
                        if (i != lastDesy)
                        {
                            if (table[firstDesx, i + 1].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (commonDesY)
            {
                for (int i = firstDesx; i <= lastDesx; i++)
                {
                    if (table[i, firstDesy].Type == FieldEnum.DESTINATION)
                    {
                        if (i != lastDesx)
                        {
                            if (table[i + 1, firstDesy].Type != FieldEnum.NOTHING)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private bool correctSameRowDockAndDes(bool commonDesX, bool commonDockX, int firstDesx, int firstDockx, int lastDesx, int lastDockx)
        {
            if (commonDesX && commonDockX)
            {
                int xDiff = firstDesx - firstDockx;

                if (xDiff > 0)
                {
                    foreach (Coordinate d in dockPositions)
                    {
                        if (!(d.x < firstDesx))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    foreach (Coordinate d in dockPositions)
                    {
                        if (!(d.x > lastDesx))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool correctSameColumnDockAndDes(bool commonDesY, bool commonDockY, int firstDesy, int firstDocky, int lastDesy, int lastDocky)
        {
            if (commonDesY && commonDockY)
            {
                int yDiff = firstDesy - firstDocky;

                if (yDiff > 0)
                {
                    foreach (Coordinate d in dockPositions)
                    {
                        if (!(d.y < firstDesy))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    foreach (Coordinate d in dockPositions)
                    {
                        if (!(d.y > lastDesy))
                        {
                            return false;

                        }
                    }
                }
            }

            return true;

        }



        private bool commonDockLine(int firstDockx, int lastDockx, int firstDocky, int lastDocky, ref bool commonDockX, ref bool commonDockY)
        {

            if (firstDockx != lastDockx)
            {
                if (firstDocky != lastDocky)
                {
                    return false;
                }
                else
                {
                    foreach (Coordinate d in dockPositions)
                    {
                        if (firstDocky != d.y)
                            return false;
                    }
                    commonDockY = true;
                }
            }
            else
            {
                foreach (Coordinate d in dockPositions)
                {
                    if (firstDockx != d.x)
                        return false;
                }
                commonDockX = true;
            }
            return true;
        }

        private bool commonDestinationLine(int firstDesx, int lastDesx, int firstDesy, int lastDesy, ref bool commonDesX, ref bool commonDesY)
        {
            if (firstDesx != lastDesx)
            {
                if (firstDesy != lastDesy)
                {
                    return false;
                }
                else
                {
                    foreach (Coordinate D in destinationPositions)
                    {
                        if (firstDesy != D.y)
                            return false;
                    }
                    commonDesY = true;
                }
            }
            else
            {
                foreach (Coordinate D in destinationPositions)
                {
                    if (firstDesx != D.x)
                        return false;
                }
                commonDesX = true;
            }
            return true;
        }
        /// <summary>
        /// Nincs elég leadási hely. Vagyis több féle termék van mint leadási hely.
        /// </summary>
        /// <returns>Logikai érték</returns>
        private bool isNotEnoughDestinations()
        {
            return (!(destinationCounter >= productsTypeNumber));
        }
        /// <summary>
        /// Visszaadja hogy túl sok robot van-e. Itt a túl sok robot azt jelenti hogy több robot van mint polc
        /// </summary>
        /// <returns>Logikai érték</returns>
        private bool isTooManyRobots()
        {
            return (robotCounter > podCounter);
        }
        /// <summary>
        /// Visszaadja hogy a robotok max energiája megfelelő-e
        /// </summary>
        /// <returns></returns>
        private bool isBadMaxEnergy()
        {
            return (!(robotMax >= ((sizeX * sizeY) / 2) + 15));
        }
        /// <summary>
        /// Visszaadja hogy túl nagy-e a tábla
        /// </summary>
        /// <returns>Logikai érték</returns>
        private bool isTooBigTable()
        {
            return (sizeX > 15 || sizeY > 20);
        }
        /// <summary>
        /// Visszaadja hogy túl kicsi-e a tábla.
        /// </summary>
        /// <returns>Logikai érték</returns>
        private bool isTooSmallTable()
        {
            return (sizeX < 5 || sizeY < 5);
        }
        /// <summary>
        /// Van 
        /// </summary>
        /// <returns></returns>
        private bool destinationsInTheEdge()
        {
            foreach (Coordinate D in destinationPositions)
            {
                if (!(D.x == 0 || D.y == 0 || D.x == sizeX - 1 || D.y == sizeY - 1))
                    return true;
            }
            return false;
        }
        
        private bool docksInTheEdge()
        {
            foreach (Coordinate d in dockPositions)
            {
                if (!(d.x == 0 || d.y == 0 || d.x == sizeX - 1 || d.y == sizeY - 1))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Visszaadja hogy van-e a szélen polc
        /// </summary>
        /// <returns>Logikai érték</returns>
        private bool podsInTheEdge()
        {
            foreach (Coordinate p in podPositions)
            {
                if (p.x == 0 || p.y == 0 || p.x == sizeX - 1 || p.y == sizeY - 1)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Vissszaadja hogy van-e olyan hely ami körbe van véve polcokkal
        /// </summary>
        /// <returns>Logikai érték</returns>
        private bool crowdedPods()
        {
            for (int i = 0; i < sizeX - 2; i++)
            {
                for (int j = 0; j < sizeY - 2; j++)
                {
                    if (podInTheMiddle(i, j))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Visszaadja hogy üresek-e a sarkok
        /// </summary>
        /// <returns>Logikai érték</returns>
        private bool somethingInTheCorners()
        {
            return (table[0, 0].Type != FieldEnum.NOTHING ||
                table[sizeX - 1, sizeY - 1].Type != FieldEnum.NOTHING ||
                    table[sizeX - 1, 0].Type != FieldEnum.NOTHING ||
                        table[0, SizeY - 1].Type != FieldEnum.NOTHING);
        }
        /// <summary>
        /// Visszaadja, hogy az adott koordináta körbe van-e véve polcokkal.
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool podInTheMiddle(int i, int j)
        {
            if (table[i, j].Type == FieldEnum.POD &&
                 table[i, j + 1].Type == FieldEnum.POD &&
                    table[i, j + 2].Type == FieldEnum.POD)
            {
                if (table[i + 1, j].Type == FieldEnum.POD &&
                table[i + 1, j + 1].Type == FieldEnum.POD &&
                table[i + 1, j + 2].Type == FieldEnum.POD)
                {
                    if (table[i + 2, j].Type == FieldEnum.POD &&
                    table[i + 2, j + 1].Type == FieldEnum.POD &&
                    table[i + 2, j + 2].Type == FieldEnum.POD)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
