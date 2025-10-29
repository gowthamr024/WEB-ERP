USE [ERP]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[EmplId] [int] NULL,
	[Username] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[FullName] [nvarchar](200) NOT NULL,
	[Email] [nvarchar](255) NULL,
	[MobileNumber] [nvarchar](20) NULL,
	[RoleID] [int] NOT NULL FOREIGN KEY REFERENCES Roles(RoleID),
	[IsActive] [bit] NOT NULL,
	[IsBlocked] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[PasswordOrg] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [RoleID] --RoleID 1- Data Entry
GO

ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [IsBlocked]
GO

ALTER TABLE [dbo].[Users] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO


