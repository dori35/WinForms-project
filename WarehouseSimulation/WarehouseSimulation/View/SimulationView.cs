using System;
using System.Drawing;
using System.Windows.Forms;
using Model;
using Persistence;

namespace WarehouseSimulation
{
    public partial class SimulationView : Form
    {
        #region Members
        private SimulationModel model;
        private Button[,] buttonGrid;
        private ListBox list;
        private Timer timer;
        private OpenFileDialog openFileDialog;
        private Button start;
        private Button stop;
        private Button slowBtn;
        private Button normalBtn;
        private Button fastBtn;
        private int speed;
        private bool isStarted;
        #endregion

        #region Constructor
        /// <summary>
        /// Konstruktor,létrehozza a SimulationView objektumot.
        /// </summary>
        public SimulationView()
        {
            model = new SimulationModel(new FileDataAccess());
            
            InitializeComponent();

            timer = new Timer();
            speed = 0;
            start = new Button();
            stop = new Button();
            start.Enabled = false;
            startItem.Enabled = false;
            stopItem.Enabled = false;
            openFileDialog = new OpenFileDialog();
            slowBtn = new Button();
            normalBtn = new Button();
            fastBtn = new Button();
            
            start.Click += new EventHandler(onStartClick);
            startItem.Click += new EventHandler(onStartClick);
            stop.Click += new EventHandler(onStopClick);
            stopItem.Click += new EventHandler(onStopClick);
            timer.Tick += new EventHandler(timerTick);
            model.Refresh += new EventHandler(onRefresh);
            slowBtn.Click += new EventHandler(slowSpeed);
            normalBtn.Click += new EventHandler(normalSpeed);
            fastBtn.Click += new EventHandler(fastSpeed);
            slowBtn.TabStop = false;

            model.End += new EventHandler<EndGameEventArgs>(onTheEnd);
        }



        #endregion

        #region EventHandlers
        /// <summary>
        /// Kezeli az end eseményt. Kiírja hogy vége a szimulációnak és leállítja a szimulációt.
        /// </summary>
        /// <param name="sender">Object, ez a küldő objektum</param>
        /// <param name="e">EndGameEventArgs</param>
        private void onTheEnd(object sender, EndGameEventArgs e)
        {
            timer.Stop();
            stop.Enabled = false;
            stopItem.Enabled = false;
            start.Enabled = false;
            startItem.Enabled = false;
            MessageBox.Show("The robots have completed their tasks", "TheEnd");
        }
        /// <summary>
        /// Kezeli a Refresh eseményt. Frissíti az ablakot. 
        /// </summary>
        /// <param name="sender">Object, küldő</param>
        /// <param name="e">EventArgs</param>
        private void onRefresh(object sender, EventArgs e)
        {
            updateTable();
            setupList();
        }
        /// <summary>
        /// Kezeli a timer tick eseményét. Meghívja a robotok léptetéséért felelős függvényt.
        /// </summary>
        /// <param name="sender">Object, küldő</param>
        /// <param name="e">EventArgs</param>
        private void timerTick(object sender, EventArgs e)
        {
            model.move();
        }
        /// <summary>
        /// Kezeli azt ha a felhasználó rákattint a stop gombra. Megállítja a szimulációt.  
        /// </summary>
        /// <param name="sender">Object, küldő</param>
        /// <param name="e">EventArgs</param>
        private void onStopClick(object sender, EventArgs e)
        {
            timer.Stop();
            stop.Enabled = false;
            stopItem.Enabled = false;
            start.Enabled = true;
            startItem.Enabled = true;
        }

        /// <summary>
        /// Kezeli azt ha a felhasználó rákattint a start gombra. Elindítja a szimulációt.  
        /// </summary>
        /// <param name="sender">Object, küldő</param>
        /// <param name="e">EventArgs</param>
        private void onStartClick(object sender, EventArgs e)
        {
            start.Enabled = false;
            startItem.Enabled = false;
            stop.Enabled = true;
            stopItem.Enabled = true;
            if (speed == 0) {
                speed = 1000;
            }

            timer.Interval = speed;

            setupSpeedMenu();

            if (!isStarted)
            {
                model.start();
                isStarted = true;
            }

            timer.Start();
        }
        /// <summary>
        /// Kezeli azt ha a felhasználó rákattint a Load gombra. Megjelenik egy fájlbetöltő ablak, 
        /// ahol ki kell választani a betöteni kívánt fájlt. Ezután betölti ezt.  
        /// </summary>
        /// <param name="sender">Object, küldő</param>
        /// <param name="e">EventArgs</param>
        private async void loadFromFile(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                start.Enabled = true;

                try
                {
                    if (buttonGrid != null)
                    {
                        deleteTable();
                    }

                    if (list != null)
                    {
                        deleteListBox();
                    }

                    await model.load(openFileDialog.FileName);

                    generateTable();
                    generateList();
                    updateTable();
                    setupList();
                    setupSpeedMenu();
                    isStarted = false;
                    start.Enabled = true;
                    startItem.Enabled = true;
                    
                }
                catch (Exception)
                {
                    MessageBox.Show("Szimuláció betöltése sikertelen!" + Environment.NewLine + "Hibás az elérési út, vagy a fájlformátum.", "Hiba!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    start.Enabled = false;
                    startItem.Enabled = false;
                }
            }
        }
        /// <summary>
        /// Kezeli azt ha a felhasználó rákattint a Fast gombra. Gyorsra állítja a szimulációt.  
        /// </summary>
        /// <param name="sender">Object, küldő</param>
        /// <param name="e">EventArgs</param>
        private void fastSpeed(object sender, EventArgs e)
        {
            speed = 250;
            timer.Interval = speed;
            setupSpeedMenu();
        }
        /// <summary>
        /// Kezeli azt ha a felhasználó rákattint a Normal gombra. Normal sebességre állítja a szimulációt.  
        /// </summary>
        /// <param name="sender">Object, küldő</param>
        /// <param name="e">EventArgs</param>
        private void normalSpeed(object sender, EventArgs e)
        {
            speed = 1000;
            timer.Interval = speed;
            setupSpeedMenu();
        }

        /// <summary>
        /// Kezeli azt ha a felhasználó rákattint a Slow gombra. Lassúra allítja a szimulációt.  
        /// </summary>
        /// <param name="sender">Object, küldő</param>
        /// <param name="e">EventArgs</param>
        private void slowSpeed(object sender, EventArgs e)
        {
            speed = 2000;
            timer.Interval = speed;
            setupSpeedMenu();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Frissíti a robotok listájának a megjelenítését.
        /// </summary>
        private void setupList()
        {
            for (int i = 0; i < model.getRobotCount(); i++)
            {
                list.Items[i]= (i + 1) + ". Robot: " + model.getEnergy(i+1) + " / " + model.getRobotMax();
            }
        }
        /// <summary>
        /// Létrehozza a robotok megjelenítését lehetővé tevő listát
        /// </summary>
        private void generateList()
        {
            int h = model.SimTable.SizeX;
            int w = model.SimTable.SizeY;

            list = new ListBox();
            list.Location = new Point(w * 50, 25);
            list.Size = new Size(300, h / 2 * 50);

            for (int i = 0; i < model.getRobotCount(); i++)
            {
                list.Items.Add((i + 1) + ". Robot: " + model.getEnergy(i + 1) + " / " + model.SimTable.RobotMax);
            }

            Controls.Add(list);
        }

        /// <summary>
        /// Annak függvényében hogy épp melyik menüpont van kiválasztva a sebességnél, beállítja a kijelölését
        /// </summary>
        private void setupSpeedMenu()
        {
            slowItem.Checked = speed == 2000;
            normalItem.Checked = speed == 1000;
            fastItem.Checked = speed == 250;

            slowBtn.TabStop = speed == 2000;
            normalBtn.TabStop = speed == 1000;
            fastBtn.TabStop = speed == 250;

            if (speed == 2000)
            {
                slowBtn.Focus();
            }

            if (speed == 1000)
            {
                normalBtn.Focus();
            }

            if (speed == 250)
            {
                fastBtn.Focus();
            }
        }
        /// <summary>
        /// Frissíti a táblát
        /// </summary>
        private void updateTable()
        {
            tableWithoutProducts();
            addProducts();

            Invalidate();
        }
        /// <summary>
        /// Hozzáadja a táblához a termékeket. Megjeleníti őket a polcokon.
        /// </summary>
        private void addProducts()
        {
            for (int i = 0; i < buttonGrid.GetLength(0); i++)
            {
                for (int j = 0; j < buttonGrid.GetLength(1); j++)
                {
                    if (hasPodProducts(i, j))
                    {
                        toSeeProducts(i, j);
                    }
                }
            }
        }
        /// <summary>
        /// Beállítja a tábla tartalmát, kivéve a termékeket
        /// </summary>
        private void tableWithoutProducts()
        {
            for (int i = 0; i < buttonGrid.GetLength(0); i++)
            {
                for (int j = 0; j < buttonGrid.GetLength(1); j++)
                {
                    if (isNothing(i, j))
                    {
                        toSeeNothing(i, j);
                    }
                    else if (isDock(i, j))
                    {
                        toSeeDock(i, j);
                    }
                    else if (isDestination(i, j))
                    {
                        toSeeDestination(i, j);
                    }
                    else if (isPod(i, j)) 
                    {
                        toSeePod(i, j);
                    }
                    
                    if (isRobot(i, j))
                    {
                        toSeeRobot(i, j);
                        robotColorChangeSpecialPlaces(i,j);
                    }
                }
            }
        }
        /// <summary>
        /// Állítja a háttér színét annak függvényében hogy épp milyen mezőn halad keresztül a robot keresztül.
        /// Sárga - üres mezők
        /// Piros - polcos mezők
        /// Sötétkék - töltőhely
        /// világos zöld - leadási hely
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        private void robotColorChangeSpecialPlaces(int i, int j)
        {
            if (robotUnderPod(i, j))
            {
                buttonGrid[i, j].BackColor = Color.Red;
            }
            else if (robotOnDock(i, j))
            {
                buttonGrid[i, j].BackColor = Color.DeepSkyBlue;
            }
            else if (robotOnDestination(i, j))
            {
                buttonGrid[i, j].BackColor = Color.LightGreen;
            }
        }
        /// <summary>
        /// Visszaadja hogy az adott koordinátán van-e robot
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        /// <returns>Logikai érték, van-e a koordinátán robot</returns>
        private bool isRobot(int i, int j)
        {
            return model.SimTable.getRobot(i, j) == RobotEnum.ROBOT;
        }
        /// <summary>
        /// Visszaadja hogy a megadott mező polc-e
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        /// <returns>Logikai érték, a megadott koordinátán van-e polc</returns>
        private bool isPod(int i, int j)
        {
            return model.SimTable.getValue(i, j).Type == FieldEnum.POD;
        }
        /// <summary>
        /// Visszaadja hogy a megadott mező leadási hely-e
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool isDestination(int i, int j)
        {
            return model.SimTable.getValue(i, j).Type == FieldEnum.DESTINATION;
        }

        /// <summary>
        /// Visszaadja hogy az adott mező töltő-e
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool isDock(int i, int j)
        {
            return model.SimTable.getValue(i, j).Type == FieldEnum.DOCK;
        }
        /// <summary>
        /// Visszaadja hogy az adott mező üres-e
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool isNothing(int i, int j)
        {
            return model.SimTable.getValue(i, j).Type == FieldEnum.NOTHING;
        }
        /// <summary>
        /// Visszaadja hogy az adott mezőn lévő polcon van-e termék
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool hasPodProducts(int i, int j)
        {
            return model.getPodProducts(i, j) != null;
        }
        /// <summary>
        /// A koordinátával megadott polcra ráírja az ott található termékeket.
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        private void toSeeProducts(int i, int j)
        {
            foreach (int product in model.getPodProducts(i, j))
            {
                if (buttonGrid[i, j].Text == "P")
                {
                    buttonGrid[i, j].Text += "\n" + product;
                }
                else
                {
                    buttonGrid[i, j].Text += "," + product;
                }
            }
        }
        /// <summary>
        /// Visszadja hogy a robot a leadási helyen van-e
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool robotOnDestination(int i, int j)
        {
           return model.SimTable.getValue(i, j).Type == FieldEnum.DESTINATION;
        }
        /// <summary>
        /// Visszadja hogy a robot a Töltőhelyen van-e helyen van-e
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool robotOnDock(int i, int j)
        {
            return model.SimTable.getValue(i, j).Type == FieldEnum.DOCK;
        }
        /// <summary>
        /// Visszadja hogy a robot a polc alatt van-e
        /// </summary>
        /// <param name="i">Egész szám</param>
        /// <param name="j">Egész szám</param>
        /// <returns>Logikai érték</returns>
        private bool robotUnderPod(int i, int j)
        {
           return  model.SimTable.getValue(i, j).Type == FieldEnum.POD;
        }
        /// <summary>
        /// Beállítja az adott mezőt sárgára és beállítja szövegnek a robot id-ját
        /// </summary>
        /// <param name="i">Egész zsám</param>
        /// <param name="j">Egész szám</param>
        private void toSeeRobot(int i, int j)
        {
            string number;
            buttonGrid[i, j].BackColor = Color.Yellow;
            number = Convert.ToString(model.getRobotNumber(i, j));
            buttonGrid[i, j].Text = "R" + number;
        }
        /// <summary>
        /// Beállítja az adott mező helyét szürkére és beállítja szövegnek hogy P
        /// </summary>
        /// <param name="i">Egész zsám</param>
        /// <param name="j">Egész szám</param>
        private void toSeePod(int i, int j)
        {
            buttonGrid[i, j].BackColor = Color.Gray;
            buttonGrid[i, j].Text = "P";
        }
        /// <summary>
        /// Beállítja az adott mező színét zöldre és beállítja szövegnek a leadási hely id-ját
        /// </summary>
        /// <param name="i">Egész zsám</param>
        /// <param name="j">Egész szám</param>
        private void toSeeDestination(int i, int j)
        {
            string number;
            buttonGrid[i, j].BackColor = Color.Green;
            number = Convert.ToString(model.getDestinationId(i, j));
            buttonGrid[i, j].Text = "S" + number;
        }
        /// <summary>
        /// Beállítja az adott mező színét világos kékre és beállítja szövegnek a töltő id-ját
        /// </summary>
        /// <param name="i">Egész zsám</param>
        /// <param name="j">Egész szám</param>
        private void toSeeDock(int i, int j)
        {
            string number;
            buttonGrid[i, j].BackColor = Color.LightBlue;
            number = Convert.ToString(model.getDockId(i, j));
            buttonGrid[i, j].Text = "D" + number;
        }
        /// <summary>
        /// Beállítja az adott mező színét fehérre  és beállítja a szöveget üres szövegnek
        /// </summary>
        /// <param name="i">Egész zsám</param>
        /// <param name="j">Egész szám</param>
        private void toSeeNothing(int i, int j)
        {
            buttonGrid[i, j].BackColor = Color.White;
            buttonGrid[i, j].Text = "";
        }
        /// <summary>
        /// Létrehozza a táblát amit látunk majd a szimuláció során , illetve a kezelő gombokat 
        /// </summary>
        private void generateTable()
        {
            int h = model.SimTable.SizeX;
            int w = model.SimTable.SizeY;

            buttonGrid = new Button[h, w];
            tableButtonMaker(h,w);

            Size = new Size(w * 50 + 340, h * 50 + 75);

            if (w >= 8 && h >= 8)
            {
                biggerTableStartButtonMaker(h, w);
                biggerTableStopButtonMaker(h, w);
                biggerTableSlowButtonMaker(h, w);
                biggerTableNormalButtonMaker(h, w);
                biggerTableFastButtonMaker(h, w);
            }
            else
            {
                smallerTableStartButtonMaker(h, w);
                smallerTableStopButtonMaker(h, w);
                smallerableSlowButtonMaker(h, w);
                smallerableNormalButtonMaker(h, w);
                smallerTableFastButtonMaker(h, w);
            }
        }
        /// <summary>
        /// Létrehozza kisebb tábla esetén a gyors sebességet beállító gombot.
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void smallerTableFastButtonMaker(int h, int w)
        {
            fastBtn.Size = new Size(60, 40);
            fastBtn.Location = new Point(slowBtn.Location.X + 200, start.Location.Y + 50);
            fastBtn.Text = "Fast";
            fastBtn.BackColor = Color.White;
            Controls.Add(fastBtn);
        }
        /// <summary>
        /// Létrehozza kisebb tábla esetén a normál sebességet beállító gombot.
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void smallerableNormalButtonMaker(int h, int w)
        {
            normalBtn.Size = new Size(60, 40);
            normalBtn.Location = new Point(slowBtn.Location.X + 100, start.Location.Y + 50);
            normalBtn.Text = "Normal";
            normalBtn.BackColor = Color.White;
            Controls.Add(normalBtn);
        }
        /// <summary>
        /// Létrehozza kisebb tábla esetén a stop gombot.
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void smallerTableStopButtonMaker(int h, int w)
        {
            stop.Size = new Size(200, 40);
            stop.Location = new Point((w + 1) * 50, (3 * h / 4) * 50 - 15);
            stop.Text = "Stop";
            stop.BackColor = Color.White;
            Controls.Add(stop);
            stop.Enabled = false;
        }
        /// <summary>
        /// Létrehozza kisebb tábla esetén a lassú sebességet beállító gombot.
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void smallerableSlowButtonMaker(int h, int w)
        {
            slowBtn.Size = new Size(60, 40);
            slowBtn.Location = new Point((w + 1) * 50 - 30, start.Location.Y + 50);
            slowBtn.Text = "Slow";
            slowBtn.BackColor = Color.White;
            Controls.Add(slowBtn);
        }
        /// <summary>
        /// Létrehozza kisebb tábla esetén a start gombot.
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void smallerTableStartButtonMaker(int h, int w)
        {
            start.Size = new Size(200, 40);
            start.Location = new Point((w + 1) * 50, (3 * h / 4) * 50 + 25);
            start.Text = "Start";
            start.BackColor = Color.White;
            Controls.Add(start);
        }
        /// <summary>
        /// Létrehozza nagyobb tábla esetén a gyors sebességet beállító gombot.
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void biggerTableFastButtonMaker(int h, int w)
        {
            fastBtn.Size = new Size(80, 50);
            fastBtn.Location = new Point(slowBtn.Location.X + 200, start.Location.Y + 60);
            fastBtn.Text = "Fast";
            fastBtn.BackColor = Color.White;
            Controls.Add(fastBtn);
        }
        /// <summary>
        /// Létrehozza nagyobb tábla esetén a normál sebességet beállító gombot.
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void biggerTableNormalButtonMaker(int h, int w)
        {
            normalBtn.Size = new Size(80, 50);
            normalBtn.Location = new Point(slowBtn.Location.X + 100, start.Location.Y + 60);
            normalBtn.Text = "Normal";
            normalBtn.BackColor = Color.White;
            Controls.Add(normalBtn);
        }
        /// <summary>
        /// Létrehozza nagyobb tábla esetén a lassú sebességet beállító gombot.
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void biggerTableSlowButtonMaker(int h, int w)
        {
            slowBtn.Size = new Size(80, 50);
            slowBtn.Location = new Point(start.Location.X - 40, start.Location.Y + 60);
            slowBtn.Text = "Slow";
            slowBtn.BackColor = Color.White;
            Controls.Add(slowBtn);
        }
        /// <summary>
        /// Létrehozza nagyobb tábla esetén a stop gombot
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void biggerTableStopButtonMaker(int h, int w)
        {
            stop.Size = new Size(200, 50);
            stop.Location = new Point((w + 1) * 50, (3 * h / 4) * 50 - 25);
            stop.Text = "Stop";
            stop.BackColor = Color.White;
            Controls.Add(stop);
            stop.Enabled = false;
        }
        /// <summary>
        /// Létrehozza nagyobb tábla esetén a start gombot
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void biggerTableStartButtonMaker(int h, int w)
        {
            start.Size = new Size(200, 50);
            start.Location = new Point((w + 1) * 50, (3 * h / 4) * 50 + 25);
            start.Text = "Start";
            start.BackColor = Color.White;
            Controls.Add(start);
        }
        /// <summary>
        /// A táblán található összes kezelő gombo létrehozásáért felelős 
        /// </summary>
        /// <param name="h">Egész zsám</param>
        /// <param name="w">Egész szám</param>
        private void tableButtonMaker(int h, int w)
        {
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    buttonGrid[i, j] = new Button();
                    buttonGrid[i, j].Enabled = false;

                    buttonGrid[i, j].Size = new Size(50, 50);

                    buttonGrid[i, j].Location = new Point(50 * j, 50 * i + 25);

                    Controls.Add(buttonGrid[i, j]);
                }
            }
        }
        /// <summary>
        /// Kitörli a táblát, erre akkor van szükség ha új táblát töltünk be
        /// </summary>
        private void deleteTable()
        {
            for (int i = 0; i < model.SimTable.SizeX; i++)
            {
                for (int j = 0; j < model.SimTable.SizeY; j++)
                {
                    Controls.Remove(buttonGrid[i, j]);
                }
            }
        }
        /// <summary>
        /// Kitörli a robotok megjelenítéséért felelős ListBox-ot. Erre akkor van szükség ha új táblát töltünk be 
        /// </summary>
        private void deleteListBox()
        {
            Controls.Remove(list);
        }
        #endregion
    }
}
