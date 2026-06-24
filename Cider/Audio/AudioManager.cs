using Cider.Assets;
using System;
using System.Threading.Tasks;

namespace Cider.Audio
{
    public class AudioManager
    {
        public static async Task PlayWithDefaultMixer(AudioAsset asset)
        {
            var audio = await asset.Load(AudioMixer.DefaultPlayback);
            AudioMixer.DefaultPlayback.Play(audio);
        }
    }
}
