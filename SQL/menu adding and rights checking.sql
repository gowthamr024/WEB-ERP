--Check permission
usp_modulesaccesslistwithscreens 2

Select * from Modules
SELECT * FROM Screens
Select * from UserModules
select * from UserPermissions

insert into Modules
(ModuleName, ModuleCode, DefaultController, DefaultAction)
values ('Payroll', 'PAY1', 'Home', 'Privacy')

insert into Screens (ScreenName, ModuleID, ScreenCode, ControllerName, ActionName, MenuOrder)
select 'Employee Master', 2, 'EMP_MAS', 'Payroll', 'Employee_Master', 2


insert into UserModules (ModuleID, UserID)
select 2,2

insert into UserPermissions (UserID, ScreenID, CanView)
select 2, 3, 1