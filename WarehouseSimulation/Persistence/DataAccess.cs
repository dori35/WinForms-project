using System.Threading.Tasks;

namespace Persistence
{
    public interface DataAccess
    {
        Task<SimulationTable> loadFromFile(string path);

        public void log(object sender, EndGameEventArgs args);
    }
}
