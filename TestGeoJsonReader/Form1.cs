using sGISContainers.GeoJson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace TestGeoJsonReader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            string path = @"D:\40_프로젝트\# 자율주행 과제\정밀 지도 기반 캘리브레이션\한국교통대학(충주캠퍼스)_송부용_211105\한국교통대학(충주캠퍼스)_송부용_211102\HDMap\B2_SURFACELINEMARK.geojson";


            var file = File.OpenText(path);
            string data = file.ReadToEnd();

            cGeoJson _geoJson = JsonSerializer.Deserialize<cGeoJson>(data);

            //Console.WriteLine(_geoJson.ToString());

            TreeNode rootNode = _geoJson.ToTreeNode();
            tvGeoJson.Nodes.Clear();
            tvGeoJson.Nodes.Add(rootNode);
        }
    }
}
