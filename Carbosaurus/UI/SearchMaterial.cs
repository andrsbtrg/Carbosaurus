using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;

namespace Carbosaurus.UI
{
    [Guid("B4DD5A61-6CF7-406D-A80B-5040DAF7C5A1")]
    public class SearchMaterial: Eto.Forms.Panel
    {
        public static System.Guid PanelId => typeof(SearchMaterial).GUID;
        public string Title { get; set; }
        
        private List<LCA.BuildingMaterial> materials;

        private readonly uint m_document_sn = 0;

        private DynamicLayout layout;
        private Button DefaultButton;
        private Button AbortButton;
        private Button SearchButton;
        private TextBox inputArea;
        private DropDown dropdown_mat;
        
        private TableLayout buttons;
        private TableLayout inputLayout;
        public SearchMaterial() { }


        // Eto controls
        private Label title { get; set; }
        private TextArea body { get; set; }
        
        /// <summary>
        /// Creates Eto panel passing document serial number
        /// </summary>
        /// <param name="documentSerialNumber"></param>
        public SearchMaterial(uint documentSerialNumber)
        {
            m_document_sn=documentSerialNumber;
            Title = GetType().Name;

            // Create buttons
            DefaultButton = new Button { Text = "Send" };
            //DefaultButton.Click += (sender, e) => Close(false);
            DefaultButton.Click += DefaultButton_Click;

            AbortButton = new Button { Text = "Cancel" };
            SearchButton = new Button { Text = "Search" };
            SearchButton.Click += (sender, e) => SearchButton_Click(inputArea.Text);

            


            // Create elements for the panel
            title = new Label()
            {
                Text = "Material search",
                TextAlignment = TextAlignment.Center,
            };

            inputArea = new TextBox();
            inputLayout = new TableLayout()
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new Label() { Text = "Search" }, new TableRow(inputArea, null,SearchButton) }

            };

            body = new TextArea()
            {
                Size = new Size(400, 200),
                Text = "",
                TextAlignment = TextAlignment.Center,
                TextColor = Eto.Drawing.Color.FromArgb(53)
            };

            buttons = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, DefaultButton, AbortButton, null) }
            };



            dropdown_mat = new DropDown() { BackgroundColor = Eto.Drawing.Color.FromArgb(15, 15, 15)};
            dropdown_mat.SelectedValueChanged += handleValueChanged;

            RepaintLayout();
        }

        private void handleValueChanged(object sender, EventArgs e)
        {
            DropDown dropdown = sender as DropDown;

            // Deserialize material
            LCA.BuildingMaterial mat = (LCA.BuildingMaterial)dropdown.SelectedValue;
            string json = mat.ToJSON();
            body.Text = json;

            RepaintLayout();
        }

        private void SearchButton_Click(string query)
        {
            //body.Text = LCAClient.SearchMaterial(query);
            materials = LCAClient.GetMaterial(query);
            if (materials == null)
                return;
            List<string> material_names = new List<string>();
            foreach (LCA.BuildingMaterial material in materials)
            {
                material_names.Add(material.name_en);
            }

            //dropdown_mat = new DropDown()
            //{
            //    DataStore = material_names
            //};
            dropdown_mat.Items.Clear();
            dropdown_mat.DataStore = materials;
            dropdown_mat.ItemTextBinding = Binding.Delegate<LCA.BuildingMaterial, string>(obj => obj.name_en);


            RepaintLayout();
        }

        private void RepaintLayout()
        {
            // Control for the whole panel will be dynamic
            layout = new DynamicLayout()
            {
                Padding = new Padding(10),
                Spacing = new Size(5, 5),
            };

            // Here we add the layour elements created before
            layout.AddCentered(title);
            layout.AddSeparateRow(inputLayout);
            layout.AddRow(dropdown_mat);
            layout.AddSeparateRow(body);
            layout.AddRow(null);
            layout.AddCentered(buttons);

            Content = layout;
        }

        private void DefaultButton_Click(object sender, EventArgs e)
        {
            body.Text = LCAClient.GetMaterials();
        }
    }
}
