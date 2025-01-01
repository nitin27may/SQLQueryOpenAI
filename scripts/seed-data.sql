/****** Object:  Table [dbo].[Customer]    Script Date: 2025-01-01 1:45:18 PM ******/
USE [master]
GO

-- Create a new database
IF NOT EXISTS (SELECT *
FROM sys.databases
WHERE name = 'DemoDB')
BEGIN
	CREATE DATABASE DemoDB;
END;
GO

USE [DemoDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[CustomerID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NULL,
	[Phone] [nvarchar](15) NULL,
PRIMARY KEY CLUSTERED 
(
	[CustomerID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Order]    Script Date: 2025-01-01 1:45:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[OrderID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NOT NULL,
	[OrderDate] [datetime] NOT NULL,
	[TotalAmount] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderDetails]    Script Date: 2025-01-01 1:45:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderDetails](
	[OrderDetailID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[ProductName] [nvarchar](100) NOT NULL,
	[Quantity] [int] NOT NULL,
	[Price] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderDetailID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Customer] ON 
GO
INSERT [dbo].[Customer] ([CustomerID], [CustomerName], [Email], [Phone]) VALUES (1, N'John Doe', N'john.doe@example.com', N'123-456-7890')
GO
INSERT [dbo].[Customer] ([CustomerID], [CustomerName], [Email], [Phone]) VALUES (2, N'Jane Smith', N'jane.smith@example.com', N'987-654-3210')
GO
INSERT [dbo].[Customer] ([CustomerID], [CustomerName], [Email], [Phone]) VALUES (3, N'Alice Johnson', N'alice.johnson@example.com', N'555-123-4567')
GO
SET IDENTITY_INSERT [dbo].[Customer] OFF
GO
SET IDENTITY_INSERT [dbo].[Order] ON 
GO
INSERT [dbo].[Order] ([OrderID], [CustomerID], [OrderDate], [TotalAmount]) VALUES (1, 1, CAST(N'2023-12-01T00:00:00.000' AS DateTime), CAST(95.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[Order] ([OrderID], [CustomerID], [OrderDate], [TotalAmount]) VALUES (2, 2, CAST(N'2023-12-02T00:00:00.000' AS DateTime), CAST(200.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[Order] ([OrderID], [CustomerID], [OrderDate], [TotalAmount]) VALUES (3, 3, CAST(N'2023-12-03T00:00:00.000' AS DateTime), CAST(300.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[Order] ([OrderID], [CustomerID], [OrderDate], [TotalAmount]) VALUES (4, 1, CAST(N'2023-12-04T00:00:00.000' AS DateTime), CAST(175.00 AS Decimal(10, 2)))
GO
SET IDENTITY_INSERT [dbo].[Order] OFF
GO
SET IDENTITY_INSERT [dbo].[OrderDetails] ON 
GO
INSERT [dbo].[OrderDetails] ([OrderDetailID], [OrderID], [ProductName], [Quantity], [Price]) VALUES (1, 1, N'Widget A', 2, CAST(25.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[OrderDetails] ([OrderDetailID], [OrderID], [ProductName], [Quantity], [Price]) VALUES (2, 1, N'Widget B', 3, CAST(15.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[OrderDetails] ([OrderDetailID], [OrderID], [ProductName], [Quantity], [Price]) VALUES (3, 2, N'Widget C', 1, CAST(200.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[OrderDetails] ([OrderDetailID], [OrderID], [ProductName], [Quantity], [Price]) VALUES (4, 3, N'Widget A', 1, CAST(25.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[OrderDetails] ([OrderDetailID], [OrderID], [ProductName], [Quantity], [Price]) VALUES (5, 3, N'Widget D', 5, CAST(55.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[OrderDetails] ([OrderDetailID], [OrderID], [ProductName], [Quantity], [Price]) VALUES (6, 4, N'Widget E', 3, CAST(50.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[OrderDetails] ([OrderDetailID], [OrderID], [ProductName], [Quantity], [Price]) VALUES (7, 4, N'Widget A', 1, CAST(25.00 AS Decimal(10, 2)))
GO
SET IDENTITY_INSERT [dbo].[OrderDetails] OFF
GO
ALTER TABLE [dbo].[Order] ADD  DEFAULT (getdate()) FOR [OrderDate]
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customer] ([CustomerID])
GO
ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Order] ([OrderID])
GO
