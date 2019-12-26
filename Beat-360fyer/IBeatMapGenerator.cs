using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    public interface IBeatMapGenerator<TSettings>
    {
        BeatMap FromNormal(BeatMap normal, TSettings generatorSettings);
    }
}
