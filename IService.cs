using System.Collections.Generic;
using System.Threading.Tasks;

namespace ppl.Services.Core
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