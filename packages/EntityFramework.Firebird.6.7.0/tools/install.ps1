param($installPath, $toolsPath, $package, $project)                                                                                                                    
Add-EFProvider $project 'FirebirdSql.Data.FirebirdClient' 'EntityFramework.Firebird.FbProviderServices, EntityFramework.Firebird'
Add-EFDefaultConnectionFactory $project 'EntityFramework.Firebird.FbConnectionFactory, EntityFramework.Firebird'