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
            Path = rawModel.Path,
            DownloadUrl = rawModel.Download_url,
            Hash = rawModel.Sha,
            Size = rawModel.Size,
        };

        return model;
    }
}
