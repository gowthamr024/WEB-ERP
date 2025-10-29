CREATE PROCEDURE usp_CheckPermission  
    @UserID INT,  
    @ControllerName NVARCHAR(100),  
    @ActionName NVARCHAR(100),  
    @PermissionType NVARCHAR(1)  -- 'View', 'Add', 'Edit', 'Delete', 'Approve'  
AS  
BEGIN
    DECLARE @HasPermission BIT = 0;  
  
    SELECT @HasPermission = CASE @PermissionType  
        WHEN 'A' THEN CanAdd  
        WHEN 'E' THEN CanEdit  
        WHEN 'V' THEN CanView  
        WHEN 'D' THEN CanDelete  
        WHEN 'P' THEN CanPrint 
        WHEN 'A' THEN CanApprove  
        WHEN 'R' THEN CanReject  
        WHEN 'C' THEN CanCancel  
    END  
    FROM UserPermissions up  
    INNER JOIN Screens s ON up.ScreenID = s.ScreenID  
    WHERE up.UserID = @UserID  
      AND s.ControllerName = @ControllerName  
      AND s.ActionName = @ActionName;  
  
    SELECT @HasPermission AS HasPermission;  
END  