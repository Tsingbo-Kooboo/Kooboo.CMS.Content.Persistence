# Kooboo.CMS.Content.Persistence
保存Kooboo CMS的MediaLibrary中的文件到阿里云OSS等


##示例配置

###文件名: AliyunOSSSettings.json

```javascript
 {
    "Endpoint": "http://oss-cn-shanghai.aliyuncs.com",
    "AccessKeyId": "Your Access Key Id Here",
    "AccessKeySecret": "Your Access Key Secrect Here",
    "BucketName": "Your Bucket Name Here",
    "CustomDomain": "http://cdn.kooboo.com",
    "RepositoryBuckets": [
        {
            "Endpoint": "http://oss-cn-hangzhou.aliyuncs.com",
            "RepositoryName": "Sample",
            "BucketName": "Sample",
            "CustomDomain": "http://sample.kooboo.com"
        }
    ]
}
```