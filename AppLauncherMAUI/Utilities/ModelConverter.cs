using AppLauncherMAUI.MVVM.Models.RawDownloadModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncherMAUI.Utilities;

internal sealed class ModelConverter
{
    public static StandardRawModel RawGitToStandard(GithubRawModel rawModel)
    {
        StandardRawModel model = new()
        {
            Name = rawModel.Name,
            Path = rawModel.Path,
            Type = rawModel.Type,
            DownloadUrl = rawModel.Download_url,
            DirectoryUrl = rawModel.Url,
            Hash = rawModel.Sha,
            Size = rawModel.Size,
        };

        return model;
    }
}
