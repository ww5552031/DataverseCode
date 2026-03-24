---
name: copilot-instructions
description: "Workspace instructions for train.dataverse — 常用构建/测试命令、关键文件和示例提示。"
---

# 简要说明
此仓库包含若干 .NET 项目（库、测试与 WebResource），常用命令以 `dotnet` 为主。以下信息用于加速日常开发与 AI 助手交互（例如构建、测试、运行和定位关键文件）。

**Build & Test**
- **构建解决方案**: `dotnet build train.dataverse.sln`
- **运行单元测试**: `dotnet test train.testproj`
- **运行 WebResource 项目（开发）**: `dotnet run --project WebResource`

**编程规范**
- 遵循 C# 9.0 及以上语法和 .NET 8 及以上框架约定。
- 类名使用 PascalCase，方法和属性使用 camelCase。
- 代码注释应清晰描述方法功能和参数，使用 XML 注释格式

**关键文件/位置**
- 解决方案: [train.dataverse.sln](../train.dataverse.sln)
- 共享库: [train.dataverse.comm](../train.dataverse.comm/train.dataverse.comm.csproj)
- 测试项目: [train.testproj.csproj](../train.testproj/train.testproj.csproj)
- WebResource: [WebResource/Program.cs](../WebResource/Program.cs)