using Newtonsoft.Json;


namespace Carbosaurus.LCA
{
    public class MaterialBase
    {
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
