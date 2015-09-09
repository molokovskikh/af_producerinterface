use producerinterface;
RENAME TABLE `users` TO `ProducerUser`;
RENAME TABLE `user_permissions` TO `UserPermission`;
RENAME TABLE `user_role` TO `usertouserrole`;
RENAME TABLE `user_permission_role` TO `userpermissionrole`;
RENAME TABLE `user_roles` TO `userrole`;
RENAME TABLE `profile_news` TO `profilenews`;
ALTER TABLE `usertouserrole`
	CHANGE COLUMN `UserId` `ProducerUserId` INT(11) UNSIGNED NULL DEFAULT NULL FIRST;
	ALTER TABLE `usertouserrole`
	CHANGE COLUMN `PermissionId` `UserPermissionId` INT(11) UNSIGNED NULL DEFAULT NULL AFTER `RoleId`;
	ALTER TABLE `usertouserrole`
	CHANGE COLUMN `RoleId` `UserRoleId` INT(11) UNSIGNED NULL DEFAULT NULL AFTER `ProducerUserId`;	
	
ALTER TABLE `userpermissionrole`
	ALTER `RoleId` DROP DEFAULT;
ALTER TABLE `userpermissionrole`
	CHANGE COLUMN `PermissionId` `UserPermissionId` INT(11) UNSIGNED NULL DEFAULT NULL FIRST,
	CHANGE COLUMN `RoleId` `UserRoleId` INT(11) UNSIGNED NOT NULL AFTER `UserPermissionId`;
	
ALTER TABLE `user_logs`
	CHANGE COLUMN `UserId` `ProducerUserId` INT(11) UNSIGNED ZEROFILL NULL DEFAULT NULL AFTER `Id`;