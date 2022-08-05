using Eto.Forms;
using Newtonsoft.Json;

namespace Carbosaurus.LCA
{
    public class LandscapeMaterial: LCA.MaterialBase
    {
        [JsonProperty(PropertyName = "M1")]
        public string M1 { get; set; }

        [JsonProperty(PropertyName = "M2")]
        public string M2 { get; set; }

        [JsonProperty(PropertyName = "M3")]
        public string M3 { get; set; }

        [JsonProperty(PropertyName = "CO2-Removal")]
        public double Co2Removal { get; set; }

        [JsonProperty(PropertyName = "Embodied-CO2eq")]
        public double GWP { get; set; }


        [JsonProperty(PropertyName = "Carbon-seqrate")]
        public double CarbonSeq { get; set; }

    }
}
