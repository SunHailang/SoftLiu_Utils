using UnityEngine;
using System.Collections;

namespace Ubisoft.DNA
{
    interface IProviderDNA
    {
        void UpdateInitializationState(DnaToolkitUtil.InitState result);
    }
}