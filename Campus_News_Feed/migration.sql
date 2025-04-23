CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Categories` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(200) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Categories` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Users` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Username` longtext CHARACTER SET utf8mb4 NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
    `UsePasswordLogin` tinyint(1) NOT NULL,
    `RegisteredAt` datetime(6) NOT NULL,
    `LastLoginAt` datetime(6) NULL,
    `IsAdmin` tinyint(1) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `News` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Content` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CategoryId` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `ClickCount` int NOT NULL,
    CONSTRAINT `PK_News` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_News_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Tokens` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Value` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Type` int NOT NULL,
    `UserId` int NULL,
    `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `ExpiresAt` datetime(6) NOT NULL,
    `IsUsed` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Tokens` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Tokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `UserPreferences` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `CategoryId` int NOT NULL,
    CONSTRAINT `PK_UserPreferences` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserPreferences_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserPreferences_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

INSERT INTO `Categories` (`Id`, `Description`, `Name`)
VALUES (1, '学校的最新动态和公告', '校园动态'),
(2, '学术讲座、研讨会等相关信息', '学术资讯'),
(3, '校内外体育比赛和活动', '体育赛事'),
(4, '艺术展览、文化节等活动', '文化活动'),
(5, '求职、实习和就业相关信息', '就业指导');

INSERT INTO `Users` (`Id`, `Email`, `IsActive`, `IsAdmin`, `LastLoginAt`, `PasswordHash`, `RegisteredAt`, `UsePasswordLogin`, `Username`)
VALUES (1, '2964959746@qq.com', TRUE, TRUE, NULL, '000000', TIMESTAMP '2025-04-23 04:30:32', TRUE, 'admin');

CREATE INDEX `IX_News_CategoryId` ON `News` (`CategoryId`);

CREATE INDEX `IX_Tokens_UserId` ON `Tokens` (`UserId`);

CREATE INDEX `IX_UserPreferences_CategoryId` ON `UserPreferences` (`CategoryId`);

CREATE INDEX `IX_UserPreferences_UserId` ON `UserPreferences` (`UserId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250423043034_AddIsActiveToUser', '9.0.0');

COMMIT;

