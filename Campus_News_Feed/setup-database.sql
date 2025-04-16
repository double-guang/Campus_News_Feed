-- MySQL数据库初始化脚本
-- 首先创建数据库（如果不存在）
CREATE DATABASE IF NOT EXISTS campus_news CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 使用该数据库
USE campus_news;

-- 创建用户（可选，如果需要特定用户访问权限）
-- CREATE USER 'campus_news_user'@'localhost' IDENTIFIED BY 'your_password';
-- GRANT ALL PRIVILEGES ON campus_news.* TO 'campus_news_user'@'localhost';
-- FLUSH PRIVILEGES;

-- 注意：剩余表结构将由Entity Framework Core自动创建
-- 运行应用程序时，EF Core的Migration将创建所有必要的表 