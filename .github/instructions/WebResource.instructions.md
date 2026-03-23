---
name: WebResource.instructions
description: "Project-level instructions for WebResource（Web 前端/静态资源）。"
applyTo: "WebResource/**"
---
 
简要说明：包含前端静态资源、WebHost 和配置，用于开发/上传 WebResource 内容。

常用命令：
- 本地运行: `dotnet run --project WebResource`

关键文件/目录：
- `Program.cs` — 启动入口
- `wwwroot/` — 前端静态文件
- `UploaderMapping.config` — 上传映射配置

示例提示：
- "如何在本地运行并调试 `WebResource`？"
- "生成并打包 `wwwroot/js` 的最新脚本用于上传到 Dataverse 中解决方案中。"
-  编写JavaScript脚本如果
-  wwwroot/js/***.js  如果用来操作powerapps 模型驱动应用中的表单元素，请参考:
  [Client API Reference for model-driven apps](https://learn.microsoft.com/en-us/power-apps/developer/model-driven-apps/clientapi/reference)

  注意：此 instruction 仅在 `WebResource` 目录下加载。