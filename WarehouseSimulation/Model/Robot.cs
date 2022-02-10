using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Persistence;

namespace Model
{
    public class Robot
    {
        #region Members
        private int id;
        private int energy;
        private int maxEnergy;
        private int sumEnergy;
        private int state;
        private int stepCount;
        private Direction dir;
        private Coordinate position;
        private Coordinate destination;
        Coordinate originalPosition;
        private Pod _pod;
        private List<Char> path;
        private int chargeCounter;
        private RobotEnum[,] robotTable;

        //varakozási idot szamolo
        private int waitNumber;
        private int dockDistance;
        private int dockId;
        #endregion

        #region Properties
        public int Id { get { return id; } set { id = value; } }
        public int Energy { get { return energy; } set { energy = value; } }
        public int MaxEnergy { get { return maxEnergy; } set { maxEnergy = value; } }
        public int SumEnergy { get { return sumEnergy; } set { sumEnergy = value; } }
        public int State { get { return state; } set { state = value; } }
        public int ChargeCounter { get { return chargeCounter; } set { chargeCounter = value; } }
        public int StepCount { get { return stepCount; } set { stepCount = value; } }
        public Direction Dir { get { return dir; } set { dir = value; } }
        public Coordinate Position { get { return position; } set { position = value; } }
        public Coordinate Destination { get { return destination; } set { destination = value; } }
        public List<Char> Path { get { return path; } set { path = value; } }
        public RobotEnum[,] RobotTable { get { return robotTable; } set { robotTable = value; } }
        public int WaitNumber { get { return waitNumber; } set { waitNumber = value; } }

        public Pod Pod { get => _pod; set => _pod = value; }
        public int DockDistance { get => dockDistance; set => dockDistance = value; }
        public int DockId { get => dockId; set => dockId = value; }
        public Coordinate OriginalPosition { get => originalPosition; set => originalPosition = value; }

        #endregion

        #region Methods
        /// <summary>
        /// Konstuktor, létrehozza a Robot objektumot
        /// </summary>
        /// <param name="id">Egész szám, robot id-ja</param>
        /// <param name="maxEnergy">Egész szám ,robot mx energiája</param>
        /// <param name="posX">Egész szám</param>
        /// <param name="posY">Egész szám</param>
        /// <param name="d">Direction típusú, a robot kezdeti iránya.</param>
        /// <param name="robotTable">RobotEnum[,] típusú, ez a robotokat tartalmazó tábla</param>
        public Robot(int id, int maxEnergy, int posX, int posY, Direction d, RobotEnum[,] robotTable)
        {
            waitNumber = 0;
            this.robotTable = robotTable;

            this.id = id;
            this.maxEnergy = maxEnergy;
            this.Energy = maxEnergy;//??
            this.position.x = posX;
            this.position.y = posY;
            this.originalPosition.x = posX;
            this.originalPosition.y = posY;
            this.dir = d;

            path = new List<char>();

            sumEnergy = 0;
            state = 0;
            stepCount = 0;
            chargeCounter = 0;
        }
        /// <summary>
        /// Átállítja a robot irányát
        /// </summary>
        /// <param name="d">Direction típusú,az új irány</param>
        private void turn(Direction d)
        {
            dir = d;
        }
        /// <summary>
        /// Visszaadja hogy üres-e a robot útvonalát tartalmazó lista.
        /// </summary>
        /// <returns>Logikai érték</returns>
        public bool pathIsEmpty()
        {
            return path.Count == 0;
        }
        /// <summary>
        /// Minden lépéskor ez hívódik meg. Ellenőrzi hogy mi a következő elem az 
        /// útvonalat tartalmazó listából, és elvégzi az annak megfelelő műveletet.
        /// </summary>
        public void move()
        {
            if (state == 2 && currentPositionIsDestination())
            {
                chargeCounter++;
            }
            char next='q';
            if (path.Count > 0)
            {
                next = path[0];
                if ('m' == next)
                {
                    whenTheNextIsM();
                }
                else if ('r' == next)
                {
                    turningTheRobotAndManageEnergyChangeAndPathAndWaitnumber(Direction.RIGHT);
                }
                else if ('u' == next)
                {
                    turningTheRobotAndManageEnergyChangeAndPathAndWaitnumber(Direction.UP);
                }
                else if ('l' == next)
                {
                    turningTheRobotAndManageEnergyChangeAndPathAndWaitnumber(Direction.LEFT);
                }
                else if ('d' == next)
                {
                    turningTheRobotAndManageEnergyChangeAndPathAndWaitnumber(Direction.DOWN);
                }
                else if ('s' == next)
                {
                    whenTheNextIsS();
                }                
                else if ('a' == next)
                {                   
                    path.RemoveAt(0);
                    Pod.Products.RemoveAt(0);                    
                }
                checkWait();               
                if (next == 'm' && state!=2)
                {
                    if (currentPositionIsDestination())
                    {                        
                        SafeArrive();
                    }
                }                                 
            }
            checkCharge();
        }
        /// <summary>
        /// Fordítja a robotot a megfelelő irányba és állítja az energiáját is.
        /// </summary>
        /// <param name="dir">Direction, új irány. </param>
        private void turningTheRobotAndManageEnergyChangeAndPathAndWaitnumber(Direction dir)
        {
            turn(dir);
            manageEnergyAndPathAndWaitNumber();
        }
        /// <summary>
        /// Lépteti egyel előre a robotot a robot irányának megfelelelően.
        /// </summary>
        private void whenTheNextIsM()
        {
            if (dir == Direction.LEFT)
            {
                if (IsFreeSpot(position.x, position.y - 1))
                {                    
                    position.y--;
                    manageEnergyAndPathAndWaitNumber();                                        
                }
                else
                {
                    waitCounter();
                }
            }
            else if (dir == Direction.UP)
            {
                if (IsFreeSpot(position.x - 1, position.y))
                {   
                    position.x--;
                    manageEnergyAndPathAndWaitNumber();
                }
                else
                {
                    waitCounter();
                }
            }
            else if (dir == Direction.RIGHT)
            {
                if (IsFreeSpot(position.x, position.y + 1))
                {  
                    position.y++;
                    manageEnergyAndPathAndWaitNumber();
                }
                else
                {
                    waitCounter();
                }
            }
            else
            {
                if (IsFreeSpot(position.x + 1, position.y))
                {
                    position.x++;
                    manageEnergyAndPathAndWaitNumber();
                }
                else
                {
                    waitCounter();
                }
            }
        }
        /// <summary>
        /// Állítja a robot energiáját, és karbantartja az utat tartalmazó listát.
        /// </summary>
        private void manageEnergyAndPathAndWaitNumber()
        {
            path.RemoveAt(0);
            energy--;
            sumEnergy++;
            waitNumber = 0;
        }
        /// <summary>
        /// Felveszi, vagy leteszi a polcot ami alatt áll, a robot állapotától függően. Ha 1-es vagy 2-es állapotban van akkor leteszi, 
        /// ha 0-ás allapotban van akkkor felveszi. Illetve kiváltja a megfelelő eseményt. 
        /// </summary>
        private void whenTheNextIsS()
        {
            if (1 == state)
            {
                state = 0;
                Pod.State = 0;
                Pod = null;
                SafePodHome();
            }
            else if (0 == state)
            {
                state = 1;
            }
            else if (state == 2)
            {
                Pod.State = 0;
                if (!Pod.onOriginalPosition() || Pod.Products.Count > 0)
                {
                    SafeLeftPod();
                }
                Pod = null;
                SafePodHome();

            }
            manageEnergyAndPathAndWaitNumber();
        }
        /// <summary>
        /// Ellenőrzi a töltéssel kapcsolatos dolgokat, hogy mennie kell-e tölteni, vagy hogy feltöltött-e már.
        /// </summary>
        private void checkCharge()
        {
            if (needCharge() && state != 2)
            {
                SafeNeedCharge();
            }

            if (chargeCounter == 5)
            {
                energy = maxEnergy;
                chargeCounter = 0;
                state = 0;
                SafeCharged();
            }
        }
        /// <summary>
        /// Visszaadja hogy a jelenlegi pozíció a cél-e.
        /// </summary>
        /// <returns>Logikai érték.</returns>
        private bool currentPositionIsDestination()
        {
            return position.x == destination.x && position.y == destination.y;
        }
        /// <summary>
        /// Ellenőrzi hogy kell-e már új utat keresni konfliktus esetén,vagy még várhat.
        /// </summary>
        private void checkWait()
        {
            if (waitNumber == 5)
            {
                SafeNeedNewWay();
            }          
        }
        /// <summary>
        /// Biztonságosan kiváltja a NeedNewWay eseményt.
        /// </summary>
        private void SafeNeedNewWay()
        {
            if (NeedNewWay != null)
            {
                Coordinate oppRobotCor = oppositeRobotCoordinate();
                NeedNewWay(this, oppRobotCor);
            }
        }
        /// <summary>
        /// Visszaadja a robottal szmeben lévő robot koordinátáját
        /// </summary>
        /// <returns>Coordinate, szemben lévő robot coordinátája</returns>
        private Coordinate oppositeRobotCoordinate()
        {
            Coordinate coord = new Coordinate();
            if (this.dir == Direction.DOWN)
            {
                coord.x = this.position.x+1;
                coord.y = this.position.y;
            }
            else if (this.dir == Direction.UP)
            {
                coord.x = this.position.x - 1;
                coord.y = this.position.y;
            }
            else if (this.dir == Direction.LEFT)
            {
                coord.x = this.position.x;
                coord.y = this.position.y-1;
            }
            else if (this.dir == Direction.RIGHT)
            {
                coord.x = this.position.x;
                coord.y = this.position.y + 1;
            }
            return coord;
        }

       
        /// <summary>
        /// Számolja konfliktus esetén a várásokat.
        /// </summary>
        private void waitCounter()
        {
            if (waitNumber == 6)
            {
                waitNumber = 1;
            }
            else {
                waitNumber++;
            }            
        }
        /// <summary>
        /// Visszaadja hogy szabad-e az adott pozícióra lépnie a robotnak.
        /// </summary>
        /// <param name="x">Egész szám</param>
        /// <param name="y">egész szám</param>
        /// <returns>Logikai érték, szabad-e a mező</returns>
        private bool IsFreeSpot(int x, int y)
        {
            return robotTable[x, y] != RobotEnum.ROBOT;
        }
        /// <summary>
        /// Visszaadja hogy a robotnak kell-e tölteni mennie.
        /// </summary>
        /// <returns>Logikai érték, kell-e tölteni</returns>
        private bool needCharge()
        {
            return dockDistance >= energy;
        }
        #endregion

        #region Events
        public event EventHandler Arrive;
        public event EventHandler NeedCharge;
        public event EventHandler<Coordinate> NeedNewWay;
        public event EventHandler Charged;
        public event EventHandler LeftPod;
        public event EventHandler PodHome;
        
        /// <summary>
        /// Biztonságosan kiváltja az Arrive eseményt.
        /// </summary>
        private void SafeArrive()
        {
            if (null != Arrive)
            {
                Arrive(this, null);
            }
        }

        /// <summary>
        /// Biztonságosan kiváltja a PodHome eseményt.
        /// </summary>
        private void SafePodHome()
        {
            if (null != PodHome)
            {
                PodHome(this, null);
            }
        }
        /// <summary>
        /// Biztonságosan kiváltja a NeedCharge eseményt.
        /// </summary>
        private void SafeNeedCharge()
        {
            if (null != NeedCharge)
            {
                NeedCharge(this, null);
            }
        }
        /// <summary>
        /// Biztonságosan kiváltja a Charged eseményt.
        /// </summary>
        private void SafeCharged()
        {
            if (null != Charged)
            {
                Charged(this, null);
            }
        }
        /// <summary>
        /// Biztonságosan kiváltja a LeftPod eseményt.
        /// </summary>
        private void SafeLeftPod()
        {
            if(null != LeftPod)
            {
                LeftPod(this, null);
            }
        }
        #endregion
    }
}
