delete from UserManagement.Control where ParentControlId in (select ControlId from UserManagement.Control where DisplayName = 'Suggest Properties');

delete from UserManagement.Control where DisplayName = 'Suggest Properties';