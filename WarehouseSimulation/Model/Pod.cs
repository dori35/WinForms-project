using System.Collections.Generic;

using Persistence;

namespace Model
{
    public class Pod
    {
        #region Members
        private int id;
        private List<int> products;
        private Coordinate position;
        private Coordinate originalPosition;
        private int state;
        #endregion

        #region Properties
        public int Id { get { return id; } set { id = value; } }
        public List<int> Products { get { return products; } set { products = value; } }
        public Coordinate Position { get { return position; } set { position = value; } }
        public Coordinate OriginalPosition { get { return originalPosition; } set { originalPosition = value; } }
        public int State { get { return state; } set { state = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Konstruktor, létrehozza a Pod objektumot.
        /// </summary>
        /// <param name="posX">Egész szám</param>
        /// <param name="posY">Egész szám</param>
        /// <param name="products">List<int>, a polcon lévő termékek listája</param>
        /// <param name="id">Egész szám</param>
        public Pod(int posX, int posY, List<int> products, int id)
        {
            this.position.x = posX;
            this.position.y = posY;

            this.originalPosition.x = posX;
            this.originalPosition.y = posY;

            this.products = products;
            this.id = id;

            state = 0;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Visszaadja hogy a robot az eredeti helyén van-e.
        /// </summary>
        /// <returns>Loikai érték, eredeti helyén van-e</returns>
        public bool onOriginalPosition ()
        {
            return position.x == originalPosition.x && position.y == originalPosition.y;
        }
        #endregion
    }
}
