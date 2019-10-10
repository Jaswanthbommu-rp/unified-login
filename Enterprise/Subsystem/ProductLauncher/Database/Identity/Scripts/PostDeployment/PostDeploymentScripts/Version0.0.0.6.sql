EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 1
	,@ProductGUID = '733481ca-d054-41e4-8eea-98a32d9ab933'
	,@Name = 'OneSite'
	,@Description = 'The OneSite environment provides access to Leasing and Rents, Facilities, Purchasing, and Document Management for your properties, depending the mix of products which are licensed.  Use this logo for future state Leasing & Rents.  Also need to discuss whether one tile for Leasing & Rents will apply to Affordable, Senior, and Student. '
	,@ProductTypeId = 101

EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 4
	,@ProductGUID = 'aa4b4561-de40-4bf1-a934-cab15d9e8d57'
	,@Name = 'Asset Optimization'
	,@Description = 'RealPage Portfolio Asset Management (PAM) is a solution designed and developed specifically for general partners, limited partners, and property management professionals, to provide the portfolio data, critical metrics, and thorough analysis you need, regardless of asset type or operational platform. You’ll have the power to collect financial and operating data and collaborate with property management partners, enabling them to continue to leverage their existing operational structures and best business practices.'
	,@ProductTypeId = 406

EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 5
	,@ProductGUID = '19f0c725-1f91-45da-898e-0a1f8719f393'
	,@Name = 'Propertyware'
	,@Description = 'Provides a customizable property management solution designed to deliver the best value to your business and include eSignature, Payments, Screening, Inspections, Maintenance Projects and Portals.  (We created an icon for this that we can use when we launch beta for NRT by end of 2017.)'
	,@ProductTypeId = 101

EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 6
	,@ProductGUID = 'a08b46bc-0abf-4ee3-8b60-5c74d26ac4b8'
	,@Name = 'Lead2Lease'
	,@Description = 'The Lead2Lease solution helps you identify the quality of your leads, increase lead conversion, improve leasing efficiency, and better target your marketing efforts. Make sure no inquiry goes unanswered. Prioritize follow-up, keep track of leasing agent performance, and know where your marketing dollars are most effective through detailed reporting.'
	,@ProductTypeId = 305

EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 7
	,@ProductGUID = '201346b3-285d-4cf5-892a-ff4578c78fed'
	,@Name = 'YieldStar'
	,@Description = 'Revenue Management tools leverage our lease-transaction transaction data to provide a daily.'
	,@ProductTypeId = 401
			
EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 8
	,@ProductGUID = '99fa12b6-cd85-4a74-ae92-36a765e23b61'
	,@Name = 'RealPage Accounting'
	,@Description = 'RealPage Accounting is a feature-rich, web-based property management accounting solution designed for corporate operations of any size.'
	,@ProductTypeId = 102
			
EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 9
	,@ProductGUID = '87f7b143-8bf1-42f8-abdb-2b26a54e2abe'
	,@Name = 'Websites & Syndication'
	,@Description = 'Provides access to Marketing Center which provides tools to manage your community website and other related marketing content.'
	,@ProductTypeId = 303
			
EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 10
	,@ProductGUID = '55da302a-eb6a-40df-ba5f-9cca53416b35'
	,@Name = 'Prospect Contact Center'
	,@Description = 'Gives you the ability to interact with the tools used by the RealPage Contact Center answering your calls to cultivate the premium leads that convert to more leases, and acting as a natural extension of your leasing and marketing team. '
	,@ProductTypeId = 302
			
EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 12
	,@ProductGUID = 'ff9a1fb8-5e94-4868-af29-2d730bda7981'
	,@Name = 'Ops Bid'
	,@Description = 'RealPage Vendor Credentialing software is a web-based tracking platform that provides a full.'
	,@ProductTypeId = 104
			
EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 13
	,@ProductGUID = '491ccc70-365d-4f8b-8812-3d9c03b7d5bb'
	,@Name = 'Spend Management'
	,@Description = 'With RealPage Spend Management software solutions, your portfolio’s entire P.O. and approval process is automated with real time visibility into spending against your budget.  Includes selected vendors online catalogs for electronic orders and electronic invoicing.'
	,@ProductTypeId = 104
			
EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 15
	,@ProductGUID = '28b3c90b-8b33-4749-b18f-bb80cfbf84e3'
	,@Name = 'Renters Insurance'
	,@Description = 'RealPage Renters Insurance makes covering your assets, and those of your residents, simple. Our eRenterPlan program offers residents affordable, comprehensive coverage, while optional RenterProtection provides gap coverage for vacant units or uninsured residents.'
	,@ProductTypeId = 204
			
EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 16
	,@ProductGUID = '73dad5cd-b6c2-448d-b6aa-7fcaa0982194'
	,@Name = 'Vendor Services'
	,@Description = 'RealPage Vendor Credentialing software is a web-based tracking platform that provides a full-service solution that manages vendor compliance and ensures they continue to work within your guidelines.'
	,@ProductTypeId = 105

EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 17
	,@ProductGUID = '9dd95752-ee9c-495b-b822-0ee12abb51e0'
	,@Name = 'Active Building'
	,@Description = 'The RealPage ActiveBuilding Resident Portal engages residents, boost renewals, and provides.'
	,@ProductTypeId = 201

EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 18
	,@ProductGUID = '57eba82a-64cb-4921-bdca-71c8d9b99f67'
	,@Name = 'Utility Management'
	,@Description = 'Utility Management provides powerful platform for your business including one-bill.'
	,@ProductTypeId = 205
			
EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 20
	,@ProductGUID = 'dbcdc320-76d1-46e7-a70e-0c76739c6b12'
	,@Name = 'RealPage Document Management'
	,@Description = 'Provides a centralized storage resource for electronic records and other digital information required for compliance with RealPage Document Management software. Go paperless and save even more time and hassles with best-in-class electronic signature capability via DocuSign®.'
	,@ProductTypeId = NULL

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='7'