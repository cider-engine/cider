using Microsoft.Build.Framework;
using System;
using System.IO;

namespace Cider.Task
{
    public class AssetTask : BuildTask
    {
        [Required]
        public ITaskItem[] Assets { get; set; }

        public override bool Execute()
        {
            if (Assets is null) return false;
            foreach (var asset in Assets)
            {
                var fullPath = asset.GetMetadata("FullPath");
                var relativePath = asset.GetMetadata("Identity");
                if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(relativePath))
                {
                    Log.LogWarning($"The input FullPath: {fullPath ?? "null"} or Identity {relativePath ?? "null"} doesn't mean a file");
                    continue;
                }

                // 元数据文件
                if (fullPath.EndsWith(".cider.meta"))
                {
                    var originFile = fullPath.Substring(0, fullPath.Length - ".cider.meta".Length);
                    if (!File.Exists(originFile))
                    {
                        //File.Delete(fullPath);
                        Log.LogError($"Cider meta file '{fullPath}' has no corresponding asset file.");
                        return false;
                    }
                    continue;
                }

                // 此处开始处理资源文件
                if (!File.Exists(fullPath))
                {
                    Log.LogError($"Asset file '{fullPath}' does not exist.");
                    return false;
                }

                if (File.Exists(fullPath + ".cider.meta")) continue;
                File.WriteAllText(fullPath + ".cider.meta", $$"""
                    {
                        "uid": "_{{Guid.NewGuid().ToString("N").ToUpper()}}"
                    }
                    """);
            }

            return true;
        }
    }
}
