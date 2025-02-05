**Steps for Aws S3:**
-	Create a new asp.netcore web api
-	Install awssdk.s3 NuGet package
-	 Copy Access key and Secret key and add in appsetting.json
-	Create connection with the  Amazons3Client
-	Create Api controller
-	Inject IConfiguration
-	Create action methods for each operation

**Operations Handled:**
1.	Listing Buckets
2.	Updating bucket settings 
3.	Adding folders and subfolders to a bucket
4.	Deleting buckets,
5.	Uploading  and listing object in a bucket  
6.	Updating file/object metadata 
7.	Copying files from one bucket to another
