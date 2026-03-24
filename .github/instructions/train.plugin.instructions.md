---
name: train.plugin
description: "Project-level instructions for the train.plugin project: build, signing, deployment and agent prompts."
applyTo: "***/train.plugin/**"
---

# 简要说明
本文件为 `train.plugin` 项目的团队级约定与快速上手说明，适用于开发、构建、打包与部署 Dynamics CRM365/Dataverse 插件。

## 构建 & 发布
- **构建项目（开发）**: `dotnet build train.plugin\train.plugin.csproj -c Debug`
- **发布（生成用于注册/部署的 dll）**: 使用项目的发布或直接从 `bin\Debug\net462\publish` 中取文件，或使用 MSBuild 打包脚本。

## 目标框架与签名
- **目标框架**: .NET Framework 4.6.2（项目使用 `net462` 编译目标）。
- **强名称/签名**: 项目包含 `train.plugin.snk`，生成的程序集通常需要强名称以便用于某些注册场景；请保留签名文件并在 CI 中安全读取。

## 插件结构与约定
- **入口类**: `ContactSimplePlugin.cs` 为示例插件；公共基类位于 `PluginBase.cs`。遵循以下约定：类名使用 PascalCase，方法与属性使用 camelCase。
- **异常处理**: 插件内部请捕获并记录异常，不要吞掉异常信息。将可恢复/不可恢复逻辑区分清楚并记录上下文。
- **依赖注入**: 若需共享库，请把共享逻辑放在 `train.dataverse.comm` 中并通过 NuGet/项目引用管理。

## 打包与注册
- **打包**: 将需要的 `dll` 与依赖文件复制到注册器所需路径，或使用现有的注册脚本/工具（例如 Plugin Registration Tool）。
- **注册注意**: 确认目标环境的 .NET 兼容性和强名称要求；为不同环境（沙箱/非沙箱）准备不同的注册配置。

## 本地调试建议
- 在本地使用日志与模拟上下文调试，必要时写入临时文件或 Application Insights。不要在生产日志中输出敏感数据。

## 代码规范与测试
- 遵循仓库总体约定（C# PascalCase、方法 camelCase）。
- 目前仓库主要测试位于 `train.testproj`，若为插件添加可复用逻辑，请在共享库中添加单元测试。

## 常见命令
- 构建解决方案: `dotnet build train.dataverse.sln`
- 运行测试（共享库/测试工程）: `dotnet test train.testproj`

## 检查清单（提交前）
- **签名**: 确认程序集签名未丢失。
- **依赖**: 确认所需依赖已包含或通过引用分发。
- **日志**: 添加足够的日志以支持排查，但不泄露敏感信息。



