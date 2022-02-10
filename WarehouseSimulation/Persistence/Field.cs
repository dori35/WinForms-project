using System.Collections.Generic;

namespace Persistence
{
    public class Field
    {
        #region Members
        private FieldEnum type;
        private int id;
        private List<int> products;
        #endregion

        #region Properties
        public FieldEnum Type { get { return type; } set { type = value; } }
        public int Id { get { return id; } set { id = value; } }
        public List<int> Products { get { return products; } set { products = value; } }
        #endregion

        /// <summary>
        /// Létrehoz egy field objektumot
        /// </summary>
        /// <param name="type">Fieldenum, ez adja mega a mező típusát</param>
        /// <param name="id">Egész szám , megadja a mező id-ját, ha van</param>
        /// <param name="products">List<int>, ha polc akkor itt adódnak át a termékek</param>
        #region Constructor
        public Field(FieldEnum type, int id, List<int> products)
        {
            this.type = type;
            this.id = id;
            this.products =  products;
        }
        #endregion
    }
}
