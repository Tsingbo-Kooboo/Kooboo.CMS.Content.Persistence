#region License
// 
// Copyright (c) 2013, Kooboo team
// 
// Licensed under the BSD License
// See the file LICENSE.txt for details.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models.Paths;
using Kooboo.CMS.Content.Models;
using Kooboo.Web.Url;
using Kooboo.CMS.Content.Persistence.AliyunOSS.Models;

namespace Kooboo.CMS.Content.Persistence.AliyunOSS
{
    public static class TextContentBlobHelper
    {
        public static string GetTextContentDirectoryPath(this TextContent textContent)
        {
            var textFolder = textContent.GetFolder();
            return UrlUtility.Combine(new string[] {
                textContent.Repository,
                ConstValues.TextContentFileDirectoryName
            }.Concat(textFolder.NamePaths)
               .Concat(new[] {
                   textContent.UUID
               }).ToArray());
        }
        public static string GetTextContentFilePath(this TextContent textContent, ContentFile contentFile)
        {
            return UrlUtility.Combine(GetTextContentDirectoryPath(textContent), contentFile.FileName);
        }
    }
}
