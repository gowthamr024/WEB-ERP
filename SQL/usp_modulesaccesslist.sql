CREATE PROCEDURE usp_modulesaccesslist 
    @UserID INT
AS
BEGIN
    SELECT DISTINCT 
        m.ModuleID, 
        m.ModuleName, 
        m.DefaultController,
        m.DefaultAction 
    FROM 
    (
        Select ModuleID from UserModules where UserID = @UserID 
    ) um
    Left JOIN 
    (
        Select ModuleID, ModuleName, DefaultController, DefaultAction from Modules where IsActive = 1
    ) m ON um.ModuleID = m.ModuleID
    WHERE m.ModuleID is not null
    ORDER BY m.ModuleName
END