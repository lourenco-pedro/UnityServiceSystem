using System.Threading.Tasks;

namespace Services
{
    public interface IService
    {
        string Name { get; }

        Task AsyncSetup();
                
#if UNITY_EDITOR
        void DebugService();
#endif
    }
}