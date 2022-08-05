using Rhino;
using Rhino.Commands;
using Rhino.Input.Custom;
using Rhino.UI;


namespace RhinoLCA
{
    public class OpenPanelCommand : Command
    {
        public OpenPanelCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Rhino.UI.Panels.RegisterPanel(PlugIn, typeof(Carbosaurus.UI.SearchMaterial), "Search Material", null);

            Rhino.UI.Panels.RegisterPanel(PlugIn, typeof(Carbosaurus.UI.AssignMaterialUI), "Select Material", null);

            Rhino.UI.Panels.RegisterPanel(PlugIn, typeof(Carbosaurus.UI.ResultsPanelWebUI), "Results", null);

            Instance = this;
            
        }

        ///<summary>The only instance of this command.</summary>
        public static OpenPanelCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "CarboStart";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            Panels.OpenPanel(typeof(Carbosaurus.UI.AssignMaterialUI).GUID);

            //var panel_id = Views.SampleCsEtoPanel.PanelId;
            var panel_id = Carbosaurus.UI.AssignMaterialUI.PanelId;
            var visible = Panels.IsPanelVisible(panel_id);

            if (!visible)
                Panels.OpenPanel(panel_id);

            return Result.Success;
        }
    }
}
