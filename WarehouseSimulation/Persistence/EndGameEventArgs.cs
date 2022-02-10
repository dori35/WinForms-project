using System;
using System.Collections.Generic;

namespace Persistence
{
    /// <summary>
    /// Ez az osztály felel a naplózáshoz szükséges adatok átküldéséért.
    /// </summary>
    public class EndGameEventArgs : EventArgs
    {
        /// <summary>
        /// Létrehoz egy EndGameEventArgs objektumot
        /// </summary>
        /// <param name="steps">Egész szám, lépések száma</param>
        /// <param name="robotsE">List<int>, a robotok elhasznált energiája</param>
        public EndGameEventArgs(int steps, List<int> robotsE)
        {
            this.steps = steps;
            this.robotsE = robotsE;
        }
        public int steps;
        public List<int> robotsE;
    }
}
