using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Carbosaurus.UI
{
    [Guid("991D6351-BA96-4379-9D29-AECCC3AA48A1")]
    public class ResultsPanelWebUI: Eto.Forms.Panel
    {
        public string Title { get; set; }

        public static System.Guid PanelID => typeof(ResultsPanelWebUI).GUID;

        private readonly uint m_document_sn = 0;

        string uri = "http://localhost:8501";

        WebView webView = new WebView { Height = 1000 };

        public ResultsPanelWebUI(uint documentSN)
        {
            m_document_sn = documentSN;
            Title = GetType().Name;

            DrawContent(uri);

        }

        public void DrawContent(string path)
        {
            var layout = new DynamicLayout { DefaultSpacing = new Size(5, 5), Padding = new Padding(0) };
            webView.Url = new System.Uri(uri);
            //webView.LoadHtml(uri);
            webView.DocumentLoading += (sender, e) =>
            {

                e.Cancel = true; // prevent navigation
                                 //CommunicateWithWebView(e.Uri.PathAndQuery);
            };

            layout.AddSeparateRow(webView);
            layout.Add(null);
            Content = layout;
        }
    }
}
