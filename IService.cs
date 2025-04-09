using System.Threading.Tasks;

namespace ppl.Services.Core
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