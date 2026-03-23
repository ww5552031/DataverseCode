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
- 遵循 C# 9.0+ 语法和 .NET 8+ 框架约定
- 类名使用 PascalCase，方法和属性使用 camelCase

**关键文件/位置**
- 解决方案: [train.dataverse.sln](../train.dataverse.sln)
- 共享库: [train.dataverse.comm](../train.dataverse.comm/train.dataverse.comm.csproj)
- 测试项目: [train.testproj.csproj](../train.testproj/train.testproj.csproj)
- WebResource: [WebResource/Program.cs](../WebResource/Program.cs)

**常见约定**
- 单一解决方案管理多个项目，测试项目位于 `train.testproj`，使用 MSTest 框架。
- 请优先修改源项目再更新测试；更改后使用 `dotnet test` 验证。

**建议性工作流**
1. 拉取最新代码并恢复子模块（如果有）
2. `dotnet build train.dataverse.sln`
3. 在修改后运行 `dotnet test train.testproj`

**示例提示（可以直接发送给 AI 助手）**
- "帮我在仓库中找到所有使用 `FetchXmlBuilder` 的位置，并说明其用法。"
- "检查 `train.testproj` 的失败测试并提供修复建议。"
- "生成一个示例的 `README.md`，描述如何在本地运行 WebResource 项目。"

如果你希望我把这些说明扩展为更详细的贡献指南或添加 `applyTo` 模式，请告诉我要覆盖的子路径（例如 `WebResource/**` 或 `train.dataverse.comm/**`）。
