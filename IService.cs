using System.Collections.Generic;
using System.Threading.Tasks;

namespace ppl.ServiceManagement
{
    public interface IService
    {
        string Name { get; }

        Task AsyncSetup(Dictionary<string, object> args = null);
                
#if UNITY_EDITOR
        void DebugService();
#endif
    }
}