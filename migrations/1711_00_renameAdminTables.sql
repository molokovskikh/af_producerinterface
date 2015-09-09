use accessright;
RENAME TABLE `admin_role` TO `AdminToAdminRole`;
ALTER TABLE `AdminToAdminRole`
	CHANGE COLUMN `RoleId` `AdminRoleId` INT(11) UNSIGNED NULL DEFAULT NULL AFTER `AdminId`;

RENAME TABLE `admin_permissions` TO `AdminPermission`;
RENAME TABLE `admin_roles` TO `AdminRole`;
ALTER TABLE `admintoadminrole`
	CHANGE COLUMN `PermissionId` `AdminPermissionId` INT(11) UNSIGNED NULL DEFAULT NULL AFTER `AdminRoleId`;
RENAME TABLE `admin_permission_role` TO `adminPermissionRole`;
ALTER TABLE `adminPermissionRole`
	ALTER `RoleId` DROP DEFAULT;
ALTER TABLE `adminPermissionRole`
	CHANGE COLUMN `PermissionId` `AdminPermissionId` INT(11) UNSIGNED NULL DEFAULT NULL FIRST,
	CHANGE COLUMN `RoleId` `AdminRoleId` INT(11) UNSIGNED NOT NULL AFTER `AdminPermissionId`;
	