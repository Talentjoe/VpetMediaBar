# VpetMediaBar 插件 / VpetMediaBar Plugin

## 项目简介 / Overview

VpetMediaBar 是为 Vpet 生态打造的插件。它能够读取 Windows 系统中的 SMTC（System Media Transport Controls）信息，并通过内置的对话框展示当前媒体状态，同时提供基础的播放控制能力。插件通过 Vpet 的 `DynamicResources` 共享媒体信息，使用键名 `"MediaInfo"` 存储 `MediaClient` 实例，从而让其他模块可以异步访问这些数据。

VpetMediaBar is a plugin designed for the Vpet ecosystem. It retrieves SMTC (System Media Transport Controls) information from Windows, shows the current media status in a built-in dialog, and exposes basic playback controls. The plugin shares media data through Vpet's `DynamicResources`, storing the `MediaClient` instance under the key `"MediaInfo"`, so that other modules can access the information asynchronously.

## 主要特性 / Key Features

- 读取并展示系统 SMTC 媒体信息。
- 内置对话框用于显示媒体状态，并执行播放、暂停等控制。
- 通过 `DynamicResources` 与其他 Vpet 模块共享媒体数据。
- 订阅 “Response to music” 事件，媒体信息变更时自动更新。
- 当前版本为预览版，后续会持续优化体验与稳定性。

- Read and display SMTC media information from the system.
- Built-in dialog for showing media state and providing play/pause controls.
- Share media data with other Vpet modules via `DynamicResources`.
- Subscribe to the "Response to music" event and update on media changes.
- The current release is a preview version and will continue to improve.

## 使用示例 / Usage Example

```csharp
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
        var resource = MW.DynamicResources["MediaInfo"];
        if (resource is MediaClient mediaClient)
        {
            mediaClient.OnMediaInfoReceived += info =>
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

该示例展示了如何在加载插件时等待 `MediaInfo` 资源可用，并订阅媒体信息更新事件。当新的媒体信息到达时，可以调用 `COllamaAPI.ResponseToMusic` 进行处理。

The example shows how to wait for the `MediaInfo` resource to become available when the plugin loads and how to subscribe to media information updates. When new media data arrives, `COllamaAPI.ResponseToMusic` is triggered for further handling.

## 平台支持 / Platform Support

- 需要 Windows 10 或更高版本以确保 SMTC 功能可用。
- 并非所有媒体软件都原生支持 SMTC；部分应用（如网易云音乐）需要安装额外插件。

- Requires Windows 10 or later to ensure SMTC functionality.
- Not every media application supports SMTC natively; some (e.g., Netease Cloud Music) may require additional plugins.

## 未来规划 / Roadmap

- 改进界面体验与交互设计。
- 扩展媒体控制能力，例如音量调节或播放队列管理。
- 优化与第三方媒体软件的兼容性。

- Improve UI experience and interaction design.
- Extend media control capabilities, such as volume adjustment or queue management.
- Enhance compatibility with third-party media software.

## 贡献 / Contributing

欢迎提交 Issue 或 Pull Request，分享反馈或改进建议。

Feel free to open issues or submit pull requests with feedback and improvement ideas.

