START TRANSACTION;
CREATE UNIQUE INDEX `IX_EmployeeSchedules_UserBusinessId_DayOfWeek` ON `EmployeeSchedules` (`UserBusinessId`, `DayOfWeek`);

CREATE INDEX `IX_Appointments_BusinessId_StartDate` ON `Appointments` (`BusinessId`, `StartDate`);

ALTER TABLE `Appointments` DROP INDEX `IX_Appointments_BusinessId`;

ALTER TABLE `Appointments` DROP INDEX `IX_Appointments_EmployeeId`;

ALTER TABLE `EmployeeSchedules` DROP INDEX `IX_EmployeeSchedules_UserBusinessId`;

ALTER TABLE `UsersBusiness` MODIFY COLUMN `Role` varchar(20) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Users` MODIFY COLUMN `RefreshToken` varchar(500) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Users` MODIFY COLUMN `PasswordHash` varchar(500) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Users` MODIFY COLUMN `LastName` varchar(50) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Users` MODIFY COLUMN `FirstName` varchar(50) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Users` MODIFY COLUMN `Email` varchar(255) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Services` MODIFY COLUMN `Name` varchar(150) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Services` MODIFY COLUMN `Description` varchar(1000) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Appointments` MODIFY COLUMN `Status` varchar(30) CHARACTER SET utf8mb4 NOT NULL;

CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

CREATE INDEX `IX_ServiceCategories_BusinessId_Order` ON `ServiceCategories` (`BusinessId`, `Order`);

CREATE INDEX `IX_Appointments_EmployeeId_StartDate` ON `Appointments` (`EmployeeId`, `StartDate`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260908135722_CompleteEntityConfigurations', '9.0.16');

COMMIT;

