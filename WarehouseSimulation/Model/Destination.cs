
using Persistence;

namespace Model
{
    public class Destination
    {
        #region Members
        private Coordinate position;
        private int id;
        #endregion

        #region Properties
        public int Id { get { return id; } set { id = value; } }
        public Coordinate Position { get { return position; } set { position = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Konstruktor, létrehoz egy Destination objektumot.
        /// </summary>
        /// <param name="p">Coordinate</param>
        /// <param name="id">Egész szám </param>
        public Destination(Coordinate p, int id)
        {
            position = p;
            this.id = id;
        }
        #endregion
    }
}
