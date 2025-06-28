这是一个为Vpet开发的插件。该插件可以获取系统SMTC信息并且带有基础的对话框可以显示信息以及控制媒体播放。该插件的信息通过vpet DynamicResources 进行信息共享。以"MediaInfo"作为key，存储media client class。可以通过以下方式异步加载。订阅Response to music事件。且在媒体信息变换时收到更新信息。当前仅为预览版本，会进行更进一步的优化。

请注意 win10及以上系统才支持SMTC，且并非所有软件都适配SMTC，需要下载插件才能进行支持（如网易云）。

```c#
        public async void LoadWithDelay()
        {
            int waitMs = 0;
            await Task.Delay(3000);
            while (!MW.DynamicResources.ContainsKey("MediaInfo") && waitMs < 5000)
            {
                await Task.Delay(200);
                waitMs += 200;
            }

            try
            {
                var a = MW.DynamicResources["MediaInfo"];Add commentMore actions
                if (a is MediaClient mediaClient)
                {
                    mediaClient.OnMediaInfoReceived += (info)=>
                    {
                        Task.Run(() => COllamaAPI.ResponseToMusic(info));
                    };
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show("VpetMediaBar not loaded, please check the plugin.".Translate() + ex.Message);
            }
        }

```

当前版本仅为预览版本。
