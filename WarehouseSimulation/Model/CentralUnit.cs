using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Persistence;

namespace Model
{
    class Location
    {
        public int X;
        public int Y;
        public int F;
        public int G;
        public int H;
        public Location Parent;
    }

    public class CentralUnit
    {
        #region Members
        private int steps;
        private List<Robot> robots;
        private List<Pod> pods;
        private List<Dock> docks;
        private List<Destination> destinations;

        private Field[,] table;
        #endregion

        #region Properties

        public int Steps { get { return steps; } }
        public List<Robot> Robots { get { return robots; } set { robots = value; } }
        public List<Pod> Pods { get { return pods; } set { pods = value; } }

        public List<Dock> Docks { get { return docks; } set { docks = value; } }
        public List<Destination> Destinations { get { return destinations; } set { destinations = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Létrehozza a Central unit-ot
        /// </summary>
        /// <param name="table">A raktár alaprajza a robotk nélkül</param>
        public CentralUnit(Field[,] table)
        {
            this.table = table;
            robots = new List<Robot>();
            pods = new List<Pod>();
            docks = new List<Dock>();
            destinations = new List<Destination>();

            steps = 0;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Elindítja 
        /// </summary>
        public void start()
        {
            searchWay();
        }
        /// <summary>
        /// Minden tick-re lépteti a robotokat és mindig kiszámolja a távolságot a legközelebbi töltőtől
        /// </summary>
        public void stepAllRobotsWhenTheTimerTick()
        {
            foreach (Robot robot in robots)
            {                
                int minDis=10000;
                int id=-1;
                foreach(Dock d in docks)
                {
                    if(calculateDockDistance(d,robot) < minDis)
                    {
                        minDis = calculateDockDistance(d,robot);
                        id = d.Id;
                    }
                }
                robot.DockDistance = minDis+Math.Max(table.GetLength(0), table.GetLength(1));
                robot.DockId = id;
                robot.move();
                //ha van a roboton polc, akkor a polc pozt atallitja a robotera
                if (robot.Pod != null)
                {
                    robot.Pod.Position = robot.Position;
                }   
                SafeRefreshTable();
            }
            steps++;
        }
        
        /// <summary>
        /// Létrehozzá a robotokat a coordinátájuk alapján 
        /// </summary>
        /// <param name="robot">A robotok kezdeti helyeit tartalmazó lista</param>
        /// <param name="maxE">A robotok max energiája</param>
        /// <param name="robotTable">A robot tábla ami tartalmazza a robotokat </param>
        public void robotMaker(List<Coordinate> robot, int maxE,RobotEnum[,] robotTable)
        {
            for (int i = 0; i < robot.Count; ++i)
            {
                this.robots.Add(new Robot(i+1, maxE, robot[i].x, robot[i].y, Direction.LEFT, robotTable));
            }

            foreach (Robot r in robots)
            {
                r.NeedNewWay += new EventHandler<Coordinate>(onNeedNewWayBecauseTheNextPositionIsRobot);
                r.Arrive += new EventHandler(onRobotArrive);
                r.NeedCharge += new EventHandler(onNeedCharge);
                r.Charged += new EventHandler(onCharged);
                r.LeftPod += new EventHandler(onLeftPod);
                r.PodHome += new EventHandler(onPodHome);
            }
        }
        /// <summary>
        /// Létrehozza a polcokat
        /// </summary>
        /// <param name="pod">A polcok koordinátáit tartalmazó lista</param>
        public void podMaker(List<Coordinate> pod)
        {
            for (int i = 0; i < pod.Count; ++i)
            {
                this.pods.Add(new Pod(pod[i].x, pod[i].y, table[pod[i].x, pod[i].y].Products, i + 1));
            }
        }
        /// <summary>
        /// Létrehozza a töltőket
        /// </summary>
        /// <param name="docks">A töltők koordinátáit tartalmazó lista</param>
        public void dockMaker(List<Coordinate> docks)
        {
            for (int i = 0; i < docks.Count; ++i)
            {
                this.docks.Add(new Dock(docks[i], i + 1));
            }
        }
        /// <summary>
        /// Létrehozza a leadási helyeket
        /// </summary>
        /// <param name="dests">A leadási helyek koordinátáit tartalmazó lista</param>
        public void destinationMaker(List<Coordinate> dests)
        {
            for (int i = 0; i < dests.Count; ++i)
            {
                destinations.Add(new Destination(dests[i], i + 1));
            }
        }
        /// <summary>
        /// Ellenőrzi hogy vége van-e a szimulációnak az által hogy megnézi, 
        /// hogy minden polc üres-e és hogy a helyükön vannak-e
        /// </summary>
        /// <returns>Bool érték,vége van-e a szimulációnak</returns>
        public bool isEnd()
        {
            foreach (Pod p in pods)
            {
                if (p.Products != null && (!podIsEmpty(p) || !p.onOriginalPosition()))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Ha egy robot letesz egy polcot akkor a többi robot, amelyeken van polc, új utat keres az aktuális célpontjához,
        /// mert egy új akadály került a raktárba amit ki kell kerülniek 
        /// </summary>
        /// <param name="sender">Az eseményt küldő objektum, nem haznált</param>
        /// <param name="e">Adat amit a küldő átküld, nem hazsnált</param>
        private void onPodHome(object sender, EventArgs e)
        {
            foreach (Robot r in robots)
            {
                if (r.State == 1)
                {

                    if (r.Path[0] == 'a')
                    {
                        makeWayWithoutBadcoord(r, r.Destination, true);
                        r.Path.Insert(0, 'a');
                    }
                    else if (r.Path[0] == 's')
                    {
                        makeWayWithoutBadcoord(r, r.Destination, true);
                        r.Path.Insert(0, 's');
                    }
                    else
                    {
                        makeWayWithoutBadcoord(r, r.Destination, true);
                    }
                }
            }
        }
        /// <summary>
        /// Amikor egy tölteni menő robot letesz egy polcot, akkor megnézi, hogy van-e szabad robot. 
        /// Ha van akkor annak utat keres a polchoz
        /// </summary>
        /// <param name="sender">Küldő objektum, itt egy robot</param>
        /// <param name="e">Adat amit a küldő átküld, nem használt</param>
        private void onLeftPod(object sender, EventArgs e)
        {
            Robot senderRobot = sender as Robot; 
            List<Robot> freeRobots = new List<Robot>();
            foreach(Robot r in robots)
            {
                if((r.State==0 && r.Path.Count==0) || r.State==3)
                {
                    freeRobots.Add(r);
                }
            }
            if (freeRobots.Count > 0)
            {
                freeRobots[0].Destination = senderRobot.Pod.Position;
                freeRobots[0].State = 0;
                makeWayWithoutBadcoord(freeRobots[0],freeRobots[0].Destination,false);
                foreach(Pod p in pods)
                {
                    if(p.Position.x==freeRobots[0].Destination.x && p.Position.y == freeRobots[0].Destination.y)
                    {
                        p.State = 1;
                        break;
                    }
                } 
            }
        }
        /// <summary>
        /// Amikor feltöltött a robot akkor megnézi hogy van-e még elszállítandó polc. 
        /// Ha van akkor utat keres hozzá. 
        /// Ha nincs akkor elmegy az eredeti helyére a robot  
        /// </summary>
        /// <param name="sender">Küldő objektum,itt egy robot</param>
        /// <param name="e">Adat amit a küldő átküld, nem használt</param>
        private void onCharged(object sender, EventArgs e)
        {
            Robot senderRobot = sender as Robot;
            bool findPod=false;
            foreach (Pod p in pods)
            {
                if((p.Products!=null && !podIsEmpty(p) && p.State==0) || (p.State==0 && !p.onOriginalPosition()))
                {
                    findPod = true;
                    senderRobot.Destination = p.Position;
                    p.State = 1;
                    makeWayWithoutBadcoord(senderRobot, senderRobot.Destination, false);
                    break;
                }
                
            }
            if (!findPod)
            {
                senderRobot.State = 3;
                senderRobot.Destination = senderRobot.OriginalPosition;
                makeWayWithoutBadcoord(senderRobot, senderRobot.Destination,false);
            }
        }
        /// <summary>
        /// A paraméterként átadott robotnak keres utat, a szintén paraméterként átadott célponthoz.
        /// Ez olyankor lehet ha nem konfliktus miatt keres másik utat, mert nincs rossz koordináta.
        /// </summary>
        /// <param name="robot">Egy Robot példány</param>
        /// <param name="destination">Egy Coordinate</param>
        /// <param name="podsAreBad">Bool érték</param>
        private void makeWayWithoutBadcoord(Robot robot,Coordinate destination,bool podsAreBad)
        {
            Coordinate c;
            c.x = -1;
            c.y = -1;
            makeWay(robot, destination, c, podsAreBad);
        }
        /// <summary>
        /// Megoldja a hogy a robot eljusson  atöltőhöz
        /// </summary>
        /// <param name="sender">Object típusú, a küldő objektum</param>
        /// <param name="e">Nincs használva</param>
        private void onNeedCharge(object sender, EventArgs e)
        {
            Robot senderRobot = sender as Robot;
            Dock theClosestDock=searchTheClosestDock(senderRobot);
            if (senderRobot.State == 0)
            {
                podStateManipulation(senderRobot);
            }
            makeWayWithoutBadcoord(senderRobot,theClosestDock.Position,false);
            bool onDestination=false;
            if (senderRobot.State == 1 )
            {
                //ha Destinationon állunk akkor ne azonnal tegye le!
                onDestination=robotOnDestinationWithAPod(senderRobot);
                if (!onDestination)
                    senderRobot.Path.Insert(0, 's');
                else
                {
                    robotDoNotPutDownPodOnDestination(senderRobot);
                }
            }
            senderRobot.State = 2;
        }
        /// <summary>
        /// Állítja a polc állapotát ahol a robot áll
        /// </summary>
        /// <param name="senderRobot">Robot típusú</param>
        private void podStateManipulation(Robot senderRobot)
        {
            foreach (Pod p in pods)
            {
                if (robotDestinationIsEqualToPodPosition(senderRobot, p))
                {
                    p.State = 0;
                    break;
                }
            }
        }
        /// <summary>
        /// Megállapítja hogy a polc pozíciója egyezik-e a robotéval
        /// </summary>
        /// <param name="senderRobot">Robot típusú</param>
        /// <param name="p">Pod típusú</param>
        /// <returns>Logikai (bool) érték</returns>
        private bool robotDestinationIsEqualToPodPosition( Robot senderRobot, Pod p)
        {
            return senderRobot.Destination.x == p.Position.x && senderRobot.Destination.y == p.Position.y;
        }
        /// <summary>
        /// Visszaadja a robothoz legközelebbi töltőhelyet(Dock)
        /// </summary>
        /// <param name="senderRobot">Robot típusú</param>
        /// <returns>Dock típusú</returns>
        private Dock searchTheClosestDock(Robot senderRobot)
        {
            foreach (Dock d in docks)
            {
                if (d.Id == senderRobot.DockId)
                {
                    return  d;
                    
                }
            }
            return null;
        }
        /// <summary>
        /// Biztosítja hogy a robot ne tegye le a polcot a leadási helyen (Destination)
        /// </summary>
        /// <param name="senderRobot">Robot típusú</param>
        private void robotDoNotPutDownPodOnDestination(Robot senderRobot)
        {
            if (senderRobot.Path[0] != 'm')
            {
                if (senderRobot.Path[1] != 'm')
                {
                    if (senderRobot.Path[2] == 'm')
                    {
                        senderRobot.Path.Insert(3, 's');
                    }
                }
                else
                {
                    senderRobot.Path.Insert(2, 's');
                }
            }
            else
            {
                senderRobot.Path.Insert(1, 's');
            }
        }
        /// <summary>
        /// Új utat keres a robotnak mert konfliktusba került egy másik robottal
        /// </summary>
        /// <param name="sender">Object típusú</param>
        /// <param name="badCoord">Coordinate típusú</param>
        private void onNeedNewWayBecauseTheNextPositionIsRobot(object sender, Coordinate badCoord)
        {
            Robot senderRobot = sender as Robot;
            Robot oppositRobot=searchOppositRobot(badCoord);            
            if (theOppositRobotGoCharge(oppositRobot) && theSenderRobotDoNotGoCharge(senderRobot) && theBadcoordIsNotTheSenderRobotDestination(badCoord, senderRobot))
            {
                makeWayWithBadCoord(senderRobot, badCoord);

            }
            else if((senderRobot.Id > oppositRobot.Id || oppositRobot.pathIsEmpty()) && theSenderRobotDoNotGoCharge(senderRobot) && theBadcoordIsNotTheSenderRobotDestination(badCoord,senderRobot))
            {
                makeWayWithBadCoord(senderRobot, badCoord);

            }
            else if(oppositRobot.pathIsEmpty() && theSenderRobotGoCharge(senderRobot))
            {
                makeWay(senderRobot, senderRobot.Destination, badCoord, false);
            }
            else if (theSenderRobotCoordIsTheOppositRobotDestination(senderRobot,oppositRobot)) 
            {
                makeWayWithBadCoord(senderRobot, badCoord);
            }

        }
        /// <summary>
        /// Visszaadja hogy a robot megy-e tölteni
        /// </summary>
        /// <param name="senderRobot">Robot típusú</param>
        /// <returns>Logikai érték (bool)</returns>
        private bool theSenderRobotGoCharge(Robot senderRobot)
        {
            return senderRobot.State == 2;
        }
        /// <summary>
        /// Utat keres a robotnak, amikor van rossz koordináta
        /// </summary>
        /// <param name="senderRobot">Robot típusú</param>
        /// <param name="badCoord">Coordinate típusú</param>
        private void makeWayWithBadCoord(Robot senderRobot, Coordinate badCoord)
        {
            if (senderRobot.State == 0)
            {
                makeWay(senderRobot, senderRobot.Destination, badCoord, false);
            }
            else
            {
                makeWay(senderRobot, senderRobot.Destination, badCoord, true);
            }
        }
        /// <summary>
        /// Megkeresi az adott koordinátán lévő robotot
        /// </summary>
        /// <param name="badCoord">Coordinate típusú</param>
        /// <returns>Robot típusú</returns>
        private Robot searchOppositRobot(Coordinate badCoord)
        {
            foreach (Robot robot in robots)
            {
                if (badCoordIsEqualToRobotPosition(robot, badCoord))
                {
                    return robot;
                }
            }
            return null;
        }
        private bool badCoordIsEqualToRobotPosition(Robot robot, Coordinate badCoord)
        {
            return badCoord.x == robot.Position.x && badCoord.y == robot.Position.y;
        }
        /// <summary>
        /// Utat keres az összes robotnak amikor elkezdődik a szimuláció
        /// </summary>
        private void searchWay()
        {
            List<Pod> podWithProd = new List<Pod>();

            foreach (Pod p in pods)
            {
                if (p.Products != null && p.State==0)
                {
                    podWithProd.Add(p);
                }
            }

            for (int i = 0; i < robots.Count && i < podWithProd.Count; i++)
            {
                makeWayWithoutBadcoord(robots[i], podWithProd[i].Position, false);
            }
        }
        /// <summary>
        /// Kiszámolja a heurisztikát
        /// </summary>
        /// <param name="x">Egész érték</param>
        /// <param name="y">Egész érték</param>
        /// <param name="targetX">Egész érték</param>
        /// <param name="targetY">Egész érték</param>
        /// <returns></returns>
        static int ComputeHScore(int x, int y, int targetX, int targetY)
        {
            return Math.Abs(targetX - x) + Math.Abs(targetY - y) + 1;
        }
        /// <summary>
        /// Ez a függvény az A* útkereső algoritmus implementációja. 
        /// Itt történik maga az útkeresés
        /// </summary>
        /// <param name="robot">Robot típusú</param>
        /// <param name="position">Coordinate típusú</param>
        /// <param name="badCord">Coordinate típusú</param>
        /// <param name="podsAreBad">Logikai érték (bool)</param>
        private void makeWay(Robot robot, Coordinate position,Coordinate badCord,bool podsAreBad)
        {
            robot.Destination = position;
            int x = table.GetLength(0);
            int y = table.GetLength(1);
            Location current = null;
            var start = new Location { X = robot.Position.x, Y = robot.Position.y };
            var target = new Location { X = position.x, Y = position.y };
            var openList = new List<Location>();
            var closedList = new List<Location>();
            int g = 0;
            openList.Add(start);
            while (openList.Count > 0)
            {
                var lowest = openList.Min(l => l.F);
                current = openList.First(l => l.F == lowest);
                closedList.Add(current);
                openList.Remove(current);
                if (closedList.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null)
                    break;

                var adjacentSquares = GetWalkableAdjacentSquares(current.X, current.Y, table,badCord, podsAreBad,robot);
                g++;
                iterateWalkableAdjacentSquares(ref adjacentSquares, ref closedList,ref openList,ref g, target,current);
            }
            createRobotPath(current,robot);
        }

        /// <summary>
        /// Átadja a robotnak a megkeresett utat,átalakítva a robot számára érthetővé
        /// </summary>
        /// <param name="current">Location típusú</param>
        /// <param name="robot">Robot típusú</param>
        private void createRobotPath(Location current, Robot robot)
        {
            List<Location> pathFromLocations = new List<Location>();
            while (current != null)
            {
                pathFromLocations.Add(current);
                current = current.Parent;
            }
            pathFromLocations.Reverse();
            List<Char> pathChar = new List<Char>();
            Direction direction = robot.Dir;
            for (int i = 1; i < pathFromLocations.Count; i++)
            {
                List<char> l = convertToChar(pathFromLocations[i - 1], pathFromLocations[i], ref direction);
                foreach (char c in l)
                {
                    pathChar.Add(c);
                }
            }
            robot.Path = pathChar;
        }
        /// <summary>
        /// Végigiterál azokon a mezőkön ahová lehet lépni 
        /// </summary>
        /// <param name="adjacentSquares">Location list, lehetséges lépések</param>
        /// <param name="closedList"></param>
        /// <param name="openList"></param>
        /// <param name="g"></param>
        /// <param name="target"></param>
        /// <param name="current"></param>
        private void iterateWalkableAdjacentSquares(ref List<Location> adjacentSquares, ref List<Location> closedList, ref List<Location> openList, ref int g,Location target, Location current )
        {
            foreach (var adjacentSquare in adjacentSquares)
            {
                // if this adjacent square is already in the closed list, ignore it
                if (closedList.FirstOrDefault(l => l.X == adjacentSquare.X
                        && l.Y == adjacentSquare.Y) != null)
                    continue;
                if (openList.FirstOrDefault(l => l.X == adjacentSquare.X
                        && l.Y == adjacentSquare.Y) == null)
                {
                    // compute its score, set the parent
                    adjacentSquare.G = g;
                    adjacentSquare.H = ComputeHScore(adjacentSquare.X, adjacentSquare.Y, target.X, target.Y);
                    adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                    adjacentSquare.Parent = current;
                    openList.Insert(0, adjacentSquare);
                }
                else
                {
                    // test if using the current G score makes the adjacent square's F score
                    // lower, if yes update the parent because it means it's a better path
                    if (g + adjacentSquare.H < adjacentSquare.F)
                    {
                        adjacentSquare.G = g;
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                        adjacentSquare.Parent = current;
                    }
                }
            }
        } 
        /// <summary>
        /// Átalakítja a location listát, amit az útkeresés gyártott, egy a robot számára értelmezhető karakter listává
        /// </summary>
        /// <param name="current">Location típusú, az aktuális helyzet </param>
        /// <param name="next"> Location típusú, a következő mező amire lépünk</param>
        /// <param name="direction">Direction típuzsú, a robot iránya</param>
        /// <returns></returns>
        private List<char> convertToChar(Location current, Location next, ref Direction direction )
        {
            List<char> charList=new List<char>(); 
            int xChange =next.X-current.X;
            int yChange = next.Y - current.Y;
            switch (direction) {
                case Direction.DOWN :
                    convertToCharWhenTheDirectionIsDown(ref charList, ref direction, xChange, yChange);
                    break;
                case Direction.UP:
                    convertToCharWhenTheDirectionIsUp(ref charList, ref direction, xChange, yChange);
                    break;
                case Direction.LEFT:
                    convertToCharWhenTheDirectionIsLeft(ref charList, ref direction, xChange, yChange);
                    break;
                case Direction.RIGHT:
                    convertToCharWhenTheDirectionIsRight(ref charList, ref direction, xChange, yChange);
                    break;
            }
            return charList;
        }
        /// <summary>
        /// Átalakítja a listát karakter listává,ha a robot jobbra néz
        /// </summary>
        /// <param name="charList">List<char> típusú</param>
        /// <param name="direction">Direction típusú</param>
        /// <param name="xChange">Egész szám</param>
        /// <param name="yChange">Egész szám</param>
        private void convertToCharWhenTheDirectionIsRight(ref List<char> charList, ref Direction direction, int xChange, int yChange)
        {
            if (yChange > 0)
            {
                charList.Add('m');

            }
            else if (xChange > 0)
            {
                charList.Add('d');
                charList.Add('m');
                direction = Direction.DOWN;
            }
            else if (yChange < 0)
            {
                charList.Add('d');
                charList.Add('l');
                charList.Add('m');
                direction = Direction.LEFT;
            }
            else if (xChange < 0)
            {
                charList.Add('u');
                charList.Add('m');
                direction = Direction.UP;
            }
        }
        /// <summary>
        /// Átalakítja a listát karakter listává,ha a robot felfelé néz
        /// </summary>
        /// <param name="charList">List<char> típusú</param>
        /// <param name="direction">Direction típusú</param>
        /// <param name="xChange">Egész szám</param>
        /// <param name="yChange">Egész szám</param>
        private void convertToCharWhenTheDirectionIsUp(ref List<char> charList, ref Direction direction, int xChange, int yChange)
        {
            if (xChange < 0)
            {
                charList.Add('m');

            }
            else if (yChange > 0)
            {
                charList.Add('r');
                charList.Add('m');
                direction = Direction.RIGHT;
            }
            else if (xChange > 0)
            {
                charList.Add('r');
                charList.Add('d');
                charList.Add('m');
                direction = Direction.DOWN;
            }
            else if (yChange < 0)
            {
                charList.Add('l');
                charList.Add('m');
                direction = Direction.LEFT;
            }
        }
        /// <summary>
        /// Átalakítja a listát karakter listává,ha a robot lefelé néz
        /// </summary>
        /// <param name="charList">List<char> típusú</param>
        /// <param name="direction">Direction típusú</param>
        /// <param name="xChange">Egész szám</param>
        /// <param name="yChange">Egész szám</param>
        private void convertToCharWhenTheDirectionIsDown(ref List<char> charList, ref Direction direction, int xChange, int yChange)
        {
            if (xChange > 0)
            {
                charList.Add('m');

            }
            else if (yChange < 0)
            {
                charList.Add('l');
                charList.Add('m');
                direction = Direction.LEFT;
            }
            else if (xChange < 0)
            {
                charList.Add('l');
                charList.Add('u');
                charList.Add('m');
                direction = Direction.UP;
            }
            else if (yChange > 0)
            {
                charList.Add('r');
                charList.Add('m');
                direction = Direction.RIGHT;
            }
        }
        /// <summary>
        /// Átalakítja a listát karakter listává,ha a robot balra néz
        /// </summary>
        /// <param name="charList">List<char> típusú</param>
        /// <param name="direction">Direction típusú</param>
        /// <param name="xChange">Egész szám</param>
        /// <param name="yChange">Egész szám</param>
        private void convertToCharWhenTheDirectionIsLeft(ref List<char> charList, ref Direction direction, int xChange,int yChange)
        {
            if (yChange < 0)
            {
                charList.Add('m');

            }
            else if (xChange < 0)
            {
                charList.Add('u');
                charList.Add('m');
                direction = Direction.UP;
            }
            else if (yChange > 0)
            {
                charList.Add('u');
                charList.Add('r');
                charList.Add('m');
                direction = Direction.RIGHT;
            }
            else if (xChange > 0)
            {
                charList.Add('d');
                charList.Add('m');
                direction = Direction.DOWN;
            }
        }
        /// <summary>
        /// Visszaadja a robot számára járható mezőket, amikor utat keres
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">Egész szám</param>
        /// <param name="table">Field[,] típusú</param>
        /// <param name="badCoord">Coordinate típusú</param>
        /// <param name="podsAreBad">Logikai érték (bool)</param>
        /// <param name="robot">Robot típusú</param>
        /// <returns></returns>
        private List<Location> GetWalkableAdjacentSquares(int x, int y, Field[,] table,Coordinate badCoord,bool podsAreBad, Robot robot)
        {
            var proposedLocations = new List<Location>()
            {
                new Location { X = x, Y = y - 1 },
                new Location { X = x, Y = y + 1 },
                new Location { X = x - 1, Y = y },
                new Location { X = x + 1, Y = y },
            };
            if (podsAreBad)
            {
                
                if (badCoord.x == -1 && badCoord.y == -1)
                {
                    return proposedLocations.Where(l => (theCoordinateIsOnTable(l) && 
                    theCoordinateIsNotPod(l)) ||
                    theCoordIsRobotDestinationAndIsAPod(l,robot)).ToList();
                    
                }
                else
                {
                    return proposedLocations.Where(l => (theCoordinateNotEqualBadCoord(l,badCoord) && 
                    theCoordinateIsOnTable(l) && 
                    theCoordinateIsNotPod(l)) || 
                    theCoordIsRobotDestinationAndIsAPod(l,robot)).ToList();
                    
                }
            }
            else {

                if (badCoord.x == -1 && badCoord.y == -1)
                {
                    return proposedLocations.Where(l => theCoordinateIsOnTable(l)).ToList();
                }
                else
                {
                    return proposedLocations.Where(l => theCoordinateNotEqualBadCoord(l,badCoord) && theCoordinateIsOnTable(l)).ToList();
                }


            }
        }
        
        
        /// <param name="l">Location típusú</param>
        /// <param name="robot">Robot típusú</param>
        /// <returns>Logikai érték, a koordináta a robot célja és ott egy polc van </returns>
        private bool theCoordIsRobotDestinationAndIsAPod(Location l, Robot robot)
        {
            return l.X == robot.Destination.x && l.Y == robot.Destination.y && table[l.X, l.Y].Type == FieldEnum.POD;
        }
        /// <summary>
        /// Kiszámolja a robot töltőtől való távolságát
        /// </summary>
        /// <param name="d">Dock típusú</param>
        /// <param name="r">Robot típusú</param>
        /// <returns>Egész szám, A robot töltőtől való távolsága</returns>
        private int calculateDockDistance(Dock d, Robot r)
        {
            return Math.Abs(d.Position.x - r.Position.x) + Math.Abs(d.Position.y - r.Position.y);
        }
        /// <summary>
        /// Megállapítja hogy üres-e a polc
        /// </summary>
        /// <param name="pod"></param>
        /// <returns>Logikai érték, üres-e a polc</returns>
        private bool podIsEmpty(Pod pod)
        {
            return pod.Products.Count == 0;
        }
        /// <summary>
        /// Visszaadja hogy az adott koordináta a táblán van-e
        /// </summary>
        /// <param name="pos">Location típusú</param>
        /// <returns>Logikai érték, a koordináta a táblán van-e</returns>
        private bool theCoordinateIsOnTable(Location pos)
        {
            return (pos.X >= 0 && pos.X < table.GetLength(0)) && (pos.Y >= 0 && pos.Y < table.GetLength(1));
        }
        /// <summary>
        /// Megvizsgálja hogy a koordináta megegyezik-e a rossz koordinátával
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="badCoord"></param>
        /// <returns>Logikai érték, a koordináta a rossz koordináta-e</returns>
        private bool theCoordinateNotEqualBadCoord(Location pos, Coordinate badCoord)
        {
            return !(pos.X == badCoord.x && pos.Y == badCoord.y);
        }
        /// <summary>
        /// Visszadja hogy a koordinátán van-e polc
        /// </summary>
        /// <param name="pos">Location típusú</param>
        /// <returns>Logikai érték, a koordináta polc-e</returns>
        private bool theCoordinateIsNotPod(Location pos)
        {
            return table[pos.X, pos.Y].Type != FieldEnum.POD;
        }
        /// <summary>
        /// Visszadja hogy a paraméterként megadott robot töteni megy-e, ha nem akkor igaz, ha igen akkor hamis
        /// </summary>
        /// <param name="senderRobot">Robot típusú</param>
        /// <returns>Logikai érték, A robot tölteni megy-e</returns>
        private bool theSenderRobotDoNotGoCharge(Robot senderRobot)
        {
            return senderRobot.State != 2;
        }
        /// <summary>
        /// A robot megy-e tölteni
        /// </summary>
        /// <param name="oppositRobot">Robot típusú</param>
        /// <returns>Logikai érték</returns>
        private bool theOppositRobotGoCharge(Robot oppositRobot)
        {
            return oppositRobot.State == 2;
        }
        /// <summary>
        /// Visszaadja hogy a rossz koordináta robot célja-e  
        /// </summary>
        /// <param name="badCoord">Coordinate típusú</param>
        /// <param name="senderRobot">Robot típusú</param>
        /// <returns>Logikai érték, a cél egyezik-e a rossz koordinátával</returns>
        private bool theBadcoordIsNotTheSenderRobotDestination(Coordinate badCoord, Robot senderRobot)
        {
            return !(badCoord.x == senderRobot.Destination.x && badCoord.y == senderRobot.Destination.y);
        }
        /// <summary>
        /// Visszaadja hogy a robot senderRobot koordinátája az oppositRobot célja-e 
        /// </summary>
        /// <param name="senderRobot">Robot típusú</param>
        /// <param name="oppositRobot">Robot típusú</param>
        /// <returns>Logikai érték</returns>
        private bool theSenderRobotCoordIsTheOppositRobotDestination(Robot senderRobot, Robot oppositRobot)
        {
            return oppositRobot.Destination.x == senderRobot.Position.x && oppositRobot.Destination.y == senderRobot.Position.y;
        }
        /// <summary>
        /// Visszaadja hogy a robot leadási helyen van-e
        /// </summary>
        /// <param name="senderRobot">Robot típusú</param>
        /// <returns>Logikai érték, leadási helyen van-e a robot</returns>
        private bool robotOnDestinationWithAPod(Robot senderRobot)
        {
            foreach (Destination D in destinations)
            {
                if (senderRobot.Position.x == D.Position.x && senderRobot.Position.y == D.Position.y)
                {
                    return true;
                }

            }
            return false;
        }
        #endregion

        #region Events
        /// <summary>
        /// Ezt az esemény akkor váltódik ki ha frissíteni kell a táblát 
        /// </summary>
        public event EventHandler RefreshTable;


        
        
        /// <summary>
        /// Biztonságosan kiváltja a RefreshTable eseményt
        /// </summary>
        private void SafeRefreshTable()
        {
            if (null != RefreshTable)
            {
                RefreshTable(this, null);
            }
        }

       
        
        public void onRobotArrive(object sender, EventArgs e)
        {
           
            Robot senderRobot = sender as Robot;
            
            
            // még nincs a robotnál polc, vagyis polchoz érkezett
            if (senderRobot.State == 0)
            {
                
                foreach (Pod p in pods)
                {
                    if (p.Position.x == senderRobot.Position.x && p.Position.y == senderRobot.Position.y)
                    {
                        //itt tudja h emelve van es roboté lesz
                        p.State = 1;
                        senderRobot.Pod = p;
                        break;
                    }
                }

                //_robot.liftShelf(id);
                if (senderRobot.Pod!=null && senderRobot.Pod.Products!=null && senderRobot.Pod.Products.Count > 0)
                {
                    foreach (Destination d in destinations)
                    {
                        if (d.Id == senderRobot.Pod.Products[0])
                        {
                            senderRobot.Destination = d.Position;
                            break;
                        }
                    }
                }
                else
                {
                    senderRobot.Destination = senderRobot.Pod.OriginalPosition;
                }
                makeWayWithoutBadcoord(senderRobot, senderRobot.Destination, true);
                //itt "emeli" robot
                senderRobot.Path.Insert(0, 's');

            } 
            //már van a robotnál polc, így leadási helyhez érkezett
            else if (senderRobot.State == 1 && !podIsEmpty(senderRobot.Pod))
            {
                
                foreach (Destination d in destinations)
                {
                    //még van termék a polcon, így a következő leadási helyhez megy
                    if (senderRobot.Pod.Products.Count>1 && d.Id == senderRobot.Pod.Products[1])
                    {
                        senderRobot.Destination = d.Position;
                        break;
                    }
                    // már üres a polc így visszaviszi a polcot a helyére
                    else
                    {
                        senderRobot.Destination = senderRobot.Pod.OriginalPosition;
                    }
                }
                makeWayWithoutBadcoord(senderRobot, senderRobot.Destination, true);
                senderRobot.Path.Insert(0, 'a');
            }

            //megérkezett a polc eredeti helyéhez, és leteszi az üres polcot
            else if (senderRobot.State == 1  && podIsEmpty(senderRobot.Pod) ) {
                List<Pod> podWithProd = new List<Pod>();

                foreach (Pod p in pods)
                {
                    if (p.Products !=null  && p.Products.Count>0 && p.State == 0)
                    {
                        podWithProd.Add(p);
                    }
                }

                if (podWithProd.Count > 0)
                {
                    senderRobot.Destination = podWithProd[0].Position;
                    podWithProd[0].State = 1;
                    makeWayWithoutBadcoord(senderRobot, senderRobot.Destination, false);
                }
                //leteszi
                senderRobot.Path.Insert(0, 's');

            }else if (senderRobot.State == 3)
            {
                //hazament
                senderRobot.State = 0;
            }

        }
        #endregion
    }
}
