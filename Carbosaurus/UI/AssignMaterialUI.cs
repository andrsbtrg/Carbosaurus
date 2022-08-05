using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Carbosaurus.Utils;
using Rhino.Display;
using System.Diagnostics;

namespace Carbosaurus.UI
{
    [Guid("8B3CEB9D-81FD-4FB7-B710-24A4169707F2")]
    public class AssignMaterialUI : Eto.Forms.Panel
    {
        private string dashboard_url = "http://localhost:8501";
        private string material_url = "http://localhost:8501/About_materials";
        public string Title { get; set; }   
        public static System.Guid PanelId => typeof(AssignMaterialUI).GUID;

        private readonly uint m_document_sn = 0;

        // Forms
        private Button DefaultButton;
        private Button AbortButton;
        private Button SendButton;
        private Button previewOnOff;
        private DropDown level1Drop;
        private DropDown level2Drop;
        private DropDown level3Drop;
        private ColorPicker ColorSelector1;
        private ColorPicker ColorSelector2;
        private Button LaunchDashboard;
        private CheckBox saveResults;
        private LinkButton aboutMaterials;
        private ToggleButton ToggleMaterialButton;
        private static string[] previewOption = { "Contribution to variance", "Embodied Carbon" };
        private DropDown previewOptionsDropdown;

        private LCA.TreeNode landscapeCollection => LCAClient.GetLandscapeCollection();


        // Layouts
        private DynamicLayout Layout; 
        private TableLayout buttons;
        private Label currentMaterial;


        // Empty constructor is requiered
        public AssignMaterialUI() { }

        public string CurrentAttribute { get; set; }
        public string CurrentLevel { get; set; }

        private string selectedAttribute { get; set; }
        private string selectedLevel { get; set; }


        /// <summary>
        /// Creates Eto panel passing document serial number
        /// </summary>
        /// <param name="documentSerialNumber"></param>
        public AssignMaterialUI(uint documentSerialNumber)
        {
            m_document_sn = documentSerialNumber;
            Title = GetType().Name;

            // Create options dropdown
            previewOptionsDropdown = new DropDown() { DataStore = previewOption , SelectedIndex = 0};
            previewOptionsDropdown.SelectedValueChanged += PreviewOptionsDropdown_SelectedValueChanged;

            // Save results toggle
            saveResults = new CheckBox() { Checked = false , Text = "Save results", ThreeState = false};
            saveResults.CheckedChanged += SaveResults_CheckedChanged;

            // Preview materials toggle button
            ToggleMaterialButton = new ToggleButton() { Text = "Preview" , Checked = true};
            ToggleMaterialButton.CheckedChanged += ToggleMaterialButton_CheckedChanged;

            // About materials link
            aboutMaterials = new LinkButton() { Text = "About materials" };
            aboutMaterials.Click += (sender, e) => LaunchWebsite(material_url);

            // Create color selectors
            ColorSelector1 = new ColorPicker { Value = ResultsPreview.Instance.Color1}; 
            ColorSelector2 = new ColorPicker { Value= ResultsPreview.Instance.Color2};

            ColorSelector2.ValueChanged += RedrawPreviews;
            ColorSelector1.ValueChanged += RedrawPreviews;
            // Selected as empty;
            Rhino.RhinoDoc.SelectObjects += RhinoDoc_SelectObjects;

            Rhino.RhinoDoc.DeselectAllObjects += RhinoDoc_DeselectAllObjects;
            // Create buttons
            DefaultButton = new Button { Text = "Select and Assign"};
            DefaultButton.Click += AssignButtonClick;

            AbortButton = new Button { Text = "Remove" };

            AbortButton.Click += RemoveMaterials; 

            SendButton = new Button { Text = "Start simulation" };
            SendButton.Click += SendButton_Click;

            previewOnOff = new Button { Text = "Preview" };
            previewOnOff.Click += PreviewOnOff_Click;

            LaunchDashboard = new Button { Text = "Launch Dashboard" };
            LaunchDashboard.Click += (sender, e) => LaunchWebsite(dashboard_url);

            level1Drop = new DropDown()
            {
                DataStore = landscapeCollection?.children,
                ItemTextBinding = Binding.Delegate<LCA.TreeNode, string>(obj => obj.name)
            };

            level1Drop.SelectedValueChanged += Level1Selected;
            level2Drop = new DropDown();
            level2Drop.Visible = false;
            level2Drop.SelectedValueChanged += Level2Selected;
            level3Drop = new DropDown();
            level3Drop.Visible = false;
            level3Drop.SelectedValueChanged += Level3Selected;


            buttons = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, DefaultButton, AbortButton, null) }
            };

            currentMaterial = new Label() { Text = "None"};

            RepaintLayout();
        }

        private void SaveResults_CheckedChanged(object sender, EventArgs e)
        {
            var checkbox  = (CheckBox)sender;
            LCAClient.saveResults = (bool)checkbox.Checked;
        }

        private void ToggleMaterialButton_CheckedChanged(object sender, EventArgs e)
        {
            var toggle = (ToggleButton)sender;
            MaterialsPreview.Instance.SetPreview(toggle.Checked);
            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
        }

        private void LaunchWebsite(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception)
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    dashboard_url = dashboard_url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {dashboard_url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", dashboard_url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", dashboard_url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void PreviewOptionsDropdown_SelectedValueChanged(object sender, EventArgs e)
        {
            ResultsPreview.Instance.PreviewValues(previewOptionsDropdown.SelectedKey);
        }

        private void RedrawPreviews(object sender, EventArgs e)
        {
            ResultsPreview.Instance.Color1 = ColorSelector1.Value;
            ResultsPreview.Instance.Color2 = ColorSelector2.Value;
            ResultsPreview.Instance.PreviewValues(previewOptionsDropdown.SelectedKey);
        }

        private void RhinoDoc_DeselectAllObjects(object sender, Rhino.DocObjects.RhinoDeselectAllObjectsEventArgs e)
        {
                selectedLevel = "";
                selectedAttribute = "";
                currentMaterial.Text = selectedAttribute;
        }


        private void RhinoDoc_SelectObjects(object sender, Rhino.DocObjects.RhinoObjectSelectionEventArgs e)
        {
            var level = "";
            var attribute = "";
            if (e == null | e.RhinoObjects.Length == 0)
                return;
            var rhobject = e.RhinoObjects[0];

            if (!rhobject.Attributes.HasUserData)
                return;
            level = rhobject.Attributes.GetUserString("Level");
            attribute = rhobject.Attributes.GetUserString("Attribute");

            selectedAttribute = attribute;
            selectedLevel = level;

            currentMaterial.Text = selectedAttribute;

        }

        private void PreviewOnOff_Click(object sender, EventArgs e)
        {
            bool state = ResultsPreview.Instance.Visible;

            if (state == true)
                previewOnOff.Text = "Turn On";
            else if (state == false)
                previewOnOff.Text = "Turn Off";

            ResultsPreview.Instance.SetVisible(!state);
            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            Rhino.RhinoApp.WriteLine("Sending materials for simulation...");
            Task running = Task.Run( async () =>{ await LCAClient.SendBOM(); });

            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();   
        }

        private void Level3Selected(object sender, EventArgs e)
        {
            CurrentLevel = "M3";
            if (level3Drop.SelectedValue is LCA.TreeNode current)
                CurrentAttribute = current.name;
        }

        private void Level2Selected(object sender, EventArgs e)
        {
            DropDown dropdown2 = (DropDown)sender;
            LCA.TreeNode level2material = (LCA.TreeNode)dropdown2.SelectedValue;

            if (level2material == null)
                return;

            level3Drop.Items.Clear();

            var level3 = level2material.children;
            level3Drop.DataStore = level3;
            level3Drop.ItemTextBinding = Binding.Delegate<LCA.TreeNode, string>(obj => obj.name);
            level3Drop.Visible = true;
            RepaintLayout();
            CurrentLevel = "M2";
            if (level2Drop.SelectedValue is LCA.TreeNode current)
                CurrentAttribute = current.name;
        }

        private void Level1Selected(object sender, EventArgs e)
        {
            DropDown dropdown1 = (DropDown)sender;
            LCA.TreeNode level1material = (LCA.TreeNode)dropdown1.SelectedValue;

            level2Drop.Items.Clear();
            level3Drop.Items.Clear();
            
            

            var level2 = level1material.children;
            level2Drop.DataStore = level2;
            level2Drop.ItemTextBinding = Binding.Delegate<LCA.TreeNode, string>(obj => obj.name);
            //level2Drop.SelectedKeyChanged += Level2Selected;
            level3Drop.Visible = false;
            level2Drop.Visible = true;
            RepaintLayout();

            CurrentLevel = "M1";
            if (level1Drop.SelectedValue is LCA.TreeNode current)
                CurrentAttribute = current.name;
        }

        private void RepaintLayout()
        {
            //level1Drop.DataStore = landscapeCollection?.children;
            // Control for the whole panel will be dynamic
            Layout = new DynamicLayout()
            {
                Padding = new Padding(10),
                Spacing = new Size(10, 10),
            
            };
            // Here we add the layour elements created before
            GroupBox current = new GroupBox()
            {
                Style = "text",
                Text = "Current",
            };


            Layout.BeginGroup("Info", Padding = 10) ;

            Layout.BeginVertical();
            Layout.AddRow(aboutMaterials);
            Layout.AddRow(new Label() { Text = "Selected Material: " }, currentMaterial, null, new Label() { Text = "Preview materials: " }, ToggleMaterialButton);
            Layout.EndVertical();
            Layout.EndGroup();

            Layout.BeginGroup("Assign materials", Padding = 10);
            Layout.BeginVertical();
            Layout.AddRow(new Label() { Text = "Select a material" }, new Rhino.UI.Controls.LabelSeparator());
            Layout.AddRow(new Label() { Text = "Lvl 1: " }, level1Drop);
            Layout.AddRow(new Label() { Text = "Lvl 2: " }, level2Drop);
            Layout.AddRow(new Label() { Text = "Lvl 3: " }, level3Drop);
            Layout.EndVertical();

            Layout.BeginVertical(Padding = 10);
            Layout.AddRow(null);
            Layout.AddCentered(buttons);
            Layout.EndVertical();

            Layout.BeginVertical();
            Layout.AddRow(saveResults);
            Layout.AddRow(SendButton);
            Layout.EndVertical();
            Layout.EndGroup();

            #region Results
            Layout.BeginGroup("Results", Padding = 10);

            Layout.BeginVertical();
            Layout.AddSeparateRow(new Label() { Text = "Min" }, new Label() { });
            Layout.EndVertical();

            Layout.BeginVertical();
            Layout.AddRow(new Label() { Text = "Preview: " }, previewOptionsDropdown);
            Layout.EndVertical();

            Layout.BeginVertical();
            Layout.AddRow(null);
            Layout.AddRow(new Label() { Text = "Colour range:"}, ColorSelector1, ColorSelector2);
            Layout.EndVertical();
            Layout.BeginVertical();
            Layout.AddRow(new Label() { Text = "Toggle results" }, previewOnOff);
            Layout.EndVertical();
            Layout.BeginVertical();
            Layout.AddRow(null);
            Layout.AddRow(LaunchDashboard);
            Layout.AddRow(null);
            Layout.EndVertical();
            Layout.EndGroup();
            #endregion
            Content = Layout;
        }



        private void AssignButtonClick(object sender, EventArgs e)
        {
            // Apply material to object

            var surfaces = Utils.AssignMaterial.SelectSurfaceRef(out var areas);

            bool result = Utils.AssignMaterial.WriteCustomDict(surfaces, areas, CurrentLevel, CurrentAttribute);

            if (result == false)
            {
                Rhino.RhinoApp.WriteLine("Cancelled. Nothing applied");
            }
            else if (result == true)
            {
                Rhino.RhinoApp.WriteLine($"Assigned {surfaces.Count} elements.");
            }
                            
            MaterialsPreview.Instance.AddToPreview(surfaces);
        }

        private void RemoveMaterials(object sernder, EventArgs e)
        {

            var surfaces = Utils.AssignMaterial.SelectSurfaceRef(out var areas);

            // Remove user dictionary
            Utils.AssignMaterial.RemoveMaterials(surfaces);
            // Remove from preview
            Utils.MaterialsPreview.Instance.RemoveMaterial(surfaces);

            MaterialsPreview.Instance.UpdatePreview();
        }

    }



}

