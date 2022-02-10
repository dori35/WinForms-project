using Persistence;

namespace Model
{
   public class Dock
    {
        #region Members
        private int id;
        private Coordinate position;
        private int state;
        #endregion

        #region Properties
        public int Id{ get { return id; } set { id = value; } }
        public Coordinate Position { get { return position; } set { position= value; } }
        public int State { get { return state; } set { state = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Konstruktor, létrehozza aDock objektumot.
        /// </summary>
        /// <param name="p">Coordinate</param>
        /// <param name="id">Egész szám</param>
        public Dock(Coordinate p,int id)
        {
            position = p;
            state = 0;
            this.id = id;
        }
        #endregion
    }
}
