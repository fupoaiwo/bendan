# 数据库迁移 

## code First 迁移具有两个需要用户了解的主要命令。
Add-Migration 将基于自上次迁移创建以来对模型所做的更改来构建下一次迁移
Update-Database 将对数据库应用任意挂起的迁移

Remove-Migration 移除

例：
Add-Migration InitialCreate