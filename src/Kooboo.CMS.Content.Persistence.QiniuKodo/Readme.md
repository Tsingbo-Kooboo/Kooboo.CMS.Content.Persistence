## 示例配置文件,配置文件放在bin中：
### 文件名：QiniuSettings.json

``` json
{
  "Endpoint": "http://img.kooboo.com",
  "AccessKeyId": "AccessKeyId_here",
  "AccessKeySecret": "AccessKeySecret_here",
  "BucketName": "kooboobucket",
  "CustomDomain": "http://img.kooboo.com",
  "RepositoryBuckets": [
    {
      "Endpoint": "http://sample.kooboo.com",
      "RepositoryName": "sample",
      "BucketName": "sample-site",
      "CustomDomain": "http://sample.kooboo.com"
    }
  ]
}
```