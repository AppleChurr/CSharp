using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace sGISContainers.GeoJson
{
    /// <summary>
    /// GeoJSON 구조를 나타내기 위한 인터페이스.
    /// TreeView 컴포넌트에 표시될 수 있는 TreeNode 객체를 생성하는 메서드를 정의합니다.
    /// </summary>
    interface IGeoJosn
    {
        TreeNode ToTreeNode();
    }



    /// <summary>
    /// GeoJSON의 FeatureCollection을 나타내는 클래스입니다.
    /// 여러 GeoJSON 기능들을 포함합니다.
    /// </summary>
    public class cGeoJson : IGeoJosn
    {
        public string FileName { get; set; } = "*.geojson";
        public string type { get; set; } = "FeatureCollection";
        public List<cGeoObject> features { get; set; } = new List<cGeoObject>();

        #region functions

        /// <summary>
        /// 현재 인스턴스를 JSON 문자열로 직렬화합니다.
        /// </summary>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// 인스턴스의 내용을 가독성 있는 문자열로 변환합니다.
        /// JSON 형태의 문자열로 변환하지만 실제 JSON 직렬화는 아닙니다.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("\t\"type\": \"FeatureCollection\",");
            sb.AppendLine("\t\"features\": [");
            for (int i = 0; i < features.Count; i++)
            {
                sb.Append("\t\t");
                sb.Append(features[i].ToString().Replace("\n", "\n\t\t"));
                if (i < features.Count - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }
            sb.AppendLine("\t]");
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// TreeView 컨트롤에 표시될 수 있도록 TreeNode 객체를 생성합니다.
        /// </summary>
        public TreeNode ToTreeNode()
        {
            var node = new TreeNode($"GeoJson: {type}");
            foreach (var feature in features)
            {
                node.Nodes.Add(feature.ToTreeNode());
            }
            return node;
        }
        #endregion
    }

    /// <summary>
    /// GeoJSON의 기능(Feature)을 나타내는 클래스입니다.
    /// 기능의 유형, 지오메트리, 그리고 속성을 포함합니다.
    /// </summary>
    public class cGeoObject : IGeoJosn
    {
        public string type { get; set; }
        public cGeometry geometry { get; set; }
        public cProperties properties { get; set; }

        #region functions

        /// <summary>
        /// 기능의 문자열 표현을 제공합니다. 가독성을 위해 인덴트와 줄바꿈을 사용합니다.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"\t\"type\": \"{type}\",");
            sb.AppendLine("\t\"geometry\": " + geometry.ToString().Replace("\n", "\n\t") + ",");
            sb.AppendLine("\t\"properties\": " + properties.ToString().Replace("\n", "\n\t"));
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// TreeView 컨트롤에 표시하기 위한 TreeNode 객체를 생성합니다.
        /// </summary>
        public TreeNode ToTreeNode()
        {
            var node = new TreeNode($"Feature: {type}");
            node.Nodes.Add(geometry.ToTreeNode());
            node.Nodes.Add(properties.ToTreeNode());
            return node;
        }
        #endregion
    }


    /// <summary>
    /// GeoJSON 기능의 지오메트리를 나타내는 클래스입니다.
    /// 지오메트리 유형, 경계 상자(bbox), 좌표 배열을 포함합니다.
    /// </summary>
    public class cGeometry : IGeoJosn
    {
        public string type { get; set; } = "";
        public double[] bbox { get; set; }
        public double[][] coordinates { get; set; }

        #region functions

        /// <summary>
        /// 지오메트리의 문자열 표현을 제공합니다.
        /// 좌표 및 경계 상자 정보를 포함한 상세 문자열로 변환합니다.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"\t\"type\": \"{type}\",");
            sb.Append("\t\"bbox\": [");
            if (bbox != null) sb.Append(string.Join(", ", bbox));
            sb.AppendLine("],");
            sb.AppendLine("\t\"coordinates\": [");
            if (coordinates != null)
            {
                for (int i = 0; i < coordinates.Length; i++)
                {
                    sb.Append("\t\t[");
                    sb.Append(string.Join(", ", coordinates[i]));
                    sb.Append("]");
                    if (i < coordinates.Length - 1)
                    {
                        sb.AppendLine(",");
                    }
                    else
                    {
                        sb.AppendLine();
                    }
                }
            }
            sb.AppendLine("\t]");
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// TreeView 컨트롤에 표시하기 위한 TreeNode 객체를 생성합니다.
        /// 지오메트리의 유형, 경계 상자, 좌표를 포함한 노드를 생성합니다.
        /// </summary>
        public TreeNode ToTreeNode()
        {
            var node = new TreeNode($"Geometry: {type}");
            var bboxNode = new TreeNode($"Bbox: [{string.Join(", ", bbox ?? new double[0])}]");
            var coordinatesNode = new TreeNode("Coordinates");
            foreach (var coord in coordinates ?? new double[0][])
            {
                coordinatesNode.Nodes.Add(new TreeNode($"[{string.Join(", ", coord)}]"));
            }
            node.Nodes.Add(bboxNode);
            node.Nodes.Add(coordinatesNode);
            return node;
        }
        #endregion
    }

    /// <summary>
    /// GeoJSON 기능의 속성을 나타내는 클래스입니다.
    /// 기능의 다양한 속성을 포함합니다, 예를 들어 ID, AdminCode, Type 등입니다.
    /// </summary>
    public class cProperties : IGeoJosn
    {
        public string ID { get; set; } = "";
        public string AdminCode { get; set; } = "";
        public string Type { get; set; } = "";
        public string Kind { get; set; } = "";
        public string R_LinkID { get; set; } = "";
        public string L_LinkID { get; set; } = "";
        public string Maker { get; set; } = "";
        public string UpdateDate { get; set; } = "";
        public string Version { get; set; } = "";
        public string Remark { get; set; } = "";
        public string HistType { get; set; } = "";
        public string HistRemark { get; set; } = "";

        #region functions

        /// <summary>
        /// 속성의 문자열 표현을 제공합니다.
        /// 속성 정보를 포함한 상세 문자열로 변환합니다.
        /// </summary>
        public override string ToString()
        {
            return "{\n" +
                   $"\t\"ID\": \"{ID}\",\n" +
                   $"\t\"AdminCode\": \"{AdminCode}\",\n" +
                   $"\t\"Type\": \"{Type}\",\n" +
                   $"\t\"Kind\": \"{Kind}\",\n" +
                   $"\t\"R_LinkID\": \"{R_LinkID}\",\n" +
                   $"\t\"L_LinkID\": \"{L_LinkID}\",\n" +
                   $"\t\"Maker\": \"{Maker}\",\n" +
                   $"\t\"UpdateDate\": \"{UpdateDate}\",\n" +
                   $"\t\"Version\": \"{Version}\",\n" +
                   $"\t\"Remark\": \"{Remark}\",\n" +
                   $"\t\"HistType\": \"{HistType}\",\n" +
                   $"\t\"HistRemark\": \"{HistRemark}\"\n" +
                   "}";
        }

        /// <summary>
        /// TreeView 컨트롤에 표시하기 위한 TreeNode 객체를 생성합니다.
        /// 속성 정보를 나타내는 여러 하위 노드를 포함합니다.
        /// </summary>
        public TreeNode ToTreeNode()
        {
            var node = new TreeNode("Properties");
            node.Nodes.Add(new TreeNode($"ID: {ID}"));
            node.Nodes.Add(new TreeNode($"AdminCode: {AdminCode}"));
            node.Nodes.Add(new TreeNode($"Type: {Type}"));
            node.Nodes.Add(new TreeNode($"Kind: {Kind}"));
            node.Nodes.Add(new TreeNode($"R_LinkID: {R_LinkID}"));
            node.Nodes.Add(new TreeNode($"L_LinkID: {L_LinkID}"));
            node.Nodes.Add(new TreeNode($"Maker: {Maker}"));
            node.Nodes.Add(new TreeNode($"UpdateDate: {UpdateDate}"));
            node.Nodes.Add(new TreeNode($"Version: {Version}"));
            node.Nodes.Add(new TreeNode($"Remark: {Remark}"));
            node.Nodes.Add(new TreeNode($"HistType: {HistType}"));
            node.Nodes.Add(new TreeNode($"HistRemark: {HistRemark}"));
            return node;
        }
        #endregion
    }
}