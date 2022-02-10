using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using Persistence;
using Moq;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace WSTests
{
    [TestClass]
    public class UnitTest1
    {
        private Mock<DataAccess> mock;
        private SimulationModel model;
        private SimulationTable mockedTable1;
        private SimulationTable mockedTable2;
        private SimulationTable mockedTable3;

        [TestInitialize]
        public void Initialize()
        {
            mockedTable1 = mockedTable1Maker();
            mockedTable2 = mockedTable2Maker();
            mockedTable3 = mockedTable3Maker();
            mock = new Mock<DataAccess>();
        }

        [TestMethod]
        public void SmallTableBasicInitTest()
        {
            modelMaker(mockedTable1);

            Assert.AreEqual(model.SimTable.SizeX, 5);
            Assert.AreEqual(model.SimTable.SizeY, 5);
            Assert.AreEqual(model.CU.Pods[0].Products[0], 1);
            Assert.AreEqual(model.CU.Pods[0].Products[1], 2);
            for (int i = 0; i < model.CU.Pods.Count; i++)
            {
                Assert.AreEqual(model.CU.Pods[i].State, 0);
            }
            for (int i = 0; i < model.CU.Robots.Count; i++)
            {
                Assert.AreEqual(model.CU.Robots[i].State, 0);
            }
            for (int i = 0; i < model.CU.Robots.Count; i++)
            {
                Assert.AreEqual(model.CU.Robots[i].Energy, 30);
                Assert.AreEqual(model.CU.Robots[i].Energy, model.CU.Robots[i].MaxEnergy);
            }
            for (int i = 0; i < model.CU.Robots.Count; i++)
            {
                Assert.AreEqual(model.CU.Robots[i].Path.Count, 0);
            }
        }

        [TestMethod]
        public void IsCorrectTableTest1()
        {
            modelMaker(mockedTable1);
            Assert.IsTrue(model.SimTable.isGood());
        }

        [TestMethod]
        public void IsCorrectTableTest2()
        {
            modelMaker(mockedTable2);
            Assert.IsTrue(model.SimTable.isGood());
        }

        [TestMethod]
        public void IsCorrectTableTest3()
        {
            modelMaker(mockedTable3);
            Assert.IsTrue(model.SimTable.isGood());
        }

        [TestMethod]
        public void SmallTableStartTest()
        {
            modelMaker(mockedTable1);
            model.start();
            Assert.IsTrue(model.CU.Robots[0].Path.Count > 0);
            Assert.IsTrue(model.CU.Robots[1].Path.Count == 0);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].Energy, 29);
            Assert.AreEqual(model.CU.Robots[1].Energy, 30);
        }

        [TestMethod]
        public void OneRobotTest()
        {
            modelMaker(mockedTable2);
            Assert.AreEqual(model.SimTable.SizeX, 5);
            Assert.AreEqual(model.SimTable.SizeY, 6);
            Assert.AreEqual(model.CU.Pods[0].Products[0], 1);
            Assert.AreEqual(model.CU.Pods[1].Products[0], 1);
            Assert.AreEqual(model.CU.Pods[1].Products[1], 2);
            for (int i = 0; i < model.CU.Pods.Count; i++)
            {
                Assert.AreEqual(model.CU.Pods[i].State, 0);
            }
            Assert.AreEqual(model.CU.Robots[0].Energy, model.CU.Robots[0].MaxEnergy);
            Assert.AreEqual(model.CU.Robots[0].Energy, 30);

        }


        [TestMethod]
        public void TableSizeInitialisationTest()
        {
            modelMaker(mockedTable3);
            Assert.AreEqual(model.SimTable.SizeX, 6);
            Assert.AreEqual(model.SimTable.SizeY, 5);
        }

        [TestMethod]
        public void ProductsInitialisationTest()
        {
            modelMaker(mockedTable3);
            Assert.AreEqual(model.CU.Pods[0].Products[0], 2);
            Assert.AreEqual(model.CU.Pods[1].Products[0], 1);
            Assert.AreEqual(model.CU.Pods[1].Products[1], 2);
        }

        [TestMethod]
        public void RobotsInitTest3()
        {
            modelMaker(mockedTable3);
            for (int i = 0; i < model.CU.Pods.Count; i++)
            {
                Assert.AreEqual(model.CU.Pods[i].State, 0);
            }
            Assert.AreEqual(model.CU.Robots[0].Energy, model.CU.Robots[0].MaxEnergy);
            Assert.AreEqual(model.CU.Robots[0].Energy, 100);
        }

        [TestMethod]
        public void RobotDirectionTest()
        {
            Robot r = new Robot(1, 100, 0, 0, Direction.DOWN, new RobotEnum[2, 2]);
            Assert.AreEqual(r.Dir, Direction.DOWN);
            Robot r2 = new Robot(1, 100, 0, 0, Direction.LEFT, new RobotEnum[2, 2]);
            Assert.AreEqual(r2.Dir, Direction.LEFT);
            Robot r3 = new Robot(1, 100, 0, 0, Direction.UP, new RobotEnum[2, 2]);
            Assert.AreEqual(r3.Dir, Direction.UP);
            Robot r4 = new Robot(1, 100, 0, 0, Direction.RIGHT, new RobotEnum[2, 2]);
            Assert.AreEqual(r4.Dir, Direction.RIGHT);
        }

        [TestMethod]
        public void InitOriginalPositionTest1()
        {
            modelMaker(mockedTable1);
            foreach (Pod p in model.CU.Pods)
            {
                Assert.IsTrue(p.onOriginalPosition());
            }
        }

        [TestMethod]
        public void InitOriginalPositionTest2()
        {
            modelMaker(mockedTable2);
            foreach (Pod p in model.CU.Pods)
            {
                Assert.IsTrue(p.onOriginalPosition());
            }
        }

        [TestMethod]
        public void InitOriginalPositionTest3()
        {
            modelMaker(mockedTable3);
            foreach (Pod p in model.CU.Pods)
            {
                Assert.IsTrue(p.onOriginalPosition());
            }
        }

        [TestMethod]
        public void NotOnOriginalPositionTest1()
        {
            modelMaker(mockedTable1);
            model.start();
            while ('s' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            while ('m' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            foreach (Pod p in model.CU.Pods)
            {
                if (p.Position.x == model.CU.Robots[0].Position.x && p.Position.y == model.CU.Robots[0].Position.y)
                {
                    Assert.AreEqual(p.onOriginalPosition(), false);
                }
            }
        }

        [TestMethod]
        public void NotOnOriginalPositionTest2()
        {
            modelMaker(mockedTable2);
            model.start();
            while ('s' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            while ('m' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            foreach (Pod p in model.CU.Pods)
            {
                if (p.Position.x == model.CU.Robots[0].Position.x && p.Position.y == model.CU.Robots[0].Position.y)
                {
                    Assert.AreEqual(p.onOriginalPosition(), false);
                }
            }
        }

        [TestMethod]
        public void NotOnOriginalPositionTest3()
        {
            modelMaker(mockedTable3);
            model.start();
            while ('s' != model.CU.Robots[1].Path[0])
            {
                model.move();
            }
            while ('m' != model.CU.Robots[1].Path[0])
            {
                model.move();
            }
            model.move();
            foreach (Pod p in model.CU.Pods)
            {
                if (p.Position.x == model.CU.Robots[1].Position.x && p.Position.y == model.CU.Robots[1].Position.y)
                {
                    Assert.AreEqual(p.onOriginalPosition(), false);
                }
            }
        }

        [TestMethod]
        public void RobotMaxTest1()
        {
            modelMaker(mockedTable1);
            Assert.AreEqual(30, model.getRobotMax());
        }

        public void RobotMaxTest2()
        {
            modelMaker(mockedTable2);
            Assert.AreEqual(30, model.getRobotMax());
        }

        public void RobotMaxTest3()
        {
            modelMaker(mockedTable3);
            Assert.AreEqual(30, model.getRobotMax());
        }

        [TestMethod]
        public void StartMovingTest1()
        {
            modelMaker(mockedTable1);
            model.start();
            int robot0_x = model.CU.Robots[0].Position.x;
            int robot0_y = model.CU.Robots[0].Position.y;
            while ('m' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            Assert.IsTrue(robot0_x != model.CU.Robots[0].Position.x || robot0_y != model.CU.Robots[0].Position.y);
        }

        [TestMethod]
        public void StartMovingTest2()
        {
            modelMaker(mockedTable2);
            model.start();
            int robot0_x = model.CU.Robots[0].Position.x;
            int robot0_y = model.CU.Robots[0].Position.y;
            while ('m' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            Assert.IsTrue(robot0_x != model.CU.Robots[0].Position.x || robot0_y != model.CU.Robots[0].Position.y);
        }

        [TestMethod]
        public void StartMovingTest3()
        {
            modelMaker(mockedTable3);
            model.start();
            int robot1_x = model.CU.Robots[1].Position.x;
            int robot1_y = model.CU.Robots[1].Position.y;
            while ('m' != model.CU.Robots[1].Path[0])
            {
                model.move();
            }
            model.move();
            Assert.IsTrue(robot1_x != model.CU.Robots[1].Position.x || robot1_y != model.CU.Robots[1].Position.y);
        }

        [TestMethod]
        public void RobotTurnTest1()
        {
            modelMaker(mockedTable3);
            model.start();
            while ('l' != model.CU.Robots[1].Path[0])
            {
                model.move();
            }
            model.move();
            Assert.AreEqual(model.CU.Robots[1].Dir, Direction.LEFT);
        }

        public void RobotTurnTest2()
        {
            modelMaker(mockedTable3);
            model.start();
            while ('r' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            Assert.AreEqual(model.CU.Robots[0].Dir, Direction.RIGHT);
        }

        [TestMethod]
        public void RobotState0Test1()
        {
            modelMaker(mockedTable1);
            for (int i = 0; i < model.CU.Robots.Count; i++)
            {
                Assert.AreEqual(model.CU.Robots[i].State, 0);
            }
        }

        [TestMethod]
        public void RobotState0Test2()
        {
            modelMaker(mockedTable2);
            for (int i = 0; i < model.CU.Robots.Count; i++)
            {
                Assert.AreEqual(model.CU.Robots[i].State, 0);
            }
        }

        [TestMethod]
        public void RobotState0Test3()
        {
            modelMaker(mockedTable3);
            for (int i = 0; i < model.CU.Robots.Count; i++)
            {
                Assert.AreEqual(model.CU.Robots[i].State, 0);
            }
        }

        [TestMethod]
        public void PodState0Test1()
        {
            modelMaker(mockedTable1);
            for (int i = 0; i < model.CU.Pods.Count; i++)
            {
                Assert.AreEqual(model.CU.Pods[i].State, 0);
            }
        }

        [TestMethod]
        public void PodState0Test2()
        {
            modelMaker(mockedTable2);
            for (int i = 0; i < model.CU.Pods.Count; i++)
            {
                Assert.AreEqual(model.CU.Pods[i].State, 0);
            }
        }

        [TestMethod]
        public void PodState0Test3()
        {
            modelMaker(mockedTable3);
            for (int i = 0; i < model.CU.Pods.Count; i++)
            {
                Assert.AreEqual(model.CU.Pods[i].State, 0);
            }
        }

        [TestMethod]
        public void State1Test1()
        {
            modelMaker(mockedTable1);
            model.start();
            while ('s' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            Assert.AreEqual(model.CU.Robots[0].State, 1);
            Assert.AreEqual(model.CU.Robots[0].Pod != null, true);
            foreach (Pod p in model.CU.Pods)
            {
                if (p.Position.x == model.CU.Robots[0].Position.x && p.Position.y == model.CU.Robots[0].Position.y)
                {
                    Assert.AreEqual(p.State, 1);
                }
            }
        }

        [TestMethod]
        public void State1Test2()
        {
            modelMaker(mockedTable2);
            model.start();
            while ('s' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            Assert.AreEqual(model.CU.Robots[0].State, 1);
            Assert.AreEqual(model.CU.Robots[0].Pod != null, true);
            foreach (Pod p in model.CU.Pods)
            {
                if (p.Position.x == model.CU.Robots[0].Position.x && p.Position.y == model.CU.Robots[0].Position.y)
                {
                    Assert.AreEqual(p.State, 1);
                }
            }
        }

        [TestMethod]
        public void State1Test3()
        {
            modelMaker(mockedTable3);
            model.start();
            while ('s' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            Assert.AreEqual(model.CU.Robots[0].State, 1);
            Assert.AreEqual(model.CU.Robots[0].Pod != null, true);
            foreach (Pod p in model.CU.Pods)
            {
                if (p.Position.x == model.CU.Robots[0].Position.x && p.Position.y == model.CU.Robots[0].Position.y)
                {
                    Assert.AreEqual(p.State, 1);
                }
            }
        }

        [TestMethod]
        public void DestinationTest1()
        {
            modelMaker(mockedTable1);
            model.start();
            while ('s' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            Coordinate co;
            co.x = 4;
            co.y = 1;
            Assert.AreEqual(model.CU.Robots[0].Destination, co);
        }

        [TestMethod]
        public void DestinationTest2()
        {
            modelMaker(mockedTable2);
            model.start();
            while ('s' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            Coordinate co;
            co.x = 4;
            co.y = 1;
            Assert.AreEqual(model.CU.Robots[0].Destination, co);
        }

        [TestMethod]
        public void DestinationTest3()
        {
            modelMaker(mockedTable3);
            model.start();
            while ('s' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            model.move();
            Coordinate co;
            co.x = 4;
            co.y = 0;
            Assert.AreEqual(model.CU.Robots[0].Destination, co);
        }

        [TestMethod]
        public void Robot1LeftPodTest1()
        {
            modelMaker(mockedTable1);
          
            model.start();
            Coordinate leftPodCoordinate = model.CU.Robots[0].Position;
            while (model.CU.Robots[0].State != 2)
            {
                model.move();
                leftPodCoordinate = model.CU.Robots[0].Position;
            }

            model.move();
            model.move();
            model.move();

            Assert.AreEqual(model.SimTable.Table[leftPodCoordinate.x, leftPodCoordinate.y].Type, FieldEnum.POD);
            Assert.IsTrue(model.CU.Robots[0].Pod == null);

        }


        [TestMethod]
        public void Robot2LeftPodTest1()
        {
            modelMaker(mockedTable1);
           
            model.start();
            Coordinate leftPodCoordinate = model.CU.Robots[1].Position;
            while (model.CU.Robots[1].State != 2 )
            {
                model.move();
                leftPodCoordinate = model.CU.Robots[1].Position;
            }
            model.move();
            model.move();
            model.move();

            Assert.AreEqual(model.SimTable.Table[leftPodCoordinate.x, leftPodCoordinate.y].Type,FieldEnum.POD);
            Assert.IsTrue(model.CU.Robots[1].Pod==null);

        }

        [TestMethod]
        public void Robot1LeftPodTest2()
        {
            modelMaker(mockedTable2);

            model.start();
            Coordinate leftPodCoordinate = model.CU.Robots[0].Position;
            while (model.CU.Robots[0].State != 2)
            {
                model.move();
                leftPodCoordinate = model.CU.Robots[0].Position;
            }

            model.move();
            model.move();
            model.move();

            Assert.AreEqual(model.SimTable.Table[leftPodCoordinate.x, leftPodCoordinate.y].Type, FieldEnum.POD);
            Assert.IsTrue(model.CU.Robots[0].Pod==null);
        }


        [TestMethod]
        public void Robot1NeedChargeStateTest1()
        {
            modelMaker(mockedTable1);
            foreach (Robot r in model.CU.Robots)
            {
                r.NeedCharge += new EventHandler(onNeedCharge);
            }
            model.start();
            while (model.CU.Robots[0].DockDistance < model.CU.Robots[0].Energy)
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[0].State, 2);
        }

        [TestMethod]
        public void Robot1ChargedTest1()
        {
            modelMaker(mockedTable1);
            foreach (Robot r in model.CU.Robots)
            {
                r.NeedCharge += new EventHandler(onNeedCharge);
            }
            model.start();
            while (!(model.CU.Robots[0].State == 2 && model.CU.Robots[0].Position.x == 3 && model.CU.Robots[0].Position.y == 0))
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 0);
            Assert.IsTrue(model.CU.Robots[0].State == 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 1);
            Assert.IsTrue(model.CU.Robots[0].State == 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 2);
            Assert.IsTrue(model.CU.Robots[0].State == 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 3);
            Assert.IsTrue(model.CU.Robots[0].State == 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 4);
            Assert.IsTrue(model.CU.Robots[0].State == 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 0);
            Assert.IsTrue(model.CU.Robots[0].State != 2);

        }


        public void Robot1ChargedTest2()
        {
            modelMaker(mockedTable2);
            foreach (Robot r in model.CU.Robots)
            {
                r.NeedCharge += new EventHandler(onNeedCharge);
            }
            model.start();
            while (!(model.CU.Robots[0].State == 2 && model.CU.Robots[0].Position.x == 3 && model.CU.Robots[0].Position.y == 0))
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 0);
            Assert.IsTrue(model.CU.Robots[0].State == 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 1);
            Assert.IsTrue(model.CU.Robots[0].State == 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 2);
            Assert.IsTrue(model.CU.Robots[0].State == 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 3);
            Assert.IsTrue(model.CU.Robots[0].State == 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 4);
            Assert.IsTrue(model.CU.Robots[0].State == 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].ChargeCounter, 0);
            Assert.IsTrue(model.CU.Robots[0].State != 2);
        }


        [TestMethod]
        public void Robot2NeedChargeStateTest1()
        {
            modelMaker(mockedTable1);
            foreach (Robot r in model.CU.Robots)
            {
                r.NeedCharge += new EventHandler(onNeedCharge);
            }
            model.start();
            while (model.CU.Robots[1].DockDistance < model.CU.Robots[1].Energy)
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[1].State, 2);
        }

        [TestMethod]
        public void NeedChargeStateTest2()
        {
            modelMaker(mockedTable2);

            foreach (Robot r in model.CU.Robots)
            {
                r.NeedCharge += new EventHandler(onNeedCharge);
            }
            model.start();
            while (model.CU.Robots[0].DockDistance < model.CU.Robots[0].Energy)
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[0].State, 2);
        }

        [TestMethod]
        public void SmallerIndexRobotJustWaitConflictTest1()
        {
            modelMaker(mockedTable1);
            model.start();
            while (!(model.CU.Robots[0].Position.x == 3 && model.CU.Robots[0].Position.y == 2
                    && model.CU.Robots[1].Position.x == 3 && model.CU.Robots[1].Position.y == 1))
            {
                model.move();
            }
            List<char> lastList = model.CU.Robots[0].Path;
            Assert.AreEqual(model.CU.Robots[0].WaitNumber, 0);
            Assert.AreEqual(lastList, model.CU.Robots[0].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].WaitNumber, 1);
            Assert.AreEqual(lastList, model.CU.Robots[0].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].WaitNumber, 2);
            Assert.AreEqual(lastList, model.CU.Robots[0].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].WaitNumber, 3);
            Assert.AreEqual(lastList, model.CU.Robots[0].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].WaitNumber, 4);
            Assert.AreEqual(lastList, model.CU.Robots[0].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].WaitNumber, 5);
            Assert.AreEqual(lastList, model.CU.Robots[0].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].WaitNumber, 6);
            Assert.AreEqual(lastList, model.CU.Robots[0].Path);
            model.move();
            Assert.IsTrue(lastList == model.CU.Robots[0].Path);
        }

        [TestMethod]
        public void BiggerIndexRobotFindNewWayConflictTest1()
        {
            modelMaker(mockedTable1);
            model.start();
            while (!(model.CU.Robots[0].Position.x == 3 && model.CU.Robots[0].Position.y == 2
                    && model.CU.Robots[1].Position.x == 3 && model.CU.Robots[1].Position.y == 1))
            {
                model.move();
            }
            List<char> lastList = model.CU.Robots[1].Path;
            Assert.AreEqual(model.CU.Robots[1].WaitNumber, 0);
            Assert.AreEqual(lastList, model.CU.Robots[1].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[1].WaitNumber, 1);
            Assert.AreEqual(lastList, model.CU.Robots[1].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[1].WaitNumber, 2);
            Assert.AreEqual(lastList, model.CU.Robots[1].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[1].WaitNumber, 3);
            Assert.AreEqual(lastList, model.CU.Robots[1].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[1].WaitNumber, 4);
            Assert.AreEqual(lastList, model.CU.Robots[1].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[1].WaitNumber, 5);
            Assert.IsTrue(lastList != model.CU.Robots[1].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[1].WaitNumber, 0);
        }

        [TestMethod]
        public void BiggerIndexRobotFindNewWayConflictEventTest1()
        {
            modelMaker(mockedTable1);
            foreach (Robot r in model.CU.Robots)
            {
                r.NeedNewWay += new EventHandler<Coordinate>(onNeedNewWayBecauseTheNextPositionIsRobot);
            }
            model.start();
            while (!(model.CU.Robots[0].Position.x == 3 && model.CU.Robots[0].Position.y == 2
                    && model.CU.Robots[1].Position.x == 3 && model.CU.Robots[1].Position.y == 1))
            {
                model.move();
            }
            List<char> lastList = model.CU.Robots[1].Path;
            while (model.CU.Robots[1].WaitNumber != 5)
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[1].WaitNumber, 5);
            model.move();
            Assert.AreEqual(model.CU.Robots[1].WaitNumber, 0);
        }

        [TestMethod]
        public void WaitForTheTherRobotConflictTest3()
        {
            modelMaker(mockedTable3);
            model.start();
            while (!(model.CU.Robots[0].Position.x == 1 && model.CU.Robots[0].Position.y == 2
                    && model.CU.Robots[1].Position.x == 2 && model.CU.Robots[1].Position.y == 2))
            {
                model.move();
            }
            List<char> lastList = model.CU.Robots[0].Path;
            Assert.AreEqual(model.CU.Robots[0].WaitNumber, 0);
            Assert.AreEqual(lastList, model.CU.Robots[0].Path);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].WaitNumber, 1);
            Assert.AreEqual(lastList, model.CU.Robots[0].Path);
        }

        [TestMethod]
        public void WhenRobot1ArriveDesProductsCountDecreaseTest1()
        {
            modelMaker(mockedTable1);
            model.start();
            while ('a' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 1);
            while ('a' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 1);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 0);
        }

        [TestMethod]
        public void WhenRobot1ArriveDesProductsCountDecreaseTest2()
        {
            modelMaker(mockedTable2);
            model.start();
            while ('a' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 1);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 0);

        }


        [TestMethod]
        public void WhenRobot1ArriveDesProductsCountDecreaseTest3()
        {
            modelMaker(mockedTable3);
            model.start();
            while ('a' != model.CU.Robots[0].Path[0])
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 1);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 0);
        }

        public void WhenRobot2ArriveDesProductsCountDecreaseTest3()
        {
            modelMaker(mockedTable3);
            model.start();
            while ('a' != model.CU.Robots[1].Path[0])
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 2);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 1);
            while ('a' != model.CU.Robots[1].Path[0])
            {
                model.move();
            }
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 1);
            model.move();
            Assert.AreEqual(model.CU.Robots[0].Pod.Products.Count, 0);
        }

        [TestMethod]
        public void RobotArriveEventTest1()
        {
            modelMaker(mockedTable1);
            foreach (Robot r in model.CU.Robots)
            {
                r.Arrive += new EventHandler(onRobotArrive);
            }
            model.start();
            while (model.CU.isEnd())
            {
                model.move();
            }
        }

        [TestMethod]
        public void RobotArriveEventTest2()
        {
            modelMaker(mockedTable2);
            foreach (Robot r in model.CU.Robots)
            {
                r.Arrive += new EventHandler(onRobotArrive);
            }
            model.start();
            while (model.CU.isEnd())
            {
                model.move();
            }
        }

        [TestMethod]
        public void RobotArriveEventTest3()
        {
            modelMaker(mockedTable3);
            foreach (Robot r in model.CU.Robots)
            {
                r.Arrive += new EventHandler(onRobotArrive);
            }
            model.start();
            while (model.CU.isEnd())
            {
                model.move();
            }
        }

        [TestMethod]
        private SimulationTable mockedTable1Maker()
        {
            SimulationTable mockedTable = new SimulationTable(5, 5);

            mockedTable.setFields();
            mockedTable.setDock(3, 0);
            mockedTable.setPod(1, 3);
            mockedTable.setPod(2, 2);
            mockedTable.setPod(2, 3);
            mockedTable.setDestination(4, 1);
            mockedTable.setDestination(4, 3);
            mockedTable.setRobotTrue(0, 0);
            mockedTable.setRobotTrue(0, 1);
            mockedTable.RobotMax = 30;
            mockedTable.ProductsTypeNumber = 2;
            List<int> productList = new List<int>();
            productList.Add(1);
            productList.Add(2);
            mockedTable.putPod(1, productList);
            return mockedTable;

        }
        private SimulationTable mockedTable2Maker()
        {
            SimulationTable mockedTable = new SimulationTable(5, 6);
            mockedTable.setFields();
            mockedTable.setDock(3, 0);
            mockedTable.setPod(1, 1);
            mockedTable.setPod(1, 2);
            mockedTable.setPod(2, 1);
            mockedTable.setPod(2, 2);
            mockedTable.setDestination(4, 1);
            mockedTable.setDestination(4, 3);
            mockedTable.setRobotTrue(1, 5);
            mockedTable.RobotMax = 30;
            mockedTable.ProductsTypeNumber = 2;
            List<int> productList = new List<int>();
            productList.Add(1);
            List<int> productList2 = new List<int>();
            productList2.Add(1);
            productList2.Add(2);
            mockedTable.putPod(1, productList);
            mockedTable.putPod(2, productList2);
            return mockedTable;
        }

        private SimulationTable mockedTable3Maker()
        {
            SimulationTable mockedTable = new SimulationTable(6, 5);
            mockedTable.setFields();
            mockedTable.setPod(1, 2);
            mockedTable.setPod(3, 2);
            mockedTable.setPod(4, 2);
            mockedTable.setPod(4, 3);
            mockedTable.setDestination(1, 0);
            mockedTable.setDestination(4, 0);
            mockedTable.setRobotTrue(5, 1);
            mockedTable.setRobotTrue(5, 4);
            mockedTable.setDock(1, 4);
            mockedTable.setDock(3, 4);
            mockedTable.RobotMax = 100;
            List<int> productList = new List<int>();
            productList.Add(2);
            List<int> productList2 = new List<int>();
            productList2.Add(1);
            productList2.Add(2);
            List<int> productList3 = new List<int>();
            productList3.Add(1);
            mockedTable.putPod(1, productList);
            mockedTable.putPod(2, productList2);
            mockedTable.putPod(4, productList3);
            return mockedTable;
        }


        private void onNeedNewWayBecauseTheNextPositionIsRobot(object sender, Coordinate e)
        {
            Robot r = sender as Robot;
            Assert.IsTrue(r.Position.x + 1 == e.x && r.Position.y == e.y || r.Position.x - 1 == e.x && r.Position.y == e.y ||
                          r.Position.x == e.x && r.Position.y + 1 == e.y || r.Position.x == e.x && r.Position.y - 1 == e.y);

        }

        private void onRobotArrive(object sender, EventArgs e)
        {
            Robot r = sender as Robot;
            Assert.AreEqual(r.Position.x, r.Destination.x);
            Assert.AreEqual(r.Position.y, r.Destination.y);
        }

        private void onTheEnd(Object sender, EndGameEventArgs e)
        {
            Assert.IsTrue(model.CU.isEnd());
        }

        private void onRefresh(Object sender, EventArgs e)
        {
            for (int i = 0; i < model.CU.Robots.Count; i++)
            {
                Assert.IsTrue(model.CU.Robots[i].Energy >= 0);
            }
        }

        private void onNeedCharge(object sender, EventArgs e)
        {
            Robot r = sender as Robot;
            Assert.IsTrue(r.Energy <= r.DockDistance);
        }

        private void modelMaker(SimulationTable mockedTable)
        {
            mock.Setup(mock => mock.loadFromFile(It.IsAny<String>())).Returns(() => Task.FromResult(mockedTable));
            model = new SimulationModel(mock.Object);
            model.load(mockedTable);
            model.End += new EventHandler<EndGameEventArgs>(onTheEnd);
            model.Refresh += new EventHandler(onRefresh);
        }

    }
}
