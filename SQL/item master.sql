

CREATE TABLE ColorMaster(
    ColorID int PRIMARY KEY IDENTITY(1,1),
    Color VARCHAR(50) NOT NULL unique,
    ColorCode varchar(50) not null unique
)

CREATE TABLE SizeMaster (
    SizeID INT PRIMARY KEY IDENTITY(1,1),
    Size VARCHAR(10) NOT NULL unique,
    SizeOrder int
)

CREATE TABLE SizeGroup (
    SizeGroupID INT PRIMARY KEY IDENTITY(1,1),
    GroupName varchar(20) unique
)

CREATE TABLE SizeGroupDetails (
    GroupID INT PRIMARY KEY FOREIGN KEY REFERENCES SizeGroup(SizeGroupID),
    SizeID int FOREIGN KEY REFERENCES  SizeMaster(SizeID)
)

CREATE TABLE UOMMaster (
    UOMID INT PRIMARY KEY IDENTITY(1,1),
    UOMCode NVARCHAR(20) UNIQUE NOT NULL,
    UOM_Name NVARCHAR(100) NOT NULL,
    DecimalPlaces INT NOT NULL DEFAULT 2, 
    IsActive BIT NOT NULL DEFAULT 1
)


CREATE TABLE FabricMaster (
    FabricID int PRIMARY KEY IDENTITY(1,1),
    FabricCode NVARCHAR(50) not null unique,
    FabricName varchar(200) not null,
    GSM DECIMAL(10,2),
    Width DECIMAL(10,2),
    Construction NVARCHAR(100),     -- Single Jersey, Interlock, etc.
    Composition NVARCHAR(100),
    FinishType NVARCHAR(50),        -- Greige, Dyed, Printed
    UOMID int NOT NULL FOREIGN KEY REFERENCES UOMMaster(UOMID),
    IsActive BIT NOT NULL DEFAULT 1
)


--CREATE TABLE ItemMaster (
--    ItemID INT PRIMARY KEY IDENTITY(1,1),
--    ItemCode NVARCHAR(50) NOT NULL UNIQUE,
--    ItemName NVARCHAR(150) NOT NULL,
--    ItemType NVARCHAR(50) NOT NULL, -- Yarn, Fabric, Chemical, Garment
--    UOMID int NOT NULL,      -- Kg, Meter, Piece
--    IsActive BIT NOT NULL DEFAULT 1
--);

--CREATE TABLE ItemDetailsYarn (
--    ItemDetailID INT PRIMARY KEY IDENTITY(1,1),
--    ItemID INT FOREIGN KEY REFERENCES ItemMaster(ItemID),
--    Count NVARCHAR(20) NOT NULL,   -- e.g., 30s, 40s
--    Ply INT NOT NULL,              -- single, double
--    Blend NVARCHAR(100) NULL,      -- Cotton/Poly % split
--    Twist DECIMAL(10,2) NULL
--);

--CREATE TABLE ItemDetailsFabric (
--    ItemDetailID INT PRIMARY KEY IDENTITY(1,1),
--    ItemID INT FOREIGN KEY REFERENCES ItemMaster(ItemID),
--    Width DECIMAL(10,2) NOT NULL,
--    GSM DECIMAL(10,2) NOT NULL,
--    Construction NVARCHAR(100) NULL, -- single jersey, interlock
--    Composition  NVARCHAR(100) NULL,
--    FinishType NVARCHAR(100) NULL   -- dyed, compacted, finished
--);

--CREATE TABLE ItemDetailsTrims (
--    ItemDetailID INT PRIMARY KEY IDENTITY(1,1),
--    ItemID INT FOREIGN KEY REFERENCES ItemMaster(ItemID),
--    ColorID int FOREIGN KEY REFERENCES ColorMaster(ColorID) not null ,
--    SizeID int FOREIGN KEY REFERENCES SizeMaster(SizeID) not null 
--);

--CREATE TABLE ItemDetailsChemical (
--    ItemID INT PRIMARY KEY FOREIGN KEY REFERENCES ItemMaster(ItemID),
--    Concentration DECIMAL(10,2) NULL,
--    Density DECIMAL(10,2) NULL,
--    SupplierSpec NVARCHAR(200) NULL
--);

--CREATE TABLE ItemDetailsGarment (
--    ItemDetailID INT PRIMARY KEY IDENTITY(1,1),
--    ItemID INT FOREIGN KEY REFERENCES ItemMaster(ItemID),
--    StyleCode NVARCHAR(50) NOT NULL,
--    Fit NVARCHAR(50) NULL,
--    SizeID int FOREIGN KEY REFERENCES SizeMaster(SizeID) not null,
--    FabricID INT NULL FOREIGN KEY REFERENCES ItemMaster(ItemID) ,
--    ColorID int FOREIGN KEY REFERENCES ColorMaster(ColorID) not null 
--);



