using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbosaurus.LCA
{
    public class BuildingMaterial
    {
        [JsonProperty(PropertyName = "Name (en)")]
        public string name_en { get; set; }

        [JsonProperty(PropertyName = "Name (de)")]
        public string name_de { get; set; }
        [JsonProperty(PropertyName = "Reference-size")]
        public double? reference_size { get; set; }

        [JsonProperty(PropertyName = "Reference-unit")]
        public string reference_unit { get; set; }
        [JsonProperty(PropertyName = "Density (kg/m3)")]
        public double? density { get; set; }
        [JsonProperty(PropertyName = "Area weight (kg/m2)")]
        public double? area_weight { get; set; }
        [JsonProperty(PropertyName = "GWP")]
        public Dictionary<string, double> GWP   { get; set; }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
