using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.AccessControl;

namespace DirectoryBrowser.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        public List<NodeInfo> RootInfos { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            RootInfos = new List<NodeInfo>();
            _logger = logger;
            _configuration = configuration;

            IEnumerable<IConfigurationSection>? sections = _configuration.GetSection("AppSettings:Folders")?.GetChildren();

            if (sections != null)
            {
                foreach (string folder in sections.Select(x => x.Value))
                {
                    NodeInfo rootInfo = new NodeInfo(this);
                    DirectoryInfo dir = new DirectoryInfo(folder);
                    rootInfo.DirectoryInfo = dir;
                    rootInfo.Populate();
                    RootInfos.Add(rootInfo);
                }
            }
        }
    }

    public class NodeInfo
    {
        public DirectoryInfo? DirectoryInfo { get; set; }
        public FileInfo? FileInfo { get; set; }
        public List<NodeInfo> Infos { get; set; } = new List<NodeInfo>();
        public string Html { get; set; } = string.Empty;
        public HtmlString HtmlString
        {
            get
            {
                string html = string.Empty;

                if (DirectoryInfo != null)
                    GetHtml(DirectoryInfo.FullName, ref html);

                return new HtmlString(html);
            }
        }

        public IndexModel Model { get; set; }

        public NodeInfo(IndexModel model)
        {
            Model = model;
        }

        public void Populate()
        {
            if (DirectoryInfo != null)
            {
                IEnumerable<DirectoryInfo> directoryInfos = DirectoryInfo.EnumerateDirectories();
                directoryInfos = directoryInfos.Where(x => (x.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden);

                foreach (DirectoryInfo directoryInfo in directoryInfos)
                {
                    NodeInfo info = new NodeInfo(Model);
                    info.DirectoryInfo = directoryInfo;
                    Infos.Add(info);
                    info.Populate();
                }

                foreach (FileInfo fileInfo in DirectoryInfo.GetFiles().Where(x => (x.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden))
                {
                    NodeInfo info = new NodeInfo(Model);
                    info.FileInfo = fileInfo;
                    Infos.Add(info);
                }
            }
        }

        public void GetHtml(string rootFolder, ref string html)
        {
            foreach (NodeInfo info in Infos)
            {
                if (info.DirectoryInfo != null)
                {
                    html +=
                        "<div class=\"dir\">" +
                        "   <div class=\"headerDir\">" +
                        "       <img class=\"imgDir\" src=\"../folder6.png\">" + info.DirectoryInfo.Name +
                        "   </div>";

                    info.GetHtml(rootFolder, ref html);
                }
                else if (info.FileInfo != null)
                {
                    html +=
                        "<a href=\"" + info.FileInfo.FullName.Substring(rootFolder.Length) + "\" download>" +
                        "   <div class=\"file\">" +
                        "       <img class=\"imgDir\" src=\"../file.png\">" + info.FileInfo.Name +
                        "   </div>" +
                        "</a>";
                }
                else
                    throw new Exception("Errore");

                if (info.DirectoryInfo != null)
                    html += "</div>";
            }
        }
    }
}